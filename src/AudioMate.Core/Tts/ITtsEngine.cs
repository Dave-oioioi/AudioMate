using AudioMate.Core.Narration;

namespace AudioMate.Core.Tts;

/// <summary>
/// Speaks a narration request using Edge TTS or a system fallback.
/// </summary>
public interface ITtsEngine
{
    Task SpeakAsync(NarrationRequest request, CancellationToken cancellationToken = default);
}
