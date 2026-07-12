namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Session selection from MainMenu into play scene bootstrap.
    /// </summary>
    public static class ScenarioSession
    {
        public const string DefaultId = "green_valley";
        public static string SelectedId { get; set; } = DefaultId;
        public static bool LoadSaveOnPlay { get; set; }
        public static int ContinueSlot { get; set; } = 1;

        public static string ResolveSelectedId()
        {
            return string.IsNullOrEmpty(SelectedId) ? DefaultId : SelectedId;
        }

        public static bool ConsumeLoadSaveOnPlay()
        {
            if (!LoadSaveOnPlay)
            {
                return false;
            }

            LoadSaveOnPlay = false;
            return true;
        }

        public static int ResolveContinueSlot()
        {
            return ContinueSlot < 1 ? 1 : (ContinueSlot > 3 ? 3 : ContinueSlot);
        }
    }
}
