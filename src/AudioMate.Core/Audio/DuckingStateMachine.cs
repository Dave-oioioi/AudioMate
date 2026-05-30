using AudioMate.Core.Configuration;

namespace AudioMate.Core.Audio;

public sealed class DuckingStateMachine
{
    private readonly HashSet<AudioTriggerKind> _activeTriggers = [];
    private AudioMateSettings _settings;

    public DuckingStateMachine(AudioMateSettings settings)
    {
        _settings = settings.Normalize();
    }

    public DuckingSnapshot Snapshot => new(
        IsDucking: _settings.IsEnabled
            && _settings.Mode is not AudioMateMode.Paused
            && _activeTriggers.Count > 0
            && AllowsCurrentTriggers(),
        ActiveTriggerCount: _activeTriggers.Count,
        DuckVolumePercent: _settings.DuckVolumePercent,
        RestoreDelay: TimeSpan.FromMilliseconds(_settings.RestoreDelayMilliseconds));

    public void ApplySettings(AudioMateSettings settings)
    {
        _settings = settings.Normalize();
    }

    public DuckingSnapshot EnterTrigger(AudioTriggerKind trigger)
    {
        if (trigger is AudioTriggerKind.Microphone && !_settings.IsMicrophoneTriggerEnabled)
        {
            return Snapshot;
        }

        _activeTriggers.Add(trigger);
        return Snapshot;
    }

    public DuckingSnapshot ExitTrigger(AudioTriggerKind trigger)
    {
        _activeTriggers.Remove(trigger);
        return Snapshot;
    }

    public DuckingSnapshot Clear()
    {
        _activeTriggers.Clear();
        return Snapshot;
    }

    private bool AllowsCurrentTriggers()
    {
        return _settings.Mode is AudioMateMode.AutomaticDucking
            || _activeTriggers.Contains(AudioTriggerKind.CodexNarration);
    }
}
