namespace CleanEnergy.Save
{
    /// <summary>
    /// Steam Cloud stub without Steamworks SDK — behaves as unavailable / local passthrough.
    /// </summary>
    public sealed class SteamCloudSaveStoreStub : ICloudSaveStore
    {
#if CLEANENERGY_STEAMWORKS
        // Real Steamworks wiring would go here when the package is approved.
#endif

        public bool IsCloudAvailable => false;

        public string ResolveSaveDirectory(string localDirectory)
        {
            return localDirectory ?? string.Empty;
        }
    }
}
