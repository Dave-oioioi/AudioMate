using AudioMate.Core.Configuration;

namespace AudioMate.Core.Tests;

public sealed class JsonSettingsStoreTests
{
    [Fact]
    public void Load_WhenMissing_CreatesDefaultConfig()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "config.json");
        var store = new JsonSettingsStore(path);

        var settings = store.Load();

        Assert.True(settings.IsEnabled);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Load_WhenCorrupted_BacksUpAndReturnsDefaults()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "config.json");
        File.WriteAllText(path, "{not json");
        var store = new JsonSettingsStore(path);

        var settings = store.Load();

        Assert.True(settings.IsEnabled);
        Assert.Contains(Directory.EnumerateFiles(temp.Path), file => file.Contains(".corrupt-", StringComparison.Ordinal));
    }
}
