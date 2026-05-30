using AudioMate.Core.Configuration;
using AudioMate.Core.Music;

namespace AudioMate.Core.Tests;

public sealed class CustomMusicTargetUpdaterTests
{
    [Fact]
    public void FindNewCandidates_ExcludesKnownTargetsAndAudioMate()
    {
        var settings = new AudioMateSettings
        {
            EnabledBuiltInPresets = ["spotify"],
            CustomMusicProcessNames = ["vlc"],
            IgnoredTriggerProcessNames = ["teams"],
        };

        var candidates = CustomMusicTargetUpdater.FindNewCandidates(
            settings,
            ["Spotify.exe", "AudioMate.App", "VLC", "Teams.exe", "MyPlayer.exe"]);

        Assert.Equal(["myplayer"], candidates);
    }

    [Fact]
    public void FindNewCandidates_PreservesScanPriority()
    {
        var candidates = CustomMusicTargetUpdater.FindNewCandidates(
            new AudioMateSettings(),
            ["MixerFirst.exe", "PcScanSecond.exe", "AnotherApp.exe"]);

        Assert.Equal(["mixerfirst", "pcscansecond", "anotherapp"], candidates);
    }

    [Fact]
    public void AddCustomTargets_NormalizesAndMergesDistinctTargets()
    {
        var settings = new AudioMateSettings
        {
            CustomMusicProcessNames = ["vlc"],
        };

        var updated = CustomMusicTargetUpdater.AddCustomTargets(settings, ["VLC.exe", "MyPlayer.exe"]);

        Assert.Equal(["myplayer", "vlc"], updated.CustomMusicProcessNames);
    }

    [Fact]
    public void AddIgnoredTriggers_NormalizesAndMergesDistinctTriggers()
    {
        var settings = new AudioMateSettings
        {
            IgnoredTriggerProcessNames = ["teams"],
        };

        var updated = CustomMusicTargetUpdater.AddIgnoredTriggers(settings, ["Teams.exe", "Zoom.exe"]);

        Assert.Equal(["teams", "zoom"], updated.IgnoredTriggerProcessNames);
    }
}
