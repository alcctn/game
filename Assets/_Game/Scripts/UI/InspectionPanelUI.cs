using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
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
        [SerializeField] private SimulationClock simulationClock;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private ScenarioController scenarioController;

        private string _actionMessage = string.Empty;

        public void Configure(
            MapDebugOverlay overlay,
            MapGenerator generator,
            PlacementController placement,
            EnergyNetworkService network,
            SimulationClock clock = null,
            ResearchController research = null,
            ScenarioController scenario = null)
        {
            debugOverlay = overlay;
            mapGenerator = generator;
            placementController = placement;
            networkService = network;
            simulationClock = clock;
            researchController = research;
            scenarioController = scenario;
        }

        private void OnGUI()
        {
            const float width = 280f;
            var x = Screen.width - width - 12f;
            GUILayout.BeginArea(new Rect(x, 12f, width, 520f), GUI.skin.box);
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
                    DrawProductionBreakdown(building, coordinate);
                    if (GUILayout.Button("Repair"))
                    {
                        if (MaintenanceService.TryManualRepair(
                                building, placementController.Wallet, out var fail))
                        {
                            _actionMessage = "Repaired.";
                        }
                        else
                        {
                            _actionMessage = fail;
                        }
                    }
                }

                if (def.Id == MaintenanceService.DepotBuildingId)
                {
                    if (GUILayout.Button("Repair All In Range"))
                    {
                        if (MaintenanceService.TryBulkRepairInDepotRange(
                                building,
                                placementController.Occupancy.Occupied,
                                placementController.Wallet,
                                out var count,
                                out var cost,
                                out var fail))
                        {
                            _actionMessage = $"Repaired {count} for {cost:F0}.";
                        }
                        else
                        {
                            _actionMessage = fail;
                        }
                    }
                }

                if (def.IsStorage)
                {
                    GUILayout.Label($"Stored {building.StoredEnergy:F1} / {def.StorageCapacity:F0}");
                }

                if (def.IsConsumer)
                {
                    GUILayout.Label($"Demand {def.BaseDemand:F1}");
                    if (scenarioController != null)
                    {
                        GUILayout.Label($"Population {scenarioController.Settlement.Population:F1}");
                    }
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

                if (GUILayout.Button("Demolish"))
                {
                    if (placementController.TryDemolish(coordinate, out var refund))
                    {
                        _actionMessage = $"Demolished (+{refund:F0})";
                        debugOverlay.ClearSelection();
                    }
                    else
                    {
                        _actionMessage = "Demolish failed.";
                    }
                }
            }
            else
            {
                GUILayout.Label("Building: (empty)");
                GUILayout.Label($"Network: {InspectionStatus.NetworkLabel(InspectionNetworkStatus.NoBuilding)}");
            }

            if (!string.IsNullOrEmpty(_actionMessage))
            {
                GUILayout.Space(4f);
                GUILayout.Label(_actionMessage);
            }

            if (placementController != null
                && placementController.HasDemolishUndo
                && !placementController.IsPlacementActive)
            {
                GUILayout.Space(6f);
                if (GUILayout.Button("Undo Demolish"))
                {
                    if (placementController.TryUndoLastDemolish())
                    {
                        _actionMessage = "Demolish undone.";
                    }
                    else
                    {
                        _actionMessage = "Undo demolish failed.";
                    }
                }
            }

            GUILayout.EndArea();
        }

        private void DrawProductionBreakdown(Buildings.BuildingInstance building, Grid.GridCoordinate coordinate)
        {
            var def = building.Definition;
            var context = simulationClock != null
                ? simulationClock.CreateContextSnapshot()
                : new SimulationContext(0, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            var bonus = researchController != null
                ? researchController.Service.GetEfficiencyBonus(def.Id)
                : 0f;
            var networkFactor = NetworkLoadFactor.ResolveForBuilding(
                building, networkService != null ? networkService.Graph : null);
            var deliveryFactor = TransmissionLoss.ResolveForBuilding(
                building, networkService != null ? networkService.Graph : null);
            var breakdown = ProductionEstimate.BreakDown(
                def,
                coordinate,
                mapGenerator.Grid,
                mapGenerator.Settings,
                context,
                placementController.Occupancy,
                bonus,
                building.MaintenanceLevel,
                building.InstanceId,
                networkFactor,
                deliveryFactor);

            GUILayout.Label($"Potential {breakdown.ResourcePotential:F2}");
            GUILayout.Label($"Phase {breakdown.PhaseFactor:F2}");
            GUILayout.Label($"Efficiency {breakdown.Efficiency:F2}");
            GUILayout.Label($"Maintenance {breakdown.Maintenance:F2}");
            if (WindWakeFactor.AppliesTo(def))
            {
                GUILayout.Label($"WakeFactor {breakdown.WakeFactor:F2}");
            }

            GUILayout.Label($"NetworkFactor {breakdown.NetworkFactor:F0}");
            GUILayout.Label($"Loss {(1f - breakdown.DeliveryFactor) * 100f:F0}%");
            GUILayout.Label($"= Production {breakdown.Production:F1}");
        }
    }
}
