using AudioMate.Core.Configuration;
using AudioMate.Core.Music;

namespace AudioMate.Core.Tests;

public sealed class MusicTargetMatcherTests
{
    [Fact]
    public void IsMusicTarget_MatchesBuiltInAndCustomProcesses()
    {
        var settings = new AudioMateSettings
        {
            EnabledBuiltInPresets = ["spotify"],
            CustomMusicProcessNames = ["myplayer.exe"],
        };
        var matcher = new MusicTargetMatcher(settings);

        Assert.True(matcher.IsMusicTarget("Spotify.exe"));
        Assert.True(matcher.IsMusicTarget("MYPLAYER"));
        Assert.False(matcher.IsMusicTarget("notepad"));
    }

    [Fact]
    public void IsIgnoredTrigger_MatchesIgnoredProcesses()
    {
        var settings = new AudioMateSettings
        {
            IgnoredTriggerProcessNames = ["teams.exe"],
        };
        var matcher = new MusicTargetMatcher(settings);

        Assert.True(matcher.IsIgnoredTrigger("Teams"));
    }
}
