namespace AudioMate.Core.Configuration;

public interface ISettingsStore
{
    AudioMateSettings Load();

    void Save(AudioMateSettings settings);
}
