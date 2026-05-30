using AudioMate.Core.Audio;
using AudioMate.Core.Tts;

namespace AudioMate.Core.Narration;

/// <summary>
/// Polls the file queue and serializes narration playback.
/// </summary>
public sealed class NarrationQueueWorker(
    INarrationQueue queue,
    IAudioCore audioCore,
    ITtsEngine ttsEngine,
    TimeSpan? pollInterval = null)
{
    private readonly TimeSpan _pollInterval = pollInterval ?? TimeSpan.FromMilliseconds(500);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var item = queue.TryClaimNext();
            if (item is null)
            {
                await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
                continue;
            }

            await ProcessItemAsync(item, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessItemAsync(NarrationQueueItem item, CancellationToken cancellationToken)
    {
        audioCore.EnterTrigger(AudioTriggerKind.CodexNarration);
        try
        {
            await ttsEngine.SpeakAsync(item.Request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            audioCore.ExitTrigger(AudioTriggerKind.CodexNarration);
            queue.Complete(item);
        }
    }
}
