using UnityEngine;

/// <summary>
/// ScriptableObject for storing vehicle configuration presets
/// Create via: Assets > Create > Vehicle Config > Config Data
/// </summary>
[CreateAssetMenu(fileName = "VehicleConfig", menuName = "Vehicle Config/Config Data", order = 1)]
public class VehicleConfigData : ScriptableObject
{
    [Header("Visual Configuration")]
    public Color bodyColor = Color.red;
    public string materialPresetName = "Metallic";
    public int wheelStyleIndex = 0;
    
    [Header("Interior (Optional)")]
    public Color interiorColor = Color.black;
    public string interiorMaterial = "Leather";
    
    [Header("Metadata")]
    public string configName = "Default";
    public string description = "Default vehicle configuration";
    
    /// <summary>
    /// Creates a deep copy of this configuration
    /// </summary>
    public VehicleConfigData Clone()
    {
        VehicleConfigData clone = CreateInstance<VehicleConfigData>();
        clone.bodyColor = this.bodyColor;
        clone.materialPresetName = this.materialPresetName;
        clone.wheelStyleIndex = this.wheelStyleIndex;
        clone.interiorColor = this.interiorColor;
        clone.interiorMaterial = this.interiorMaterial;
        clone.configName = this.configName + " (Copy)";
        clone.description = this.description;
        return clone;
    }
    
    /// <summary>
    /// Exports configuration as JSON
    /// </summary>
    public string ToJSON()
    {
        return JsonUtility.ToJson(this, true);
    }
    
    /// <summary>
    /// Loads configuration from JSON
    /// </summary>
    public static VehicleConfigData FromJSON(string json)
    {
        VehicleConfigData config = CreateInstance<VehicleConfigData>();
        JsonUtility.FromJsonOverwrite(json, config);
        return config;
    }
}
