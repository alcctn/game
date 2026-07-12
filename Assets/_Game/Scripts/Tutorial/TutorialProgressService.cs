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
    /// Ordered Green Valley Level 1 tutorial; only the current step can complete.
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

        public static bool IsEnabledForScenario(string scenarioId)
        {
            return string.Equals(scenarioId, TutorialScenarioId, StringComparison.Ordinal);
        }

        public static string ResolveBuildTargetId(TutorialStepId step)
        {
            switch (step)
            {
                case TutorialStepId.PlaceWaterWheel:
                    return "water_wheel";
                case TutorialStepId.PlaceWind:
                    return "small_wind";
                default:
                    return null;
            }
        }

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

        public static int StepCount => 6;

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
                case TutorialStepId.HireEngineer:
                    titleKey = StringKeys.TutorialHireEngineerTitle;
                    hintKey = StringKeys.TutorialHireEngineerHint;
                    break;
                case TutorialStepId.PlaceWaterWheel:
                    titleKey = StringKeys.TutorialPlaceWaterWheelTitle;
                    hintKey = StringKeys.TutorialPlaceWaterWheelHint;
                    break;
                case TutorialStepId.HireTechnician:
                    titleKey = StringKeys.TutorialHireTechnicianTitle;
                    hintKey = StringKeys.TutorialHireTechnicianHint;
                    break;
                case TutorialStepId.PlaceWind:
                    titleKey = StringKeys.TutorialPlaceWindTitle;
                    hintKey = StringKeys.TutorialPlaceWindHint;
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
