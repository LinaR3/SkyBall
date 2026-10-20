using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCameraX : MonoBehaviour
{
    private float speed = 200;
    public GameObject player;
    private InputSystem_Actions controls;

    void Awake()
    {
        controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = controls.Player.Move.ReadValue<Vector2>().x;
        transform.Rotate(Vector3.up, horizontalInput * speed * Time.deltaTime);

        transform.position = player.transform.position; // Move focal point with player

    }
}
