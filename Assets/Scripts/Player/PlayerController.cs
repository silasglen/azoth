using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down 2D player controller.  Reads Move / Interact / Sprint from the
/// project's InputSystem_Actions asset via a sibling <see cref="PlayerInput"/>
/// component.  Detects nearby <see cref="IInteractable"/> objects with an
/// overlap-circle in the facing direction.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput), typeof(Inventory))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintMultiplier = 1.6f;

    [Header("Interaction")]
    [SerializeField] float interactRadius = 0.5f;
    [SerializeField] float interactOffset = 0.8f;
    [SerializeField] LayerMask interactLayers = ~0;

    public Inventory Inventory { get; private set; }

    /// <summary>Normalised direction the player last moved in.</summary>
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    /// <summary>True when the player is providing movement input.</summary>
    public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;

    Rigidbody2D _rb;
    InputAction _moveAction;
    InputAction _sprintAction;
    InputAction _interactAction;

    Vector2 _moveInput;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        Inventory = GetComponent<Inventory>();

        var input = GetComponent<PlayerInput>();
        _moveAction = input.actions["Move"];
        _sprintAction = input.actions["Sprint"];
        _interactAction = input.actions["Interact"];
    }

    void OnEnable()
    {
        _interactAction.performed += OnInteract;
    }

    void OnDisable()
    {
        _interactAction.performed -= OnInteract;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            _moveInput = Vector2.zero;
            return;
        }

        _moveInput = _moveAction.ReadValue<Vector2>();
        if (_moveInput.sqrMagnitude > 0.01f)
            FacingDirection = _moveInput.normalized;
    }

    void FixedUpdate()
    {
        bool sprinting = _sprintAction.IsPressed();
        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        _rb.linearVelocity = _moveInput.normalized * speed;
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        var nearest = GetNearestInteractable();
        nearest?.Interact(this);
    }

    /// <summary>Returns the nearest IInteractable in range, or null.</summary>
    public IInteractable GetNearestInteractable()
    {
        Vector2 origin = (Vector2)transform.position + FacingDirection * interactOffset;
        var hits = Physics2D.OverlapCircleAll(origin, interactRadius, interactLayers);

        IInteractable best = null;
        float bestDist = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var candidate))
            {
                float dist = Vector2.Distance(origin, hit.transform.position);
                if (dist < bestDist) { best = candidate; bestDist = dist; }
            }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + FacingDirection * interactOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, interactRadius);
    }
}
