using System;
using CleanEnergy.DebugTools;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Runtime IMGUI for seed control, generation, and debug view modes.
    /// </summary>
    public sealed class MapDebugUI : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private MapDebugOverlay debugOverlay;

        private string _seedInput = "12345";
        private DebugViewMode _viewMode = DebugViewMode.Normal;

        public void Configure(MapGenerator generator, MapDebugOverlay overlay)
        {
            mapGenerator = generator;
            debugOverlay = overlay;
            if (mapGenerator != null && mapGenerator.Settings != null)
            {
                _seedInput = mapGenerator.Settings.Seed;
            }
        }

        private void OnGUI()
        {
            const float width = 280f;
            GUILayout.BeginArea(new Rect(12f, 12f, width, 260f), GUI.skin.box);
            GUILayout.Label("Terrain Debug");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed", GUILayout.Width(40f));
            _seedInput = GUILayout.TextField(_seedInput ?? string.Empty);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Random"))
            {
                _seedInput = UnityEngine.Random.Range(0, int.MaxValue).ToString();
            }

            if (GUILayout.Button("Generate"))
            {
                GenerateRequested();
            }

            GUILayout.EndHorizontal();

            GUILayout.Label("View Mode");
            var newMode = (DebugViewMode)GUILayout.Toolbar(
                (int)_viewMode,
                new[] { "Normal", "Height", "Slope" });
            if (newMode != _viewMode)
            {
                _viewMode = newMode;
                if (debugOverlay != null)
                {
                    debugOverlay.SetMode(_viewMode);
                }
            }

            DrawSelectedCell();
            GUILayout.EndArea();
        }

        private void GenerateRequested()
        {
            if (mapGenerator == null)
            {
                Debug.LogError("[UI] MapGenerator is missing.");
                return;
            }

            mapGenerator.SetSeed(_seedInput);
            mapGenerator.Generate();
            if (debugOverlay != null)
            {
                debugOverlay.ClearSelection();
                debugOverlay.SetMode(_viewMode);
            }
        }

        private void DrawSelectedCell()
        {
            if (debugOverlay == null || !debugOverlay.TryGetSelectedCellData(out var cell))
            {
                GUILayout.Label("Selected: (click map)");
                return;
            }

            GUILayout.Label($"Cell: ({cell.X}, {cell.Y})");
            GUILayout.Label($"Elevation: {cell.Elevation:F2}");
            GUILayout.Label($"Slope: {cell.Slope:F1}°");
            GUILayout.Label($"Buildable: {cell.IsBuildable}");
        }
    }
}
