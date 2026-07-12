namespace CleanEnergy.Simulation
{
    public enum SeasonKind
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }

    /// <summary>
    /// Lite seasonal solar/wind multipliers driven by sim day index.
    /// </summary>
    public sealed class SeasonService
    {
        public const int DaysPerSeason = 180;

        public const float SpringSolar = 1f;
        public const float SpringWind = 1f;
        public const float SummerSolar = 1.2f;
        public const float SummerWind = 0.85f;
        public const float AutumnSolar = 0.9f;
        public const float AutumnWind = 1.15f;
        public const float WinterSolar = 0.7f;
        public const float WinterWind = 1.25f;

        public SeasonKind Current { get; private set; } = SeasonKind.Spring;
        public float SolarMultiplier => GetSolarMultiplier(Current);
        public float WindMultiplier => GetWindMultiplier(Current);

        public void Reset()
        {
            Current = SeasonKind.Spring;
        }

        /// <summary>
        /// Syncs season from absolute day index (0-based). Advances every <see cref="DaysPerSeason"/> days.
        /// </summary>
        public void SyncFromDayIndex(int dayIndex)
        {
            Current = ResolveSeason(dayIndex);
        }

        public static SeasonKind ResolveSeason(int dayIndex)
        {
            var day = dayIndex < 0 ? 0 : dayIndex;
            var seasonIndex = (day / DaysPerSeason) % 4;
            return (SeasonKind)seasonIndex;
        }

        public static float GetSolarMultiplier(SeasonKind season)
        {
            switch (season)
            {
                case SeasonKind.Summer:
                    return SummerSolar;
                case SeasonKind.Autumn:
                    return AutumnSolar;
                case SeasonKind.Winter:
                    return WinterSolar;
                default:
                    return SpringSolar;
            }
        }

        public static float GetWindMultiplier(SeasonKind season)
        {
            switch (season)
            {
                case SeasonKind.Summer:
                    return SummerWind;
                case SeasonKind.Autumn:
                    return AutumnWind;
                case SeasonKind.Winter:
                    return WinterWind;
                default:
                    return SpringWind;
            }
        }

        public static string DisplayName(SeasonKind season)
        {
            switch (season)
            {
                case SeasonKind.Summer:
                    return "Summer";
                case SeasonKind.Autumn:
                    return "Autumn";
                case SeasonKind.Winter:
                    return "Winter";
                default:
                    return "Spring";
            }
        }
    }
}
