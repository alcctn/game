namespace CleanEnergy.Tutorial
{
    /// <summary>
    /// Ordered tutorial steps. Battery is always-unlocked — there is no UnlockBattery step.
    /// </summary>
    public enum TutorialStepId
    {
        Camera = 0,
        OpenWaterLayer = 1,
        PlaceWaterWheel = 2,
        PlacePowerLine = 3,
        OpenSolarLayer = 4,
        UnlockSolar = 5,
        PlaceSolar = 6,
        PlaceBattery = 7,
        MeetDemand = 8,
        Completed = 9
    }
}
