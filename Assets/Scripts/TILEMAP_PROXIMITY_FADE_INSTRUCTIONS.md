# Tilemap Proximity Fade — Implementation Instructions

## Overview

Implement a shader-based proximity fade system for Unity 2D tilemaps using URP. When the player walks behind a foreground tilemap layer (rooftops, canopies, upper walls, etc.), tiles near the player become semi-transparent, revealing the player underneath. The fade is a smooth radial gradient driven by a Shader Graph and a C# script.

**Requirements:** Unity 2D project using URP (Universal Render Pipeline).

---

## File Manifest

You will create the following files:

| File | Location | Purpose |
|------|----------|---------|
| `TilemapProximityFade.shadergraph` | `Assets/Shaders/` | URP Sprite shader with distance-based alpha fade |
| `TilemapProximityFade.cs` | `Assets/Scripts/Tilemaps/` | C# MonoBehaviour that feeds player position to the shader |

You will also create one material asset via script or instruct the user to create it manually.

---

## Step 1: Create the Shader Graph Asset

Create the file `Assets/Shaders/TilemapProximityFade.shadergraph`.

**Because Shader Graph assets are binary/YAML and cannot be reliably authored from a text editor or CLI, you must instead create a helper Editor script that programmatically builds the shader.** Create the following EditorWindow script at `Assets/Editor/CreateProximityFadeShader.cs`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CreateProximityFadeShader : MonoBehaviour
{
    [MenuItem("Tools/Create Proximity Fade Shader (Manual Setup Guide)")]
    public static void ShowInstructions()
    {
        string msg =
            "CREATE SHADER GRAPH MANUALLY:\n\n" +
            "1. Right-click in Assets/Shaders/ → Create → Shader Graph → URP → Sprite Lit Shader Graph\n" +
            "2. Name it 'TilemapProximityFade'\n" +
            "3. Open it and follow the node setup below.\n\n" +
            "See the Console for full node instructions.";
        EditorUtility.DisplayDialog("Proximity Fade Shader Setup", msg, "OK");

        Debug.Log(SHADER_GRAPH_INSTRUCTIONS);
    }

    private const string SHADER_GRAPH_INSTRUCTIONS = @"
========================================
TILEMAP PROXIMITY FADE — SHADER GRAPH NODE SETUP
========================================

GRAPH SETTINGS (Graph Inspector → gear icon):
- Surface Type: Transparent
- Blending Mode: Alpha
- Two Sided: On

PROPERTIES (Blackboard panel, click '+'):
──────────────────────────────────────
1. Name: _MainTex       Type: Texture2D    Default: white    Reference: _MainTex
2. Name: _PlayerPos     Type: Vector2      Default: (0, 0)   Reference: _PlayerPos
3. Name: _FadeRadius    Type: Float        Default: 2.0      Reference: _FadeRadius
4. Name: _FadedAlpha    Type: Float        Default: 0.3      Reference: _FadedAlpha
5. Name: _EdgeSoftness  Type: Float        Default: 1.0      Reference: _EdgeSoftness

IMPORTANT: For _MainTex, make sure the Reference string is exactly '_MainTex' (click the property in the blackboard and check the Graph Inspector). This is required for tilemap renderer compatibility.

NODE CHAIN:
──────────────────────────────────────

--- SECTION A: Texture Sampling ---
1. Drag _MainTex property onto the graph.
2. Add a 'Sample Texture 2D' node.
3. Connect _MainTex → Sample Texture 2D (Texture input).
4. Leave the UV input disconnected (it uses mesh UVs by default, which is correct for tilemaps).
5. The output gives you RGBA and A (alpha) separately. You will use both later.

--- SECTION B: Distance Calculation ---
6. Add a 'Position' node. Set Space to 'World'. (It defaults to Object; change it.)
7. Add a 'Swizzle' node. Set it to 'XY' (outputs a Vector2).
8. Connect Position (Out) → Swizzle (In).
9. Drag _PlayerPos property onto the graph.
10. Add a 'Distance' node.
11. Connect Swizzle (Out) → Distance (A input).
12. Connect _PlayerPos → Distance (B input).
    Result: 'distFromPlayer' — the world-space distance from this fragment to the player.

--- SECTION C: Compute Fade Edge ---
13. Drag _FadeRadius property onto the graph.
14. Drag _EdgeSoftness property onto the graph.
15. Add an 'Add' node.
16. Connect _FadeRadius → Add (A).
17. Connect _EdgeSoftness → Add (B).
    Result: the outer edge of the fade zone (FadeRadius + EdgeSoftness).

--- SECTION D: Smoothstep ---
18. Add a 'Smoothstep' node.
19. Connect _FadeRadius → Smoothstep (Edge1).
20. Connect Add (Out) → Smoothstep (Edge2).
21. Connect Distance (Out) → Smoothstep (In).
    Result: 0 when inside the fade radius, 1 when outside, smooth blend in between.

--- SECTION E: Lerp Alpha ---
22. Drag _FadedAlpha property onto the graph.
23. Add a 'Lerp' node.
24. Connect _FadedAlpha → Lerp (A).
25. Create a 'Float' constant node, set value to 1. Connect it → Lerp (B).
    (Alternatively, just type 1 directly into the B field if the node inspector allows it.)
26. Connect Smoothstep (Out) → Lerp (T).
    Result: 'finalAlpha' — equals _FadedAlpha when player is close, 1.0 when far away.

--- SECTION F: Combine and Output ---
27. Add a 'Multiply' node.
28. Connect Sample Texture 2D (A / alpha output) → Multiply (A).
29. Connect Lerp (Out) → Multiply (B).
    Result: final alpha that respects both the tile's own transparency AND the proximity fade.

30. Connect Sample Texture 2D (RGBA, the color output) → Fragment 'Base Color'.
31. Connect Multiply (Out) → Fragment 'Alpha'.

DONE. Save the Shader Graph (Ctrl+S / Cmd+S in the graph window).

========================================
SUMMARY OF CONNECTIONS:
========================================

_MainTex ──────────► Sample Texture 2D ──► RGBA ──► Base Color (Fragment)
                                          ──► A ────┐
                                                     ├──► Multiply ──► Alpha (Fragment)
Position(World) ► Swizzle(XY) ──┐                    │
                                 ├► Distance ──┐     │
_PlayerPos ─────────────────────┘              │     │
                                               ▼     │
_FadeRadius ──────────────────► Smoothstep ──► Lerp ─┘
_FadeRadius + _EdgeSoftness ──►  (Edge1/2)    (T)
                                               ▲
_FadedAlpha ──────────────────────────────► Lerp(A)
1.0 ──────────────────────────────────────► Lerp(B)

========================================
";
}
#endif
```

**Why an editor script instead of raw file creation:** Shader Graph `.shadergraph` files are complex serialized YAML with GUIDs, node position data, and internal references that vary by Unity version. Attempting to write one from scratch will almost certainly produce a corrupt or incompatible file. The editor script above prints step-by-step node wiring instructions into the Console so the user can build it quickly by hand.

---

## Step 2: Create the Material

After the user has created the Shader Graph, they need a material. Create an editor script that automates this at `Assets/Editor/CreateProximityFadeMaterial.cs`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CreateProximityFadeMaterial : MonoBehaviour
{
    [MenuItem("Tools/Create Proximity Fade Material")]
    public static void Create()
    {
        // Find the shader
        Shader shader = Shader.Find("Shader Graphs/TilemapProximityFade");
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find 'Shader Graphs/TilemapProximityFade'. " +
                "Make sure you have created the Shader Graph first and it compiled without errors.", "OK");
            return;
        }

        // Create material
        Material mat = new Material(shader);
        mat.SetFloat("_FadeRadius", 2f);
        mat.SetFloat("_FadedAlpha", 0.3f);
        mat.SetFloat("_EdgeSoftness", 1f);

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = "Assets/Materials/TilemapProximityFade.mat";
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success", $"Material created at {path}", "OK");
        Selection.activeObject = mat;
    }
}
#endif
```

---

## Step 3: Create the Runtime C# Script

Create `Assets/Scripts/Tilemaps/TilemapProximityFade.cs` with the following exact content:

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Drives a proximity-fade shader on a foreground tilemap layer.
/// Attach this to the same GameObject as the foreground TilemapRenderer,
/// or any convenient GameObject in the scene.
///
/// Setup:
/// 1. Assign the foreground TilemapRenderer (the layer that should fade).
/// 2. Assign the player Transform.
/// 3. The TilemapRenderer's material must use the TilemapProximityFade shader.
///
/// Optional: Enable overlap detection to only fade when the player is actually
/// behind the foreground layer (requires a trigger Collider2D on this GameObject
/// covering the "behind" area).
/// </summary>
public class TilemapProximityFade : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TilemapRenderer on the foreground layer that should fade.")]
    [SerializeField] private TilemapRenderer _foregroundTilemap;

    [Tooltip("The player's Transform. Position is sent to the shader every frame.")]
    [SerializeField] private Transform _player;

    [Header("Fade Settings")]
    [Tooltip("Radius around the player (in world units) where tiles begin to fade.")]
    [SerializeField] private float _fadeRadius = 2f;

    [Tooltip("Minimum alpha value at the player's position (0 = fully invisible, 1 = no fade).")]
    [Range(0f, 1f)]
    [SerializeField] private float _fadedAlpha = 0.3f;

    [Tooltip("Width of the soft edge between faded and opaque regions (in world units).")]
    [SerializeField] private float _edgeSoftness = 1f;

    [Header("Overlap Detection (Optional)")]
    [Tooltip("If true, the fade only activates when the player is inside this object's trigger collider. " +
             "If false, the fade is always active based on proximity alone.")]
    [SerializeField] private bool _useOverlapDetection = false;

    // Private state
    private Material _instancedMaterial;
    private bool _playerBehind = false;

    // Cached shader property IDs for performance
    private static readonly int PropPlayerPos = Shader.PropertyToID("_PlayerPos");
    private static readonly int PropFadeRadius = Shader.PropertyToID("_FadeRadius");
    private static readonly int PropFadedAlpha = Shader.PropertyToID("_FadedAlpha");
    private static readonly int PropEdgeSoftness = Shader.PropertyToID("_EdgeSoftness");

    private void Start()
    {
        if (_foregroundTilemap == null)
        {
            Debug.LogError($"[TilemapProximityFade] No foreground TilemapRenderer assigned on {gameObject.name}.", this);
            enabled = false;
            return;
        }

        if (_player == null)
        {
            Debug.LogError($"[TilemapProximityFade] No player Transform assigned on {gameObject.name}.", this);
            enabled = false;
            return;
        }

        // Instance the material so we don't modify the shared asset
        _instancedMaterial = new Material(_foregroundTilemap.material);
        _foregroundTilemap.material = _instancedMaterial;

        ApplySettings();
    }

    private void LateUpdate()
    {
        if (_instancedMaterial == null) return;

        // If using overlap detection, set radius to 0 when the player is NOT behind the layer.
        // This makes the Smoothstep treat everything as "outside the radius" → fully opaque.
        float effectiveRadius = (_useOverlapDetection && !_playerBehind) ? 0f : _fadeRadius;
        _instancedMaterial.SetFloat(PropFadeRadius, effectiveRadius);

        // Send player world position to shader
        Vector2 pos = _player.position;
        _instancedMaterial.SetVector(PropPlayerPos, new Vector4(pos.x, pos.y, 0f, 0f));
    }

    /// <summary>
    /// Push all inspector settings to the material. Called on Start and from OnValidate.
    /// </summary>
    private void ApplySettings()
    {
        if (_instancedMaterial == null) return;

        _instancedMaterial.SetFloat(PropFadeRadius, _fadeRadius);
        _instancedMaterial.SetFloat(PropFadedAlpha, _fadedAlpha);
        _instancedMaterial.SetFloat(PropEdgeSoftness, _edgeSoftness);
    }

    private void OnValidate()
    {
        // Live-update in Play Mode when the user tweaks inspector values
        ApplySettings();
    }

    private void OnDestroy()
    {
        // Clean up the instanced material to avoid leaks
        if (_instancedMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(_instancedMaterial);
            else
                DestroyImmediate(_instancedMaterial);
        }
    }

    // --- Overlap Detection (only used when _useOverlapDetection is true) ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_useOverlapDetection && other.CompareTag("Player"))
            _playerBehind = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_useOverlapDetection && other.CompareTag("Player"))
            _playerBehind = false;
    }
}
```

---

## Step 4: Scene Setup Instructions

After creating the above files, print or log the following setup instructions for the user. These are the manual steps they must complete in the Unity Editor:

### 4a. Tilemap Layer Structure

1. In the scene Hierarchy, ensure there are at least two Tilemaps under the Grid object:
   - **Ground / Base** tilemap (player walks on this — lower sorting order)
   - **Foreground** tilemap (the layer that should fade — higher sorting order, rendered above the player)
2. The Foreground tilemap's **Tilemap Renderer → Sorting Layer / Order in Layer** must be set so it renders ABOVE the player sprite. For example, if the player is on sorting order 5, set the foreground tilemap to sorting order 10.

### 4b. Apply the Material

1. Select the Foreground tilemap GameObject.
2. On its **Tilemap Renderer** component, set the **Material** field to the `TilemapProximityFade` material created in Step 2 (found at `Assets/Materials/TilemapProximityFade.mat`).

### 4c. Attach the Script

1. Add the `TilemapProximityFade` component to any GameObject in the scene (the Foreground tilemap GameObject itself is a good choice).
2. In the Inspector:
   - **Foreground Tilemap**: Drag in the Foreground tilemap's TilemapRenderer.
   - **Player**: Drag in the player's Transform.
   - **Fade Radius**: Start with `1.5` — this is the world-unit radius of the transparent zone.
   - **Faded Alpha**: Start with `0.25` — how see-through tiles become at minimum.
   - **Edge Softness**: Start with `1.0` — how gradual the boundary is.
   - **Use Overlap Detection**: Leave OFF for always-on proximity fade. Turn ON if you want the fade to only activate when the player enters a trigger zone.

### 4d. Overlap Detection Setup (Optional)

If `Use Overlap Detection` is enabled:

1. On the same GameObject that has the `TilemapProximityFade` script, add a **BoxCollider2D** (or PolygonCollider2D).
2. Check **Is Trigger** on the collider.
3. Size and position the collider to cover the area where the player can be "behind" the foreground (e.g., the footprint of a building).
4. Make sure the player GameObject has the tag **"Player"** and has a **Rigidbody2D** (can be Kinematic) so trigger events fire.

---

## Step 5: Verification Checklist

After setup, verify the following in Play Mode:

- [ ] The foreground tilemap is visible and renders above the player.
- [ ] Walking the player under/behind the foreground tilemap causes tiles near the player to become transparent.
- [ ] The fade is a smooth circular gradient, not a hard edge.
- [ ] Walking away restores full opacity.
- [ ] Adjusting `Fade Radius`, `Faded Alpha`, and `Edge Softness` in the Inspector during Play Mode updates the effect in real time.
- [ ] Tiles that are already partially transparent (e.g., decorative tiles with built-in alpha) still look correct — they should fade further, not snap to a wrong alpha.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Entire tilemap is invisible | Material's shader not set to Transparent surface type | Open Shader Graph → Graph Inspector → Surface Type: Transparent |
| No fade effect at all | `_MainTex` reference string doesn't match | Open Shader Graph blackboard → click _MainTex → Graph Inspector → Reference must be exactly `_MainTex` |
| Fade works but tiles look wrong / black | Shader Graph not set to Sprite Lit or Sprite Unlit | Delete and recreate as URP → Sprite Lit Shader Graph |
| Fade circle follows wrong position | Wrong Transform assigned to Player field | Verify the correct player Transform is assigned, not a child object |
| Triggers don't fire | Player missing tag or Rigidbody2D | Add "Player" tag and a Rigidbody2D (Kinematic is fine) to the player |
| Pink/magenta tiles | Shader compilation error | Open Shader Graph, check for errors in the node connections, re-save |
| Fade is too abrupt | Edge Softness too low | Increase `_EdgeSoftness` to 1.5–2.0 |
| Performance concern | N/A — this shader adds one distance calc per fragment | No action needed; this is negligible overhead |

---

## Recommended Starting Values

```
Fade Radius:    1.5
Faded Alpha:    0.25
Edge Softness:  1.0
```

These work well for a 32px-per-unit pixel art setup. For higher-resolution art or larger tile sizes, increase `Fade Radius` and `Edge Softness` proportionally.
