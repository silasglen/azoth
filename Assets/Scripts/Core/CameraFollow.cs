using UnityEngine;

/// <summary>
/// Smooth-follow camera for 2D.  Attach to the Main Camera.  Optionally
/// clamp position to world bounds so the camera never shows past the
/// tilemap edges.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Transform to follow (usually the Player).")]
    [SerializeField] Transform target;

    [Tooltip("How quickly the camera catches up (smaller = snappier).")]
    [SerializeField] float smoothTime = 0.15f;

    [Tooltip("If true, the camera is clamped inside worldBounds.")]
    [SerializeField] bool useBounds;

    [Tooltip("World-space rectangle the camera centre must stay within.")]
    [SerializeField] Rect worldBounds = new(-50, -50, 100, 100);

    Vector3 _velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = target.position;
        goal.z = transform.position.z; // keep camera depth

        Vector3 pos = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);

        if (useBounds)
        {
            pos.x = Mathf.Clamp(pos.x, worldBounds.xMin, worldBounds.xMax);
            pos.y = Mathf.Clamp(pos.y, worldBounds.yMin, worldBounds.yMax);
        }

        transform.position = pos;
    }
}
