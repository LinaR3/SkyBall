// using UnityEngine;
// using UnityEngine.InputSystem;


// public class RotateCamera : MonoBehaviour
// {
//     private InputSystem_Actions control;
//     public float rotationForce;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Awake()
//     {
//         control = new InputSystem_Actions();
//     }

//     private void OnEnable()
//     {
//         control.Player.Enable();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         Vector2 moveInput = control.Player.Move.ReadValue<Vector2>();
//         transform.Rotate(Vector3.up, moveInput.x * rotationForce * Time.deltaTime);
//     }
// }