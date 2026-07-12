using System;

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

        private static readonly TutorialStepInfo[] StepInfos =
        {
            new TutorialStepInfo(TutorialStepId.Camera, "Move the camera", "Use WASD, Q/E, or scroll wheel."),
            new TutorialStepInfo(TutorialStepId.OpenWaterLayer, "Open Water layer", "Select Water in Terrain Debug view modes."),
            new TutorialStepInfo(TutorialStepId.PlaceWaterWheel, "Build a Water Wheel", "Place water_wheel next to a stream."),
            new TutorialStepInfo(TutorialStepId.PlacePowerLine, "Connect with Power Line", "Place a power_line near your buildings."),
            new TutorialStepInfo(TutorialStepId.OpenSolarLayer, "Open Solar layer", "Select Solar in Terrain Debug view modes."),
            new TutorialStepInfo(TutorialStepId.UnlockSolar, "Research Basic Solar", "Spend RP to unlock solar_basic."),
            new TutorialStepInfo(TutorialStepId.PlaceSolar, "Build Small Solar", "Place a small_solar panel."),
            new TutorialStepInfo(TutorialStepId.PlaceBattery, "Build a Battery", "Place a battery on the network."),
            new TutorialStepInfo(TutorialStepId.MeetDemand, "Sustain village demand", "Keep coverage high for 10 ticks.")
        };

        public TutorialStepId CurrentStep { get; private set; }
        public bool IsComplete => CurrentStep == TutorialStepId.Completed;
        public event Action<TutorialStepId> StepChanged;
        public event Action Completed;

        public TutorialProgressService()
        {
            Reset();
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
            if (IsComplete)
            {
                return new TutorialStepInfo(TutorialStepId.Completed, "Tutorial complete", "Keep supplying the village.");
            }

            return StepInfos[(int)CurrentStep];
        }

        public static TutorialStepInfo GetInfo(TutorialStepId step)
        {
            if (step == TutorialStepId.Completed || (int)step < 0 || (int)step >= StepInfos.Length)
            {
                return new TutorialStepInfo(TutorialStepId.Completed, "Tutorial complete", "Keep supplying the village.");
            }

            return StepInfos[(int)step];
        }

        public static int StepCount => StepInfos.Length;
    }
}
