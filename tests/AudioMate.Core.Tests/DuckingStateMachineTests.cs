using AudioMate.Core.Audio;
using AudioMate.Core.Configuration;

namespace AudioMate.Core.Tests;

public sealed class DuckingStateMachineTests
{
    [Fact]
    public void EnterTrigger_WhenAutomaticMode_StartsDucking()
    {
        var machine = new DuckingStateMachine(new AudioMateSettings { DuckVolumePercent = 20 });

        var snapshot = machine.EnterTrigger(AudioTriggerKind.OtherAudio);

        Assert.True(snapshot.IsDucking);
        Assert.Equal(1, snapshot.ActiveTriggerCount);
        Assert.Equal(20, snapshot.DuckVolumePercent);
    }

    [Fact]
    public void EnterTrigger_WhenCodexOnly_IgnoresOtherAudioButAllowsNarration()
    {
        var machine = new DuckingStateMachine(new AudioMateSettings
        {
            Mode = AudioMateMode.CodexNarrationOnly,
        });

        Assert.False(machine.EnterTrigger(AudioTriggerKind.OtherAudio).IsDucking);
        Assert.True(machine.EnterTrigger(AudioTriggerKind.CodexNarration).IsDucking);
    }

    [Fact]
    public void ExitTrigger_WhenLastTriggerEnds_StopsDucking()
    {
        var machine = new DuckingStateMachine(new AudioMateSettings());

        machine.EnterTrigger(AudioTriggerKind.OtherAudio);
        var snapshot = machine.ExitTrigger(AudioTriggerKind.OtherAudio);

        Assert.False(snapshot.IsDucking);
        Assert.Equal(0, snapshot.ActiveTriggerCount);
    }
}
