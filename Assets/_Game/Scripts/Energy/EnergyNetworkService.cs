using System;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Energy
{
    public sealed class EnergyShortageEvent
    {
        public float Shortage { get; }

        public EnergyShortageEvent(float shortage)
        {
            Shortage = shortage;
        }
    }

    public sealed class NetworkChangedEvent
    {
    }

    /// <summary>
    /// Tracks placed energy buildings and rebuilds the network graph when needed.
    /// </summary>
    public sealed class EnergyNetworkService : MonoBehaviour
    {
        [SerializeField] private PlacementController placementController;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private int defaultConnectionRange = 4;

        private readonly EnergyNetworkGraph _graph = new EnergyNetworkGraph();
        private System.Func<string, float> _efficiencyBonusProvider;
        private bool _dirty = true;

        public EnergyNetworkGraph Graph => _graph;
        public event Action<NetworkChangedEvent> NetworkChanged;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            PlacementController placement,
            MapGenerator generator,
            System.Func<string, float> efficiencyBonusProvider = null)
        {
            Unsubscribe();
            placementController = placement;
            mapGenerator = generator;
            _efficiencyBonusProvider = efficiencyBonusProvider;
            Subscribe();
            MarkDirty();
            Rebuild();
        }

        public void SetEfficiencyBonusProvider(System.Func<string, float> efficiencyBonusProvider)
        {
            _efficiencyBonusProvider = efficiencyBonusProvider;
            MarkDirty();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public void RebuildIfNeeded()
        {
            if (_dirty)
            {
                Rebuild();
            }
        }

        public void Rebuild()
        {
            _graph.Clear();
            if (placementController == null || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                _dirty = false;
                NetworkChanged?.Invoke(new NetworkChangedEvent());
                return;
            }

            var grid = mapGenerator.Grid;
            var settings = mapGenerator.Settings;
            var hubs = new System.Collections.Generic.List<EnergyNetworkNode>();
            var energyNodes = new System.Collections.Generic.List<EnergyNetworkNode>();

            foreach (var pair in placementController.Occupancy.Occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null)
                {
                    continue;
                }

                var def = instance.Definition;
                var producer = BuildingEnergyFactory.TryCreateProducer(
                    instance, grid, settings, _efficiencyBonusProvider);
                var consumer = BuildingEnergyFactory.TryCreateConsumer(instance);
                var storage = BuildingEnergyFactory.TryCreateStorage(instance);
                var isHub = IsHubDefinition(def);
                var range = def.ConnectionRange > 0 ? def.ConnectionRange : defaultConnectionRange;

                if (producer == null && consumer == null && storage == null && !isHub)
                {
                    continue;
                }

                var node = new EnergyNetworkNode(
                    instance.InstanceId,
                    instance.Coordinate,
                    producer,
                    consumer,
                    storage,
                    isHub,
                    range,
                    def.Id);
                _graph.AddOrReplaceNode(node);

                if (isHub)
                {
                    hubs.Add(node);
                }
                else
                {
                    energyNodes.Add(node);
                }
            }

            for (var i = 0; i < hubs.Count; i++)
            {
                var hub = hubs[i];
                for (var j = 0; j < energyNodes.Count; j++)
                {
                    if (Manhattan(hub.Coordinate, energyNodes[j].Coordinate) <= hub.ConnectionRange)
                    {
                        _graph.Connect(hub.Id, energyNodes[j].Id);
                    }
                }

                for (var j = i + 1; j < hubs.Count; j++)
                {
                    var other = hubs[j];
                    var maxRange = Mathf.Max(hub.ConnectionRange, other.ConnectionRange);
                    if (Manhattan(hub.Coordinate, other.Coordinate) <= maxRange)
                    {
                        _graph.Connect(hub.Id, other.Id);
                    }
                }
            }

            _dirty = false;
            NetworkChanged?.Invoke(new NetworkChangedEvent());
            Debug.Log($"[EnergyNetwork] Rebuilt nodes={_graph.Nodes.Count} components={_graph.Components.Count}");
        }

        private void OnBuildingPlaced(BuildingPlacedEvent _)
        {
            MarkDirty();
            Rebuild();
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            _graph.Clear();
            MarkDirty();
            Rebuild();
        }

        private void Subscribe()
        {
            if (placementController != null)
            {
                placementController.BuildingPlaced += OnBuildingPlaced;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void Unsubscribe()
        {
            if (placementController != null)
            {
                placementController.BuildingPlaced -= OnBuildingPlaced;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }

        /// <summary>
        /// Network hubs are flagged via BuildingDefinition.IsNetworkHub only.
        /// </summary>
        public static bool IsHubDefinition(BuildingDefinition def)
        {
            return def != null && def.IsNetworkHub;
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }
    }
}
