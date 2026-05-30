namespace AudioMate.App.Settings;

internal sealed record ApplicationScanCandidate(string ProcessName, ApplicationScanSource Source)
{
    public override string ToString()
    {
        var sourceLabel = Source is ApplicationScanSource.VolumeMixer
            ? "音量合成器"
            : "PC应用";

        return $"{ProcessName}  [{sourceLabel}]";
    }
}
