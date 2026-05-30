using AudioMate.Core.Configuration;
using System.Runtime.InteropServices;

namespace AudioMate.Core.Audio;

/// <summary>
/// Polls render sessions and raises OtherAudio while non-music apps are audible.
/// </summary>
public sealed class OrdinaryAudioDuckingWorker(
    IAudioSessionScanner scanner,
    IAudioCore audioCore,
    Func<AudioMateSettings> getSettings,
    TimeSpan? pollInterval = null)
{
    private readonly TimeSpan _pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(250);
    private bool _isTriggerActive;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var shouldTrigger = false;
            try
            {
                var sessions = scanner.ScanRenderSessions();
                shouldTrigger = AudioSessionTriggerDetector.HasAudibleNonMusicTrigger(getSettings(), sessions);
            }
            catch (InvalidOperationException)
            {
                shouldTrigger = false;
            }
            catch (COMException)
            {
                shouldTrigger = false;
            }

            if (shouldTrigger && !_isTriggerActive)
            {
                audioCore.EnterTrigger(AudioTriggerKind.OtherAudio);
                _isTriggerActive = true;
            }
            else if (!shouldTrigger && _isTriggerActive)
            {
                audioCore.ExitTrigger(AudioTriggerKind.OtherAudio);
                _isTriggerActive = false;
            }

            await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
