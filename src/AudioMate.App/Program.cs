using AudioMate.App.Tray;
using AudioMate.Core.Configuration;
using AudioMate.Core.Narration;

namespace AudioMate.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var paths = AudioMatePaths.ForCurrentUser();
        var settingsStore = new JsonSettingsStore(paths.ConfigFilePath);
        var queue = new FileNarrationQueue(paths.NarrationQueuePath);

        Application.Run(new AudioMateApplicationContext(settingsStore, queue));
    }
}
