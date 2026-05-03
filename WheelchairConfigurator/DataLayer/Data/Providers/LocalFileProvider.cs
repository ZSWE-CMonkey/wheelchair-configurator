using System.IO;

namespace WheelchairConfigurator.Data.Providers;

/// <summary>
/// Responsible solely for locating and providing paths to data files.
/// </summary>
public class LocalFileProvider : ILocalFileProvider
{
    /// <summary>
    /// Returns an array of file paths to JSON seed files that need to be processed.
    /// </summary>
    /// <returns>An array of file paths as strings.</returns>
    public string[] GetSeedFilePaths()
    {
        return new string[]
        {
            Path.Combine(FileSystem.AppDataDirectory, "seed_data.json")
        };
    }
}