namespace AudioMate.Core.Audio;

public interface IAudioSessionScanner
{
    IReadOnlyList<AudioSessionInfo> ScanRenderSessions();
}
