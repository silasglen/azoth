using UnityEngine;

/// <summary>
/// Code-based sprite animator for 4-directional movement.
/// Cycles through sprite sheet frames on the SpriteRenderer.
/// Sprite arrays are populated by the PlayerAnimatorSetup editor script.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Frame Rate")]
    [SerializeField] int walkFramesPerSecond = 8;
    [SerializeField] int idleFramesPerSecond = 3;

    [Header("Idle Sprites (auto-populated)")]
    [SerializeField] Sprite[] idleFront;
    [SerializeField] Sprite[] idleBack;
    [SerializeField] Sprite[] idleLeft;
    [SerializeField] Sprite[] idleRight;

    [Header("Walk Sprites (auto-populated)")]
    [SerializeField] Sprite[] walkFront;
    [SerializeField] Sprite[] walkBack;
    [SerializeField] Sprite[] walkLeft;
    [SerializeField] Sprite[] walkRight;

    PlayerController _player;
    SpriteRenderer _sr;
    float _timer;
    int _frame;
    Sprite[] _currentClip;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (_player == null) return;

        Sprite[] clip = PickClip();

        // Reset frame when clip changes
        if (clip != _currentClip)
        {
            _currentClip = clip;
            _frame = 0;
            _timer = 0f;
        }

        if (_currentClip == null || _currentClip.Length == 0) return;

        // Advance frame
        _timer += Time.deltaTime;
        int fps = _player.IsMoving ? walkFramesPerSecond : idleFramesPerSecond;
        float interval = 1f / fps;
        if (_timer >= interval)
        {
            _timer -= interval;
            _frame = (_frame + 1) % _currentClip.Length;
        }

        _sr.sprite = _currentClip[_frame];
    }

    Sprite[] PickClip()
    {
        Vector2 dir = _player.FacingDirection;
        bool moving = _player.IsMoving;

        // Determine dominant axis
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0
                ? (moving ? walkRight : idleRight)
                : (moving ? walkLeft : idleLeft);
        else
            return dir.y > 0
                ? (moving ? walkBack : idleBack)
                : (moving ? walkFront : idleFront);
    }
}
