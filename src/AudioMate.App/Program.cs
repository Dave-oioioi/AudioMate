using AudioMate.App.Tray;
using AudioMate.Core.Configuration;
using AudioMate.Core.Narration;
using AudioMate.Core.Runtime;

namespace AudioMate.App;

internal static class Program
{
    private const string SingleInstanceName = @"Local\Dave.AudioMate.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var instanceGuard = SingleInstanceGuard.TryAcquire(SingleInstanceName);
        if (!instanceGuard.HasOwnership)
        {
            return;
        }

        ApplicationConfiguration.Initialize();

        var paths = AudioMatePaths.ForCurrentUser();
        var settingsStore = new JsonSettingsStore(paths.ConfigFilePath);
        var queue = new FileNarrationQueue(paths.NarrationQueuePath);

        Application.Run(new AudioMateApplicationContext(settingsStore, queue));
    }
}
