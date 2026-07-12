namespace CleanEnergy.Save
{
    /// <summary>
    /// Abstraction for local vs future Steam Cloud save roots (S109 stub).
    /// </summary>
    public interface ICloudSaveStore
    {
        /// <summary>True when a real cloud backend is available.</summary>
        bool IsCloudAvailable { get; }

        /// <summary>
        /// Resolves the directory used for slot files. Local implementations return
        /// <paramref name="localDirectory"/> unchanged.
        /// </summary>
        string ResolveSaveDirectory(string localDirectory);
    }
}
