using System.Diagnostics;
using AudioMate.Core.Configuration;
using AudioMate.Core.Narration;
using AudioMate.Core.Tts;

namespace AudioMate.App.Settings;

internal sealed class PowerShellAuralTtsEngine(Func<AudioMateSettings> getSettings) : ITtsEngine
{
    public async Task SpeakAsync(NarrationRequest request, CancellationToken cancellationToken = default)
    {
        var settings = getSettings();
        var text = settings.Tts.CondenseResponses
            ? NarrationTextCondensor.Condense(request.Text, maxLength: 150)
            : request.Text;

        var scriptPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".codex",
            "skills",
            "speech",
            "scripts",
            "tts_speak.py");

        if (File.Exists(scriptPath))
        {
            await SpeakWithAuralPythonAsync(scriptPath, text, settings.Tts, cancellationToken).ConfigureAwait(false);
            return;
        }

        await SpeakWithSystemPowerShellAsync(text, cancellationToken).ConfigureAwait(false);
    }

    private static async Task SpeakWithAuralPythonAsync(
        string scriptPath,
        string text,
        TtsSettings settings,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "python",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("--voice");
        startInfo.ArgumentList.Add(settings.Voice);
        startInfo.ArgumentList.Add("--rate");
        startInfo.ArgumentList.Add(settings.Rate);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start Python TTS process.");

        await process.StandardInput.WriteAsync(text.AsMemory(), cancellationToken).ConfigureAwait(false);
        process.StandardInput.Close();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException($"Aural TTS failed with exit code {process.ExitCode}: {error}");
        }
    }

    private static async Task SpeakWithSystemPowerShellAsync(string text, CancellationToken cancellationToken)
    {
        var escaped = text.Replace("'", "''", StringComparison.Ordinal);
        var command = "Add-Type -AssemblyName System.Speech;"
            + "$s = New-Object System.Speech.Synthesis.SpeechSynthesizer;"
            + "$s.Speak('"
            + escaped
            + "')";

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add(command);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start system TTS process.");

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    }
}
