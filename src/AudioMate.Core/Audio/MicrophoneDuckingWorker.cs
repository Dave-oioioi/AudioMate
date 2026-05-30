using AudioMate.Core.Configuration;
using System.Runtime.InteropServices;

namespace AudioMate.Core.Audio;

/// <summary>
/// Polls the default capture device and raises Microphone while input is above threshold.
/// </summary>
public sealed class MicrophoneDuckingWorker(
    IMicrophoneLevelScanner scanner,
    IAudioCore audioCore,
    Func<AudioMateSettings> getSettings,
    TimeSpan? pollInterval = null)
{
    private readonly TimeSpan _pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(120);
    private bool _isTriggerActive;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var shouldTrigger = false;
            try
            {
                shouldTrigger = MicrophoneTriggerDetector.ShouldTrigger(
                    getSettings(),
                    scanner.GetDefaultMicrophonePeakValue());
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
                audioCore.EnterTrigger(AudioTriggerKind.Microphone);
                _isTriggerActive = true;
            }
            else if (!shouldTrigger && _isTriggerActive)
            {
                audioCore.ExitTrigger(AudioTriggerKind.Microphone);
                _isTriggerActive = false;
            }

            await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
