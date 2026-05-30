namespace AudioMate.Core.Music;

public static class MusicPresetCatalog
{
    public static IReadOnlyList<MusicPreset> BuiltInPresets { get; } =
    [
        new("Spotify", "spotify"),
        new("NetEase Cloud Music", "cloudmusic"),
        new("QQMusic", "qqmusic"),
        new("Kugou", "kugou"),
        new("Kuwo", "kuwo"),
        new("foobar2000", "foobar2000"),
        new("MusicBee", "musicbee"),
        new("AIMP", "aimp"),
        new("PotPlayer", "potplayermini64"),
        new("VLC", "vlc"),
        new("Apple Music", "applemusic"),
    ];

    public static IReadOnlyList<string> DefaultPresetProcessNames { get; } =
        BuiltInPresets.Select(static preset => preset.ProcessName).ToArray();
}
