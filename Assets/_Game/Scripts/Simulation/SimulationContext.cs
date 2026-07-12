namespace CleanEnergy.Simulation
{
    /// <summary>
    /// Immutable snapshot passed to energy systems each tick.
    /// </summary>
    public readonly struct SimulationContext
    {
        public int TickIndex { get; }
        public float TickDurationSeconds { get; }
        public SimulationSpeed Speed { get; }
        public float DayNormalized { get; }
        public DayPhase DayPhase { get; }

        public float DemandMultiplier => DayCycleService.GetDemandMultiplier(DayPhase);
        public float DaylightFactor => DayCycleService.GetDaylightFactor(DayPhase);

        public SimulationContext(
            int tickIndex,
            float tickDurationSeconds,
            SimulationSpeed speed,
            float dayNormalized = 0.3f,
            DayPhase dayPhase = DayPhase.Noon)
        {
            TickIndex = tickIndex;
            TickDurationSeconds = tickDurationSeconds;
            Speed = speed;
            DayNormalized = dayNormalized;
            DayPhase = dayPhase;
        }
    }
}
