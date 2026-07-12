namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Mutable checklist / soft-lose state for the active scenario.
    /// </summary>
    public sealed class ScenarioObjectiveState
    {
        public int CoverageStreakTicks { get; set; }
        public bool DemandObjectiveComplete { get; set; }
        public bool DiversityObjectiveComplete { get; set; }
        public bool BatteryObjectiveComplete { get; set; }
        public bool ResearchObjectiveComplete { get; set; }
        public int ActiveProducerTypeCount { get; set; }
        public float CoverageRatio { get; set; }
        public float Satisfaction { get; set; }
        public int ShortageStreakTicks { get; set; }
        public bool IsAtRisk { get; set; }
        public bool HasWon { get; set; }
        public bool HasLost { get; set; }

        public bool AllObjectivesComplete =>
            DemandObjectiveComplete
            && DiversityObjectiveComplete
            && BatteryObjectiveComplete
            && ResearchObjectiveComplete;

        public void Reset(float initialSatisfaction)
        {
            CoverageStreakTicks = 0;
            DemandObjectiveComplete = false;
            DiversityObjectiveComplete = false;
            BatteryObjectiveComplete = false;
            ResearchObjectiveComplete = false;
            ActiveProducerTypeCount = 0;
            CoverageRatio = 0f;
            Satisfaction = initialSatisfaction;
            ShortageStreakTicks = 0;
            IsAtRisk = false;
            HasWon = false;
            HasLost = false;
        }
    }
}
