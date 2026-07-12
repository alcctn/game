using System;
using CleanEnergy.Economy;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.Workers
{
    /// <summary>Hires engineers/technicians against the wallet using LevelDefinition costs.</summary>
    public sealed class WorkerService
    {
        private readonly WorkerPool _pool = new WorkerPool();
        private LevelDefinition _level;
        private Wallet _wallet;
        private Func<bool> _canHireTechnician;

        public WorkerPool Pool => _pool;
        public event Action WorkersChanged;

        public void Configure(
            LevelDefinition level,
            Wallet wallet,
            Func<bool> canHireTechnician = null)
        {
            _level = level;
            _wallet = wallet;
            _canHireTechnician = canHireTechnician;
        }

        public void Reset()
        {
            _pool.Reset();
            WorkersChanged?.Invoke();
        }

        public float EngineerHireCost => _level != null ? _level.EngineerHireCost : 40f;
        public float TechnicianHireCost => _level != null ? _level.TechnicianHireCost : 40f;

        public bool CanHireTechnician()
        {
            return _canHireTechnician == null || _canHireTechnician();
        }

        public bool TryHireEngineer()
        {
            return TryHire(WorkerType.Engineer, EngineerHireCost);
        }

        public bool TryHireTechnician()
        {
            var gate = CanHireTechnician();
            var money = _wallet != null ? _wallet.Money : -1f;
            if (!gate)
            {
                // #region agent log
                CleanEnergy.DebugTools.AgentDebugLog.Write(
                    "T1",
                    "WorkerService.TryHireTechnician",
                    "gate_fail",
                    "{\"money\":" + money.ToString("F0") + "}");
                // #endregion
                return false;
            }

            var ok = TryHire(WorkerType.Technician, TechnicianHireCost);
            // #region agent log
            CleanEnergy.DebugTools.AgentDebugLog.Write(
                "T2",
                "WorkerService.TryHireTechnician",
                ok ? "hired" : "money_fail",
                "{\"money\":" + money.ToString("F0") +
                ",\"cost\":" + TechnicianHireCost.ToString("F0") +
                ",\"techCount\":" + _pool.TechnicianCount + "}");
            // #endregion
            return ok;
        }

        private bool TryHire(WorkerType type, float cost)
        {
            if (_wallet == null || !_wallet.TrySpend(Mathf.Max(0f, cost)))
            {
                return false;
            }

            _pool.TryAdd(type);
            WorkersChanged?.Invoke();
            return true;
        }
    }
}
