using System;
using System.Collections.Generic;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Energy
{
    public sealed class EnergyNetworkNode
    {
        public string Id { get; }
        public string BuildingTypeId { get; }
        public GridCoordinate Coordinate { get; }
        public IEnergyProducer Producer { get; }
        public IEnergyConsumer Consumer { get; }
        public IEnergyStorage Storage { get; }
        public bool IsHub { get; }
        public int ConnectionRange { get; }

        public EnergyNetworkNode(
            string id,
            GridCoordinate coordinate,
            IEnergyProducer producer,
            IEnergyConsumer consumer,
            IEnergyStorage storage,
            bool isHub,
            int connectionRange,
            string buildingTypeId = null)
        {
            Id = id;
            BuildingTypeId = buildingTypeId ?? string.Empty;
            Coordinate = coordinate;
            Producer = producer;
            Consumer = consumer;
            Storage = storage;
            IsHub = isHub;
            ConnectionRange = Mathf.Max(1, connectionRange);
        }
    }

    public sealed class EnergyNetworkComponent
    {
        public int ComponentId { get; }
        public IReadOnlyList<EnergyNetworkNode> Nodes { get; }

        public EnergyNetworkComponent(int componentId, List<EnergyNetworkNode> nodes)
        {
            ComponentId = componentId;
            Nodes = nodes;
        }
    }

    /// <summary>
    /// Undirected graph of energy nodes; rebuilds connected components on demand.
    /// </summary>
    public sealed class EnergyNetworkGraph
    {
        private readonly Dictionary<string, EnergyNetworkNode> _nodes = new Dictionary<string, EnergyNetworkNode>();
        private readonly Dictionary<string, HashSet<string>> _edges = new Dictionary<string, HashSet<string>>();
        private List<EnergyNetworkComponent> _components = new List<EnergyNetworkComponent>();
        private bool _dirty = true;

        public IReadOnlyDictionary<string, EnergyNetworkNode> Nodes => _nodes;
        public IReadOnlyList<EnergyNetworkComponent> Components
        {
            get
            {
                if (_dirty)
                {
                    RebuildComponents();
                }

                return _components;
            }
        }

        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
            _components.Clear();
            _dirty = true;
        }

        public void AddOrReplaceNode(EnergyNetworkNode node)
        {
            if (node == null)
            {
                return;
            }

            _nodes[node.Id] = node;
            if (!_edges.ContainsKey(node.Id))
            {
                _edges[node.Id] = new HashSet<string>();
            }

            _dirty = true;
        }

        public void RemoveNode(string nodeId)
        {
            if (!_nodes.Remove(nodeId))
            {
                return;
            }

            if (_edges.TryGetValue(nodeId, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (_edges.TryGetValue(neighbor, out var other))
                    {
                        other.Remove(nodeId);
                    }
                }

                _edges.Remove(nodeId);
            }

            _dirty = true;
        }

        public void Connect(string a, string b)
        {
            if (a == b || !_nodes.ContainsKey(a) || !_nodes.ContainsKey(b))
            {
                return;
            }

            if (!_edges.TryGetValue(a, out var setA))
            {
                setA = new HashSet<string>();
                _edges[a] = setA;
            }

            if (!_edges.TryGetValue(b, out var setB))
            {
                setB = new HashSet<string>();
                _edges[b] = setB;
            }

            if (setA.Add(b))
            {
                setB.Add(a);
                _dirty = true;
            }
        }

        public bool AreConnected(string a, string b)
        {
            if (a == b)
            {
                return _nodes.ContainsKey(a);
            }

            foreach (var component in Components)
            {
                var hasA = false;
                var hasB = false;
                for (var i = 0; i < component.Nodes.Count; i++)
                {
                    var id = component.Nodes[i].Id;
                    if (id == a) hasA = true;
                    if (id == b) hasB = true;
                }

                if (hasA && hasB)
                {
                    return true;
                }
            }

            return false;
        }

        private void RebuildComponents()
        {
            _components = new List<EnergyNetworkComponent>();
            var visited = new HashSet<string>();
            var componentId = 0;

            foreach (var pair in _nodes)
            {
                if (!visited.Add(pair.Key))
                {
                    continue;
                }

                var list = new List<EnergyNetworkNode>();
                var queue = new Queue<string>();
                queue.Enqueue(pair.Key);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    if (_nodes.TryGetValue(current, out var node))
                    {
                        list.Add(node);
                    }

                    if (!_edges.TryGetValue(current, out var neighbors))
                    {
                        continue;
                    }

                    foreach (var neighbor in neighbors)
                    {
                        if (visited.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                _components.Add(new EnergyNetworkComponent(componentId++, list));
            }

            _dirty = false;
        }
    }
}
