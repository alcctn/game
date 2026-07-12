using CleanEnergy.Energy;
using CleanEnergy.Map;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Research
{
    /// <summary>
    /// MonoBehaviour hub for research tree, RP grants and generate reset.
    /// </summary>
    public sealed class ResearchController : MonoBehaviour
    {
        [SerializeField] private ResearchTreeDefinition treeDefinition;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private EnergyNetworkService networkService;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private MapGenerator mapGenerator;

        private ResearchService _service;
        private ResearchProgressTracker _tracker;
        private readonly System.Collections.Generic.HashSet<string> _typeBuffer =
            new System.Collections.Generic.HashSet<string>();

        public ResearchService Service => _service;
        public event System.Action StateChanged;

        private void OnEnable()
        {
            EnsureService();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            ResearchTreeDefinition tree,
            EnergySimulationDriver driver,
            EnergyNetworkService network,
            ScenarioController scenario,
            MapGenerator generator)
        {
            Unsubscribe();
            treeDefinition = tree != null ? tree : ResearchService.CreateRuntimeDefaultTree();
            energyDriver = driver;
            networkService = network;
            scenarioController = scenario;
            mapGenerator = generator;
            _service = new ResearchService(treeDefinition);
            _tracker = new ResearchProgressTracker(_service);
            if (_service != null)
            {
                _service.StateChanged += OnServiceStateChanged;
            }

            Subscribe();
            StateChanged?.Invoke();
        }

        public void ResetResearch()
        {
            EnsureService();
            _tracker?.Reset();
            _service.Reset();
            StateChanged?.Invoke();
        }

        public void NotifyAfterLoad(int activeProducerTypeCount)
        {
            EnsureService();
            _tracker?.Reset();
            if (activeProducerTypeCount >= 2)
            {
                _tracker?.MarkDiversityBonusGranted();
            }

            StateChanged?.Invoke();
        }

        private void EnsureService()
        {
            if (_service != null)
            {
                return;
            }

            if (treeDefinition == null)
            {
                treeDefinition = ResearchService.CreateRuntimeDefaultTree();
            }

            _service = new ResearchService(treeDefinition);
            _tracker = new ResearchProgressTracker(_service);
            _service.StateChanged += OnServiceStateChanged;
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
            ResetResearch();
        }

        private void OnBalanceUpdated(EnergyBalanceResult result)
        {
            if (_tracker == null || _service == null)
            {
                return;
            }

            var types = CountActiveProducerTypes();
            _tracker.OnBalanceTick(result, types);
            StateChanged?.Invoke();
        }

        private int CountActiveProducerTypes()
        {
            _typeBuffer.Clear();
            if (networkService?.Graph == null || _service == null)
            {
                return scenarioController != null
                    ? scenarioController.State?.ActiveProducerTypeCount ?? 0
                    : 0;
            }

            var context = new SimulationContext(0, 0.5f, SimulationSpeed.One);
            foreach (var component in networkService.Graph.Components)
            {
                for (var i = 0; i < component.Nodes.Count; i++)
                {
                    var node = component.Nodes[i];
                    if (node.Producer == null || string.IsNullOrEmpty(node.BuildingTypeId))
                    {
                        continue;
                    }

                    if (node.Producer.GetAvailableProduction(context) > 0.0001f)
                    {
                        _typeBuffer.Add(node.BuildingTypeId);
                    }
                }
            }

            return _typeBuffer.Count;
        }

        private void OnServiceStateChanged()
        {
            StateChanged?.Invoke();
        }

        private void OnDestroy()
        {
            if (_service != null)
            {
                _service.StateChanged -= OnServiceStateChanged;
            }
        }
    }
}
