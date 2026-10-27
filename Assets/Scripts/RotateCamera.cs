using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 150f;
    [SerializeField] private float followDistance = 10.5f;
    [SerializeField] private float followHeight = 7.5f;
    [SerializeField] private float followSmoothing = 7f;

    public static RotateCamera Instance { get; private set; }
    public bool IsFollowCamera { get; private set; }

    private Transform gameplayCamera;
    private Transform player;
    private Vector3 orbitLocalPosition;
    private Quaternion orbitLocalRotation;

    private void Awake()
    {
        Instance = this;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            gameplayCamera = mainCamera.transform;
            orbitLocalPosition = gameplayCamera.localPosition;
            orbitLocalRotation = gameplayCamera.localRotation;
        }
    }

    private void Update()
    {
        float horizontalInput = PlayerController.Instance != null ? PlayerController.Instance.HorizontalInput : 0f;
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            ToggleCameraMode();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void LateUpdate()
    {
        if (!IsFollowCamera || gameplayCamera == null) return;
        if (player == null && PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
        if (player == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 targetPosition = player.position + Vector3.up * followHeight - forward * followDistance;
        float followFactor = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
        gameplayCamera.position = Vector3.Lerp(gameplayCamera.position, targetPosition, followFactor);

        Vector3 lookTarget = player.position + Vector3.up * 0.45f;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - gameplayCamera.position, Vector3.up);
        gameplayCamera.rotation = Quaternion.Slerp(gameplayCamera.rotation, targetRotation, followFactor);
    }

    private void ToggleCameraMode()
    {
        IsFollowCamera = !IsFollowCamera;
        if (!IsFollowCamera && gameplayCamera != null)
        {
            gameplayCamera.localPosition = orbitLocalPosition;
            gameplayCamera.localRotation = orbitLocalRotation;
        }
    }

}
