namespace AudioMate.Core.Narration;

public interface INarrationQueue
{
    void Enqueue(NarrationRequest request);

    IReadOnlyList<NarrationRequest> ReadPending();

    NarrationQueueItem? TryClaimNext();

    void Complete(NarrationQueueItem item);
}
