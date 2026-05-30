namespace AudioMate.App.Settings;

internal sealed class ProcessSelectionDialog : Form
{
    private readonly IReadOnlyList<ApplicationScanCandidate> _candidates;
    private readonly HashSet<string> _selectedProcessNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly CheckedListBox _processList = new();
    private readonly TextBox _searchInput = new();
    private bool _isRefreshing;

    public ProcessSelectionDialog(string title, IReadOnlyList<ApplicationScanCandidate> candidates)
    {
        _candidates = candidates;
        Text = title;
        Width = 460;
        Height = 560;
        MinimizeBox = false;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        Controls.Add(BuildLayout());
        RefreshProcessList();
    }

    public IReadOnlyList<string> SelectedProcessNames =>
        _candidates
            .Select(static candidate => candidate.ProcessName)
            .Where(_selectedProcessNames.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private Control BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 1,
            RowCount = 4,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        layout.Controls.Add(new Label
        {
            Text = "优先勾选音量合成器中的应用：",
            AutoSize = true,
        }, 0, 0);

        _searchInput.Dock = DockStyle.Fill;
        _searchInput.PlaceholderText = "搜索应用/进程名，例如 chrome、spotify、wechat";
        _searchInput.TextChanged += (_, _) => RefreshProcessList();
        layout.Controls.Add(_searchInput, 0, 1);

        _processList.CheckOnClick = true;
        _processList.Dock = DockStyle.Fill;
        _processList.IntegralHeight = false;
        _processList.ItemCheck += ProcessListItemCheck;

        layout.Controls.Add(_processList, 0, 2);
        layout.Controls.Add(BuildButtons(), 0, 3);
        return layout;
    }

    private void RefreshProcessList()
    {
        var query = _searchInput.Text.Trim();
        var filteredCandidates = _candidates
            .Where(candidate =>
                string.IsNullOrWhiteSpace(query)
                || candidate.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || candidate.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        _isRefreshing = true;
        _processList.BeginUpdate();
        _processList.Items.Clear();
        foreach (var candidate in filteredCandidates)
        {
            var index = _processList.Items.Add(candidate);
            _processList.SetItemChecked(index, _selectedProcessNames.Contains(candidate.ProcessName));
        }

        _processList.EndUpdate();
        _isRefreshing = false;
    }

    private void ProcessListItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var processName = ((ApplicationScanCandidate)_processList.Items[e.Index]).ProcessName;
        if (e.NewValue == CheckState.Checked)
        {
            _selectedProcessNames.Add(processName);
            return;
        }

        _selectedProcessNames.Remove(processName);
    }

    private Control BuildButtons()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0),
        };

        var okButton = new Button
        {
            Text = "添加",
            Width = 92,
            DialogResult = DialogResult.OK,
        };
        var cancelButton = new Button
        {
            Text = "取消",
            Width = 92,
            DialogResult = DialogResult.Cancel,
        };

        AcceptButton = okButton;
        CancelButton = cancelButton;

        panel.Controls.Add(okButton);
        panel.Controls.Add(cancelButton);
        return panel;
    }
}
