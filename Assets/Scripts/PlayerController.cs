using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed = 3f;
    public bool hasPowerup;
    public GameObject powerupIndicator;
    [SerializeField] private float speedBoostMultiplier = 1.75f;
    [SerializeField] private float powerupDuration = 5f;
    [SerializeField] private float powerupStrength = 15f;

    private InputSystem_Actions controls;
    private Rigidbody playerRb;
    private GameObject focalPoint;
    private float baseSpeed;
    private Coroutine powerupRoutine;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
        baseSpeed = playerSpeed;
        startPosition = transform.position;
        startRotation = transform.rotation;
        GoalReset.CacheInitialPositions();
    }

    private void Start()
    {
        if (powerupIndicator != null)
            powerupIndicator.SetActive(hasPowerup);
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        if (focalPoint != null && playerRb != null)
        {
            Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
            playerRb.AddForce(focalPoint.transform.forward * moveInput.y * playerSpeed * Time.deltaTime);
        }

        if (powerupIndicator != null && powerupIndicator.activeSelf)
            powerupIndicator.transform.position = transform.position + Vector3.down * 0.5f;

        if (transform.position.y < -10f)
            ResetPlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            Destroy(other.gameObject);
            hasPowerup = true;
            playerSpeed = baseSpeed * speedBoostMultiplier;

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
        playerSpeed = baseSpeed;
        if (powerupIndicator != null)
            powerupIndicator.SetActive(false);
        powerupRoutine = null;
    }

    public void ResetPlayer()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
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
