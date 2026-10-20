using UnityEngine;
using UnityEngine.InputSystem;


public class RotateCamera : MonoBehaviour
{
    private InputSystem_Actions control;
    public float rotationSpeed = 50f;
    
    void Awake()
    {
        control = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        control.Player.Enable();
    }

    void Update()
    {
        Vector2 moveInput = control.Player.Move.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, moveInput.x * rotationSpeed * Time.deltaTime);
    }
}