using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Handles AR/MR placement of the vehicle using AR Foundation
/// Supports both Quest passthrough and mobile AR
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class MRPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VehicleConfigurator vehicleConfigurator;
    [SerializeField] private GameObject placementIndicator;
    [SerializeField] private ARPlaneManager planeManager;
    
    [Header("Placement Settings")]
    [SerializeField] private float placementHeight = 0f; // Offset from detected plane
    [SerializeField] private bool autoRotateToCamera = true;
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;
    [SerializeField] private Renderer indicatorRenderer;
    
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private Pose placementPose;
    private bool placementPoseValid = false;
    private Camera arCamera;
    
    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }
    
    private void OnEnable()
    {
        // Subscribe to plane detection events
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }
    
    private void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }
    
    private void Update()
    {
        // Only update placement if vehicle hasn't been placed yet
        if (vehicleConfigurator != null && placementIndicator != null)
        {
            UpdatePlacementPose();
            UpdatePlacementIndicator();
        }
    }
    
    #region Placement Logic
    
    /// <summary>
    /// Updates the placement pose based on screen center raycast
    /// </summary>
    private void UpdatePlacementPose()
{
    #if UNITY_EDITOR
    // EDITOR MODE: Always make placement valid for testing
    placementPoseValid = true;
    
    // Place 2 meters in front of camera, on the floor (y=0)
    if (arCamera != null)
    {
        Vector3 forward = arCamera.transform.forward;
        forward.y = 0; // Keep on ground level
        
        placementPose.position = arCamera.transform.position + forward.normalized * 5f;
        placementPose.position.y = 0; // Force to ground level
        placementPose.rotation = Quaternion.LookRotation(forward);
    }
    else
    {
        // Fallback position
        placementPose.position = new Vector3(0, 0, 2);
        placementPose.rotation = Quaternion.identity;
    }
    
    #else
    // DEVICE MODE: Use real AR raycasting
    Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    
    if (raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinPolygon))
    {
        placementPoseValid = true;
        placementPose = raycastHits[0].pose;
        placementPose.position += Vector3.up * placementHeight;
        
        if (autoRotateToCamera && arCamera != null)
        {
            Vector3 cameraForward = arCamera.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
    else
    {
        placementPoseValid = false;
    }
    #endif
}
    
    /// <summary>
    /// Updates visual feedback for placement indicator
    /// </summary>
    private void UpdatePlacementIndicator()
    {
        if (placementIndicator == null) return;
        
        // Show/hide indicator based on validity
        placementIndicator.SetActive(placementPoseValid);
        
        if (placementPoseValid)
        {
            // Smoothly move indicator to placement pose
            placementIndicator.transform.SetPositionAndRotation(
                placementPose.position,
                placementPose.rotation
            );
            
            // Update indicator color
            if (indicatorRenderer != null)
            {
                indicatorRenderer.material.color = validPlacementColor;
            }
        }
    }
    
    /// <summary>
    /// Called when user taps to place vehicle
    /// Hook this up to a UI button or input action
    /// </summary>
    public void OnPlacementConfirmed()
    {
        if (!placementPoseValid)
        {
            Debug.LogWarning("Cannot place vehicle - no valid placement pose");
            return;
        }
        
        // Update placement indicator to final position
        placementIndicator.transform.SetPositionAndRotation(
            placementPose.position,
            placementPose.rotation
        );
        
        // Trigger vehicle placement
        if (vehicleConfigurator != null)
        {
            vehicleConfigurator.PlaceVehicle();
        }
        
        // Disable further plane detection for performance
        if (planeManager != null)
        {
            planeManager.enabled = false;
        }
    }
    
    #endregion
    
    #region Plane Management
    
    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Handle new planes
        if (args.added.Count > 0)
        {
            Debug.Log($"Detected {args.added.Count} new plane(s)");
        }
        
        // Optional: Visualize planes for debugging
        // You can add plane visualization here if needed
    }
    
    /// <summary>
    /// Toggles visibility of detected AR planes
    /// Useful for debugging
    /// </summary>
    public void TogglePlaneVisualization(bool visible)
    {
        if (planeManager == null) return;
        
        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(visible);
        }
    }
    
    #endregion
    
    #region Manual Placement (for testing without AR)
    
    /// <summary>
    /// Places vehicle at a fixed position (for editor testing)
    /// </summary>
    public void PlaceVehicleAtPosition(Vector3 position, Quaternion rotation)
    {
        if (placementIndicator != null)
        {
            placementIndicator.transform.SetPositionAndRotation(position, rotation);
        }
        
        placementPoseValid = true;
        OnPlacementConfirmed();
    }
    
    #endregion
}
