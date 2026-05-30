namespace AudioMate.Core.Narration;

public sealed record NarrationQueueItem(NarrationRequest Request, string ClaimedFilePath);
