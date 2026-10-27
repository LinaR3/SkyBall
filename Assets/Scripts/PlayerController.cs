using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Reads player input and controls movement, jumping, and size.</summary>
public sealed class PlayerController : MonoBehaviour
{
    private const float ArenaHalfWidth = 7f;
    private const float ArenaHalfLength = 8f;

    public static PlayerController Instance { get; private set; }

    [SerializeField] private float playerSpeed = 8f;
    [SerializeField] private float acceleration = 32f;
    [SerializeField] private float braking = 38f;
    [SerializeField] private GameObject powerupIndicator;
    [SerializeField] private Transform aimReference;
    [SerializeField] private float jumpImpulse = 5.5f;
    [SerializeField] private float groundCheckDistance = 0.9f;

    private InputSystem_Actions controls;
    private Rigidbody body;
    private SphereCollider sphereCollider;
    private PlayerAbilityController abilities;
    private BallDirectionIndicator directionIndicator;
    private Vector2 moveInput;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 originalScale;
    private float originalSpeed;
    private float originalAcceleration;
    private float originalBraking;
    private bool jumpRequested;
    private string ballSizeLabel = "MEDIANA · NORMAL";

    public float RapidFireRemaining => abilities != null ? abilities.RapidFireRemaining : 0f;
    public float PowerupRemaining => abilities != null ? abilities.PowerupRemaining : 0f;
    public string PickupMessage => abilities != null ? abilities.PickupMessage : string.Empty;
    public string BallSizeLabel => ballSizeLabel;
    public float HorizontalInput => moveInput.x;
    public Transform AimReference => aimReference;

    private void Awake()
    {
        Instance = this;
        controls = new InputSystem_Actions();
        body = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        playerSpeed = Mathf.Max(playerSpeed, 8f);
        acceleration = Mathf.Max(acceleration, 32f);
        braking = Mathf.Max(braking, 38f);
        if (body != null)
        {
            body.mass = 2.2f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (aimReference == null)
            Debug.LogError("PlayerController needs an aim reference for movement and shots.", this);
        startPosition = transform.position;
        startRotation = transform.rotation;
        originalScale = transform.localScale;
        originalSpeed = playerSpeed;
        originalAcceleration = acceleration;
        originalBraking = braking;

        SkyballMatchManager.EnsureInstance();
        Collider arenaGround = SkyballArenaPresentation.Instance != null
            ? SkyballArenaPresentation.Instance.GroundCollider
            : null;
        abilities = gameObject.AddComponent<PlayerAbilityController>();
        abilities.Initialize(aimReference, powerupIndicator);
        directionIndicator = gameObject.AddComponent<BallDirectionIndicator>();
        directionIndicator.Initialize(transform, aimReference, arenaGround);
    }

    private void OnEnable()
    {
        if (controls == null) controls = new InputSystem_Actions();
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls != null) controls.Player.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (controls == null) return;
        controls.Dispose();
        controls = null;
    }

    private void Update()
    {
        if (controls != null)
        {
            moveInput = Vector2.ClampMagnitude(controls.Player.Move.ReadValue<Vector2>(), 1f);
            if (controls.Player.Jump.WasPressedThisFrame()) jumpRequested = true;
        }

        ReadGameplayShortcuts();
        if (transform.position.y < -10f) ResetPlayer();
    }

    private void FixedUpdate()
    {
        if (body == null) return;

        if (aimReference != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(aimReference.forward, Vector3.up).normalized;
            Vector3 moveDirection = forward * moveInput.y;
            float maxSpeed = playerSpeed * (abilities != null ? abilities.SpeedMultiplier : 1f);
            Vector3 targetVelocity = moveDirection * maxSpeed;
            Vector3 currentVelocity = body.linearVelocity;
            Vector3 planarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            float response = moveDirection.sqrMagnitude > 0f ? acceleration : braking;
            Vector3 velocityChange = Vector3.ClampMagnitude(targetVelocity - planarVelocity, response * Time.fixedDeltaTime);
            body.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            if (IsGrounded()) body.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }

        KeepPlayerInsideArena();
    }

    private void KeepPlayerInsideArena()
    {
        Vector3 position = body.position;
        Vector3 boundedPosition = new Vector3(
            Mathf.Clamp(position.x, -ArenaHalfWidth, ArenaHalfWidth),
            position.y,
            Mathf.Clamp(position.z, -ArenaHalfLength, ArenaHalfLength));
        if (boundedPosition == position) return;

        body.position = boundedPosition;
        Vector3 velocity = body.linearVelocity;
        if (!Mathf.Approximately(position.x, boundedPosition.x)) velocity.x = 0f;
        if (!Mathf.Approximately(position.z, boundedPosition.z)) velocity.z = 0f;
        body.linearVelocity = velocity;
    }

    private bool IsGrounded()
    {
        float scaledRadius = sphereCollider != null
            ? sphereCollider.radius * Mathf.Abs(transform.lossyScale.y)
            : groundCheckDistance;
        float checkDistance = Mathf.Max(groundCheckDistance, scaledRadius + 0.2f);
        return Physics.Raycast(transform.position, Vector3.down, checkDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void ReadGameplayShortcuts()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.digit1Key.wasPressedThisFrame) SetBallSize(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) SetBallSize(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) SetBallSize(2);
    }

    private void SetBallSize(int size)
    {
        if (body == null) return;
        switch (size)
        {
            case 0:
                transform.localScale = originalScale * 0.78f;
                playerSpeed = 8f;
                acceleration = 32f;
                braking = 38f;
                body.mass = 1.3f;
                ballSizeLabel = "PEQUEÑA · ÁGIL";
                break;
            case 1:
                transform.localScale = originalScale;
                playerSpeed = originalSpeed;
                acceleration = originalAcceleration;
                braking = originalBraking;
                body.mass = 2.2f;
                ballSizeLabel = "MEDIANA · NORMAL";
                break;
            default:
                transform.localScale = originalScale * 1.24f;
                playerSpeed = 4.8f;
                acceleration = 14f;
                braking = 18f;
                body.mass = 2.8f;
                ballSizeLabel = "GRANDE · PESADA";
                break;
        }

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }

    public void ResetPlayer()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        jumpRequested = false;
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    public void ResetForGoal()
    {
        ResetPlayer();
        SetBallSize(1);
        if (abilities != null) abilities.ResetAbilities();
        if (directionIndicator != null) directionIndicator.SetVisible(true);
    }
}
