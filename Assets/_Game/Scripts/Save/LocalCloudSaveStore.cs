namespace CleanEnergy.Save
{
    /// <summary>
    /// Passthrough cloud store — always uses the local save directory.
    /// </summary>
    public sealed class LocalCloudSaveStore : ICloudSaveStore
    {
        public bool IsCloudAvailable => false;

        public string ResolveSaveDirectory(string localDirectory)
        {
            return localDirectory ?? string.Empty;
        }
    }
}
