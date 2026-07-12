using System;
using CleanEnergy.UI;

namespace CleanEnergy.Tutorial
{
    public readonly struct TutorialStepInfo
    {
        public TutorialStepId Id { get; }
        public string Title { get; }
        public string Hint { get; }

        public TutorialStepInfo(TutorialStepId id, string title, string hint)
        {
            Id = id;
            Title = title;
            Hint = hint;
        }
    }

    /// <summary>
    /// Ordered Green Valley tutorial; only the current step can complete.
    /// </summary>
    public sealed class TutorialProgressService
    {
        public const int MeetDemandStreakTicks = 10;
        public const string TutorialScenarioId = "green_valley";

        public TutorialStepId CurrentStep { get; private set; }
        public bool IsComplete => CurrentStep == TutorialStepId.Completed;
        public event Action<TutorialStepId> StepChanged;
        public event Action Completed;

        public TutorialProgressService()
        {
            Reset();
        }

        /// <summary>Tutorial runs only for Green Valley.</summary>
        public static bool IsEnabledForScenario(string scenarioId)
        {
            return string.Equals(scenarioId, TutorialScenarioId, StringComparison.Ordinal);
        }

        /// <summary>Soft-highlight build target id for the current step, or null.</summary>
        public static string ResolveBuildTargetId(TutorialStepId step)
        {
            switch (step)
            {
                case TutorialStepId.PlaceWaterWheel:
                    return "water_wheel";
                case TutorialStepId.PlacePowerLine:
                    return "power_line";
                case TutorialStepId.PlaceSolar:
                    return "small_solar";
                case TutorialStepId.PlaceBattery:
                    return "battery";
                default:
                    return null;
            }
        }

        /// <summary>Soft highlight prefix for build menu rows (no hard lock).</summary>
        public static string FormatSoftHighlightLabel(string label, bool highlighted)
        {
            if (!highlighted || string.IsNullOrEmpty(label))
            {
                return label ?? string.Empty;
            }

            return $"> {label}";
        }

        public void Reset()
        {
            CurrentStep = TutorialStepId.Camera;
            StepChanged?.Invoke(CurrentStep);
        }

        public void Restore(TutorialStepId step)
        {
            if (step < TutorialStepId.Camera || step > TutorialStepId.Completed)
            {
                step = TutorialStepId.Camera;
            }

            CurrentStep = step;
            StepChanged?.Invoke(CurrentStep);
        }

        public bool TryComplete(TutorialStepId step)
        {
            if (IsComplete || step != CurrentStep)
            {
                return false;
            }

            CurrentStep = (TutorialStepId)((int)CurrentStep + 1);
            StepChanged?.Invoke(CurrentStep);
            if (IsComplete)
            {
                Completed?.Invoke();
            }

            return true;
        }

        public TutorialStepInfo GetCurrentInfo()
        {
            return GetInfo(CurrentStep);
        }

        public static TutorialStepInfo GetInfo(TutorialStepId step)
        {
            return GetInfo(step, null);
        }

        public static TutorialStepInfo GetInfo(TutorialStepId step, string locale)
        {
            if (step == TutorialStepId.Completed || (int)step < 0 || (int)step >= StepCount)
            {
                return new TutorialStepInfo(
                    TutorialStepId.Completed,
                    Resolve(StringKeys.TutorialCompleteTitle, locale),
                    Resolve(StringKeys.TutorialCompleteHint, locale));
            }

            ResolveKeys(step, out var titleKey, out var hintKey);
            return new TutorialStepInfo(step, Resolve(titleKey, locale), Resolve(hintKey, locale));
        }

        public static int StepCount => 9;

        private static string Resolve(string key, string locale)
        {
            return string.IsNullOrEmpty(locale)
                ? StringTable.Get(key)
                : StringTable.Get(key, locale);
        }

        private static void ResolveKeys(TutorialStepId step, out string titleKey, out string hintKey)
        {
            switch (step)
            {
                case TutorialStepId.Camera:
                    titleKey = StringKeys.TutorialCameraTitle;
                    hintKey = StringKeys.TutorialCameraHint;
                    break;
                case TutorialStepId.OpenWaterLayer:
                    titleKey = StringKeys.TutorialOpenWaterTitle;
                    hintKey = StringKeys.TutorialOpenWaterHint;
                    break;
                case TutorialStepId.PlaceWaterWheel:
                    titleKey = StringKeys.TutorialPlaceWaterWheelTitle;
                    hintKey = StringKeys.TutorialPlaceWaterWheelHint;
                    break;
                case TutorialStepId.PlacePowerLine:
                    titleKey = StringKeys.TutorialPlacePowerLineTitle;
                    hintKey = StringKeys.TutorialPlacePowerLineHint;
                    break;
                case TutorialStepId.OpenSolarLayer:
                    titleKey = StringKeys.TutorialOpenSolarTitle;
                    hintKey = StringKeys.TutorialOpenSolarHint;
                    break;
                case TutorialStepId.UnlockSolar:
                    titleKey = StringKeys.TutorialUnlockSolarTitle;
                    hintKey = StringKeys.TutorialUnlockSolarHint;
                    break;
                case TutorialStepId.PlaceSolar:
                    titleKey = StringKeys.TutorialPlaceSolarTitle;
                    hintKey = StringKeys.TutorialPlaceSolarHint;
                    break;
                case TutorialStepId.PlaceBattery:
                    titleKey = StringKeys.TutorialPlaceBatteryTitle;
                    hintKey = StringKeys.TutorialPlaceBatteryHint;
                    break;
                case TutorialStepId.MeetDemand:
                    titleKey = StringKeys.TutorialMeetDemandTitle;
                    hintKey = StringKeys.TutorialMeetDemandHint;
                    break;
                default:
                    titleKey = StringKeys.TutorialCompleteTitle;
                    hintKey = StringKeys.TutorialCompleteHint;
                    break;
            }
        }
    }
}
