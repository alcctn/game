using CleanEnergy.Save;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Static service registry for bootstrap-owned dependencies.
    /// </summary>
    public static class GameServices
    {
        public static ICloudSaveStore CloudSaveStore { get; private set; }
            = new LocalCloudSaveStore();

        public static void RegisterCloudSaveStore(ICloudSaveStore store)
        {
            CloudSaveStore = store ?? new LocalCloudSaveStore();
        }

        public static void ResetToDefaults()
        {
            CloudSaveStore = new LocalCloudSaveStore();
        }
    }
}
