using CleanEnergy.Telemetry;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Compact always-on debug box for session telemetry counters.
    /// </summary>
    public sealed class TelemetryHudUI : MonoBehaviour
    {
        [SerializeField] private TelemetryController telemetryController;

        private string _exportStatus = string.Empty;

        public void Configure(TelemetryController controller)
        {
            telemetryController = controller;
        }

        private void OnGUI()
        {
            var s = telemetryController != null ? telemetryController.Service : null;
            if (s == null)
            {
                return;
            }

            const float width = 260f;
            const float height = 190f;
            GUILayout.BeginArea(new Rect(12f, Screen.height - height - 12f, width, height), GUI.skin.box);
            GUILayout.Label("Telemetry");
            GUILayout.Label($"1st build: {Format(s.TimeToFirstBuildingSeconds)}s");
            GUILayout.Label($"1st prod: {Format(s.TimeToFirstProductionSeconds)}s");
            GUILayout.Label($"Bad place: {s.InvalidPlacementAttempts}");
            GUILayout.Label($"Layer: {s.PreferredDebugLayer}");
            GUILayout.Label($"Shortage: {s.AverageShortageRatio * 100f:F0}% ({s.ShortageTicks}/{s.BalanceTicks})");
            GUILayout.Label($"End: {Format(s.ScenarioEndElapsedSeconds)}s");
            if (!string.IsNullOrEmpty(s.FailReason))
            {
                GUILayout.Label($"Fail: {s.FailReason}");
            }

            if (GUILayout.Button("Export CSV"))
            {
                var path = s.ExportToPersistentDataPath();
                _exportStatus = "Wrote CSV";
                Debug.Log($"[Telemetry] Exported to {path}");
            }

            if (!string.IsNullOrEmpty(_exportStatus))
            {
                GUILayout.Label(_exportStatus);
            }

            GUILayout.EndArea();
        }

        private static string Format(float? value)
        {
            return value.HasValue ? value.Value.ToString("F1") : "-";
        }
    }
}
