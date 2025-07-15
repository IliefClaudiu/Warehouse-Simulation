using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Mode")]
    public bool isManualMode = false; // If true, disables automatic input control

    [Header("Settings")]
    public float moveSpeed = 5f;         // Movement speed
    public float rotationSpeed = 180f;   // Rotation speed (degrees per second)

    [Header("References")]
    public Joystick mobileJoystick;      // Mobile joystick reference (optional)
    public Transform vrController;       // VR controller transform reference (optional)

    private Vector3 inputDirection;      // Current input direction vector
    private Rigidbody rb;                // Rigidbody component for physics movement

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Enable or disable manual mode.
    /// When manual mode is enabled, this script ignores all input.
    /// </summary>
    /// <param name="manual">True to enable manual mode, false for automatic mode</param>
    public void SetManualMode(bool manual)
    {
        isManualMode = manual;
    }

    void Update()
    {
        // If in manual mode, skip automatic input and stop movement
        if (isManualMode)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        // Get input from keyboard, joystick, or VR controller
        inputDirection = GetInputDirection();

        // If input magnitude is significant, move and rotate the player
        if (inputDirection.magnitude > 0.1f)
        {
            // Move forward/backward based on input's Z axis
            Vector3 movement = transform.forward * inputDirection.z * moveSpeed;
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

            // Rotate left/right based on input's X axis
            transform.Rotate(Vector3.up, inputDirection.x * rotationSpeed * Time.deltaTime);
        }
        else
        {
            // No input, stop horizontal movement but keep vertical velocity (gravity)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    /// <summary>
    /// Returns input direction vector from appropriate input method:
    /// keyboard (desktop), joystick (mobile), or VR controller (VR).
    /// </summary>
    Vector3 GetInputDirection()
    {
        // Desktop input (keyboard arrows or WASD)
        if (SystemInfo.deviceType == DeviceType.Desktop)
            return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Mobile joystick input
        if (mobileJoystick != null)
            return new Vector3(mobileJoystick.Horizontal, 0, mobileJoystick.Vertical);

        // VR controller forward direction (assuming forward moves player)
        if (vrController != null)
            return vrController.forward;

        // Default no input
        return Vector3.zero;
    }
}