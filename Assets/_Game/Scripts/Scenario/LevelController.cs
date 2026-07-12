using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Settlements;
using CleanEnergy.Simulation;
using CleanEnergy.Workers;
using UnityEngine;

namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Owns Level 1 definition, workers, active settlement seed, progress and win hand-off.
    /// </summary>
    public sealed class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelDefinition levelDefinition;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private SimulationClock clock;

        private readonly ActiveSettlementService _settlement = new ActiveSettlementService();
        private readonly WorkerService _workers = new WorkerService();
        private LevelProgressService _progress;
        private bool _villageSeeded;
        private ProducerSettlementLinkOverlay _linkOverlay;

        public LevelDefinition Definition => levelDefinition;
        public LevelProgressService Progress => _progress;
        public LevelObjectiveState State => _progress?.State;
        public WorkerService Workers => _workers;
        public ActiveSettlementService Settlement => _settlement;
        public event System.Action<LevelObjectiveState> StateChanged;
        public event System.Action LevelCompleted;

        public void Configure(
            LevelDefinition definition,
            PlacementController placement,
            MapGenerator generator,
            EnergySimulationDriver driver,
            ScenarioController scenario,
            SimulationClock simulationClock)
        {
            Unsubscribe();
            levelDefinition = definition != null ? definition : LevelDefinition.CreateRuntimeDefault();
            placementController = placement;
            mapGenerator = generator;
            energyDriver = driver;
            scenarioController = scenario;
            clock = simulationClock;

            _settlement.Clear();
            _villageSeeded = false;
            _progress = new LevelProgressService(levelDefinition);
            _progress.StateChanged += OnProgressStateChanged;
            _progress.LevelCompleted += OnLevelCompleted;

            _workers.Configure(
                levelDefinition,
                placementController != null ? placementController.Wallet : null,
                () => _progress != null && _progress.State.WaterComplete);
            _workers.Reset();

            EnsureLinkOverlay();
            _linkOverlay.Configure(mapGenerator, _settlement);
            _linkOverlay.ClearAll();
            _linkOverlay.SetVisible(true);

            if (placementController != null)
            {
                placementController.SetLevelServices(levelDefinition, _settlement, _workers.Pool);
                if (placementController.Wallet != null)
                {
                    placementController.Wallet.SetMoney(levelDefinition.StartingMoney);
                }
            }

            energyDriver?.SetIncomePerSuppliedEnergy(levelDefinition.IncomePerSuppliedEnergy);
            // Network auto-connect configured by bootstrap after network.Configure
            Subscribe();
            TrySeedVillage();
            StateChanged?.Invoke(_progress.State);
        }

        public void BindNetworkAutoConnect(EnergyNetworkService network)
        {
            if (network == null || levelDefinition == null)
            {
                return;
            }

            network.SetAutoConnect(levelDefinition.AutoConnectEnabled, levelDefinition.PlacementRadius);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (_progress != null)
            {
                _progress.StateChanged -= OnProgressStateChanged;
                _progress.LevelCompleted -= OnLevelCompleted;
            }
        }

        private void Subscribe()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }

            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated += OnBalanceUpdated;
            }

            if (placementController != null)
            {
                placementController.BuildingPlaced += OnBuildingPlaced;
                placementController.BuildingRemoved += OnBuildingRemoved;
            }

            if (_workers != null)
            {
                _workers.WorkersChanged += OnWorkersChanged;
            }
        }

        private void Unsubscribe()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }

            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated -= OnBalanceUpdated;
            }

            if (placementController != null)
            {
                placementController.BuildingPlaced -= OnBuildingPlaced;
                placementController.BuildingRemoved -= OnBuildingRemoved;
            }

            if (_workers != null)
            {
                _workers.WorkersChanged -= OnWorkersChanged;
            }
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            _villageSeeded = false;
            _settlement.Clear();
            _workers.Reset();
            _progress?.Reset();
            _linkOverlay?.ClearAll();
            _linkOverlay?.SetVisible(true);
            if (placementController != null && levelDefinition != null)
            {
                placementController.Wallet?.SetMoney(levelDefinition.StartingMoney);
                placementController.SetLevelServices(levelDefinition, _settlement, _workers.Pool);
            }

            TrySeedVillage();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            if (evt?.Instance != null)
            {
                _linkOverlay?.UpsertProducerLink(evt.Instance);
            }

            EvaluateNow(energyDriver != null ? energyDriver.LastResult : null);
        }

        private void OnBuildingRemoved(BuildingPlacedEvent evt)
        {
            if (evt?.Instance != null)
            {
                _linkOverlay?.RemoveLink(evt.Instance.InstanceId);
            }

            EvaluateNow(energyDriver != null ? energyDriver.LastResult : null);
        }

        private void OnWorkersChanged()
        {
            EvaluateNow(energyDriver != null ? energyDriver.LastResult : null);
        }

        private void OnBalanceUpdated(EnergyBalanceResult result)
        {
            EvaluateNow(result);
        }

        private void EvaluateNow(EnergyBalanceResult result)
        {
            if (_progress == null || placementController == null)
            {
                return;
            }

            var coverage = result != null ? result.CoverageRatio : 0f;
            _progress.Evaluate(
                _workers.Pool,
                placementController.Occupancy,
                coverage,
                placementController.Wallet);
        }

        private void OnProgressStateChanged(LevelObjectiveState state)
        {
            StateChanged?.Invoke(state);
        }

        private void OnLevelCompleted()
        {
            _linkOverlay?.SetVisible(false);
            LevelCompleted?.Invoke();
            scenarioController?.ForceWin("Level 1 complete");
            if (clock != null)
            {
                clock.SetSpeed(SimulationSpeed.Paused);
            }
        }

        private void TrySeedVillage()
        {
            if (_villageSeeded || placementController == null || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            var villageDef = placementController.FindDefinition("village");
            if (villageDef == null)
            {
                return;
            }

            if (!TryFindVillageSeed(mapGenerator.Grid, out var coord))
            {
                coord = new GridCoordinate(mapGenerator.Grid.Width / 2, mapGenerator.Grid.Height / 2);
            }

            if (!placementController.TryPlaceFromSave(villageDef.Id, coord, 0, 0f, 1f))
            {
                // Fallback: occupy without spending if save place failed due to money/rules
                Debug.LogWarning("[Level] Failed to seed village via TryPlaceFromSave.");
                return;
            }

            // Refund village cost so starting money stays at LevelDefinition.StartingMoney
            if (placementController.Wallet != null && levelDefinition != null)
            {
                placementController.Wallet.SetMoney(levelDefinition.StartingMoney);
            }

            var instanceId = FindVillageInstanceId(coord);
            _settlement.Set(coord, levelDefinition != null ? levelDefinition.PlacementRadius : 10, instanceId);
            _villageSeeded = true;
        }

        private string FindVillageInstanceId(GridCoordinate coord)
        {
            if (placementController?.Occupancy == null)
            {
                return null;
            }

            foreach (var pair in placementController.Occupancy.Occupied)
            {
                if (pair.Key.Equals(coord) && pair.Value?.Definition?.Id == "village")
                {
                    return pair.Value.InstanceId;
                }
            }

            return null;
        }

        private static bool TryFindVillageSeed(GridService grid, out GridCoordinate coordinate)
        {
            coordinate = default;
            var bestScore = float.MinValue;
            var found = false;
            var cx = grid.Width / 2;
            var cy = grid.Height / 2;
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    var c = new GridCoordinate(x, y);
                    if (!grid.TryGetCell(c, out var cell) || cell.IsWater || !cell.IsBuildable)
                    {
                        continue;
                    }

                    var nearWater = false;
                    for (var dx = -2; dx <= 2 && !nearWater; dx++)
                    {
                        for (var dy = -2; dy <= 2; dy++)
                        {
                            var n = new GridCoordinate(x + dx, y + dy);
                            if (grid.TryGetCell(n, out var nc) && nc.IsWater)
                            {
                                nearWater = true;
                                break;
                            }
                        }
                    }

                    if (!nearWater)
                    {
                        continue;
                    }

                    var distCenter = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
                    var score = 100f - distCenter + cell.WaterFlow * 0.1f;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        coordinate = c;
                        found = true;
                    }
                }
            }

            return found;
        }

        private void EnsureLinkOverlay()
        {
            if (_linkOverlay != null)
            {
                return;
            }

            var go = new GameObject("ProducerSettlementLinks");
            go.transform.SetParent(transform, false);
            _linkOverlay = go.AddComponent<ProducerSettlementLinkOverlay>();
        }
    }
}
