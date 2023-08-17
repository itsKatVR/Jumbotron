using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraTargetFollower : UdonSharpBehaviour
{
    // Camera Target Follower, built by itsKat. This Script allows you to view Targets & Follow them.
    public GameObject target; // The camera's current target
    public GameObject[] targetsArray; // Array to hold all target objects (manually set in the Unity editor)

    public float rotationSpeed = 5f; // Rotation speed of the camera
    public float zoomSpeed = 5f; // Zoom speed of the camera
    public float maxZoomDistance = 10f; // Maximum zoom-in distance
    public float minZoomDistance = 1f;

    public float targetSwitchInterval = 5f; // Time interval to switch targets automatically
    public float zoomOutTime = 1f; // Time taken for zooming out before switching targets
    private float timeSinceLastTargetSwitch = 0f;

    private int currentTargetIndex = 0;
    private Camera mainCamera;
    private float targetZoom; // Store the desired zoom level
    private float currentZoom; // Store the current zoom level
    private bool isZoomingOut = false; // Flag to track if the camera is currently zooming out
    private bool isSwitchingTarget = false; // Flag to track if the camera is currently switching targets

    private void Start()
    {
        // Get the main camera component
        mainCamera = GetComponent<Camera>();

        // Set the initial target and zoom level
        ChangeTarget();
    }

    private void Update()
    {
        // Follow the current target with rotation only
        if (target != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Zoom feature
        if (target != null)
        {
            if (isZoomingOut)
            {
                // Zoom-out animation
                float targetDistance = Vector3.Distance(transform.position, target.transform.position);

                // Determine the desired zoom level based on the target distance
                targetZoom = maxZoomDistance;

                // Smoothly change the camera's field of view (zoom level)
                currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSpeed * Time.deltaTime);
                mainCamera.fieldOfView = currentZoom;

                // Check if the zoom-out animation is complete
                if (Mathf.Abs(currentZoom - maxZoomDistance) < 0.01f)
                {
                    // Zoom-out animation complete, now we can switch targets
                    isZoomingOut = false;
                    isSwitchingTarget = true;
                    ChangeTarget();
                }
            }
            else if (isSwitchingTarget)
            {
                // We are currently switching targets, wait until the zoom-in animation is complete
                float targetDistance = Vector3.Distance(transform.position, target.transform.position);

                // Set the desired zoom level to the current distance from the new target
                targetZoom = Mathf.Clamp(targetDistance, 0f, minZoomDistance);

                // Smoothly change the camera's field of view (zoom level)
                currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSpeed * Time.deltaTime);
                mainCamera.fieldOfView = currentZoom;

                // Check if the zoom-in animation is complete
                if (Mathf.Abs(currentZoom - minZoomDistance) < 0.01f)
                {
                    isSwitchingTarget = false; // Reset the flag
                }
            }
            else
            {
                // No zooming animation, follow the current target
                float targetDistance = Vector3.Distance(transform.position, target.transform.position);

                // Set the desired zoom level to the current distance from the target
                targetZoom = Mathf.Clamp(targetDistance, 0f, maxZoomDistance);

                // Smoothly change the camera's field of view (zoom level)
                currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSpeed * Time.deltaTime);
                mainCamera.fieldOfView = currentZoom;
            }
        }

        // Check if enough time has passed to start the zoom-out animation
        timeSinceLastTargetSwitch += Time.deltaTime;
        if (!isZoomingOut && !isSwitchingTarget && timeSinceLastTargetSwitch >= targetSwitchInterval)
        {
            // Start the zoom-out animation
            isZoomingOut = true;
            timeSinceLastTargetSwitch = 0f; // Reset the timer
        }
    }

    private void ChangeTarget()
    {
        // Increment the current target index and loop back to 0 if it exceeds the array length
        currentTargetIndex = (currentTargetIndex + 1) % targetsArray.Length;

        // Set the new target based on the current index
        target = targetsArray[currentTargetIndex];

        // Set the initial zoom level to the current distance from the new target
        if (target != null)
        {
            float targetDistance = Vector3.Distance(transform.position, target.transform.position);
            currentZoom = Mathf.Clamp(targetDistance, 0f, maxZoomDistance);
            targetZoom = currentZoom;
        }
    }
}