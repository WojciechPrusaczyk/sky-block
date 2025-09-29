using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Maksymalna prędkość (jednostki/s).")]
    public float maxSpeed = 5f;

    [Tooltip("Jak szybko rozpędzamy się do prędkości docelowej.")]
    public float acceleration = 20f;

    [Tooltip("Jak szybko wytracamy prędkość, gdy nie ma wejścia.")]
    public float deceleration = 30f;

    [Tooltip("Ignoruj bardzo małe sygnały z analoga.")]
    public float inputDeadzone = 0.1f;

    [Header("Input System")]
    [Tooltip("Przypnij tu akcję 'Move' (Vector2) z Input Actions.")]
    public InputActionReference moveAction;

    public bool isGrounded;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 velocity;
    private GroundDetector groundDetector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        groundDetector = GetComponentInChildren<GroundDetector>();
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
    }

    private void Update()
    {
        var raw = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        if (raw.magnitude < inputDeadzone)
            input = Vector2.zero;
        else
            input = Vector2.ClampMagnitude(raw, 1f);

        isGrounded = groundDetector.IsGrounded;
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = input * maxSpeed;
        float a = (targetVelocity.sqrMagnitude > 0.0001f) ? acceleration : deceleration;
        Vector2 diff = targetVelocity - velocity;
        Vector2 step = Vector2.ClampMagnitude(diff, a * Time.fixedDeltaTime);
        velocity += step;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public Vector2 CurrentVelocity => velocity;
}