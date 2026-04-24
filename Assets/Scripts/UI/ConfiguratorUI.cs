using UnityEngine;
using UnityEngine.UI;
using TMPro; // Using TextMeshPro for better text rendering

/// <summary>
/// Manages the UI for vehicle customization
/// Handles color selection, part swapping, and user interactions
/// Updated with three-panel navigation system and per-vehicle color panels
/// </summary>
public class ConfiguratorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VehicleConfigurator vehicleConfigurator;
    [SerializeField] private MRPlacementController placementController;

    [Header("UI Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject vehicleSelectionPanel;
    [SerializeField] private GameObject customizationPanel;
    [SerializeField] private GameObject colorPanel_GLC;  // GLC color panel (8 colors)
    [SerializeField] private GameObject colorPanel_C63;  // C63 color panel (6 colors)

    [Header("Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button screenshotButton;
    [SerializeField] private Button changeWheelsButton;

    [Header("Color Palette")]
    [SerializeField] private Button[] colorButtons_GLC; // GLC color buttons (8)
    [SerializeField] private Button[] colorButtons_C63; // C63 color buttons (6)
    [SerializeField]
    private Color[] colorPresets = new Color[]
    {
        new Color(0.8f, 0.1f, 0.1f), // Red
        new Color(0.1f, 0.3f, 0.8f), // Blue
        new Color(0.1f, 0.1f, 0.1f), // Black
        new Color(0.9f, 0.9f, 0.9f), // White
        new Color(0.2f, 0.6f, 0.2f), // Green
        new Color(0.9f, 0.7f, 0.1f), // Yellow
        new Color(0.5f, 0.5f, 0.5f), // Gray
        new Color(0.6f, 0.3f, 0.1f)  // Brown
    };

    [Header("Scale Controls")]
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private TextMeshProUGUI scaleText;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;

    [Header("Rotation Controls")]
    [SerializeField] private Button rotateLeftButton;
    [SerializeField] private Button rotateRightButton;
    [SerializeField] private float rotationStep = 45f;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI statusText;

    private bool vehiclePlaced = false;

    private void Start()
    {
        SetupButtons();
        SetupColorPalette();
        SetupSliders();
        UpdateUIState(false);
    }

    #region Setup

    private void SetupButtons()
    {
        // Reset
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        // Screenshot
        if (screenshotButton != null)
        {
            screenshotButton.onClick.AddListener(OnScreenshotClicked);
        }

        // Wheels
        if (changeWheelsButton != null)
        {
            changeWheelsButton.onClick.AddListener(OnChangeWheelsClicked);
        }

        // Rotation
        if (rotateLeftButton != null)
        {
            rotateLeftButton.onClick.AddListener(() => OnRotateClicked(-rotationStep));
        }
        if (rotateRightButton != null)
        {
            rotateRightButton.onClick.AddListener(() => OnRotateClicked(rotationStep));
        }
    }

    private void SetupColorPalette()
    {
        // Setup GLC color buttons (8 colors)
        if (colorButtons_GLC != null && colorButtons_GLC.Length > 0)
        {
            for (int i = 0; i < colorButtons_GLC.Length && i < colorPresets.Length; i++)
            {
                int index = i; // Capture for lambda
                Color color = colorPresets[i];

                // Set button color
                Image buttonImage = colorButtons_GLC[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = color;
                }

                // Add click listener
                colorButtons_GLC[i].onClick.AddListener(() => OnColorSelected(color));
            }
        }

        // Setup C63 color buttons (6 colors)
        if (colorButtons_C63 != null && colorButtons_C63.Length > 0)
        {
            for (int i = 0; i < colorButtons_C63.Length && i < colorPresets.Length; i++)
            {
                int index = i; // Capture for lambda
                Color color = colorPresets[i];

                // Set button color
                Image buttonImage = colorButtons_C63[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = color;
                }

                // Add click listener
                colorButtons_C63[i].onClick.AddListener(() => OnColorSelected(color));
            }
        }
    }

    private void SetupSliders()
    {
        if (scaleSlider != null)
        {
            scaleSlider.minValue = minScale;
            scaleSlider.maxValue = maxScale;
            scaleSlider.value = 1.0f;
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);
        }
    }

    #endregion

    #region Panel Navigation

    /// <summary>
    /// Start button clicked - show vehicle selection
    /// </summary>
    public void OnStartClicked()
    {
        startPanel.SetActive(false);
        vehicleSelectionPanel.SetActive(true);
        customizationPanel.SetActive(false);

        UpdateStatusText("Choose your Mercedes");
    }

    /// <summary>
    /// User selected a vehicle - place it and show customization
    /// </summary>
    public void OnVehicleSelected(int vehicleIndex)
    {
        // Select the vehicle
        if (vehicleConfigurator != null)
        {
            vehicleConfigurator.SelectVehicle(vehicleIndex);
        }

        // Place the vehicle
        if (placementController != null)
        {
            placementController.OnPlacementConfirmed();
        }

        vehiclePlaced = true;

        // Show correct color panel for this vehicle
        ShowColorPanelForVehicle(vehicleIndex);

        // Show customization panel
        vehicleSelectionPanel.SetActive(false);
        customizationPanel.SetActive(true);

        UpdateStatusText($"Customizing {vehicleConfigurator.GetVehicleName(vehicleIndex)}");
    }

    /// <summary>
    /// Shows the appropriate color panel for the selected vehicle
    /// </summary>
    private void ShowColorPanelForVehicle(int vehicleIndex)
    {
        if (colorPanel_GLC != null)
        {
            colorPanel_GLC.SetActive(vehicleIndex == 0); // Show for GLC
        }

        if (colorPanel_C63 != null)
        {
            colorPanel_C63.SetActive(vehicleIndex == 1); // Show for C63
        }

        Debug.Log($"Showing color panel for vehicle {vehicleIndex}");
    }

    /// <summary>
    /// Back button - return to vehicle selection
    /// </summary>
    public void OnBackToVehicleSelection()
    {
        Debug.Log("OnBackToVehicleSelection called!"); // ADD THIS

        // Reset current vehicle
        if (vehicleConfigurator != null)
        {
            Debug.Log("Resetting vehicle"); // ADD THIS
            vehicleConfigurator.ResetVehicle();
        }
        else
        {
            Debug.LogError("VehicleConfigurator is NULL!"); // ADD THIS
        }

        vehiclePlaced = false;

        // Reset scale slider
        if (scaleSlider != null)
        {
            scaleSlider.value = 1.0f;
        }

        // Show vehicle selection panel
        Debug.Log("Switching panels..."); // ADD THIS
        customizationPanel.SetActive(false);
        vehicleSelectionPanel.SetActive(true);

        UpdateStatusText("Choose your Mercedes");
        Debug.Log("Done!"); // ADD THIS
    }

    #endregion

    #region Button Handlers

    public void OnResetClicked()
    {
        // This is now replaced by OnBackToVehicleSelection
        // Keep for backward compatibility but redirect
        OnBackToVehicleSelection();
    }

    public void OnScreenshotClicked()
    {
        StartCoroutine(CaptureScreenshot());
    }

    public void OnChangeWheelsClicked()
    {
        if (vehicleConfigurator != null)
        {
            vehicleConfigurator.ChangeWheelStyle();
            UpdateStatusText("Wheel style changed");
        }
    }

    public void OnColorSelected(Color color)
    {
        if (vehicleConfigurator != null && vehiclePlaced)
        {
            vehicleConfigurator.ChangeBodyColor(color);
            UpdateStatusText($"Color changed to {GetColorName(color)}");
        }
        else
        {
            Debug.LogWarning($"Cannot change color - vehicleConfigurator: {vehicleConfigurator != null}, vehiclePlaced: {vehiclePlaced}");
        }
    }

    public void OnRotateClicked(float angle)
    {
        if (vehicleConfigurator != null && vehiclePlaced)
        {
            vehicleConfigurator.RotateVehicle(angle);
        }
    }

    #endregion

    #region Scale Control

    private void OnScaleChanged(float value)
    {
        if (vehicleConfigurator != null && vehiclePlaced)
        {
            vehicleConfigurator.ScaleVehicle(value);

            if (scaleText != null)
            {
                scaleText.text = $"Scale: {value:F1}x";
            }
        }
    }

    #endregion

    #region UI State Management

    private void UpdateUIState(bool vehicleIsPlaced)
    {
        // Panel switching is now handled by individual methods
        // Keep this for button state management

        // Update button interactivity
        if (resetButton != null)
        {
            resetButton.interactable = vehicleIsPlaced;
        }

        if (screenshotButton != null)
        {
            screenshotButton.interactable = vehicleIsPlaced;
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;

            // Auto-clear after 3 seconds
            CancelInvoke(nameof(ClearStatusText));
            Invoke(nameof(ClearStatusText), 3f);
        }
    }

    private void ClearStatusText()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    #endregion

    #region Screenshot

    private System.Collections.IEnumerator CaptureScreenshot()
    {
        UpdateStatusText("Capturing screenshot...");

        // Hide UI temporarily
        Canvas canvas = GetComponentInParent<Canvas>();
        bool wasEnabled = canvas.enabled;
        canvas.enabled = false;

        // Wait for end of frame
        yield return new WaitForEndOfFrame();

        // Capture screenshot
        string filename = $"VehicleConfig_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);

        ScreenCapture.CaptureScreenshot(filename);

        // Restore UI
        canvas.enabled = wasEnabled;

        UpdateStatusText($"Screenshot saved: {filename}");
        Debug.Log($"Screenshot saved to: {path}");
    }

    #endregion

    #region Helpers

    private string GetColorName(Color color)
    {
        // Simple color naming based on dominant channel
        if (color.r > 0.6f && color.g < 0.3f && color.b < 0.3f) return "Red";
        if (color.b > 0.6f && color.r < 0.3f && color.g < 0.3f) return "Blue";
        if (color.g > 0.5f && color.r < 0.3f && color.b < 0.3f) return "Green";
        if (color.r > 0.8f && color.g > 0.8f && color.b > 0.8f) return "White";
        if (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f) return "Black";
        if (color.r > 0.8f && color.g > 0.6f && color.b < 0.3f) return "Yellow";
        if (color.r > 0.5f && color.g > 0.5f && color.b > 0.5f) return "Gray";
        return "Custom";
    }

    #endregion

    #region Public Methods (for XR Input)

    /// <summary>
    /// Opens color selection panel
    /// Can be called from XR button/trigger
    /// </summary>
    public void ToggleColorPanel()
    {
        // Toggle whichever panel is currently active
        if (colorPanel_GLC != null && colorPanel_GLC.activeSelf)
        {
            colorPanel_GLC.SetActive(!colorPanel_GLC.activeSelf);
        }
        else if (colorPanel_C63 != null && colorPanel_C63.activeSelf)
        {
            colorPanel_C63.SetActive(!colorPanel_C63.activeSelf);
        }
    }



    #endregion
}