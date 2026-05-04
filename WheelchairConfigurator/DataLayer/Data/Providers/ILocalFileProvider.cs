namespace WheelchairConfigurator.Data.Providers;

/// <summary>
/// Contract for resolving paths to seed data files.
/// </summary>
public interface ILocalFileProvider
{
    /// <summary>
    /// Returns an array of file paths to JSON seed files that need to be processed.
    /// </summary>
    string[] GetSeedFilePaths();
}