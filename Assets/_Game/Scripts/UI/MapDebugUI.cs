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
        private static readonly string[] ViewModeLabels =
        {
            "Normal", "Height", "Slope", "Water", "Solar", "Wind", "Network", "Production", "Demand"
        };

        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private MapDebugOverlay debugOverlay;

        private string _seedInput = "12345";
        private DebugViewMode _viewMode = DebugViewMode.Normal;
        private Vector2 _scroll;

        public void Configure(MapGenerator generator, MapDebugOverlay overlay)
        {
            mapGenerator = generator;
            debugOverlay = overlay;
            if (mapGenerator != null && mapGenerator.Settings != null)
            {
                _seedInput = mapGenerator.Settings.Seed;
            }
        }

        private void Update()
        {
            if (!TryConsumeHotkey(out var mode))
            {
                return;
            }

            ApplyViewMode(mode);
        }

        private static bool TryConsumeHotkey(out DebugViewMode mode)
        {
            mode = DebugViewMode.Normal;
            if (Input.GetKeyDown(KeyCode.F1)) return DebugViewHotkeys.TryMapKey(KeyCode.F1, out mode);
            if (Input.GetKeyDown(KeyCode.F2)) return DebugViewHotkeys.TryMapKey(KeyCode.F2, out mode);
            if (Input.GetKeyDown(KeyCode.F3)) return DebugViewHotkeys.TryMapKey(KeyCode.F3, out mode);
            if (Input.GetKeyDown(KeyCode.F4)) return DebugViewHotkeys.TryMapKey(KeyCode.F4, out mode);
            if (Input.GetKeyDown(KeyCode.F5)) return DebugViewHotkeys.TryMapKey(KeyCode.F5, out mode);
            if (Input.GetKeyDown(KeyCode.F6)) return DebugViewHotkeys.TryMapKey(KeyCode.F6, out mode);
            if (Input.GetKeyDown(KeyCode.F7)) return DebugViewHotkeys.TryMapKey(KeyCode.F7, out mode);
            if (Input.GetKeyDown(KeyCode.F8)) return DebugViewHotkeys.TryMapKey(KeyCode.F8, out mode);
            if (Input.GetKeyDown(KeyCode.F9)) return DebugViewHotkeys.TryMapKey(KeyCode.F9, out mode);
            return false;
        }

        private void ApplyViewMode(DebugViewMode mode)
        {
            if (_viewMode == mode)
            {
                return;
            }

            _viewMode = mode;
            if (debugOverlay != null)
            {
                debugOverlay.SetMode(_viewMode);
            }
        }

        private void OnGUI()
        {
            const float width = 300f;
            GUILayout.BeginArea(new Rect(12f, 12f, width, 380f), GUI.skin.box);
            _scroll = GUILayout.BeginScrollView(_scroll);

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

            GUILayout.Label("View Mode (F1–F9)");
            var newMode = (DebugViewMode)GUILayout.SelectionGrid(
                (int)_viewMode,
                ViewModeLabels,
                3);
            if (newMode != _viewMode)
            {
                ApplyViewMode(newMode);
            }

            DrawSelectedCell();
            GUILayout.EndScrollView();
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
            GUILayout.Label($"WaterFlow: {cell.WaterFlow:F1}");
            GUILayout.Label($"Solar: {cell.SolarPotential:F2}");
            GUILayout.Label($"Wind: {cell.WindPotential:F2}");
            GUILayout.Label($"Biome: {cell.Biome}");
            GUILayout.Label($"Water: {cell.IsWater}");
            GUILayout.Label($"Buildable: {cell.IsBuildable}");
        }
    }
}
