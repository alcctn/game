using System;
using System.Collections.Generic;
using CleanEnergy.Economy;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Research
{
    public sealed class ResearchUnlockedEvent
    {
        public string NodeId { get; }

        public ResearchUnlockedEvent(string nodeId)
        {
            NodeId = nodeId;
        }
    }

    /// <summary>
    /// Tracks unlocked research nodes, building gates and efficiency bonuses.
    /// </summary>
    public sealed class ResearchService : IBuildingUnlockQuery
    {
        private readonly ResearchTreeDefinition _tree;
        private readonly ResearchPointWallet _wallet;
        private readonly HashSet<string> _unlockedNodes = new HashSet<string>();
        private readonly HashSet<string> _unlockedBuildings = new HashSet<string>();
        private readonly Dictionary<string, float> _efficiencyBonuses = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _storageCapacityBonuses = new Dictionary<string, float>();
        private readonly Dictionary<string, ResearchNodeDefinition> _nodesById =
            new Dictionary<string, ResearchNodeDefinition>();

        public ResearchPointWallet Wallet => _wallet;
        public ResearchTreeDefinition Tree => _tree;
        public IReadOnlyCollection<string> UnlockedNodeIds => _unlockedNodes;
        public event Action<ResearchUnlockedEvent> NodeUnlocked;
        public event Action StateChanged;

        public ResearchService(ResearchTreeDefinition tree, float startingPoints = 0f)
        {
            _tree = tree != null ? tree : CreateRuntimeDefaultTree();
            _wallet = new ResearchPointWallet(startingPoints);
            IndexNodes();
            Reset();
        }

        public void Reset()
        {
            _unlockedNodes.Clear();
            _unlockedBuildings.Clear();
            _efficiencyBonuses.Clear();
            _storageCapacityBonuses.Clear();
            _wallet.SetPoints(0f);

            foreach (var id in _tree.AlwaysUnlockedBuildingIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _unlockedBuildings.Add(id);
                }
            }

            foreach (var node in _tree.Nodes)
            {
                if (node != null && node.UnlockedByDefault)
                {
                    ApplyUnlock(node, spend: false);
                }
            }

            StateChanged?.Invoke();
        }

        public void Restore(IEnumerable<string> unlockedNodeIds, float researchPoints)
        {
            Reset();
            if (unlockedNodeIds != null)
            {
                foreach (var id in unlockedNodeIds)
                {
                    if (string.IsNullOrEmpty(id) || _unlockedNodes.Contains(id))
                    {
                        continue;
                    }

                    if (_nodesById.TryGetValue(id, out var node))
                    {
                        ApplyUnlock(node, spend: false);
                    }
                }
            }

            _wallet.SetPoints(researchPoints);
            StateChanged?.Invoke();
        }

        public bool IsNodeUnlocked(string nodeId) =>
            !string.IsNullOrEmpty(nodeId) && _unlockedNodes.Contains(nodeId);

        public bool IsBuildingUnlocked(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                return false;
            }

            return _unlockedBuildings.Contains(buildingId);
        }

        public float GetEfficiencyBonus(string buildingTypeId)
        {
            if (string.IsNullOrEmpty(buildingTypeId))
            {
                return 0f;
            }

            return _efficiencyBonuses.TryGetValue(buildingTypeId, out var bonus) ? bonus : 0f;
        }

        public float GetStorageCapacityBonus(string buildingTypeId)
        {
            if (string.IsNullOrEmpty(buildingTypeId))
            {
                return 0f;
            }

            return _storageCapacityBonuses.TryGetValue(buildingTypeId, out var bonus) ? bonus : 0f;
        }

        public bool CanUnlock(string nodeId, out string reason)
        {
            reason = string.Empty;
            if (!_nodesById.TryGetValue(nodeId, out var node))
            {
                reason = "Unknown research node.";
                return false;
            }

            if (_unlockedNodes.Contains(node.Id))
            {
                reason = "Already unlocked.";
                return false;
            }

            if (!string.IsNullOrEmpty(node.PrerequisiteNodeId)
                && !_unlockedNodes.Contains(node.PrerequisiteNodeId))
            {
                reason = $"Requires {node.PrerequisiteNodeId}.";
                return false;
            }

            if (!_wallet.CanAfford(node.ResearchPointCost))
            {
                reason = $"Need {node.ResearchPointCost:F0} RP.";
                return false;
            }

            return true;
        }

        public bool TryUnlock(string nodeId)
        {
            if (!CanUnlock(nodeId, out _))
            {
                return false;
            }

            var node = _nodesById[nodeId];
            if (!_wallet.TrySpend(node.ResearchPointCost))
            {
                return false;
            }

            ApplyUnlock(node, spend: false);
            NodeUnlocked?.Invoke(new ResearchUnlockedEvent(node.Id));
            StateChanged?.Invoke();
            Debug.Log($"[Research] Unlocked '{node.Id}'. RP={_wallet.Points:F0}");
            return true;
        }

        public ResearchNodeDefinition GetNode(string nodeId)
        {
            return _nodesById.TryGetValue(nodeId, out var node) ? node : null;
        }

        private void ApplyUnlock(ResearchNodeDefinition node, bool spend)
        {
            if (node == null)
            {
                return;
            }

            if (spend && !_wallet.TrySpend(node.ResearchPointCost))
            {
                return;
            }

            _unlockedNodes.Add(node.Id);
            foreach (var buildingId in node.UnlockBuildingIds)
            {
                if (!string.IsNullOrEmpty(buildingId))
                {
                    _unlockedBuildings.Add(buildingId);
                }
            }

            if (!string.IsNullOrEmpty(node.EfficiencyTargetBuildingId) && node.EfficiencyBonus > 0f)
            {
                if (_efficiencyBonuses.TryGetValue(node.EfficiencyTargetBuildingId, out var existing))
                {
                    _efficiencyBonuses[node.EfficiencyTargetBuildingId] = existing + node.EfficiencyBonus;
                }
                else
                {
                    _efficiencyBonuses[node.EfficiencyTargetBuildingId] = node.EfficiencyBonus;
                }
            }

            if (!string.IsNullOrEmpty(node.StorageCapacityTargetBuildingId)
                && node.StorageCapacityBonus > 0f)
            {
                if (_storageCapacityBonuses.TryGetValue(
                        node.StorageCapacityTargetBuildingId, out var existingCap))
                {
                    _storageCapacityBonuses[node.StorageCapacityTargetBuildingId] =
                        existingCap + node.StorageCapacityBonus;
                }
                else
                {
                    _storageCapacityBonuses[node.StorageCapacityTargetBuildingId] =
                        node.StorageCapacityBonus;
                }
            }
        }

        private void IndexNodes()
        {
            _nodesById.Clear();
            foreach (var node in _tree.Nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.Id))
                {
                    continue;
                }

                _nodesById[node.Id] = node;
            }
        }

        public static ResearchTreeDefinition CreateRuntimeDefaultTree()
        {
            var tree = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
            tree.name = "RuntimeGreenValleyResearch";
            tree.ConfigureGreenValleyPrototype();
            return tree;
        }
    }
}
