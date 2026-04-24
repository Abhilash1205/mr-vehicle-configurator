using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Central controller for the MR Vehicle Configurator.
/// Manages vehicle placement, customization, and state.
/// Supports multiple vehicle models with different color options.
/// </summary>
public class VehicleConfigurator : MonoBehaviour
{
    [Header("Vehicle Selection")]
    [SerializeField] private GameObject[] vehiclePrefabs; // Array of vehicle models
    [SerializeField] private int currentVehicleIndex = 0; // Currently selected vehicle
    [SerializeField] private string[] vehicleNames; // Names for UI (GLC, C63, etc.)


    [Header("Vehicle Components")]
    [SerializeField] private Transform vehicleParent;

    [Header("Color Materials")]
    [SerializeField] private Material[] colorMaterials_GLC = new Material[8]; // GLC: 8 colors
    [SerializeField] private Material[] colorMaterials_C63 = new Material[6]; // C63: 6 colors
    // Index 0: Red, 1: Blue, 2: Black, 3: White, 4: Green, 5: Yellow, 6: Gray (GLC only), 7: Brown (GLC only)

    [Header("Customizable Parts")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Renderer[] wheelRenderers;
    [SerializeField] private GameObject[] wheelModels; // Different wheel styles

    [Header("Configuration Data")]
    [SerializeField] private VehicleConfigData currentConfig;

    [Header("MR Components")]
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private GameObject placementIndicator;

    [Header("UI References")]
    [SerializeField] private Button[] vehicleSelectButtons; // Vehicle selection buttons

    // State
    private GameObject spawnedVehicle;
    private bool vehiclePlaced = false;
    private int currentWheelStyleIndex = 0;
    private bool doorsOpen = false; // Track door state
    private Dictionary<Transform, float> doorRotations = new Dictionary<Transform, float>(); // Track each door's rotation

    // Events
    public System.Action<Color> OnColorChanged;
    public System.Action<int> OnWheelStyleChanged;
    public System.Action OnVehicleReset;
    public System.Action<int> OnVehicleSelected;

    private void Start()
    {
        // Initialize with default vehicle selection
        UpdateVehicleButtonHighlights();

        // Initialize with default config
        if (currentConfig != null)
        {
            ApplyConfiguration(currentConfig);
        }
    }

    private void Update()
    {
        // Handle placement indicator visibility
        if (!vehiclePlaced && arPlaneManager != null)
        {
            UpdatePlacementIndicator();
        }
    }

    #region Vehicle Selection

    /// <summary>
    /// Selects which vehicle to spawn (0 = first vehicle, 1 = second, etc.)
    /// Can be called before or after placement
    /// </summary>
    public void SelectVehicle(int index)
    {
        if (index < 0 || index >= vehiclePrefabs.Length)
        {
            Debug.LogWarning($"Invalid vehicle index: {index}");
            return;
        }

        if (vehiclePrefabs[index] == null)
        {
            Debug.LogError($"Vehicle prefab at index {index} is not assigned!");
            return;
        }

        // Update selection
        currentVehicleIndex = index;
        UpdateVehicleButtonHighlights();
        OnVehicleSelected?.Invoke(index);

        // If vehicle already placed, switch it
        if (vehiclePlaced)
        {
            // Store current state
            Vector3 currentPos = spawnedVehicle.transform.position;
            Quaternion currentRot = spawnedVehicle.transform.rotation;
            Vector3 currentScale = spawnedVehicle.transform.localScale;

            // Destroy old vehicle
            Destroy(spawnedVehicle);

            // Spawn new vehicle at same position
            SpawnVehicleAtPosition(currentPos, currentRot);
            spawnedVehicle.transform.localScale = currentScale;

            Debug.Log($"Switched to vehicle: {GetVehicleName(index)}");
        }
        else
        {
            // Just remember selection for when they place it
            Debug.Log($"Selected vehicle: {GetVehicleName(index)}");
        }
    }

    /// <summary>
    /// Gets the display name of a vehicle
    /// </summary>
    public string GetVehicleName(int index)
    {
        if (vehicleNames != null && index < vehicleNames.Length)
        {
            return vehicleNames[index];
        }
        return $"Vehicle {index}";
    }

    /// <summary>
    /// Updates visual highlighting of vehicle selection buttons
    /// </summary>
    private void UpdateVehicleButtonHighlights()
    {
        if (vehicleSelectButtons == null) return;

        for (int i = 0; i < vehicleSelectButtons.Length; i++)
        {
            if (vehicleSelectButtons[i] != null)
            {
                ColorBlock colors = vehicleSelectButtons[i].colors;

                if (i == currentVehicleIndex)
                {
                    // Selected - bright blue highlight
                    colors.normalColor = new Color(0.3f, 0.7f, 1f, 1f);
                    colors.highlightedColor = new Color(0.4f, 0.8f, 1f, 1f);
                }
                else
                {
                    // Not selected - default
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                }

                vehicleSelectButtons[i].colors = colors;
            }
        }
    }

    #endregion

    #region Vehicle Placement

    /// <summary>
    /// Spawns the currently selected vehicle at the placement indicator position
    /// </summary>
    public void PlaceVehicle()
    {
        if (vehiclePlaced || placementIndicator == null) return;

        SpawnVehicleAtPosition(
            placementIndicator.transform.position,
            placementIndicator.transform.rotation
        );

        vehiclePlaced = true;
        placementIndicator.SetActive(false);

        Debug.Log($"Placed vehicle: {GetVehicleName(currentVehicleIndex)}");
    }

    /// <summary>
    /// Spawns the currently selected vehicle at a specific position
    /// </summary>
    private void SpawnVehicleAtPosition(Vector3 position, Quaternion rotation)
    {
        if (currentVehicleIndex >= vehiclePrefabs.Length || vehiclePrefabs[currentVehicleIndex] == null)
        {
            Debug.LogError($"Vehicle prefab {currentVehicleIndex} not assigned!");
            return;
        }

        // Spawn vehicle
        spawnedVehicle = Instantiate(
            vehiclePrefabs[currentVehicleIndex],
            position,
            rotation,
            vehicleParent
        );

        

        // Cache renderers from spawned instance
        CacheVehicleRenderers();

        // Apply current configuration
        if (currentConfig != null)
        {
            ApplyConfiguration(currentConfig);
        }
        
        // Play engine start sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEngineStart();
        }
    }

    /// <summary>
    /// Resets vehicle to default position and configuration
    /// </summary>
    public void ResetVehicle()
    {
        // Play engine stop sound before destroying vehicle
        if (AudioManager.Instance != null && spawnedVehicle != null)
        {
            AudioManager.Instance.PlayEngineStop();
        }
        
        if (spawnedVehicle != null)
        {
            Destroy(spawnedVehicle);
        }

        vehiclePlaced = false;
        placementIndicator.SetActive(true);
        currentWheelStyleIndex = 0;

        OnVehicleReset?.Invoke();
        Debug.Log("Vehicle reset");
    }

    private void UpdatePlacementIndicator()
    {
        // Simple raycast-based placement (will be enhanced with AR Foundation)
        if (placementIndicator == null) return;

        // This is a placeholder - implement proper AR plane detection
        placementIndicator.SetActive(arPlaneManager.trackables.count > 0);
    }

    #endregion

    #region Customization

    /// <summary>
    /// Changes vehicle color by swapping materials on all body parts
    /// </summary>
    public void ChangeBodyColor(Color newColor)
    {
        if (spawnedVehicle == null)
        {
            Debug.LogWarning("No vehicle spawned!");
            return;
        }

        // Get the color materials for current vehicle
        Material[] currentColorMaterials = GetColorMaterialsForCurrentVehicle();

        // Find which color index this is based on color value
        int colorIndex = GetColorIndex(newColor);

        if (colorIndex < 0 || colorIndex >= currentColorMaterials.Length)
        {
            Debug.LogWarning($"Invalid color index {colorIndex}");
            return;
        }

        if (currentColorMaterials[colorIndex] == null)
        {
            Debug.LogWarning($"No material assigned for color index {colorIndex}");
            return;
        }

        Material targetMaterial = currentColorMaterials[colorIndex];
        int partsChanged = 0;

        // Find all body parts and swap their materials
        Renderer[] allRenderers = spawnedVehicle.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in allRenderers)
        {
            Material[] mats = renderer.materials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                string matName = mats[i].name.ToLower();

                // Check if this is a body paint material
                // Matches materials with "paint" or "body" in name
                if (matName.Contains("paint") || matName.Contains("body"))
                {
                    mats[i] = targetMaterial;
                    changed = true;
                    partsChanged++;
                }
            }

            if (changed)
            {
                renderer.materials = mats;
            }
        }

        Debug.Log($"✓ Swapped to {targetMaterial.name} on {partsChanged} parts");

        // Update config
        if (currentConfig != null)
        {
            currentConfig.bodyColor = newColor;
        }

        OnColorChanged?.Invoke(newColor);
    }

    /// <summary>
    /// Gets the appropriate color materials array for the current vehicle
    /// </summary>
    private Material[] GetColorMaterialsForCurrentVehicle()
    {
        switch (currentVehicleIndex)
        {
            case 0: // GLC
                return colorMaterials_GLC;
            case 1: // C63
                return colorMaterials_C63;
            default:
                Debug.LogWarning($"Unknown vehicle index: {currentVehicleIndex}, defaulting to GLC colors");
                return colorMaterials_GLC;
        }
    }

    /// <summary>
    /// Gets the color index based on RGB values
    /// Maps color to material array index
    /// </summary>
    private int GetColorIndex(Color color)
    {
        // Match color to index in colorPresets array (defined in ConfiguratorUI)
        // 0: Red, 1: Blue, 2: Black, 3: White, 4: Green, 5: Yellow, 6: Gray, 7: Brown

        // Red: high R, low G, low B
        if (color.r > 0.6f && color.g < 0.3f && color.b < 0.3f) return 0;

        // Blue: low R, low/mid G, high B
        if (color.r < 0.3f && color.g < 0.5f && color.b > 0.6f) return 1;

        // Black: all low
        if (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f) return 2;

        // White: all high
        if (color.r > 0.8f && color.g > 0.8f && color.b > 0.8f) return 3;

        // Green: low R, high G, low B
        if (color.r < 0.4f && color.g > 0.5f && color.b < 0.4f) return 4;

        // Yellow: high R, high G, low B
        if (color.r > 0.7f && color.g > 0.6f && color.b < 0.4f) return 5;

        // Gray: all mid-range
        if (color.r > 0.4f && color.g > 0.4f && color.b > 0.4f &&
            color.r < 0.7f && color.g < 0.7f && color.b < 0.7f) return 6;

        // Brown: high R, mid G, low B
        if (color.r > 0.5f && color.g > 0.2f && color.b < 0.3f) return 7;

        Debug.LogWarning($"Unknown color: {color}");
        return -1; // Unknown color
    }

    /// <summary>
    /// Changes the vehicle body material (metallic, matte, etc.)
    /// </summary>
    public void ChangeBodyMaterial(Material newMaterial)
    {
        if (bodyRenderer != null && newMaterial != null)
        {
            bodyRenderer.material = newMaterial;
            Debug.Log($"Body material changed to: {newMaterial.name}");
        }
    }

    /// <summary>
    /// Cycles through available wheel styles
    /// </summary>
    public void ChangeWheelStyle()
    {
        if (wheelModels == null || wheelModels.Length == 0)
        {
            Debug.LogWarning("No wheel models configured");
            return;
        }

        // Deactivate current wheels
        wheelModels[currentWheelStyleIndex].SetActive(false);

        // Cycle to next style
        currentWheelStyleIndex = (currentWheelStyleIndex + 1) % wheelModels.Length;

        // Activate new wheels
        wheelModels[currentWheelStyleIndex].SetActive(true);

        OnWheelStyleChanged?.Invoke(currentWheelStyleIndex);
        Debug.Log($"Wheel style changed to index: {currentWheelStyleIndex}");
    }

    /// <summary>
    /// Opens/closes vehicle doors (if rigged)
    /// </summary>
    public void ToggleDoor(string doorName, bool open)
    {
        if (spawnedVehicle == null) return;

        Transform door = spawnedVehicle.transform.Find(doorName);
        if (door != null)
        {
            // Simple rotation animation (can be enhanced with Animation/Animator)
            float targetRotation = open ? 60f : 0f;
            door.localRotation = Quaternion.Euler(0, targetRotation, 0);
            Debug.Log($"{doorName} {(open ? "opened" : "closed")}");
        }
    }

    /// <summary>
    /// Toggles all vehicle doors open/closed with smooth animation
    /// Works for GLC which has separate door objects
    /// Rotates around door edge (hinge) even if pivot is at center
    /// </summary>
    public void ToggleAllDoors()
    {
        if (spawnedVehicle == null)
        {
            Debug.LogWarning("No vehicle spawned!");
            return;
        }

        // Toggle state
        doorsOpen = !doorsOpen;
        
        // GLC door names and their hinge side
        DoorInfo[] doors = new DoorInfo[]
        {
            new DoorInfo("F_Door.L_10", false, 60f),  // Front Left - hinge on left, opens outward
            new DoorInfo("F_Door.R_15", true, 60f),   // Front Right - hinge on right, opens outward
            new DoorInfo("B_Door.L_44", false, 60f),  // Back Left - hinge on left, opens outward
            new DoorInfo("B_Door.R_49", true, 60f)    // Back Right - hinge on right, opens outward
        };
        
        int doorsFound = 0;
        
        foreach (DoorInfo doorInfo in doors)
        {
            Transform door = FindChildRecursive(spawnedVehicle.transform, doorInfo.name);
            
            if (door != null)
            {
                // Determine target rotation
                float targetRotation = doorsOpen ? doorInfo.openAngle : 0f;
                
                // Start smooth animation with custom pivot
                StartCoroutine(AnimateDoorAroundHinge(door, targetRotation, doorInfo.isRightSide));
                doorsFound++;
            }
        }
        
        if (doorsFound > 0)
        {
            // Play door sound
            if (AudioManager.Instance != null)
            {
                if (doorsOpen)
                {
                    AudioManager.Instance.PlayDoorOpen();
                }
                else
                {
                    AudioManager.Instance.PlayDoorClose();
                }
            }
            
            Debug.Log($"✓ {(doorsOpen ? "Opening" : "Closing")} {doorsFound} doors");
        }
        else
        {
            Debug.LogWarning("No doors found on this vehicle (C63 doesn't have separate doors)");
        }
    }

    /// <summary>
    /// Helper class to store door information
    /// </summary>
    private class DoorInfo
    {
        public string name;
        public bool isRightSide;
        public float openAngle;
        
        public DoorInfo(string name, bool isRightSide, float openAngle)
        {
            this.name = name;
            this.isRightSide = isRightSide;
            this.openAngle = openAngle;
        }
    }

    /// <summary>
    /// Smoothly animates a door rotating around its hinge edge (not center pivot)
    /// This works even when the door's pivot point is at the center
    /// Handles both opening (0° → 60°) and closing (60° → 0°)
    /// </summary>
    private System.Collections.IEnumerator AnimateDoorAroundHinge(Transform door, float targetRotation, bool isRightSide)
    {
        float duration = 0.8f; // Animation duration in seconds
        float elapsed = 0f;
        
        // Calculate hinge offset based on door bounds
        Renderer doorRenderer = door.GetComponentInChildren<Renderer>();
        float hingeOffsetX = 0.4f; // Default offset (adjust if needed)
        
        // SPECIAL CASE: Front Left Door - Manual Override
        if (door.name == "F_Door.L_10")
        {
            hingeOffsetX = 1f; // ← ADJUST THIS VALUE for front left door!
            // Try: 0.3, 0.4, 0.5, 0.6, 0.7 until it works correctly
            Debug.Log($"Front Left Door - Using manual hinge offset: {hingeOffsetX}");
        }
        else if (doorRenderer != null)
        {
            // Other doors use automatic calculation
            float doorWidth = doorRenderer.bounds.size.x;
            hingeOffsetX = doorWidth / 2.5f; // Divide by 2.5 for better hinge position
        }
        
        // Hinge is at left edge for left doors, right edge for right doors
        Vector3 localHingeOffset = isRightSide ? 
            new Vector3(hingeOffsetX, 0, 0) :   // Right side - hinge on right edge
            new Vector3(-hingeOffsetX, 0, 0);   // Left side - hinge on left edge
        
        // Calculate hinge point in world space
        Vector3 hingePoint = door.position + door.TransformDirection(localHingeOffset);
        
        // Get current rotation (where the door is NOW)
        float startRotation = 0f;
        if (doorRotations.ContainsKey(door))
        {
            startRotation = doorRotations[door];
        }
        
        // Right doors need to rotate in opposite direction
        float rotationMultiplier = isRightSide ? -1f : 1f;
        float actualTargetRotation = targetRotation * rotationMultiplier;
        
        // Calculate total rotation needed (from current to target)
        float totalRotation = actualTargetRotation - startRotation;
        
        float currentRotation = startRotation;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth easing curve (ease-in-out)
            t = t * t * (3f - 2f * t); // Smoothstep formula
            
            // Calculate the rotation for this frame
            float desiredRotation = Mathf.Lerp(startRotation, actualTargetRotation, t);
            float rotationDelta = desiredRotation - currentRotation;
            
            // Rotate around the hinge point on Y-axis (vertical)
            door.RotateAround(hingePoint, Vector3.up, rotationDelta);
            
            currentRotation = desiredRotation;
            
            yield return null; // Wait for next frame
        }
        
        // Final adjustment to ensure exact target rotation
        float finalDelta = actualTargetRotation - currentRotation;
        door.RotateAround(hingePoint, Vector3.up, finalDelta);
        
        // Store the final rotation for this door
        doorRotations[door] = actualTargetRotation;
        
        Debug.Log($"Door {door.name}: {startRotation}° → {actualTargetRotation}° (total rotation: {totalRotation}°, side: {(isRightSide ? "right" : "left")})");
    }

    /// <summary>
    /// Recursively searches for a child transform by name
    /// Needed because doors are nested deep in the hierarchy
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        // Check direct children first
        Transform child = parent.Find(childName);
        if (child != null) return child;
        
        // Recursively check all descendants
        foreach (Transform t in parent)
        {
            child = FindChildRecursive(t, childName);
            if (child != null) return child;
        }
        
        return null;
    }

    #endregion

    #region Configuration Management

    /// <summary>
    /// Applies a complete vehicle configuration
    /// </summary>
    public void ApplyConfiguration(VehicleConfigData config)
    {
        if (config == null || !vehiclePlaced) return;

        ChangeBodyColor(config.bodyColor);
        // Apply other config settings here

        Debug.Log("Configuration applied");
    }

    /// <summary>
    /// Saves current configuration
    /// </summary>
    public VehicleConfigData GetCurrentConfiguration()
    {
        return currentConfig;
    }

    #endregion

    #region Scaling & Manipulation

    /// <summary>
    /// Scales the vehicle uniformly
    /// </summary>
    public void ScaleVehicle(float scaleFactor)
    {
        if (spawnedVehicle != null)
        {
            spawnedVehicle.transform.localScale = Vector3.one * scaleFactor;
            Debug.Log($"Vehicle scaled to: {scaleFactor}");
        }
    }

    /// <summary>
    /// Rotates the vehicle around world vertical axis
    /// </summary>
    public void RotateVehicle(float angle)
    {
        if (spawnedVehicle != null)
        {
            // Rotate around world Y-axis (vertical), not local axis
            spawnedVehicle.transform.Rotate(Vector3.up, angle, Space.World);
        }
    }

    #endregion

    #region Helper Methods

    private void CacheVehicleRenderers()
    {
        if (spawnedVehicle == null) return;

        // Find body renderer (typically the largest mesh)
        bodyRenderer = spawnedVehicle.GetComponentInChildren<Renderer>();

        // Find wheel renderers (look for objects with "wheel" in name)
        List<Renderer> wheels = new List<Renderer>();
        foreach (Renderer r in spawnedVehicle.GetComponentsInChildren<Renderer>())
        {
            if (r.gameObject.name.ToLower().Contains("wheel"))
            {
                wheels.Add(r);
            }
        }
        wheelRenderers = wheels.ToArray();

        Debug.Log($"Cached body renderer and {wheelRenderers.Length} wheel renderers");
    }

    #endregion
}