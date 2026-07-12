using System;
using UnityEngine;

namespace CleanEnergy.Simulation
{
    public enum WeatherEventKind
    {
        None = 0,
        Cloudy = 1,
        WindGust = 2
    }

    /// <summary>
    /// Deterministic short weather multipliers applied via SimulationContext.
    /// </summary>
    public sealed class WeatherEventService
    {
        public const int CheckIntervalTicks = 120;
        public const int ChancePercent = 25;
        public const int CloudyDurationTicks = 30;
        public const int WindGustDurationTicks = 20;
        public const float CloudySolarMultiplier = 0.5f;
        public const float WindGustWindMultiplier = 1.4f;

        public WeatherEventKind ActiveKind { get; private set; }
        public int RemainingTicks { get; private set; }
        public float SolarMultiplier { get; private set; } = 1f;
        public float WindMultiplier { get; private set; } = 1f;

        public event Action<WeatherEventKind> EventStarted;

        public void Reset()
        {
            ActiveKind = WeatherEventKind.None;
            RemainingTicks = 0;
            SolarMultiplier = 1f;
            WindMultiplier = 1f;
        }

        public void Advance(int tickIndex, int seedHash)
        {
            if (RemainingTicks > 0)
            {
                RemainingTicks--;
                if (RemainingTicks <= 0)
                {
                    Reset();
                }
            }

            if (tickIndex <= 0 || tickIndex % CheckIntervalTicks != 0)
            {
                return;
            }

            if (ActiveKind != WeatherEventKind.None)
            {
                return;
            }

            var roll = DeterministicRoll(tickIndex, seedHash);
            if (Mathf.Abs(roll) % 100 >= ChancePercent)
            {
                return;
            }

            var kind = (roll & 1) == 0 ? WeatherEventKind.Cloudy : WeatherEventKind.WindGust;
            Start(kind);
        }

        public void Start(WeatherEventKind kind)
        {
            switch (kind)
            {
                case WeatherEventKind.Cloudy:
                    ActiveKind = WeatherEventKind.Cloudy;
                    RemainingTicks = CloudyDurationTicks;
                    SolarMultiplier = CloudySolarMultiplier;
                    WindMultiplier = 1f;
                    break;
                case WeatherEventKind.WindGust:
                    ActiveKind = WeatherEventKind.WindGust;
                    RemainingTicks = WindGustDurationTicks;
                    SolarMultiplier = 1f;
                    WindMultiplier = WindGustWindMultiplier;
                    break;
                default:
                    Reset();
                    return;
            }

            EventStarted?.Invoke(ActiveKind);
        }

        public static int DeterministicRoll(int tickIndex, int seedHash)
        {
            unchecked
            {
                return tickIndex ^ seedHash;
            }
        }

        public static int HashSeed(string seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                return 0;
            }

            unchecked
            {
                var hash = 23;
                for (var i = 0; i < seed.Length; i++)
                {
                    hash = hash * 31 + seed[i];
                }

                return hash;
            }
        }
    }
}
