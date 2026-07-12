using System.Collections.Generic;
using CleanEnergy.DebugTools;

namespace CleanEnergy.Telemetry
{
    /// <summary>
    /// In-memory play-session counters for balance tuning (GDD §17).
    /// </summary>
    public sealed class SessionTelemetryService
    {
        private readonly Dictionary<DebugViewMode, int> _layerCounts = new Dictionary<DebugViewMode, int>();

        public float SessionStartTime { get; private set; }
        public float? TimeToFirstBuildingSeconds { get; private set; }
        public float? TimeToFirstProductionSeconds { get; private set; }
        public int InvalidPlacementAttempts { get; private set; }
        public int BalanceTicks { get; private set; }
        public int ShortageTicks { get; private set; }
        public float? ScenarioEndElapsedSeconds { get; private set; }
        public string FailReason { get; private set; } = string.Empty;

        public float AverageShortageRatio =>
            BalanceTicks <= 0 ? 0f : (float)ShortageTicks / BalanceTicks;

        public DebugViewMode PreferredDebugLayer
        {
            get
            {
                var best = DebugViewMode.Normal;
                var bestCount = 0;
                foreach (var pair in _layerCounts)
                {
                    if (pair.Key == DebugViewMode.Normal)
                    {
                        continue;
                    }

                    if (pair.Value > bestCount)
                    {
                        bestCount = pair.Value;
                        best = pair.Key;
                    }
                }

                return best;
            }
        }

        public void Reset(float now)
        {
            SessionStartTime = now;
            TimeToFirstBuildingSeconds = null;
            TimeToFirstProductionSeconds = null;
            InvalidPlacementAttempts = 0;
            BalanceTicks = 0;
            ShortageTicks = 0;
            ScenarioEndElapsedSeconds = null;
            FailReason = string.Empty;
            _layerCounts.Clear();
        }

        public void RecordBuildingPlaced(float now)
        {
            if (!TimeToFirstBuildingSeconds.HasValue)
            {
                TimeToFirstBuildingSeconds = now - SessionStartTime;
            }
        }

        public void RecordInvalidPlacement()
        {
            InvalidPlacementAttempts++;
        }

        public void RecordBalanceTick(float production, bool hasShortage, float now)
        {
            BalanceTicks++;
            if (hasShortage)
            {
                ShortageTicks++;
            }

            if (!TimeToFirstProductionSeconds.HasValue && production > 0.0001f)
            {
                TimeToFirstProductionSeconds = now - SessionStartTime;
            }
        }

        public void RecordDebugMode(DebugViewMode mode)
        {
            if (!_layerCounts.TryGetValue(mode, out var count))
            {
                count = 0;
            }

            _layerCounts[mode] = count + 1;
        }

        public void RecordScenarioEnded(bool lost, float now)
        {
            if (ScenarioEndElapsedSeconds.HasValue)
            {
                return;
            }

            ScenarioEndElapsedSeconds = now - SessionStartTime;
            FailReason = lost ? "shortage" : string.Empty;
        }
    }
}
