namespace AudioMate.Core.Configuration;

/// <summary>
/// Resolves AudioMate's per-user storage locations.
/// </summary>
public sealed record AudioMatePaths(
    string ConfigDirectoryPath,
    string ConfigFilePath,
    string NarrationQueuePath)
{
    public static AudioMatePaths ForCurrentUser()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var configDirectory = Path.Combine(appData, "AudioMate");
        var queueDirectory = Path.Combine(localAppData, "AudioMate", "NarrationQueue");

        return new AudioMatePaths(
            configDirectory,
            Path.Combine(configDirectory, "config.json"),
            queueDirectory);
    }
}
