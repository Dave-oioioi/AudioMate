using AudioMate.App.Settings;
using AudioMate.Core.Audio;
using AudioMate.Core.Configuration;
using AudioMate.Core.Music;
using AudioMate.Core.Narration;
using System.Runtime.InteropServices;

namespace AudioMate.App.Tray;

internal sealed class AudioMateApplicationContext : ApplicationContext
{
    private readonly ISettingsStore _settingsStore;
    private readonly INarrationQueue _narrationQueue;
    private readonly IAudioCore _audioCore;
    private readonly StartupRegistrationService _startupRegistration = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly NotifyIcon _notifyIcon;
    private AudioMateSettings _settings;
    private MiniSettingsForm? _settingsForm;
    private Task? _narrationWorkerTask;
    private Task? _ordinaryAudioWorkerTask;
    private Task? _microphoneWorkerTask;

    public AudioMateApplicationContext(ISettingsStore settingsStore, INarrationQueue narrationQueue)
    {
        _settingsStore = settingsStore;
        _narrationQueue = narrationQueue;
        _settings = _settingsStore.Load();
        _audioCore = CreateAudioCore(_settings);

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadApplicationIcon(),
            Text = "AudioMate",
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };

        _notifyIcon.DoubleClick += (_, _) => ShowSettings();
        _startupRegistration.Apply(_settings.StartAtLogin);
        StartNarrationWorker();
        StartOrdinaryAudioWorker();
        StartMicrophoneWorker();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _shutdown.Cancel();
            try
            {
                Task.WaitAll(
                    [
                        _narrationWorkerTask ?? Task.CompletedTask,
                        _ordinaryAudioWorkerTask ?? Task.CompletedTask,
                        _microphoneWorkerTask ?? Task.CompletedTask,
                    ],
                    TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
                // App shutdown should not be blocked by background narration failures.
            }

            _settingsForm?.Dispose();
            _notifyIcon.Dispose();
            _shutdown.Dispose();
            (_audioCore as IDisposable)?.Dispose();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add(CreateCheckedItem("启用 AudioMate", _settings.IsEnabled, ToggleEnabled));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CreateModeMenu());
        menu.Items.Add("扫描PC应用", null, (_, _) => ScanPcApps());
        menu.Items.Add(CreateDuckVolumeMenu());
        menu.Items.Add(CreateCheckedItem("Codex 朗读", _settings.IsCodexNarrationEnabled, ToggleCodexNarration));
        menu.Items.Add(CreateCheckedItem("开机自启动", _settings.StartAtLogin, ToggleStartup));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("打开设置", null, (_, _) => ShowSettings());
        menu.Items.Add("测试朗读", null, (_, _) => QueueTestNarration());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitThread());

        return menu;
    }

    private ToolStripMenuItem CreateModeMenu()
    {
        var item = new ToolStripMenuItem("模式");

        item.DropDownItems.Add(CreateModeItem("自动压低 BGM", AudioMateMode.AutomaticDucking));
        item.DropDownItems.Add(CreateModeItem("仅 Codex 朗读压低", AudioMateMode.CodexNarrationOnly));
        item.DropDownItems.Add(CreateModeItem("暂停", AudioMateMode.Paused));

        return item;
    }

    private ToolStripMenuItem CreateDuckVolumeMenu()
    {
        var item = new ToolStripMenuItem("压低到");

        foreach (var value in new[] { 10, 20, 30 })
        {
            var volumeItem = new ToolStripMenuItem($"{value}%")
            {
                Checked = _settings.DuckVolumePercent == value,
            };
            volumeItem.Click += (_, _) => UpdateSettings(_settings with { DuckVolumePercent = value });
            item.DropDownItems.Add(volumeItem);
        }

        return item;
    }

    private ToolStripMenuItem CreateModeItem(string label, AudioMateMode mode)
    {
        var item = new ToolStripMenuItem(label)
        {
            Checked = _settings.Mode == mode,
        };
        item.Click += (_, _) => UpdateSettings(_settings with { Mode = mode });
        return item;
    }

    private static ToolStripMenuItem CreateCheckedItem(string label, bool isChecked, EventHandler onClick)
    {
        var item = new ToolStripMenuItem(label)
        {
            Checked = isChecked,
        };
        item.Click += onClick;
        return item;
    }

    private static Icon LoadApplicationIcon()
    {
        return Icon.ExtractAssociatedIcon(Application.ExecutablePath)
            ?? SystemIcons.Application;
    }

    private void ToggleEnabled(object? sender, EventArgs e)
    {
        UpdateSettings(_settings with { IsEnabled = !_settings.IsEnabled });
    }

    private void ToggleCodexNarration(object? sender, EventArgs e)
    {
        UpdateSettings(_settings with { IsCodexNarrationEnabled = !_settings.IsCodexNarrationEnabled });
    }

    private void ToggleStartup(object? sender, EventArgs e)
    {
        UpdateSettings(_settings with { StartAtLogin = !_settings.StartAtLogin });
        _startupRegistration.Apply(_settings.StartAtLogin);
        var state = _settings.StartAtLogin ? "已启用" : "已关闭";
        _notifyIcon.ShowBalloonTip(1500, "AudioMate", $"开机自启动{state}。", ToolTipIcon.Info);
    }

    private void QueueTestNarration()
    {
        _narrationQueue.Enqueue(NarrationRequest.Create("AudioMate narration queue is ready.", "AudioMate"));
        _notifyIcon.ShowBalloonTip(1500, "AudioMate", "已加入测试朗读队列。", ToolTipIcon.Info);
    }

    private void ShowSettings()
    {
        if (_settingsForm is { IsDisposed: false })
        {
            _settingsForm.Activate();
            return;
        }

        _settingsForm = new MiniSettingsForm(_settings, SaveFromSettingsForm, ScanApplicationCandidates);
        _settingsForm.Show();
    }

    private void SaveFromSettingsForm(AudioMateSettings settings)
    {
        UpdateSettings(settings);
    }

    private void UpdateSettings(AudioMateSettings settings)
    {
        _settings = settings.Normalize();
        _settingsStore.Save(_settings);
        _audioCore.ApplySettings(_settings);
        _notifyIcon.ContextMenuStrip = BuildContextMenu();
    }

    private void StartNarrationWorker()
    {
        var ttsEngine = new PowerShellAuralTtsEngine(() => _settings);
        var worker = new NarrationQueueWorker(_narrationQueue, _audioCore, ttsEngine);
        _narrationWorkerTask = Task.Run(() => worker.RunAsync(_shutdown.Token));
    }

    private void StartOrdinaryAudioWorker()
    {
        if (_audioCore is not IAudioSessionScanner scanner)
        {
            return;
        }

        var worker = new OrdinaryAudioDuckingWorker(scanner, _audioCore, () => _settings);
        _ordinaryAudioWorkerTask = Task.Run(() => worker.RunAsync(_shutdown.Token));
    }

    private void StartMicrophoneWorker()
    {
        if (_audioCore is not IMicrophoneLevelScanner scanner)
        {
            return;
        }

        var worker = new MicrophoneDuckingWorker(scanner, _audioCore, () => _settings);
        _microphoneWorkerTask = Task.Run(() => worker.RunAsync(_shutdown.Token));
    }

    private static IAudioCore CreateAudioCore(AudioMateSettings settings)
    {
        try
        {
            return new WindowsAudioCore(settings);
        }
        catch (Exception)
        {
            return new AudioCoreFacade(settings);
        }
    }

    private void ScanPcApps()
    {
        var scanCandidates = ScanApplicationCandidates();
        var candidates = FilterScanCandidates(
            scanCandidates,
            scanCandidates.Select(static candidate => candidate.ProcessName));
        if (candidates.Count == 0)
        {
            MessageBox.Show("扫描到的应用都已经在配置里。", "扫描PC应用", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new ProcessSelectionDialog("选择音乐/BGM 应用", candidates);
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var selectedProcessNames = dialog.SelectedProcessNames;
        if (selectedProcessNames.Count == 0)
        {
            _notifyIcon.ShowBalloonTip(1500, "AudioMate", "没有添加新的应用规则。", ToolTipIcon.Info);
            return;
        }

        var updatedSettings = CustomMusicTargetUpdater.AddCustomTargets(_settings, selectedProcessNames);
        UpdateSettings(updatedSettings);

        var message = $"已添加 {selectedProcessNames.Count} 个音乐/BGM 应用。";
        _notifyIcon.ShowBalloonTip(2000, "AudioMate", message, ToolTipIcon.Info);
    }

    private IReadOnlyList<ApplicationScanCandidate> ScanApplicationCandidates()
    {
        var processNames = SystemApplicationScanner.ScanProcessNames();

        if (_audioCore is not WindowsAudioCore windowsAudioCore)
        {
            return BuildApplicationScanCandidates([], processNames);
        }

        try
        {
            var audioMixerProcessNames = windowsAudioCore.ScanRenderSessions()
                .Select(static session => session.ProcessName);

            return BuildApplicationScanCandidates(audioMixerProcessNames, processNames);
        }
        catch (Exception exception) when (exception is InvalidOperationException or COMException)
        {
            return BuildApplicationScanCandidates([], processNames);
        }
    }

    private IReadOnlyList<ApplicationScanCandidate> FilterScanCandidates(
        IReadOnlyList<ApplicationScanCandidate> scanCandidates,
        IEnumerable<string> processNames)
    {
        var allowedProcessNames = new HashSet<string>(
            CustomMusicTargetUpdater.FindNewCandidates(_settings, processNames),
            StringComparer.OrdinalIgnoreCase);

        return scanCandidates
            .Where(candidate => allowedProcessNames.Contains(candidate.ProcessName))
            .ToArray();
    }

    private static IReadOnlyList<ApplicationScanCandidate> BuildApplicationScanCandidates(
        IEnumerable<string> audioMixerProcessNames,
        IEnumerable<string> pcProcessNames)
    {
        var candidates = new List<ApplicationScanCandidate>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddCandidates(audioMixerProcessNames, ApplicationScanSource.VolumeMixer);
        AddCandidates(pcProcessNames, ApplicationScanSource.PcScan);

        return candidates;

        void AddCandidates(IEnumerable<string> processNames, ApplicationScanSource source)
        {
            foreach (var processName in ProcessNameNormalizer.NormalizeDistinctPreservingOrder(processNames))
            {
                if (processName is "system" or "idle" or "audiomate" or "audiomate.app")
                {
                    continue;
                }

                if (seen.Add(processName))
                {
                    candidates.Add(new ApplicationScanCandidate(processName, source));
                }
            }
        }
    }

    private void ShowNotImplemented(string message)
    {
        _notifyIcon.ShowBalloonTip(2000, "AudioMate", message, ToolTipIcon.Info);
    }

    protected override void ExitThreadCore()
    {
        _notifyIcon.Visible = false;
        base.ExitThreadCore();
    }
}
