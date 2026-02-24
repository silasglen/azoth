using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Drives a proximity-fade shader on the walls tilemap layer.
/// Attach this to the same GameObject as the walls TilemapRenderer,
/// or any convenient GameObject in the scene.
///
/// Setup:
/// 1. Assign the walls TilemapRenderer (the layer that should fade).
/// 2. Assign the player Transform.
/// 3. The TilemapRenderer's material must use the TilemapProximityFade shader.
///
/// Optional: Enable overlap detection to only fade when the player is actually
/// behind the walls layer (requires a trigger Collider2D on this GameObject
/// covering the "behind" area).
/// </summary>
public class TilemapProximityFade : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TilemapRenderer on the walls layer that should fade.")]
    [SerializeField] private TilemapRenderer _wallsTilemap;

    [Tooltip("The player's Transform. Position is sent to the shader every frame. " +
             "If left empty, the script will auto-find the player at runtime (works across scenes).")]
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
        if (_wallsTilemap == null)
        {
            Debug.LogError($"[TilemapProximityFade] No walls TilemapRenderer assigned on {gameObject.name}.", this);
            enabled = false;
            return;
        }

        if (_player == null)
        {
            // Auto-find the player across all loaded scenes (handles persistent scene setup)
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null)
                _player = pc.transform;
        }

        if (_player == null)
        {
            Debug.LogError($"[TilemapProximityFade] Could not find player. Assign manually or ensure a PlayerController exists.", this);
            enabled = false;
            return;
        }

        // Instance the material so we don't modify the shared asset
        _instancedMaterial = new Material(_wallsTilemap.material);
        _wallsTilemap.material = _instancedMaterial;

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
