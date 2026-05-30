using AudioMate.Core.Music;

namespace AudioMate.Core.Tests;

public sealed class ProcessNameNormalizerTests
{
    [Theory]
    [InlineData("Spotify.exe", "spotify")]
    [InlineData("\"C:\\Program Files\\VideoLAN\\VLC\\vlc.exe\"", "vlc")]
    [InlineData("  cloudmusic  ", "cloudmusic")]
    public void Normalize_RemovesPathsExtensionsAndCase(string value, string expected)
    {
        Assert.Equal(expected, ProcessNameNormalizer.Normalize(value));
    }

    [Fact]
    public void NormalizeDistinct_RemovesDuplicatesAndBlankValues()
    {
        var normalized = ProcessNameNormalizer.NormalizeDistinct(["Spotify.exe", "spotify", "", "VLC.exe"]);

        Assert.Equal(["spotify", "vlc"], normalized);
    }

    [Fact]
    public void NormalizeDistinctPreservingOrder_RemovesDuplicatesWithoutSorting()
    {
        var normalized = ProcessNameNormalizer.NormalizeDistinctPreservingOrder(["VLC.exe", "spotify", "vlc", "CloudMusic.exe"]);

        Assert.Equal(["vlc", "spotify", "cloudmusic"], normalized);
    }
}
