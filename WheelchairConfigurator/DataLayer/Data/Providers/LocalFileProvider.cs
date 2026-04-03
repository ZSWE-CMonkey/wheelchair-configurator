using System.IO;

namespace WheelchairConfigurator.Data.Providers;

/// <summary>
/// Responsible solely for locating and providing paths to data files.
/// </summary>
public class LocalFileProvider
{
    /// <summary>
    /// Returns an array of file paths to JSON seed files that need to be processed.
    /// </summary>
    /// <returns>An array of file paths as strings.</returns>
    public string[] GetSeedFilePaths()
    {
        return new string[]
        {
            "Resources/seed_data.json"
        };
    }
}