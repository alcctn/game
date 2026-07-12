using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CleanEnergy.DebugTools;
using UnityEngine;

namespace CleanEnergy.Telemetry
{
    /// <summary>
    /// In-memory play-session counters for balance tuning (GDD §17).
    /// </summary>
    public sealed class SessionTelemetryService
    {
        public const string DefaultFileName = "telemetry_session.csv";

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

        public static string CsvHeader =>
            "time_to_first_building,time_to_first_production,invalid_placements,preferred_layer,shortage_ratio,shortage_ticks,balance_ticks,scenario_end,fail_reason";

        public string ToCsvLine()
        {
            return string.Join(",",
                FormatOptional(TimeToFirstBuildingSeconds),
                FormatOptional(TimeToFirstProductionSeconds),
                InvalidPlacementAttempts.ToString(CultureInfo.InvariantCulture),
                PreferredDebugLayer.ToString(),
                AverageShortageRatio.ToString("F4", CultureInfo.InvariantCulture),
                ShortageTicks.ToString(CultureInfo.InvariantCulture),
                BalanceTicks.ToString(CultureInfo.InvariantCulture),
                FormatOptional(ScenarioEndElapsedSeconds),
                Escape(FailReason));
        }

        public string ExportToPersistentDataPath()
        {
            var path = Path.Combine(Application.persistentDataPath, DefaultFileName);
            ExportToPath(path);
            return path;
        }

        public void ExportToPath(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine(CsvHeader);
            sb.AppendLine(ToCsvLine());
            File.WriteAllText(path, sb.ToString());
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

        private static string FormatOptional(float? value)
        {
            return value.HasValue
                ? value.Value.ToString("F3", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\n' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
