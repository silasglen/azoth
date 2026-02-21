using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// General-purpose NPC component.  Implements <see cref="IInteractable"/>
/// to deliver dialogue and optionally patrols between waypoints.
/// All active NPCs are tracked in the static <see cref="All"/> list.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NPC : MonoBehaviour, IInteractable
{
    // ── Static registry ─────────────────────────────────────────────────
    static readonly List<NPC> _all = new();
    static readonly ReadOnlyCollection<NPC> _allReadOnly = new(_all);
    /// <summary>Read-only view of every active NPC in the scene.</summary>
    public static ReadOnlyCollection<NPC> All => _allReadOnly;

    // ── Inspector fields ────────────────────────────────────────────────
    [Header("Identity")]
    [SerializeField] string displayName = "NPC";
    [TextArea(1, 4)]
    [SerializeField] string[] dialogueLines = { "..." };

    [Header("Patrol (optional)")]
    [Tooltip("Assign world-space waypoints.  Leave empty for a stationary NPC.")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float waypointPause = 1f;

    public string DisplayName => displayName;
    public string InteractionPrompt => $"Talk to {displayName}";

    // ── Runtime state ───────────────────────────────────────────────────
    int _dialogueIndex;
    int _waypointIndex;
    float _pauseTimer;
    bool _waiting;

    // ── Lifecycle ───────────────────────────────────────────────────────
    void OnEnable()  => _all.Add(this);
    void OnDisable() => _all.Remove(this);

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
            return;
        if (waypoints == null || waypoints.Length < 2) return;
        Patrol();
    }

    // ── Interaction ─────────────────────────────────────────────────────
    public void Interact(PlayerController player)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        string line = dialogueLines[_dialogueIndex];
        Debug.Log($"[{displayName}] {line}");

        _dialogueIndex = (_dialogueIndex + 1) % dialogueLines.Length;
    }

    // ── Patrol ──────────────────────────────────────────────────────────
    void Patrol()
    {
        if (_waiting)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer > 0f) return;
            _waiting = false;
            _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
        }

        Transform wp = waypoints[_waypointIndex];
        if (wp == null) return;

        Vector3 target = wp.position;
        transform.position = Vector3.MoveTowards(transform.position, target,
                                                  patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            _waiting = true;
            _pauseTimer = waypointPause;
        }
    }
}
