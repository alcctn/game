using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Map;
using CleanEnergy.Research;
using CleanEnergy.Settlements;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Wires energy ticks into scenario progress and pauses the clock on win/lose.
    /// </summary>
    public sealed class ScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioDefinition definition;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private EnergyNetworkService networkService;
        [SerializeField] private SimulationClock clock;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private ResearchController researchController;

        private ScenarioProgressService _progress;
        private readonly HashSet<string> _activeTypesBuffer = new HashSet<string>();
        private readonly SettlementState _settlement = new SettlementState();

        public ScenarioProgressService Progress => _progress;
        public ScenarioObjectiveState State => _progress?.State;
        public SettlementState Settlement => _settlement;
        public event System.Action<ScenarioWonEvent> Won;
        public event System.Action<ScenarioFailedEvent> Failed;
        public event System.Action<ScenarioObjectiveState> StateChanged;

        private void OnEnable()
        {
            EnsureProgress();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            ScenarioDefinition scenarioDefinition,
            EnergySimulationDriver driver,
            EnergyNetworkService network,
            SimulationClock simulationClock,
            MapGenerator generator,
            ResearchController research = null)
        {
            Unsubscribe();
            if (_progress != null)
            {
                _progress.Won -= OnWon;
                _progress.Failed -= OnFailed;
                _progress.StateChanged -= OnStateChanged;
            }

            definition = scenarioDefinition != null
                ? scenarioDefinition
                : ScenarioProgressService.CreateRuntimeDefault();
            energyDriver = driver;
            networkService = network;
            clock = simulationClock;
            mapGenerator = generator;
            researchController = research;
            _progress = new ScenarioProgressService(definition);
            _progress.Won += OnWon;
            _progress.Failed += OnFailed;
            _progress.StateChanged += OnStateChanged;
            _settlement.Reset(definition.StartingPopulation);
            networkService?.SetDemandScaleProvider(() => _settlement.DemandScale);
            Subscribe();
            StateChanged?.Invoke(_progress.State);
        }

        public void ResetProgress()
        {
            EnsureProgress();
            _progress.Reset();
            _settlement.Reset(definition != null
                ? definition.StartingPopulation
                : SettlementState.DefaultStartingPopulation);
            networkService?.MarkDirty();
            if (clock != null && clock.Speed == SimulationSpeed.Paused)
            {
                clock.SetSpeed(SimulationSpeed.One);
            }
        }

        public void RestoreSettlement(float population)
        {
            var start = definition != null
                ? definition.StartingPopulation
                : SettlementState.DefaultStartingPopulation;
            _settlement.Restore(population, start);
            networkService?.SetDemandScaleProvider(() => _settlement.DemandScale);
            networkService?.MarkDirty();
        }

        public void RestoreProgress(ScenarioObjectiveState snapshot)
        {
            EnsureProgress();
            _progress.Restore(snapshot);
            if ((_progress.State.HasWon || _progress.State.HasLost) && clock != null)
            {
                clock.SetSpeed(SimulationSpeed.Paused);
            }
            else if (clock != null && clock.Speed == SimulationSpeed.Paused)
            {
                clock.SetSpeed(SimulationSpeed.One);
            }
        }

        private void EnsureProgress()
        {
            if (_progress != null)
            {
                return;
            }

            if (definition == null)
            {
                definition = ScenarioProgressService.CreateRuntimeDefault();
            }

            _progress = new ScenarioProgressService(definition);
            _progress.Won += OnWon;
            _progress.Failed += OnFailed;
            _progress.StateChanged += OnStateChanged;
            _settlement.Reset(definition.StartingPopulation);
            networkService?.SetDemandScaleProvider(() => _settlement.DemandScale);
        }

        private void Subscribe()
        {
            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated += OnBalanceUpdated;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void Unsubscribe()
        {
            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated -= OnBalanceUpdated;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            ResetProgress();
        }

        private void OnBalanceUpdated(EnergyBalanceResult result)
        {
            if (_progress == null || result == null)
            {
                return;
            }

            networkService?.RebuildIfNeeded();
            AnalyzeNetwork(out var typeCount, out var hasBattery);
            _settlement.Tick(result.CoverageRatio);
            _progress.Evaluate(new ScenarioTickInput(
                result.CoverageRatio,
                result.Demand,
                result.HasShortage,
                typeCount,
                hasBattery,
                IsResearchRequirementMet()));
        }

        private bool IsResearchRequirementMet()
        {
            var required = _progress?.Definition?.RequiredResearchNodeIds;
            if (required == null || required.Length == 0)
            {
                return true;
            }

            var service = researchController?.Service;
            if (service == null)
            {
                return false;
            }

            for (var i = 0; i < required.Length; i++)
            {
                var id = required[i];
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (!service.IsNodeUnlocked(id))
                {
                    return false;
                }
            }

            return true;
        }

        private void AnalyzeNetwork(out int activeProducerTypeCount, out bool hasConnectedBattery)
        {
            activeProducerTypeCount = 0;
            hasConnectedBattery = false;
            _activeTypesBuffer.Clear();

            if (networkService?.Graph == null)
            {
                return;
            }

            var context = new SimulationContext(0, 0.5f, SimulationSpeed.One);
            foreach (var component in networkService.Graph.Components)
            {
                var hasConsumer = false;
                var hasStorage = false;
                for (var i = 0; i < component.Nodes.Count; i++)
                {
                    var node = component.Nodes[i];
                    if (node.Consumer != null)
                    {
                        hasConsumer = true;
                    }

                    if (node.Storage != null)
                    {
                        hasStorage = true;
                    }

                    if (node.Producer == null || !_progress.IsCountedProducerType(node.BuildingTypeId))
                    {
                        continue;
                    }

                    if (node.Producer.GetAvailableProduction(context) > 0.0001f)
                    {
                        _activeTypesBuffer.Add(node.BuildingTypeId);
                    }
                }

                if (hasConsumer && hasStorage)
                {
                    hasConnectedBattery = true;
                }
            }

            activeProducerTypeCount = _activeTypesBuffer.Count;
        }

        private void OnWon(ScenarioWonEvent evt)
        {
            if (clock != null)
            {
                clock.SetSpeed(SimulationSpeed.Paused);
            }

            Won?.Invoke(evt);
            Debug.Log($"[Scenario] Won: {evt.ScenarioId}");
        }

        private void OnFailed(ScenarioFailedEvent evt)
        {
            if (clock != null)
            {
                clock.SetSpeed(SimulationSpeed.Paused);
            }

            Failed?.Invoke(evt);
            Debug.Log($"[Scenario] Failed: {evt.ScenarioId}");
        }

        private void OnStateChanged(ScenarioObjectiveState state)
        {
            StateChanged?.Invoke(state);
        }

        private void OnDestroy()
        {
            if (_progress == null)
            {
                return;
            }

            _progress.Won -= OnWon;
            _progress.Failed -= OnFailed;
            _progress.StateChanged -= OnStateChanged;
        }
    }
}
