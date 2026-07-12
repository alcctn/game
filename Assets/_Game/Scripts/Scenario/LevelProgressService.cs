using System;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Placement;
using CleanEnergy.Workers;
using UnityEngine;

namespace CleanEnergy.Scenario
{
    public sealed class LevelObjectiveState
    {
        public bool EngineerComplete { get; set; }
        public bool WaterComplete { get; set; }
        public bool TechnicianComplete { get; set; }
        public bool WindComplete { get; set; }
        public bool CoverageComplete { get; set; }
        public int CoverageStreakTicks { get; set; }
        public float CoverageRatio { get; set; }
        public bool WaterRewardGranted { get; set; }
        public bool TechnicianRewardGranted { get; set; }
        public bool HasCompletedLevel { get; set; }

        public float ProgressPercent { get; private set; }

        public void Reset()
        {
            EngineerComplete = false;
            WaterComplete = false;
            TechnicianComplete = false;
            WindComplete = false;
            CoverageComplete = false;
            CoverageStreakTicks = 0;
            CoverageRatio = 0f;
            WaterRewardGranted = false;
            TechnicianRewardGranted = false;
            HasCompletedLevel = false;
            ProgressPercent = 0f;
        }

        public void Recalculate(LevelDefinition level)
        {
            if (level == null)
            {
                ProgressPercent = 0f;
                return;
            }

            var total = level.WeightEngineer + level.WeightWater + level.WeightTechnician
                        + level.WeightWind + level.WeightCoverage;
            if (total <= 0)
            {
                ProgressPercent = 0f;
                return;
            }

            var score = 0f;
            if (EngineerComplete) score += level.WeightEngineer;
            if (WaterComplete) score += level.WeightWater;
            if (TechnicianComplete) score += level.WeightTechnician;
            if (WindComplete) score += level.WeightWind;
            if (CoverageComplete)
            {
                score += level.WeightCoverage;
            }
            else
            {
                var ratio = Mathf.Clamp01(CoverageStreakTicks / (float)level.RequiredCoverageTicks);
                score += ratio * level.WeightCoverage;
            }

            ProgressPercent = 100f * score / total;
        }
    }

    /// <summary>Tracks Level 1 weighted objectives and grants one-shot rewards.</summary>
    public sealed class LevelProgressService
    {
        private readonly LevelDefinition _level;
        private readonly LevelObjectiveState _state = new LevelObjectiveState();

        public LevelObjectiveState State => _state;
        public LevelDefinition Definition => _level;
        public event Action<LevelObjectiveState> StateChanged;
        public event Action LevelCompleted;

        public LevelProgressService(LevelDefinition level)
        {
            _level = level != null ? level : LevelDefinition.CreateRuntimeDefault();
            _state.Reset();
            _state.Recalculate(_level);
        }

        public void Reset()
        {
            _state.Reset();
            _state.Recalculate(_level);
            StateChanged?.Invoke(_state);
        }

        public void Evaluate(
            IWorkerQuery workers,
            GridOccupancyService occupancy,
            float coverageRatio,
            Wallet wallet)
        {
            if (_state.HasCompletedLevel)
            {
                return;
            }

            _state.CoverageRatio = coverageRatio;
            _state.EngineerComplete = workers != null && workers.EngineerCount >= 1;
            _state.TechnicianComplete = workers != null && workers.TechnicianCount >= 1;
            EvaluateBuildings(occupancy);

            if (coverageRatio + 0.0001f >= _level.RequiredCoverageRatio)
            {
                if (_state.CoverageStreakTicks < _level.RequiredCoverageTicks)
                {
                    _state.CoverageStreakTicks++;
                }
            }
            else
            {
                _state.CoverageStreakTicks = 0;
            }

            _state.CoverageComplete = _state.CoverageStreakTicks >= _level.RequiredCoverageTicks;

            GrantRewards(wallet);
            _state.Recalculate(_level);
            StateChanged?.Invoke(_state);

            if (_state.EngineerComplete
                && _state.WaterComplete
                && _state.TechnicianComplete
                && _state.WindComplete
                && _state.CoverageComplete)
            {
                _state.HasCompletedLevel = true;
                StateChanged?.Invoke(_state);
                LevelCompleted?.Invoke();
            }
        }

        private void EvaluateBuildings(GridOccupancyService occupancy)
        {
            _state.WaterComplete = false;
            _state.WindComplete = false;
            if (occupancy == null)
            {
                return;
            }

            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var pair in occupancy.Occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                var id = instance.Definition.Id;
                if (id == "water_wheel" || id == "small_hydro")
                {
                    _state.WaterComplete = true;
                }

                if (id == "small_wind")
                {
                    _state.WindComplete = true;
                }
            }
        }

        private void GrantRewards(Wallet wallet)
        {
            if (wallet == null)
            {
                return;
            }

            if (_state.WaterComplete && !_state.WaterRewardGranted)
            {
                _state.WaterRewardGranted = true;
                wallet.Add(_level.RewardWaterBuilt);
            }

            if (_state.TechnicianComplete && !_state.TechnicianRewardGranted)
            {
                _state.TechnicianRewardGranted = true;
                wallet.Add(_level.RewardTechnicianCreated);
            }
        }
    }
}
