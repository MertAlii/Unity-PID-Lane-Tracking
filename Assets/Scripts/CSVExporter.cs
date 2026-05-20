using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Globalization;

/// <summary>
/// Exports the recorded telemetry data from the VehicleController to a CSV file.
/// Useful for analytical reporting and system stability analysis in the final project.
/// </summary>
public class CSVExporter : MonoBehaviour
{
    [Tooltip("Reference to the VehicleController containing the recorded data.")]
    public VehicleController vehicleController;

    [Tooltip("Prefix for the exported CSV file name.")]
    public string fileName = "PID_Simulation_Data";

    /// <summary>
    /// Exports the current RecordedData to a CSV file in the project's root folder.
    /// </summary>
    public void ExportData()
    {
        if (vehicleController == null || vehicleController.RecordedData == null || vehicleController.RecordedData.Count == 0)
        {
            Debug.LogWarning("CSVExporter: No data to export or VehicleController not assigned.");
            return;
        }

        // Use Application.dataPath to save inside the Assets folder, or Application.persistentDataPath
        // For an academic project, saving next to the project folder is usually convenient.
        // We'll save it one level above Assets so it doesn't clutter the Unity project itself, 
        // or directly in the root of the project. Application.dataPath is ".../Assets"
        string projectPath = Directory.GetParent(Application.dataPath).FullName;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string path = Path.Combine(projectPath, $"{fileName}_{timestamp}.csv");

        StringBuilder sb = new StringBuilder();
        // Header
        sb.AppendLine("Time(s),LateralError(m),ControlOutput(u),VehiclePosX(m),ReferencePosX(m),VehicleGlobalX,VehicleGlobalZ,TargetGlobalX,TargetGlobalZ");

        // Data rows
        foreach (var data in vehicleController.RecordedData)
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, 
                "{0:F4},{1:F4},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4}", 
                data.time, data.lateralError, data.controlOutput, data.vehiclePosX, data.referencePosX,
                data.vehicleGlobalX, data.vehicleGlobalZ, data.targetGlobalX, data.targetGlobalZ));
        }

        try
        {
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"Data successfully exported to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export CSV: {e.Message}");
        }
    }
}
