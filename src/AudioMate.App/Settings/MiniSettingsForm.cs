using AudioMate.Core.Configuration;
using AudioMate.Core.Music;

namespace AudioMate.App.Settings;

internal sealed class MiniSettingsForm : Form
{
    private readonly Action<AudioMateSettings> _save;
    private readonly Func<IReadOnlyList<ApplicationScanCandidate>> _scanCandidates;
    private readonly CheckedListBox _presetList = new();
    private readonly ListBox _customTargetList = new();
    private readonly ListBox _ignoredTriggerList = new();
    private readonly NumericUpDown _duckVolume = new();
    private readonly NumericUpDown _restoreDelay = new();
    private readonly NumericUpDown _fadeOutDuration = new();
    private readonly NumericUpDown _fadeInDuration = new();
    private readonly NumericUpDown _triggerThreshold = new();
    private readonly NumericUpDown _microphoneThreshold = new();
    private readonly CheckBox _microphoneTrigger = new();
    private readonly CheckBox _codexNarration = new();
    private readonly CheckBox _fade = new();
    private AudioMateSettings _settings;

    public MiniSettingsForm(
        AudioMateSettings settings,
        Action<AudioMateSettings> save,
        Func<IReadOnlyList<ApplicationScanCandidate>> scanCandidates)
    {
        _settings = settings.Normalize();
        _save = save;
        _scanCandidates = scanCandidates;

        Text = "AudioMate 设置";
        Width = 720;
        Height = 620;
        MinimumSize = new Size(680, 560);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;

        Controls.Add(BuildLayout());
        LoadValues();
    }

    private Control BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 1,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
        };
        tabs.TabPages.Add(BuildMusicPage());
        tabs.TabPages.Add(BuildIgnorePage());
        tabs.TabPages.Add(BuildAudioPage());
        tabs.TabPages.Add(BuildCodexPage());

        layout.Controls.Add(tabs, 0, 0);
        layout.Controls.Add(BuildButtons(), 0, 1);

        return layout;
    }

    private TabPage BuildMusicPage()
    {
        var page = new TabPage("音乐/BGM 应用")
        {
            Padding = new Padding(14),
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreateSectionLabel("内置预设"), 0, 0);
        layout.Controls.Add(CreateSectionLabel("自定义应用"), 1, 0);

        _presetList.CheckOnClick = true;
        _presetList.Dock = DockStyle.Fill;
        _presetList.IntegralHeight = false;
        foreach (var preset in MusicPresetCatalog.BuiltInPresets)
        {
            _presetList.Items.Add(preset.DisplayName);
        }

        _customTargetList.Dock = DockStyle.Fill;
        _customTargetList.IntegralHeight = false;

        var customPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        customPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        customPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        customPanel.Controls.Add(_customTargetList, 0, 0);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
        };

        var removeButton = new Button
        {
            Text = "移除选中",
            Width = 110,
        };
        removeButton.Click += (_, _) => RemoveSelectedCustomTarget();
        var scanButton = new Button
        {
            Text = "扫描PC应用",
            Width = 110,
        };
        scanButton.Click += (_, _) => ScanIntoCustomTargets();
        actionPanel.Controls.Add(removeButton);
        actionPanel.Controls.Add(scanButton);
        customPanel.Controls.Add(actionPanel, 0, 1);

        layout.Controls.Add(_presetList, 0, 1);
        layout.Controls.Add(customPanel, 1, 1);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildIgnorePage()
    {
        var page = new TabPage("排除应用")
        {
            Padding = new Padding(14),
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        layout.Controls.Add(new Label
        {
            Text = "这些应用发声时不会触发 BGM 压低。适合会议软件、系统提示音、你不想影响音乐的程序。",
            AutoSize = false,
            Dock = DockStyle.Fill,
        }, 0, 0);

        _ignoredTriggerList.Dock = DockStyle.Fill;
        _ignoredTriggerList.IntegralHeight = false;
        layout.Controls.Add(_ignoredTriggerList, 0, 1);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
        };

        var removeButton = new Button
        {
            Text = "移除选中",
            Width = 110,
        };
        removeButton.Click += (_, _) => RemoveSelectedIgnoredTrigger();
        var scanButton = new Button
        {
            Text = "扫描PC应用",
            Width = 110,
        };
        scanButton.Click += (_, _) => ScanIntoIgnoredTriggers();
        actionPanel.Controls.Add(removeButton);
        actionPanel.Controls.Add(scanButton);
        layout.Controls.Add(actionPanel, 0, 2);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildAudioPage()
    {
        var page = new TabPage("音频行为")
        {
            Padding = new Padding(18),
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 9,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        ConfigurePercentInput(_duckVolume, minimum: 1, maximum: 100, increment: 1);
        ConfigureMillisecondsInput(_restoreDelay, maximum: 10000, increment: 100);
        ConfigureMillisecondsInput(_fadeOutDuration, maximum: 3000, increment: 50);
        ConfigureMillisecondsInput(_fadeInDuration, maximum: 3000, increment: 50);
        ConfigureThresholdInput(_triggerThreshold);
        ConfigureThresholdInput(_microphoneThreshold);

        _fade.Text = "启用淡入淡出";
        _fade.AutoSize = true;
        _microphoneTrigger.Text = "启用麦克风触发";
        _microphoneTrigger.AutoSize = true;

        AddSettingRow(layout, 0, "BGM 压低到", BuildInputWithUnit(_duckVolume, "%"));
        AddSettingRow(layout, 1, "触发阈值", _triggerThreshold);
        AddSettingRow(layout, 2, "恢复延迟", BuildInputWithUnit(_restoreDelay, "ms"));
        layout.Controls.Add(_fade, 1, 3);
        AddSettingRow(layout, 4, "压低渐变", BuildInputWithUnit(_fadeOutDuration, "ms"));
        AddSettingRow(layout, 5, "恢复渐变", BuildInputWithUnit(_fadeInDuration, "ms"));
        layout.Controls.Add(_microphoneTrigger, 1, 6);
        AddSettingRow(layout, 7, "麦克风阈值", _microphoneThreshold);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildCodexPage()
    {
        var page = new TabPage("Codex 朗读")
        {
            Padding = new Padding(18),
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3,
        };

        _codexNarration.Text = "启用 Codex 朗读";
        _codexNarration.AutoSize = true;
        layout.Controls.Add(_codexNarration, 0, 0);
        layout.Controls.Add(new Label
        {
            Text = "AudioMate 关闭时，Aural 仍会使用独立朗读方案。",
            AutoSize = true,
            Margin = new Padding(0, 14, 0, 0),
        }, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private Control BuildButtons()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 0, 0),
        };

        var saveButton = new Button
        {
            Text = "保存",
            Width = 104,
            Height = 32,
        };
        saveButton.Click += (_, _) => SaveAndClose();

        var cancelButton = new Button
        {
            Text = "取消",
            Width = 104,
            Height = 32,
        };
        cancelButton.Click += (_, _) => Close();

        panel.Controls.Add(saveButton);
        panel.Controls.Add(cancelButton);
        return panel;
    }

    private void LoadValues()
    {
        var enabledPresets = new HashSet<string>(_settings.EnabledBuiltInPresets, StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < MusicPresetCatalog.BuiltInPresets.Count; index++)
        {
            var preset = MusicPresetCatalog.BuiltInPresets[index];
            _presetList.SetItemChecked(index, enabledPresets.Contains(preset.ProcessName));
        }

        _customTargetList.Items.Clear();
        foreach (var processName in _settings.CustomMusicProcessNames)
        {
            _customTargetList.Items.Add(processName);
        }

        _ignoredTriggerList.Items.Clear();
        foreach (var processName in _settings.IgnoredTriggerProcessNames)
        {
            _ignoredTriggerList.Items.Add(processName);
        }

        _duckVolume.Value = _settings.DuckVolumePercent;
        _triggerThreshold.Value = Convert.ToDecimal(_settings.TriggerThreshold);
        _restoreDelay.Value = _settings.RestoreDelayMilliseconds;
        _fade.Checked = _settings.Fade.IsEnabled;
        _fadeOutDuration.Value = _settings.Fade.FadeOutMilliseconds;
        _fadeInDuration.Value = _settings.Fade.FadeInMilliseconds;
        _microphoneTrigger.Checked = _settings.IsMicrophoneTriggerEnabled;
        _microphoneThreshold.Value = Convert.ToDecimal(_settings.MicrophoneThreshold);
        _codexNarration.Checked = _settings.IsCodexNarrationEnabled;
    }

    private void SaveAndClose()
    {
        var enabledPresetNames = _presetList.CheckedIndices
            .Cast<int>()
            .Select(static index => MusicPresetCatalog.BuiltInPresets[index].ProcessName)
            .ToArray();

        var customTargets = _customTargetList.Items
            .Cast<string>()
            .ToArray();
        var ignoredTriggers = _ignoredTriggerList.Items
            .Cast<string>()
            .ToArray();

        _settings = _settings with
        {
            EnabledBuiltInPresets = enabledPresetNames,
            CustomMusicProcessNames = customTargets,
            IgnoredTriggerProcessNames = ignoredTriggers,
            DuckVolumePercent = Convert.ToInt32(_duckVolume.Value),
            TriggerThreshold = Convert.ToDouble(_triggerThreshold.Value),
            MicrophoneThreshold = Convert.ToDouble(_microphoneThreshold.Value),
            RestoreDelayMilliseconds = Convert.ToInt32(_restoreDelay.Value),
            Fade = _settings.Fade with
            {
                IsEnabled = _fade.Checked,
                FadeOutMilliseconds = Convert.ToInt32(_fadeOutDuration.Value),
                FadeInMilliseconds = Convert.ToInt32(_fadeInDuration.Value),
            },
            IsMicrophoneTriggerEnabled = _microphoneTrigger.Checked,
            IsCodexNarrationEnabled = _codexNarration.Checked,
        };

        _save(_settings);
        Close();
    }

    private void RemoveSelectedCustomTarget()
    {
        var index = _customTargetList.SelectedIndex;
        if (index >= 0)
        {
            _customTargetList.Items.RemoveAt(index);
        }
    }

    private void RemoveSelectedIgnoredTrigger()
    {
        var index = _ignoredTriggerList.SelectedIndex;
        if (index >= 0)
        {
            _ignoredTriggerList.Items.RemoveAt(index);
        }
    }

    private void ScanIntoCustomTargets()
    {
        var candidates = FindScanCandidates(
            _customTargetList.Items.Cast<string>()
                .Concat(_ignoredTriggerList.Items.Cast<string>()));

        AddFromSelectionDialog("选择音乐/BGM 应用", candidates, _customTargetList);
    }

    private void ScanIntoIgnoredTriggers()
    {
        var enabledPresetNames = _presetList.CheckedIndices
            .Cast<int>()
            .Select(static index => MusicPresetCatalog.BuiltInPresets[index].ProcessName);
        var candidates = FindScanCandidates(
            _ignoredTriggerList.Items.Cast<string>()
                .Concat(_customTargetList.Items.Cast<string>())
                .Concat(enabledPresetNames));

        AddFromSelectionDialog("选择排除应用", candidates, _ignoredTriggerList);
    }

    private IReadOnlyList<ApplicationScanCandidate> FindScanCandidates(IEnumerable<string> excludedProcessNames)
    {
        var excluded = new HashSet<string>(
            ProcessNameNormalizer.NormalizeDistinct(excludedProcessNames),
            StringComparer.OrdinalIgnoreCase);

        return _scanCandidates()
            .Where(candidate => !excluded.Contains(candidate.ProcessName))
            .Where(static candidate => candidate.ProcessName is not "system" and not "audiomate" and not "audiomate.app")
            .ToArray();
    }

    private static void AddFromSelectionDialog(
        string title,
        IReadOnlyList<ApplicationScanCandidate> candidates,
        ListBox targetList)
    {
        if (candidates.Count == 0)
        {
            MessageBox.Show("没有可添加的应用或进程。", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new ProcessSelectionDialog(title, candidates);
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        foreach (var processName in dialog.SelectedProcessNames)
        {
            AddProcessToList(targetList, processName);
        }
    }

    private static void AddProcessToList(ListBox targetList, string processName)
    {
        var normalized = ProcessNameNormalizer.Normalize(processName);
        var existing = targetList.Items
            .Cast<string>()
            .Any(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase));

        if (!existing)
        {
            targetList.Items.Add(normalized);
        }
    }

    private static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.MessageBoxFont ?? Control.DefaultFont, FontStyle.Bold),
        };
    }

    private static void AddSettingRow(TableLayoutPanel layout, int row, string label, Control control)
    {
        control.Margin = new Padding(0, 4, 0, 10);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static Control BuildInputWithUnit(Control input, string unit)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        panel.Controls.Add(input);
        panel.Controls.Add(new Label
        {
            Text = unit,
            AutoSize = true,
            Margin = new Padding(8, 7, 0, 0),
        });

        return panel;
    }

    private static void ConfigurePercentInput(NumericUpDown input, int minimum, int maximum, int increment)
    {
        input.Minimum = minimum;
        input.Maximum = maximum;
        input.Increment = increment;
        input.Width = 120;
    }

    private static void ConfigureMillisecondsInput(NumericUpDown input, int maximum, int increment)
    {
        input.Minimum = 0;
        input.Maximum = maximum;
        input.Increment = increment;
        input.Width = 120;
    }

    private static void ConfigureThresholdInput(NumericUpDown input)
    {
        input.Minimum = 0;
        input.Maximum = 1;
        input.DecimalPlaces = 2;
        input.Increment = 0.01M;
        input.Width = 120;
    }
}
