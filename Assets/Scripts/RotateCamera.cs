using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 150f;
    private InputSystem_Actions controls;

    private void Awake()
    {
        EnsureControls();
    }

    private void OnEnable()
    {
        EnsureControls();
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Player.Disable();
    }

    private void OnDestroy()
    {
        if (controls != null)
        {
            controls.Dispose();
            controls = null;
        }
    }

    private void Update()
    {
        if (controls == null) return;
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, moveInput.x * rotationSpeed * Time.deltaTime);
    }

    private void EnsureControls()
    {
        if (controls == null)
            controls = new InputSystem_Actions();
    }
}
