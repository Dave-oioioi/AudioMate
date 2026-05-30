using AudioMate.Core.Audio;
using AudioMate.Core.Configuration;
using AudioMate.Core.Narration;
using AudioMate.Core.Tts;

namespace AudioMate.Core.Tests;

public sealed class NarrationQueueWorkerTests
{
    [Fact]
    public async Task RunAsync_WhenRequestExists_SpeaksWithCodexDucking()
    {
        using var temp = new TempDirectory();
        var queue = new FileNarrationQueue(temp.Path);
        var audioCore = new RecordingAudioCore();
        var ttsEngine = new RecordingTtsEngine();
        var worker = new NarrationQueueWorker(queue, audioCore, ttsEngine, TimeSpan.FromMilliseconds(10));
        queue.Enqueue(NarrationRequest.Create("hello", "Codex"));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var runTask = worker.RunAsync(cts.Token);

        while (ttsEngine.Spoken.Count == 0 && !cts.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runTask);

        Assert.Equal(["enter:CodexNarration", "exit:CodexNarration"], audioCore.Events);
        Assert.Equal(["hello"], ttsEngine.Spoken);
        Assert.Empty(queue.ReadPending());
        Assert.Empty(Directory.EnumerateFiles(temp.Path));
    }

    private sealed class RecordingAudioCore : IAudioCore
    {
        public List<string> Events { get; } = [];

        public DuckingSnapshot CurrentState => new(false, 0, 20, TimeSpan.Zero);

        public void ApplySettings(AudioMateSettings settings)
        {
        }

        public DuckingSnapshot EnterTrigger(AudioTriggerKind trigger)
        {
            Events.Add($"enter:{trigger}");
            return CurrentState;
        }

        public DuckingSnapshot ExitTrigger(AudioTriggerKind trigger)
        {
            Events.Add($"exit:{trigger}");
            return CurrentState;
        }
    }

    private sealed class RecordingTtsEngine : ITtsEngine
    {
        public List<string> Spoken { get; } = [];

        public Task SpeakAsync(NarrationRequest request, CancellationToken cancellationToken = default)
        {
            Spoken.Add(request.Text);
            return Task.CompletedTask;
        }
    }
}
