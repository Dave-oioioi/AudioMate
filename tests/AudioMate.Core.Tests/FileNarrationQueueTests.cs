using AudioMate.Core.Narration;

namespace AudioMate.Core.Tests;

public sealed class FileNarrationQueueTests
{
    [Fact]
    public void Enqueue_WritesRequestsInReadOrder()
    {
        using var temp = new TempDirectory();
        var queue = new FileNarrationQueue(temp.Path);

        var first = new NarrationRequest(Guid.NewGuid(), "first", "Codex", "thread-1", new DateTimeOffset(2026, 5, 29, 1, 0, 0, TimeSpan.Zero));
        var second = new NarrationRequest(Guid.NewGuid(), "second", "Codex", "thread-1", new DateTimeOffset(2026, 5, 29, 1, 0, 1, TimeSpan.Zero));

        queue.Enqueue(second);
        queue.Enqueue(first);

        var pending = queue.ReadPending();

        Assert.Equal(["first", "second"], pending.Select(static item => item.Text));
    }

    [Fact]
    public void TryClaimNext_MovesOldestRequestUntilCompleted()
    {
        using var temp = new TempDirectory();
        var queue = new FileNarrationQueue(temp.Path);
        var request = NarrationRequest.Create("hello", "Codex");

        queue.Enqueue(request);

        var item = queue.TryClaimNext();

        Assert.NotNull(item);
        Assert.Equal("hello", item.Request.Text);
        Assert.Empty(queue.ReadPending());
        Assert.True(File.Exists(item.ClaimedFilePath));

        queue.Complete(item);

        Assert.False(File.Exists(item.ClaimedFilePath));
    }
}
