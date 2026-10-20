using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions control;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 500f;

    [Header("Camera Reference")]
    [SerializeField] private GameObject focalPoint;

    private void Awake()
    {
        control = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        control.Player.Enable();
    }
    private void OnDisable()
    {
        control.Player.Disable();
    }
    private void Update()
    {
        UnityEngine.Vector2 moveInput = control.Player.Move.ReadValue<Vector2>();

        if (focalPoint != null)
        {
            UnityEngine.Vector3 moveDirection = focalPoint.transform.forward * moveInput.y;
            rb.AddForce(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
}
