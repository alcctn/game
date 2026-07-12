using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Hop-based delivery loss from producer to nearest load along network edges.
    /// </summary>
    public static class TransmissionLoss
    {
        public const float LossPerHop = 0.05f;
        public const float MinDeliveryFactor = 0.25f;
        public const string PlacementGhostId = "__placement_ghost__";

        /// <summary>
        /// BFS edge hops from producer to nearest consumer/storage in the graph.
        /// </summary>
        public static float ResolveDeliveryFactor(string producerNodeId, EnergyNetworkGraph graph)
        {
            if (graph == null || string.IsNullOrEmpty(producerNodeId) || !graph.Nodes.ContainsKey(producerNodeId))
            {
                return 1f;
            }

            var hops = BfsHopsToNearestLoad(producerNodeId, graph);
            if (hops < 0)
            {
                return 1f;
            }

            return FactorFromHops(hops);
        }

        /// <summary>
        /// Legacy overload: resolves producer id inside the component, then BFS on the graph.
        /// When graph is null, returns 1 (no path loss).
        /// </summary>
        public static float ResolveDeliveryFactor(
            GridCoordinate producerCoordinate,
            EnergyNetworkComponent component,
            EnergyNetworkGraph graph)
        {
            if (graph == null || component?.Nodes == null)
            {
                return 1f;
            }

            for (var i = 0; i < component.Nodes.Count; i++)
            {
                var node = component.Nodes[i];
                if (node.Coordinate.X == producerCoordinate.X && node.Coordinate.Y == producerCoordinate.Y)
                {
                    return ResolveDeliveryFactor(node.Id, graph);
                }
            }

            return 1f;
        }

        /// <summary>
        /// Placement ghost: BFS along occupancy network edges (same hub rules) to nearest load.
        /// </summary>
        public static float ResolveDeliveryFactorForPlacement(
            GridCoordinate coordinate,
            GridOccupancyService occupancy,
            BuildingDefinition placingDefinition = null)
        {
            if (occupancy == null)
            {
                return 1f;
            }

            var graph = BuildOccupancyGraph(occupancy, coordinate, placingDefinition);
            return ResolveDeliveryFactor(PlacementGhostId, graph);
        }

        public static float ResolveForBuilding(BuildingInstance building, EnergyNetworkGraph graph)
        {
            if (building == null || graph == null)
            {
                return 1f;
            }

            return ResolveDeliveryFactor(building.InstanceId, graph);
        }

        public static float FactorFromHops(int hops)
        {
            if (hops < 0)
            {
                return 1f;
            }

            return Mathf.Clamp(1f - LossPerHop * hops, MinDeliveryFactor, 1f);
        }

        /// <summary>
        /// Shortest undirected edge path length to a consumer/storage; -1 when unreachable.
        /// </summary>
        public static int BfsHopsToNearestLoad(string startId, EnergyNetworkGraph graph)
        {
            if (graph == null || string.IsNullOrEmpty(startId) || !graph.Nodes.ContainsKey(startId))
            {
                return -1;
            }

            if (IsLoad(graph.Nodes[startId]))
            {
                return 0;
            }

            var visited = new HashSet<string> { startId };
            var queue = new Queue<(string Id, int Depth)>();
            queue.Enqueue((startId, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();
                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    if (!visited.Add(neighbor))
                    {
                        continue;
                    }

                    if (!graph.Nodes.TryGetValue(neighbor, out var node))
                    {
                        continue;
                    }

                    var nextDepth = depth + 1;
                    if (IsLoad(node))
                    {
                        return nextDepth;
                    }

                    queue.Enqueue((neighbor, nextDepth));
                }
            }

            return -1;
        }

        private static bool IsLoad(EnergyNetworkNode node) =>
            node != null && (node.Consumer != null || node.Storage != null);

        private static EnergyNetworkGraph BuildOccupancyGraph(
            GridOccupancyService occupancy,
            GridCoordinate ghostCoordinate,
            BuildingDefinition placingDefinition)
        {
            var graph = new EnergyNetworkGraph();
            var hubs = new List<EnergyNetworkNode>();
            var energyNodes = new List<EnergyNetworkNode>();
            var seen = new HashSet<string>();

            foreach (var pair in occupancy.Occupied)
            {
                var instance = pair.Value;
                var def = instance?.Definition;
                if (instance == null || def == null || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                var isHub = EnergyNetworkService.IsHubDefinition(def);
                var range = def.ConnectionRange > 0 ? def.ConnectionRange : 4;
                IEnergyConsumer consumer = null;
                IEnergyStorage storage = null;
                if (def.IsConsumer)
                {
                    consumer = PlacementLoadMarker.Instance;
                }
                else if (def.IsStorage)
                {
                    storage = PlacementStorageMarker.Instance;
                }

                if (consumer == null && storage == null && !isHub && !def.IsProducer)
                {
                    continue;
                }

                var node = new EnergyNetworkNode(
                    instance.InstanceId,
                    instance.Coordinate,
                    producer: null,
                    consumer,
                    storage,
                    isHub,
                    range,
                    def.Id,
                    def.LinkCapacity);
                graph.AddOrReplaceNode(node);
                if (isHub)
                {
                    hubs.Add(node);
                }
                else
                {
                    energyNodes.Add(node);
                }
            }

            var ghostRange = placingDefinition != null && placingDefinition.ConnectionRange > 0
                ? placingDefinition.ConnectionRange
                : 4;
            var ghost = new EnergyNetworkNode(
                PlacementGhostId,
                ghostCoordinate,
                producer: null,
                consumer: null,
                storage: null,
                isHub: false,
                ghostRange,
                placingDefinition != null ? placingDefinition.Id : "ghost");
            graph.AddOrReplaceNode(ghost);
            energyNodes.Add(ghost);

            for (var i = 0; i < hubs.Count; i++)
            {
                var hub = hubs[i];
                for (var j = 0; j < energyNodes.Count; j++)
                {
                    if (Manhattan(hub.Coordinate, energyNodes[j].Coordinate) <= hub.ConnectionRange)
                    {
                        graph.Connect(hub.Id, energyNodes[j].Id);
                    }
                }

                for (var j = i + 1; j < hubs.Count; j++)
                {
                    var other = hubs[j];
                    var maxRange = Mathf.Max(hub.ConnectionRange, other.ConnectionRange);
                    if (Manhattan(hub.Coordinate, other.Coordinate) <= maxRange)
                    {
                        graph.Connect(hub.Id, other.Id);
                    }
                }
            }

            return graph;
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }

        private sealed class PlacementLoadMarker : IEnergyConsumer
        {
            public static readonly PlacementLoadMarker Instance = new PlacementLoadMarker();
            public string NodeId => "placement_load";
            public float GetDemand(Simulation.SimulationContext context) => 0f;
        }

        private sealed class PlacementStorageMarker : IEnergyStorage
        {
            public static readonly PlacementStorageMarker Instance = new PlacementStorageMarker();
            public string NodeId => "placement_storage";
            public float StoredEnergy => 0f;
            public float Capacity => 0f;
            public float MaxChargeRate => 0f;
            public float MaxDischargeRate => 0f;
            public float Charge(float amount) => 0f;
            public float Discharge(float amount) => 0f;
        }
    }
}
