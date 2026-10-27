using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed = 6.5f;
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private float braking = 30f;
    public bool hasPowerup;
    public GameObject powerupIndicator;
    [SerializeField] private float speedBoostMultiplier = 1.75f;
    [SerializeField] private float powerupDuration = 5f;
    [SerializeField] private float powerupStrength = 15f;
    [SerializeField] private float jumpImpulse = 5.5f;
    [SerializeField] private float groundCheckDistance = 0.9f;
    [SerializeField] private float arrowForwardOffset = 1f;
    [SerializeField] private float arrowGroundOffset = 0.075f;

    private InputSystem_Actions controls;
    private Rigidbody playerRb;
    private GameObject focalPoint;
    private Vector2 moveInput;
    private Transform directionArrow;
    private Material directionArrowMaterial;
    private bool jumpRequested;
    private Coroutine powerupRoutine;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
        startPosition = transform.position;
        startRotation = transform.rotation;
        GoalReset.CacheInitialPositions();
        CreateDirectionArrow();
    }

    private void Start()
    {
        if (powerupIndicator != null)
            powerupIndicator.SetActive(hasPowerup);
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new InputSystem_Actions();
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Player.Disable();
    }

    private void OnDestroy()
    {
        if (directionArrow != null)
            Destroy(directionArrow.gameObject);
        if (directionArrowMaterial != null)
            Destroy(directionArrowMaterial);
        if (controls != null)
        {
            controls.Dispose();
            controls = null;
        }
    }

    private void Update()
    {
        if (controls != null)
        {
            moveInput = Vector2.ClampMagnitude(controls.Player.Move.ReadValue<Vector2>(), 1f);
            if (controls.Player.Jump.WasPressedThisFrame())
                jumpRequested = true;
        }

        UpdateDirectionArrow();

        if (powerupIndicator != null && powerupIndicator.activeSelf)
            powerupIndicator.transform.position = transform.position + Vector3.down * 0.5f;

        if (transform.position.y < -10f)
            ResetPlayer();
    }

    private void FixedUpdate()
    {
        if (playerRb == null || focalPoint == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(focalPoint.transform.forward, Vector3.up).normalized;
        // A/D rotates the Focal Point; W/S moves along its horizontal forward axis.
        Vector3 moveDirection = forward * moveInput.y;
        float currentMaxSpeed = playerSpeed * (hasPowerup ? speedBoostMultiplier : 1f);
        Vector3 targetVelocity = moveDirection * currentMaxSpeed;
        Vector3 currentVelocity = playerRb.linearVelocity;
        Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        float response = moveDirection.sqrMagnitude > 0f ? acceleration : braking;
        Vector3 velocityChange = Vector3.ClampMagnitude(targetVelocity - currentPlanarVelocity, response * Time.fixedDeltaTime);
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);

        if (jumpRequested)
        {
            jumpRequested = false;
            if (IsGrounded())
                playerRb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void CreateDirectionArrow()
    {
        directionArrow = new GameObject("Direction Chevrons").transform;

        Shader arrowShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (arrowShader == null) arrowShader = Shader.Find("Unlit/Color");
        directionArrowMaterial = new Material(arrowShader);
        directionArrowMaterial.color = new Color(0.2f, 0.95f, 0.84f, 1f);
        CreateArrowSegment("Front Chevron Left", new Vector3(-0.3f, 0f, -0.08f), new Vector3(0f, 0f, 0.16f));
        CreateArrowSegment("Front Chevron Right", new Vector3(0f, 0f, 0.16f), new Vector3(0.3f, 0f, -0.08f));
        CreateArrowSegment("Rear Chevron Left", new Vector3(-0.3f, 0f, -0.36f), new Vector3(0f, 0f, -0.12f));
        CreateArrowSegment("Rear Chevron Right", new Vector3(0f, 0f, -0.12f), new Vector3(0.3f, 0f, -0.36f));
    }

    private void CreateArrowSegment(string segmentName, Vector3 start, Vector3 end)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = segmentName;
        segment.transform.SetParent(directionArrow, false);
        segment.transform.localPosition = (start + end) * 0.5f;
        Vector3 direction = end - start;
        segment.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        segment.transform.localScale = new Vector3(0.095f, 0.035f, direction.magnitude);
        Destroy(segment.GetComponent<Collider>());

        MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = directionArrowMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.renderingLayerMask = uint.MaxValue;

        // Keep the floor marker readable from the gameplay camera even when the ground depth buffer covers it.
        renderer.material.renderQueue = 4000;
        renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        renderer.material.SetInt("_ZWrite", 0);
    }

    private void UpdateDirectionArrow()
    {
        if (directionArrow == null || focalPoint == null) return;
        Vector3 forward = Vector3.ProjectOnPlane(focalPoint.transform.forward, Vector3.up).normalized;
        Vector3 arrowPosition = transform.position + forward * arrowForwardOffset;
        if (Physics.Raycast(arrowPosition + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit,
                6f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            arrowPosition = groundHit.point + Vector3.up * arrowGroundOffset;
        else
            arrowPosition += Vector3.down * 0.72f;

        directionArrow.SetPositionAndRotation(arrowPosition, Quaternion.LookRotation(forward, Vector3.up));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            Destroy(other.gameObject);
            hasPowerup = true;

            if (powerupIndicator != null)
                powerupIndicator.SetActive(true);

            if (powerupRoutine != null)
                StopCoroutine(powerupRoutine);
            powerupRoutine = StartCoroutine(PowerupCountdownRoutine());
        }
        else if (other.CompareTag("Goal"))
        {
            GoalReset.ResetAll();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRigidbody == null) return;

            Vector3 awayFromPlayer = (collision.transform.position - transform.position).normalized;
            enemyRigidbody.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
        }
    }

    private IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(powerupDuration);
        hasPowerup = false;
        if (powerupIndicator != null)
            powerupIndicator.SetActive(false);
        powerupRoutine = null;
    }

    public void ResetPlayer()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        jumpRequested = false;
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }
}

// Shared reset helper for goal triggers.
public static class GoalReset
{
    private static Rigidbody ballRigidbody;
    private static Vector3 ballStartPosition;
    private static Quaternion ballStartRotation;

    public static void CacheInitialPositions()
    {
        GameObject ball = GameObject.Find("Ball");
        if (ball == null) return;
        ballRigidbody = ball.GetComponent<Rigidbody>();
        if (ballRigidbody != null)
        {
            ballStartPosition = ball.transform.position;
            ballStartRotation = ball.transform.rotation;
        }
    }

    public static void ResetAll()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.ResetPlayer();
        }

        if (ballRigidbody == null) CacheInitialPositions();
        if (ballRigidbody != null)
        {
            ballRigidbody.transform.SetPositionAndRotation(ballStartPosition, ballStartRotation);
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
