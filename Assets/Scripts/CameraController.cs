using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camera controller with two modes:
///   1. Third-Person (Chase): Smooth follow behind the vehicle
///   2. Top-Down (Bird's Eye): Overhead view looking straight down
/// 
/// Switch between modes using the C key or UI button.
/// Smooth transitions between camera positions using Lerp/Slerp.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Camera Modes
    // ─────────────────────────────────────────────
    public enum CameraMode
    {
        ThirdPerson,  // Chase camera behind the vehicle
        TopDown       // Bird's eye view from above
    }

    [Header("Target")]
    [Tooltip("The vehicle transform to follow")]
    public Transform target;

    [Header("Camera Mode")]
    public CameraMode currentMode = CameraMode.ThirdPerson;

    // ─────────────────────────────────────────────
    // Third-Person Settings
    // ─────────────────────────────────────────────
    [Header("Third-Person (Chase) Settings")]
    [Tooltip("Distance behind the vehicle")]
    public float chaseDistance = 8f;

    [Tooltip("Height above the vehicle")]
    public float chaseHeight = 4f;

    [Tooltip("Smoothing speed for position tracking")]
    [Range(1f, 20f)]
    public float positionSmoothSpeed = 5f;

    [Tooltip("Smoothing speed for rotation tracking")]
    [Range(1f, 20f)]
    public float rotationSmoothSpeed = 8f;

    [Tooltip("Look-ahead offset (how far in front of the car to look)")]
    public float lookAheadDistance = 5f;

    // ─────────────────────────────────────────────
    // Top-Down Settings
    // ─────────────────────────────────────────────
    [Header("Top-Down (Bird's Eye) Settings")]
    [Tooltip("Height above the vehicle for top-down view")]
    public float topDownHeight = 40f;

    [Tooltip("Whether the top-down camera follows the vehicle horizontally")]
    public bool topDownFollowsTarget = true;

    // ─────────────────────────────────────────────
    // Transition
    // ─────────────────────────────────────────────
    [Header("Transition")]
    [Tooltip("Speed of smooth transition between camera modes")]
    [Range(1f, 10f)]
    public float transitionSpeed = 3f;

    // ─────────────────────────────────────────────
    // Internal State
    // ─────────────────────────────────────────────
    private Vector3 _currentVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        // Toggle camera mode with C key
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            ToggleCameraMode();
        }

        // Update camera based on current mode
        switch (currentMode)
        {
            case CameraMode.ThirdPerson:
                UpdateThirdPerson();
                break;
            case CameraMode.TopDown:
                UpdateTopDown();
                break;
        }
    }

    /// <summary>
    /// Switches between Third-Person and Top-Down camera modes.
    /// </summary>
    public void ToggleCameraMode()
    {
        currentMode = currentMode == CameraMode.ThirdPerson 
            ? CameraMode.TopDown 
            : CameraMode.ThirdPerson;
    }

    /// <summary>
    /// Updates the third-person chase camera.
    /// Smoothly follows behind the vehicle, looking at a point ahead of it.
    /// </summary>
    private void UpdateThirdPerson()
    {
        // Desired position: behind and above the vehicle
        Vector3 desiredPosition = target.position 
            - target.forward * chaseDistance 
            + Vector3.up * chaseHeight;

        // Smooth position transition
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref _currentVelocity, 
            1f / positionSmoothSpeed
        );

        // Look at a point slightly ahead of the vehicle
        Vector3 lookTarget = target.position + target.forward * lookAheadDistance;

        // Smooth rotation transition
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            desiredRotation, 
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    /// <summary>
    /// Updates the top-down bird's eye camera.
    /// Looks straight down from a fixed height above the vehicle.
    /// </summary>
    private void UpdateTopDown()
    {
        Vector3 desiredPosition;

        if (topDownFollowsTarget)
        {
            // Follow the vehicle horizontally, fixed height
            desiredPosition = new Vector3(
                target.position.x,
                topDownHeight,
                target.position.z
            );
        }
        else
        {
            // Fixed position above the track center
            desiredPosition = new Vector3(20f, topDownHeight, 10f);
        }

        // Smooth transition
        transform.position = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            Time.deltaTime * transitionSpeed
        );

        // Look straight down
        Quaternion desiredRotation = Quaternion.Euler(90f, 0f, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            desiredRotation, 
            Time.deltaTime * transitionSpeed
        );
    }
}
