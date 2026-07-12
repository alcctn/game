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
        public float SeasonSolarMultiplier { get; }
        public float SeasonWindMultiplier { get; }
        public float WeatherSolarMultiplier { get; }
        public float WeatherWindMultiplier { get; }

        public float DemandMultiplier => DayCycleService.GetDemandMultiplier(DayPhase);

        public float DaylightFactor =>
            DayCycleService.GetDaylightFactor(DayPhase) * SeasonSolarMultiplier * WeatherSolarMultiplier;

        public float WindFactor =>
            DayCycleService.GetWindFactor(DayPhase) * SeasonWindMultiplier * WeatherWindMultiplier;

        public SimulationContext(
            int tickIndex,
            float tickDurationSeconds,
            SimulationSpeed speed,
            float dayNormalized = 0.3f,
            DayPhase dayPhase = DayPhase.Noon,
            float weatherSolarMultiplier = 1f,
            float weatherWindMultiplier = 1f,
            float seasonSolarMultiplier = 1f,
            float seasonWindMultiplier = 1f)
        {
            TickIndex = tickIndex;
            TickDurationSeconds = tickDurationSeconds;
            Speed = speed;
            DayNormalized = dayNormalized;
            DayPhase = dayPhase;
            SeasonSolarMultiplier = seasonSolarMultiplier <= 0f ? 0f : seasonSolarMultiplier;
            SeasonWindMultiplier = seasonWindMultiplier <= 0f ? 0f : seasonWindMultiplier;
            WeatherSolarMultiplier = weatherSolarMultiplier <= 0f ? 0f : weatherSolarMultiplier;
            WeatherWindMultiplier = weatherWindMultiplier <= 0f ? 0f : weatherWindMultiplier;
        }
    }
}
