using System;
using System.Collections.Generic;
using UnityEngine;

namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Snapshot fed into scenario evaluation each energy tick.
    /// </summary>
    public readonly struct ScenarioTickInput
    {
        public float CoverageRatio { get; }
        public float Demand { get; }
        public bool HasShortage { get; }
        public int ActiveProducerTypeCount { get; }
        public bool HasConnectedBattery { get; }
        public bool ResearchRequirementMet { get; }

        public ScenarioTickInput(
            float coverageRatio,
            float demand,
            bool hasShortage,
            int activeProducerTypeCount,
            bool hasConnectedBattery,
            bool researchRequirementMet = true)
        {
            CoverageRatio = coverageRatio;
            Demand = demand;
            HasShortage = hasShortage;
            ActiveProducerTypeCount = activeProducerTypeCount;
            HasConnectedBattery = hasConnectedBattery;
            ResearchRequirementMet = researchRequirementMet;
        }
    }

    public sealed class ScenarioWonEvent
    {
        public string ScenarioId { get; }

        public ScenarioWonEvent(string scenarioId)
        {
            ScenarioId = scenarioId;
        }
    }

    public sealed class ScenarioFailedEvent
    {
        public string ScenarioId { get; }

        public ScenarioFailedEvent(string scenarioId)
        {
            ScenarioId = scenarioId;
        }
    }

    /// <summary>
    /// Pure tick logic for coverage streak, diversity, battery, satisfaction and win/lose.
    /// </summary>
    public sealed class ScenarioProgressService
    {
        private readonly ScenarioDefinition _definition;
        private readonly ScenarioObjectiveState _state = new ScenarioObjectiveState();
        private readonly HashSet<string> _countedTypes;

        public ScenarioObjectiveState State => _state;
        public ScenarioDefinition Definition => _definition;
        public event Action<ScenarioWonEvent> Won;
        public event Action<ScenarioFailedEvent> Failed;
        public event Action<ScenarioObjectiveState> StateChanged;

        public ScenarioProgressService(ScenarioDefinition definition)
        {
            _definition = definition != null
                ? definition
                : CreateRuntimeDefault();
            _countedTypes = new HashSet<string>(_definition.CountedProducerTypeIds ?? Array.Empty<string>());
            Reset();
        }

        public void Reset()
        {
            _state.Reset(_definition.InitialSatisfaction);
            StateChanged?.Invoke(_state);
        }

        public void Restore(ScenarioObjectiveState snapshot)
        {
            if (snapshot == null)
            {
                Reset();
                return;
            }

            _state.CoverageStreakTicks = snapshot.CoverageStreakTicks;
            _state.DemandObjectiveComplete = snapshot.DemandObjectiveComplete;
            _state.DiversityObjectiveComplete = snapshot.DiversityObjectiveComplete;
            _state.BatteryObjectiveComplete = snapshot.BatteryObjectiveComplete;
            _state.ResearchObjectiveComplete = snapshot.ResearchObjectiveComplete;
            _state.ActiveProducerTypeCount = snapshot.ActiveProducerTypeCount;
            _state.CoverageRatio = snapshot.CoverageRatio;
            _state.Satisfaction = snapshot.Satisfaction;
            _state.ShortageStreakTicks = snapshot.ShortageStreakTicks;
            _state.IsAtRisk = snapshot.IsAtRisk;
            _state.HasWon = snapshot.HasWon;
            _state.HasLost = snapshot.HasLost;
            StateChanged?.Invoke(_state);
            if (_state.HasWon)
            {
                Won?.Invoke(new ScenarioWonEvent(_definition.ScenarioId));
            }
            else if (_state.HasLost)
            {
                Failed?.Invoke(new ScenarioFailedEvent(_definition.ScenarioId));
            }
        }

        public void Evaluate(ScenarioTickInput input)
        {
            if (_state.HasWon || _state.HasLost)
            {
                return;
            }

            _state.CoverageRatio = input.CoverageRatio;
            _state.ActiveProducerTypeCount = input.ActiveProducerTypeCount;
            _state.BatteryObjectiveComplete = input.HasConnectedBattery;
            _state.ResearchObjectiveComplete = input.ResearchRequirementMet;
            _state.DiversityObjectiveComplete =
                input.ActiveProducerTypeCount >= _definition.RequiredProducerTypes;

            var hasDemand = input.Demand > 0.0001f;
            var meetsCoverage = hasDemand && input.CoverageRatio + 0.0001f >= _definition.RequiredCoverageRatio;

            if (meetsCoverage)
            {
                _state.CoverageStreakTicks++;
                _state.ShortageStreakTicks = 0;
                _state.Satisfaction = Mathf.Min(
                    _definition.InitialSatisfaction,
                    _state.Satisfaction + _definition.CoverageSatisfactionRecovery);
            }
            else if (hasDemand)
            {
                _state.CoverageStreakTicks = 0;
                if (input.HasShortage || input.CoverageRatio + 0.0001f < _definition.RequiredCoverageRatio)
                {
                    _state.ShortageStreakTicks++;
                    _state.Satisfaction = Mathf.Max(
                        0f,
                        _state.Satisfaction - _definition.ShortageSatisfactionPenalty);
                }
            }
            else
            {
                _state.CoverageStreakTicks = 0;
                _state.ShortageStreakTicks = 0;
            }

            if (!_state.DemandObjectiveComplete
                && _state.CoverageStreakTicks >= _definition.RequiredCoverageTicks)
            {
                _state.DemandObjectiveComplete = true;
            }

            _state.IsAtRisk = _state.Satisfaction <= _definition.RiskSatisfactionThreshold;
            StateChanged?.Invoke(_state);

            if (_state.Satisfaction <= 0.0001f)
            {
                _state.HasLost = true;
                Failed?.Invoke(new ScenarioFailedEvent(_definition.ScenarioId));
                StateChanged?.Invoke(_state);
                return;
            }

            if (_state.AllObjectivesComplete)
            {
                _state.HasWon = true;
                Won?.Invoke(new ScenarioWonEvent(_definition.ScenarioId));
                StateChanged?.Invoke(_state);
            }
        }

        public bool IsCountedProducerType(string buildingTypeId)
        {
            return !string.IsNullOrEmpty(buildingTypeId) && _countedTypes.Contains(buildingTypeId);
        }

        public static ScenarioDefinition CreateRuntimeDefault()
        {
            var def = ScriptableObject.CreateInstance<ScenarioDefinition>();
            def.name = "RuntimeGreenValley";
            def.Configure(
                "green_valley",
                "Green Valley",
                0.95f,
                60,
                2,
                100f,
                2f,
                0.25f,
                30f);
            return def;
        }
    }
}
