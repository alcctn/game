using System.Collections.Generic;
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
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private MaintenanceController maintenanceController;

        private string _actionMessage = string.Empty;
        private readonly List<float> _sparklineScratch = new List<float>(20);

        public void Configure(
            MapDebugOverlay overlay,
            MapGenerator generator,
            PlacementController placement,
            EnergyNetworkService network,
            SimulationClock clock = null,
            ResearchController research = null,
            ScenarioController scenario = null,
            EnergySimulationDriver driver = null,
            MaintenanceController maintenance = null)
        {
            debugOverlay = overlay;
            mapGenerator = generator;
            placementController = placement;
            networkService = network;
            simulationClock = clock;
            researchController = research;
            scenarioController = scenario;
            energyDriver = driver;
            maintenanceController = maintenance;
        }

        private void OnGUI()
        {
            const float width = 280f;
            var x = Screen.width - width - 12f;
            GUILayout.BeginArea(new Rect(x, 12f, width, 560f), GUI.skin.box);
            GUILayout.Label("Inspection");

            DrawDebtAndRepay();

            if (debugOverlay != null && debugOverlay.MultiSelectedCells.Count > 0)
            {
                GUILayout.Label($"Multi-select {debugOverlay.MultiSelectedCells.Count}/{MapDebugOverlay.MaxMultiSelection}");
                if (placementController != null
                    && !placementController.IsPlacementActive
                    && GUILayout.Button("Demolish Selected"))
                {
                    if (placementController.TryDemolishMany(
                            debugOverlay.MultiSelectedCells, out var refund))
                    {
                        _actionMessage = $"Demolished group (+{refund:F0})";
                        debugOverlay.ClearSelection();
                    }
                    else
                    {
                        _actionMessage = "Demolish selected failed.";
                    }
                }

                if (placementController != null
                    && !placementController.IsPlacementActive
                    && GUILayout.Button("Repair Selected"))
                {
                    if (MaintenanceService.TryRepairSelectedProducers(
                            debugOverlay.MultiSelectedCells,
                            placementController.Occupancy.Occupied,
                            placementController.Wallet,
                            maintenanceController != null ? maintenanceController.RepairUndo : null,
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

            if (debugOverlay == null || !debugOverlay.SelectedCell.HasValue)
            {
                GUILayout.Label("Click a cell on the map.");
                DrawUndoAndMessage();
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
                    DrawSparkline(building.InstanceId);
                    if (GUILayout.Button("Repair"))
                    {
                        if (MaintenanceService.TryManualRepair(
                                building,
                                placementController.Wallet,
                                maintenanceController != null ? maintenanceController.RepairUndo : null,
                                out var fail))
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
                        RepairUndoService repairUndo = null;
                        if (maintenanceController != null)
                        {
                            repairUndo = maintenanceController.RepairUndo;
                        }

                        if (MaintenanceService.TryBulkRepairInDepotRange(
                                building,
                                placementController.Occupancy.Occupied,
                                placementController.Wallet,
                                repairUndo,
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

            DrawUndoAndMessage();
            GUILayout.EndArea();
        }

        private void DrawDebtAndRepay()
        {
            if (energyDriver == null)
            {
                return;
            }

            var debt = energyDriver.EmergencyCredit.RemainingDebt;
            if (debt <= 0.0001f)
            {
                return;
            }

            GUILayout.Label($"Debt {debt:F0}");
            if (placementController != null && GUILayout.Button("Repay"))
            {
                if (energyDriver.EmergencyCredit.TryRepay(placementController.Wallet))
                {
                    _actionMessage = "Debt repaid.";
                }
                else
                {
                    _actionMessage = "Cannot repay debt.";
                }
            }
        }

        private void DrawSparkline(string instanceId)
        {
            if (energyDriver == null
                || !energyDriver.TryGetSparklineSamples(instanceId, _sparklineScratch)
                || _sparklineScratch.Count == 0)
            {
                return;
            }

            GUILayout.Label("Production (20)");
            var max = 0.001f;
            for (var i = 0; i < _sparklineScratch.Count; i++)
            {
                if (_sparklineScratch[i] > max)
                {
                    max = _sparklineScratch[i];
                }
            }

            var rect = GUILayoutUtility.GetRect(240f, 28f);
            var barWidth = rect.width / ProductionSparklineTracker.SampleCapacity;
            for (var i = 0; i < _sparklineScratch.Count; i++)
            {
                var h = Mathf.Clamp01(_sparklineScratch[i] / max) * rect.height;
                var bar = new Rect(
                    rect.x + i * barWidth,
                    rect.y + rect.height - h,
                    Mathf.Max(1f, barWidth - 1f),
                    h);
                GUI.Box(bar, GUIContent.none);
            }
        }

        private void DrawUndoAndMessage()
        {
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
                var label = placementController.DemolishUndoStackDepth > 1
                    ? $"Undo Demolish ×{placementController.DemolishUndoStackDepth}"
                    : placementController.DemolishUndoCount > 1
                        ? $"Undo Demolish ({placementController.DemolishUndoCount})"
                        : "Undo Demolish";
                if (GUILayout.Button(label))
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

            if (maintenanceController != null
                && maintenanceController.RepairUndo.HasRepairUndo
                && placementController != null
                && !placementController.IsPlacementActive)
            {
                GUILayout.Space(4f);
                var undo = maintenanceController.RepairUndo;
                var repairLabel = undo.RepairUndoStackDepth > 1
                    ? $"Undo Repair ×{undo.RepairUndoStackDepth}"
                    : undo.RepairUndoCount > 1
                        ? $"Undo Repair ({undo.RepairUndoCount})"
                        : "Undo Repair";
                if (GUILayout.Button(repairLabel))
                {
                    if (undo.TryUndoLast(
                            placementController.Occupancy.Occupied,
                            placementController.Wallet))
                    {
                        _actionMessage = "Repair undone.";
                    }
                    else
                    {
                        _actionMessage = "Undo repair failed.";
                    }
                }
            }
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
