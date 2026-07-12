using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Maintenance
{
    /// <summary>
    /// One building entry within a repair undo batch (S107).
    /// </summary>
    public sealed class RepairUndoSnapshot
    {
        public string InstanceId { get; }
        public float PreviousMaintenanceLevel { get; }
        public float Cost { get; }

        public RepairUndoSnapshot(string instanceId, float previousMaintenanceLevel, float cost)
        {
            InstanceId = instanceId ?? string.Empty;
            PreviousMaintenanceLevel = previousMaintenanceLevel;
            Cost = Mathf.Max(0f, cost);
        }
    }

    /// <summary>
    /// LIFO repair-batch undo stack (depth ≤3), separate from demolish undo.
    /// </summary>
    public sealed class RepairUndoService
    {
        public const int MaxRepairUndoGroups = 3;

        private readonly List<List<RepairUndoSnapshot>> _stack =
            new List<List<RepairUndoSnapshot>>();

        public bool HasRepairUndo => _stack.Count > 0;
        public int RepairUndoStackDepth => _stack.Count;
        public int RepairUndoCount =>
            _stack.Count > 0 ? _stack[_stack.Count - 1].Count : 0;

        public IReadOnlyList<RepairUndoSnapshot> RepairUndoGroup =>
            _stack.Count > 0
                ? _stack[_stack.Count - 1]
                : (IReadOnlyList<RepairUndoSnapshot>)System.Array.Empty<RepairUndoSnapshot>();

        public void Clear()
        {
            _stack.Clear();
        }

        public void Push(IReadOnlyList<RepairUndoSnapshot> group)
        {
            if (group == null || group.Count == 0)
            {
                return;
            }

            var copy = new List<RepairUndoSnapshot>(group.Count);
            for (var i = 0; i < group.Count; i++)
            {
                if (group[i] != null)
                {
                    copy.Add(group[i]);
                }
            }

            if (copy.Count == 0)
            {
                return;
            }

            _stack.Add(copy);
            Trim();
        }

        /// <summary>
        /// Refunds the last repair batch cost and restores prior maintenance levels.
        /// </summary>
        public bool TryUndoLast(
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Wallet wallet)
        {
            if (_stack.Count == 0 || wallet == null)
            {
                return false;
            }

            var group = _stack[_stack.Count - 1];
            var totalCost = 0f;
            for (var i = 0; i < group.Count; i++)
            {
                totalCost += group[i].Cost;
            }

            wallet.Add(totalCost);
            RestoreLevels(occupied, group);
            _stack.RemoveAt(_stack.Count - 1);
            Debug.Log($"[Maintenance] Undo repair group ({group.Count}). Stack={_stack.Count}");
            return true;
        }

        private void Trim()
        {
            while (_stack.Count > MaxRepairUndoGroups)
            {
                _stack.RemoveAt(0);
            }
        }

        private static void RestoreLevels(
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            IReadOnlyList<RepairUndoSnapshot> group)
        {
            if (occupied == null || group == null)
            {
                return;
            }

            var byId = new Dictionary<string, BuildingInstance>();
            foreach (var pair in occupied)
            {
                var instance = pair.Value;
                if (instance == null || string.IsNullOrEmpty(instance.InstanceId))
                {
                    continue;
                }

                if (!byId.ContainsKey(instance.InstanceId))
                {
                    byId[instance.InstanceId] = instance;
                }
            }

            for (var i = 0; i < group.Count; i++)
            {
                var snap = group[i];
                if (byId.TryGetValue(snap.InstanceId, out var building))
                {
                    building.MaintenanceLevel = Mathf.Clamp(
                        snap.PreviousMaintenanceLevel,
                        MaintenanceService.MinLevel,
                        MaintenanceService.MaxLevel);
                }
            }
        }
    }
}
