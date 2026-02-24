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
