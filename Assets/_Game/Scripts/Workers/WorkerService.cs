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

        public WorkerPool Pool => _pool;
        public event Action WorkersChanged;

        public void Configure(LevelDefinition level, Wallet wallet)
        {
            _level = level;
            _wallet = wallet;
        }

        public void Reset()
        {
            _pool.Reset();
            WorkersChanged?.Invoke();
        }

        public float EngineerHireCost => _level != null ? _level.EngineerHireCost : 40f;
        public float TechnicianHireCost => _level != null ? _level.TechnicianHireCost : 40f;

        public bool TryHireEngineer()
        {
            return TryHire(WorkerType.Engineer, EngineerHireCost);
        }

        public bool TryHireTechnician()
        {
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
