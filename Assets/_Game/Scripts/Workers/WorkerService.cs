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
            if (!CanHireTechnician())
            {
                return false;
            }

            return TryHire(WorkerType.Technician, TechnicianHireCost);
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
