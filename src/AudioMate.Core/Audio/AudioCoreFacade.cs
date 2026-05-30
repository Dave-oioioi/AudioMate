using AudioMate.Core.Configuration;

namespace AudioMate.Core.Audio;

public sealed class AudioCoreFacade : IAudioCore
{
    private readonly DuckingStateMachine _stateMachine;

    public AudioCoreFacade(AudioMateSettings settings)
    {
        _stateMachine = new DuckingStateMachine(settings);
    }

    public DuckingSnapshot CurrentState => _stateMachine.Snapshot;

    public void ApplySettings(AudioMateSettings settings)
    {
        _stateMachine.ApplySettings(settings);
    }

    public DuckingSnapshot EnterTrigger(AudioTriggerKind trigger)
    {
        return _stateMachine.EnterTrigger(trigger);
    }

    public DuckingSnapshot ExitTrigger(AudioTriggerKind trigger)
    {
        return _stateMachine.ExitTrigger(trigger);
    }
}
