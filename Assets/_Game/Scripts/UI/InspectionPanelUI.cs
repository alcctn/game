using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Right-hand IMGUI panel for selected cell / building inspection.
    /// </summary>
    public sealed class InspectionPanelUI : MonoBehaviour
    {
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private EnergyNetworkService networkService;

        public void Configure(
            MapDebugOverlay overlay,
            MapGenerator generator,
            PlacementController placement,
            EnergyNetworkService network)
        {
            debugOverlay = overlay;
            mapGenerator = generator;
            placementController = placement;
            networkService = network;
        }

        private void OnGUI()
        {
            const float width = 280f;
            var x = Screen.width - width - 12f;
            GUILayout.BeginArea(new Rect(x, 12f, width, 420f), GUI.skin.box);
            GUILayout.Label("Inspection");

            if (debugOverlay == null || !debugOverlay.SelectedCell.HasValue)
            {
                GUILayout.Label("Click a cell on the map.");
                GUILayout.EndArea();
                return;
            }

            var coordinate = debugOverlay.SelectedCell.Value;
            GUILayout.Label($"Cell ({coordinate.X}, {coordinate.Y})");

            if (mapGenerator != null
                && mapGenerator.Grid.IsInitialized
                && mapGenerator.Grid.TryGetCell(coordinate, out var cell))
            {
                GUILayout.Label($"Elevation {cell.Elevation:F2}");
                GUILayout.Label($"Slope {cell.Slope:F1}°");
                GUILayout.Label($"Water {cell.WaterFlow:F1}  Solar {cell.SolarPotential:F2}  Wind {cell.WindPotential:F2}");
                GUILayout.Label($"Buildable {cell.IsBuildable}  WaterCell {cell.IsWater}");
            }

            GUILayout.Space(6f);

            if (placementController != null
                && placementController.Occupancy.TryGet(coordinate, out var building)
                && building?.Definition != null)
            {
                var def = building.Definition;
                GUILayout.Label($"Building: {def.DisplayName}");
                if (def.IsProducer)
                {
                    GUILayout.Label($"Production {building.CurrentProduction:F1}");
                    var hint = InspectionStatus.FormatEfficiencyHint(def, building.MaintenanceLevel);
                    if (!string.IsNullOrEmpty(hint))
                    {
                        GUILayout.Label(hint);
                    }
                }

                if (def.IsStorage)
                {
                    GUILayout.Label($"Stored {building.StoredEnergy:F1} / {def.StorageCapacity:F0}");
                }

                if (def.IsConsumer)
                {
                    GUILayout.Label($"Demand {def.BaseDemand:F1}");
                }

                if (def.IsNetworkHub)
                {
                    var cap = def.LinkCapacity;
                    GUILayout.Label(cap > 0f
                        ? $"Link capacity {cap:F0}"
                        : "Link capacity unlimited");
                }

                GUILayout.Label($"Maintenance {building.MaintenanceLevel:F2}");

                var graph = networkService != null ? networkService.Graph : null;
                var status = InspectionStatus.ResolveNetwork(building, graph);
                GUILayout.Label($"Network: {InspectionStatus.NetworkLabel(status)}");
            }
            else
            {
                GUILayout.Label("Building: (empty)");
                GUILayout.Label($"Network: {InspectionStatus.NetworkLabel(InspectionNetworkStatus.NoBuilding)}");
            }

            GUILayout.EndArea();
        }
    }
}
