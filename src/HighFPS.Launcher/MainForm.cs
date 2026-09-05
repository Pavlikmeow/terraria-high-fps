using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace TerrariaHighFPS.Launcher
{
    internal sealed class MainForm : Form
    {
        private static readonly Color Ink = Color.FromArgb(29, 36, 32);
        private static readonly Color Muted = Color.FromArgb(93, 103, 98);
        private static readonly Color Accent = Color.FromArgb(21, 105, 70);
        private static readonly Color Canvas = Color.FromArgb(245, 247, 246);
        private readonly TableLayoutPanel _layout;
        private readonly Label _hero;
        private readonly Label _subtitle;
        private readonly Label _compatibility;
        private readonly Label _statusLabel;
        private readonly Label _statusHint;
        private readonly Label _folderCaption;
        private readonly TextBox _path;
        private readonly ComboBox _language;
        private readonly Button _primary;
        private readonly Button _installOnly;
        private readonly Button _browse;
        private readonly Button _openFolder;
        private readonly Button _remove;
        private readonly Label _originalTitle;
        private readonly Label _originalBody;
        private readonly Label _privacy;
        private readonly Button _help;
        private readonly Button _detailsToggle;
        private readonly Button _copy;
        private readonly TextBox _details;
        private readonly Panel _detailsPanel;
        private readonly StringBuilder _activity = new StringBuilder();
        private readonly bool _preview;
        private readonly ToolTip _toolTip;
        private Localization _text;
        private string _gameDirectory;
        private bool _installed;
        private bool _busy;
        private bool _changingLanguage;
        private bool _detailsVisible;
        private string _statusKey;
        private string _hintKey;
        private Color _statusColor = Accent;

        public MainForm() : this(null, false) { }

        // EN: Preview construction has no detection, writes or game actions; used by the UI smoke harness.
        // RU: Режим предпросмотра не ищет игру, не пишет файлы и не запускает её; он нужен для UI-проверок.
        internal MainForm(string languageCode, bool preview)
        {
            _preview = preview;
            _text = new Localization(languageCode ?? LauncherSettings.LoadLanguage());
            Text = "High FPS Support · Terraria";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(980, 760);
            MinimumSize = new Size(900, 740);
            Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Canvas;
            ForeColor = Ink;
            DoubleBuffered = true;
            _toolTip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 350, ReshowDelay = 100 };

            var viewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Canvas };
            Controls.Add(viewport);
            _layout = new TableLayoutPanel {
                Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1, RowCount = 7, Padding = new Padding(32, 24, 32, 20),
                BackColor = Canvas, Margin = Padding.Empty
            };
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int height in new[] { 68, 126, 294, 104, 44, 0, 40 })
                _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            viewport.Controls.Add(_layout);

            var header = Row(2);
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 322));
            var brand = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Margin = Padding.Empty };
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            brand.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            brand.Controls.Add(Label("HIGH FPS SUPPORT", 17, FontStyle.Bold, Ink), 0, 0);
            brand.Controls.Add(Label("by pavlikmeow  /  v1.1.0", 9, FontStyle.Regular, Muted), 0, 1);
            header.Controls.Add(brand, 0, 0);
            var languageArea = Row(2);
            languageArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 114));
            languageArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var languageLabel = Label("Language / Язык", 9, FontStyle.Regular, Muted);
            languageLabel.TextAlign = ContentAlignment.TopLeft;
            languageLabel.Padding = new Padding(0, 8, 0, 0);
            languageArea.Controls.Add(languageLabel, 0, 0);
            _language = new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top,
                IntegralHeight = true, MaxDropDownItems = 7, Margin = new Padding(0, 2, 0, 0),
                Font = new Font("Segoe UI", 10), AccessibleName = "Language / Язык", TabIndex = 0
            };
            foreach (LanguageOption option in Localization.Languages) _language.Items.Add(option);
            languageArea.Controls.Add(_language, 1, 0);
            header.Controls.Add(languageArea, 1, 0);
            _layout.Controls.Add(header, 0, 0);

            var heroArea = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
            heroArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            heroArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            heroArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            heroArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _hero = Label("", 29, FontStyle.Bold, Ink);
            _subtitle = Label("", 10, FontStyle.Regular, Muted);
            _compatibility = Label("", 9, FontStyle.Regular, Muted);
            heroArea.Controls.Add(_hero, 0, 0);
            heroArea.Controls.Add(_subtitle, 0, 1);
            heroArea.Controls.Add(_compatibility, 0, 2);
            _layout.Controls.Add(heroArea, 0, 1);

            var card = new SurfacePanel { Dock = DockStyle.Fill, Padding = new Padding(22, 16, 22, 16), Margin = Padding.Empty };
            var cardLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1, Margin = Padding.Empty, BackColor = Color.White };
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int height in new[] { 30, 40, 22, 40, 14, 52, 38 }) cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            _statusLabel = Label("", 15, FontStyle.Bold, Accent);
            _statusHint = Label("", 10, FontStyle.Regular, Muted);
            _folderCaption = Label("", 8, FontStyle.Bold, Muted);
            cardLayout.Controls.Add(_statusLabel, 0, 0);
            cardLayout.Controls.Add(_statusHint, 0, 1);
            cardLayout.Controls.Add(_folderCaption, 0, 2);
            var locationRow = Row(2);
            locationRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            locationRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 188));
            var pathFrame = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 9, 10, 5), BackColor = Canvas, Margin = new Padding(0, 0, 10, 0) };
            _path = new TextBox {
                Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Canvas,
                ForeColor = Ink, Font = new Font("Segoe UI", 10), TabIndex = 1
            };
            pathFrame.Controls.Add(_path);
            locationRow.Controls.Add(pathFrame, 0, 0);
            _browse = Button(false);
            _browse.TabIndex = 2;
            _browse.Click += BrowseClicked;
            locationRow.Controls.Add(_browse, 1, 0);
            cardLayout.Controls.Add(locationRow, 0, 3);
            var actions = Row(2);
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
            _primary = Button(true);
            _primary.Margin = new Padding(0, 0, 10, 0);
            _primary.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            _primary.TabIndex = 3;
            _primary.Click += delegate { Install(true); };
            _installOnly = Button(false);
            _installOnly.TabIndex = 4;
            _installOnly.Click += delegate { Install(false); };
            actions.Controls.Add(_primary, 0, 0);
            actions.Controls.Add(_installOnly, 1, 0);
            cardLayout.Controls.Add(actions, 0, 5);
            var utilities = Row(2);
            utilities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
            utilities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
            _openFolder = LinkButton();
            _openFolder.TabIndex = 5;
            _openFolder.Click += OpenFolderClicked;
            _remove = LinkButton();
            _remove.ForeColor = Color.FromArgb(144, 55, 47);
            _remove.TabIndex = 6;
            _remove.Click += RemoveClicked;
            utilities.Controls.Add(_openFolder, 0, 0);
            utilities.Controls.Add(_remove, 1, 0);
            cardLayout.Controls.Add(utilities, 0, 6);
            card.Controls.Add(cardLayout);
            _layout.Controls.Add(card, 0, 2);

            var assurance = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(0, 18, 0, 0), Margin = Padding.Empty };
            assurance.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            assurance.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            assurance.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            assurance.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _originalTitle = Label("", 10, FontStyle.Bold, Ink);
            _originalBody = Label("", 9, FontStyle.Regular, Muted);
            _privacy = Label("", 9, FontStyle.Regular, Accent);
            assurance.Controls.Add(_originalTitle, 0, 0);
            assurance.Controls.Add(_originalBody, 0, 1);
            assurance.Controls.Add(_privacy, 0, 2);
            _layout.Controls.Add(assurance, 0, 3);

            var links = Row(3);
            links.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            links.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            links.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            _help = LinkButton();
            _help.TextAlign = ContentAlignment.MiddleLeft;
            _help.TabIndex = 7;
            _help.Click += delegate { ShowHelp(); };
            _detailsToggle = LinkButton();
            _detailsToggle.TabIndex = 8;
            _detailsToggle.Click += delegate { SetDetailsVisible(!_detailsVisible); };
            _copy = LinkButton();
            _copy.TextAlign = ContentAlignment.MiddleRight;
            _copy.TabIndex = 9;
            _copy.Click += CopyClicked;
            links.Controls.Add(_help, 0, 0);
            links.Controls.Add(_detailsToggle, 1, 0);
            links.Controls.Add(_copy, 2, 0);
            _layout.Controls.Add(links, 0, 4);
            _detailsPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(1), BackColor = Color.FromArgb(216, 223, 218), Visible = false };
            _details = new TextBox {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Muted,
                Font = new Font("Segoe UI", 9), TabIndex = 10
            };
            _detailsPanel.Controls.Add(_details);
            _layout.Controls.Add(_detailsPanel, 0, 5);
            var footer = Label("pavlikmeow  ·  High FPS Support 1.1.0", 8, FontStyle.Regular, Muted);
            footer.TextAlign = ContentAlignment.BottomLeft;
            _layout.Controls.Add(footer, 0, 6);

            AcceptButton = _primary;
            _language.SelectedIndexChanged += LanguageChanged;
            ApplyLanguage();
            SetStatus("missing", "missingHint", Accent);
            RefreshActions();
            if (!_preview) Shown += delegate { DetectGame(); };
            FormClosing += FormClosingWhileBusy;
        }

        private static TableLayoutPanel Row(int columns)
        {
            // EN: A proportional row must fit its parent; AutoSize can retain a nested panel's 100px default.
            // RU: Строка должна помещаться в родителе; AutoSize может сохранить стандартные 100px панели.
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = 1, Margin = Padding.Empty };
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return row;
        }

        private static Label Label(string text, float size, FontStyle style, Color color)
        {
            return new Label {
                Dock = DockStyle.Fill, Text = text, Font = new Font("Segoe UI", size, style),
                ForeColor = color, Margin = Padding.Empty, UseMnemonic = false, AutoEllipsis = false
            };
        }

        private static Button Button(bool primary)
        {
            var button = new Button {
                Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false,
                BackColor = primary ? Accent : Color.White, ForeColor = primary ? Color.White : Ink,
                Cursor = Cursors.Hand, Margin = Padding.Empty, UseMnemonic = false, AutoEllipsis = false
            };
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(207, 216, 211);
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(17, 87, 57) : Canvas;
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(13, 70, 46) : Color.FromArgb(230, 235, 232);
            return button;
        }

        private static Button LinkButton()
        {
            Button button = Button(false);
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.Transparent;
            button.ForeColor = Muted;
            button.Font = new Font("Segoe UI", 9);
            return button;
        }

        private void ApplyLanguage()
        {
            SuspendLayout();
            _changingLanguage = true;
            for (int i = 0; i < _language.Items.Count; i++)
                if (((LanguageOption)_language.Items[i]).Code == _text.Code) _language.SelectedIndex = i;
            _changingLanguage = false;
            _hero.Text = _text["hero"];
            _subtitle.Text = _text["subtitle"];
            _compatibility.Text = _text["compatibility"];
            _folderCaption.Text = _text["folder"];
            _path.Text = _gameDirectory ?? _text["noFolder"];
            _path.AccessibleName = _text["folder"];
            _browse.Text = _text["browse"];
            _primary.Text = _text[_installed ? "play" : "installPlay"];
            _installOnly.Text = _text["installOnly"];
            _openFolder.Text = _text["openFolder"];
            _remove.Text = _text["remove"];
            _originalTitle.Text = _text["originalTitle"];
            _originalBody.Text = _text["originalBody"];
            _privacy.Text = _text["privacy"];
            _help.Text = _text["help"];
            _detailsToggle.Text = _text[_detailsVisible ? "hideDetails" : "details"];
            _copy.Text = _text["copy"];
            _details.AccessibleName = _text["details"];
            _toolTip.SetToolTip(_path, _gameDirectory ?? _text["missingHint"]);
            _toolTip.SetToolTip(_primary, _text[_installed ? "installedHint" : "readyHint"]);
            _toolTip.SetToolTip(_browse, _text["chooseDescription"]);
            RefreshDetails();
            if (_statusKey != null) SetStatus(_statusKey, _hintKey, _statusColor);
            ResumeLayout(true);
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            if (_changingLanguage || _language.SelectedItem == null) return;
            _text = new Localization(((LanguageOption)_language.SelectedItem).Code);
            ApplyLanguage();
            if (!_preview && !LauncherSettings.SaveLanguage(_text.Code)) Append(_text["settingsFailed"]);
        }

        private void DetectGame()
        {
            RunAction("detecting", "detectHint", delegate { return GameLocator.Find(); }, delegate(object result) {
                SetGameDirectory(result as string);
            });
        }

        private void SetGameDirectory(string directory)
        {
            _gameDirectory = GameLocator.IsTerrariaDirectory(directory) ? Path.GetFullPath(directory) : null;
            _installed = _gameDirectory != null && LauncherEngine.IsInstalled(_gameDirectory);
            if (_gameDirectory != null && !_preview)
            {
                try { GameLocator.Save(_gameDirectory); }
                catch (IOException ex) { Append(_text["settingsFailed"] + Environment.NewLine + ex.Message); }
                catch (UnauthorizedAccessException ex) { Append(_text["settingsFailed"] + Environment.NewLine + ex.Message); }
                catch (SecurityException ex) { Append(_text["settingsFailed"] + Environment.NewLine + ex.Message); }
            }
            ApplyLanguage();
            RefreshReadyStatus();
            RefreshActions();
        }

        private void RefreshReadyStatus()
        {
            SetStatus(_gameDirectory == null ? "missing" : (_installed ? "installed" : "ready"),
                _gameDirectory == null ? "missingHint" : (_installed ? "installedHint" : "readyHint"), Accent);
        }

        private void BrowseClicked(object sender, EventArgs e)
        {
            if (_busy || _preview) return;
            using (var dialog = new FolderBrowserDialog {
                Description = _text["chooseDescription"], ShowNewFolderButton = false, SelectedPath = _gameDirectory ?? ""
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (!GameLocator.IsTerrariaDirectory(dialog.SelectedPath))
                {
                    MessageBox.Show(this, _text["invalidDirectory"], _text["missing"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SetGameDirectory(dialog.SelectedPath);
            }
        }

        private void Install(bool launch)
        {
            if (_busy || _preview || _gameDirectory == null) return;
            string directory = _gameDirectory;
            RunAction(launch && _installed ? "launching" : "installing", "busyHint", delegate {
                InstallResult result = LauncherEngine.Install(directory);
                if (launch) LauncherEngine.Launch(directory);
                return result;
            }, delegate(object value) {
                InstallResult result = (InstallResult)value;
                Append(result.Rebuilt ? _text.Format("created", result.Patch.TerrariaVersion, result.Patch.InsertedCalls) : _text["current"]);
                Append(_text[launch ? "launched" : "installedDone"]);
                _installed = LauncherEngine.IsInstalled(directory);
                ApplyLanguage();
                SetStatus("installed", launch ? "launched" : "installedDone", Accent);
            });
        }

        private void OpenFolderClicked(object sender, EventArgs e)
        {
            if (_busy || _preview || _gameDirectory == null) return;
            try { Process.Start(new ProcessStartInfo { FileName = _gameDirectory, UseShellExecute = true }); }
            catch (Exception ex) { ShowError(ex); }
        }

        private void RemoveClicked(object sender, EventArgs e)
        {
            if (_busy || _preview || _gameDirectory == null) return;
            if (MessageBox.Show(this, _text["removeConfirm"], _text["removeTitle"],
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            string directory = _gameDirectory;
            RunAction("removing", "busyHint", delegate { LauncherEngine.Remove(directory); return null; }, delegate(object value) {
                _installed = LauncherEngine.IsInstalled(directory);
                ApplyLanguage();
                SetStatus("ready", "removed", Accent);
                Append(_text["removed"]);
            });
        }

        // EN: Only the worker touches slow patching operations. Completion and all controls stay on the UI thread.
        // RU: Долгий патчинг выполняет фоновый поток. Завершение и доступ к элементам остаются в UI-потоке.
        private void RunAction(string statusKey, string hintKey, Func<object> work, Action<object> completed)
        {
            if (_busy || IsDisposed) return;
            _busy = true;
            RefreshActions();
            SetStatus(statusKey, hintKey, Muted);
            var worker = new BackgroundWorker();
            worker.DoWork += delegate(object sender, DoWorkEventArgs args) { args.Result = work(); };
            worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args) {
                _busy = false;
                worker.Dispose();
                if (IsDisposed || Disposing) return;
                try
                {
                    if (args.Error != null) ShowError(args.Error);
                    else completed(args.Result);
                }
                catch (Exception ex) { ShowError(ex); }
                finally { RefreshActions(); }
            };
            worker.RunWorkerAsync();
        }

        private void RefreshActions()
        {
            bool found = _gameDirectory != null;
            _browse.Enabled = !_busy;
            _language.Enabled = !_busy;
            _primary.Enabled = !_busy && found;
            _installOnly.Enabled = !_busy && found;
            _openFolder.Enabled = !_busy && found;
            _remove.Enabled = !_busy && found && _installed;
            _copy.Visible = _detailsVisible;
        }

        private void SetStatus(string statusKey, string hintKey, Color color)
        {
            _statusKey = statusKey;
            _hintKey = hintKey;
            _statusColor = color;
            _statusLabel.Text = _text[statusKey];
            _statusLabel.ForeColor = color;
            _statusHint.Text = _text[hintKey];
            _statusLabel.AccessibleName = _text[statusKey];
        }

        private string ErrorKey(Exception exception)
        {
            var launcherException = exception as LauncherException;
            if (launcherException != null)
            {
                switch (launcherException.Code)
                {
                    case "InvalidDirectory": return "invalidDirectory";
                    case "GameRunning": return "errorRunning";
                    case "OperationInProgress": return "errorInProgress";
                    case "NotInstalled":
                    case "InstallationInvalid": return "errorInstall";
                    case "EmbeddedLogicMissing": return "errorPackage";
                }
            }
            if (exception is UnauthorizedAccessException) return "errorAccess";
            if (exception is NotSupportedException || exception is BadImageFormatException) return "errorCompatibility";
            if (exception is DirectoryNotFoundException) return "invalidDirectory";
            if (exception is FileNotFoundException || exception is FileLoadException) return "errorPackage";
            if (exception.InnerException != null) return ErrorKey(exception.InnerException);
            return "errorGeneric";
        }

        private void ShowError(Exception exception)
        {
            string key = ErrorKey(exception);
            _installed = _gameDirectory != null && LauncherEngine.IsInstalled(_gameDirectory);
            ApplyLanguage();
            SetStatus("errorTitle", key, Color.FromArgb(156, 48, 38));
            Append(_text["errorTitle"] + ": " + _text[key] + Environment.NewLine + exception);
            SetDetailsVisible(true);
            MessageBox.Show(this, _text[key], _text["errorTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Append(string message)
        {
            // EN: Bound in-memory diagnostics; no telemetry, disk log or background uploads.
            // RU: Ограничиваем объём диагностики в памяти; телеметрии, записи журнала на диск и отправки нет.
            if (_activity.Length > 48000) _activity.Remove(0, _activity.Length - 32000);
            _activity.AppendLine(DateTime.Now.ToString("HH:mm:ss") + "  " + message);
            RefreshDetails();
        }

        private void RefreshDetails()
        {
            _details.Text = _text["detailsIntro"] + Environment.NewLine + Environment.NewLine + _activity;
            _details.SelectionStart = _details.TextLength;
            _details.ScrollToCaret();
        }

        private void SetDetailsVisible(bool visible)
        {
            _detailsVisible = visible;
            _detailsPanel.Visible = visible;
            _layout.RowStyles[5].Height = visible ? 160 : 0;
            _copy.Visible = visible;
            _detailsToggle.Text = _text[visible ? "hideDetails" : "details"];
        }

        private void CopyClicked(object sender, EventArgs e)
        {
            try { Clipboard.SetText(_details.Text); Append(_text["copied"]); }
            catch (ExternalException) { MessageBox.Show(this, _text["copyFailed"], Text, MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private void ShowHelp()
        {
            using (var dialog = new Form {
                Text = _text["help"], StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false,
                MinimizeBox = false, MaximizeBox = false, ClientSize = new Size(660, 490),
                MinimumSize = new Size(570, 430), Font = Font, BackColor = Canvas, Padding = new Padding(24),
                AutoScaleMode = AutoScaleMode.Dpi
            })
            {
                var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
                grid.Controls.Add(Label(_text["help"], 22, FontStyle.Bold, Ink), 0, 0);
                var body = new TextBox {
                    Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.None, BackColor = Canvas, ForeColor = Ink, Font = Font,
                    Text = _text["helpBody"].Replace("\n", Environment.NewLine), TabStop = false
                };
                grid.Controls.Add(body, 0, 1);
                Button ok = Button(true);
                ok.Text = _text["ok"];
                ok.DialogResult = DialogResult.OK;
                grid.Controls.Add(ok, 0, 2);
                dialog.Controls.Add(grid);
                dialog.AcceptButton = ok;
                dialog.CancelButton = ok;
                dialog.ShowDialog(this);
            }
        }

        private void FormClosingWhileBusy(object sender, FormClosingEventArgs e)
        {
            // EN: Do not tear down the process during a deployment commit. The user can close it once work finishes.
            // RU: Не прерываем процесс во время фиксации установки. Окно можно закрыть после завершения операции.
            if (!_busy || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing) return;
            e.Cancel = true;
            _hintKey = "closeBusy";
            _statusHint.Text = _text["closeBusy"];
        }

        internal void SetPreviewState(bool installed, bool details)
        {
            if (!_preview) throw new InvalidOperationException("Preview mode is required.");
            _gameDirectory = @"C:\Games\Terraria";
            _installed = installed;
            ApplyLanguage();
            RefreshReadyStatus();
            RefreshActions();
            SetDetailsVisible(details);
        }

        internal void SetPreviewStatus(string status, string hint)
        {
            if (!_preview) throw new InvalidOperationException("Preview mode is required.");
            SetStatus(status, hint, Accent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _toolTip != null) _toolTip.Dispose();
            base.Dispose(disposing);
        }
    }

    internal sealed class SurfacePanel : Panel
    {
        public SurfacePanel() { BackColor = Color.White; DoubleBuffered = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(218, 225, 221)))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}

