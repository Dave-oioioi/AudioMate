using System.Text.Json;

namespace AudioMate.Core.Configuration;

/// <summary>
/// Persists AudioMate settings as JSON and recovers quietly from corrupted files.
/// </summary>
public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public AudioMateSettings Load()
    {
        if (!File.Exists(filePath))
        {
            var defaults = new AudioMateSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<AudioMateSettings>(json, JsonOptions);
            return (settings ?? new AudioMateSettings()).Normalize();
        }
        catch (JsonException)
        {
            BackupCorruptedConfig();
            var defaults = new AudioMateSettings();
            Save(defaults);
            return defaults;
        }
    }

    public void Save(AudioMateSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

        var normalized = settings.Normalize();
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    private void BackupCorruptedConfig()
    {
        var backupPath = $"{filePath}.corrupt-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        File.Move(filePath, backupPath, overwrite: true);
    }
}
