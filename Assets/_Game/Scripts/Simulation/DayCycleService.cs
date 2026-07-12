using UnityEngine;

namespace CleanEnergy.Simulation
{
    /// <summary>
    /// Advances a fixed-length day and exposes demand / daylight multipliers.
    /// </summary>
    public sealed class DayCycleService
    {
        public const int DefaultTicksPerDay = 48;

        private readonly int _ticksPerDay;

        public int TicksPerDay => _ticksPerDay;
        public int TickInDay { get; private set; }
        public int DayIndex { get; private set; }

        public float DayNormalized =>
            _ticksPerDay <= 0 ? 0f : (float)TickInDay / _ticksPerDay;

        public DayPhase Phase => PhaseFromNormalized(DayNormalized);

        public DayCycleService(int ticksPerDay = DefaultTicksPerDay)
        {
            _ticksPerDay = Mathf.Max(1, ticksPerDay);
            Reset();
        }

        public void Reset()
        {
            TickInDay = 0;
            DayIndex = 0;
        }

        /// <summary>
        /// Syncs cycle position from the global simulation tick index (1-based ticks from clock).
        /// </summary>
        public void SyncFromTickIndex(int tickIndex)
        {
            if (tickIndex <= 0)
            {
                TickInDay = 0;
                DayIndex = 0;
                return;
            }

            var zeroBased = tickIndex - 1;
            DayIndex = zeroBased / _ticksPerDay;
            TickInDay = zeroBased % _ticksPerDay;
        }

        public void Advance()
        {
            TickInDay++;
            if (TickInDay >= _ticksPerDay)
            {
                TickInDay = 0;
                DayIndex++;
            }
        }

        public static DayPhase PhaseFromNormalized(float dayNormalized)
        {
            var t = dayNormalized - Mathf.Floor(dayNormalized);
            if (t < 0f)
            {
                t += 1f;
            }

            if (t < 0.25f)
            {
                return DayPhase.Morning;
            }

            if (t < 0.5f)
            {
                return DayPhase.Noon;
            }

            if (t < 0.75f)
            {
                return DayPhase.Evening;
            }

            return DayPhase.Night;
        }

        public static float GetDemandMultiplier(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Morning: return 1.0f;
                case DayPhase.Noon: return 0.75f;
                case DayPhase.Evening: return 1.45f;
                case DayPhase.Night: return 0.55f;
                default: return 1f;
            }
        }

        public static float GetDaylightFactor(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Morning: return 0.55f;
                case DayPhase.Noon: return 1.0f;
                case DayPhase.Evening: return 0.35f;
                case DayPhase.Night: return 0.0f;
                default: return 1f;
            }
        }

        /// <summary>
        /// Complementary wind curve: stronger at night/evening, weaker at noon.
        /// </summary>
        public static float GetWindFactor(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Morning: return 0.85f;
                case DayPhase.Noon: return 0.55f;
                case DayPhase.Evening: return 1.15f;
                case DayPhase.Night: return 1.35f;
                default: return 1f;
            }
        }

        public float CurrentDemandMultiplier => GetDemandMultiplier(Phase);
        public float CurrentDaylightFactor => GetDaylightFactor(Phase);
        public float CurrentWindFactor => GetWindFactor(Phase);
    }
}
