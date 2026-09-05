using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using TerrariaHighFPS.Launcher;

internal static class UiHarness
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Localization.ValidateTranslations();
            Assert(Localization.MatchCulture("ru-RU") == "ru", "Russian culture matching");
            Assert(Localization.MatchCulture("pt-PT") == "pt-BR", "Portuguese fallback");
            Assert(Localization.MatchCulture("zh-CN") == "zh-Hans", "Simplified Chinese matching");
            Assert(Localization.MatchCulture("zh-TW") == "en", "Traditional Chinese fallback");
            Assert(Localization.MatchCulture("it-IT") == "en", "Unsupported locale fallback");
            string output = args.Length > 0 ? Path.GetFullPath(args[0]) : null;
            if (output != null) Directory.CreateDirectory(output);
            int cases = 0;
            foreach (LanguageOption language in Localization.Languages)
            {
                foreach (int width in new[] { 980, 884 })
                {
                    using (var form = Preview(language.Code))
                    {
                        form.ClientSize = new Size(width, 760);
                        var text = new Localization(language.Code);
                        Button primary = FindButton(form, text["installPlay"]);
                        Assert(!primary.Enabled, language.Code + ": install requires a selected game");
                        ComboBox selector = Find<ComboBox>(form);
                        Assert(selector != null && selector.Items.Count == 7, "All native-name language choices are available");
                        form.SetPreviewState(false, false);
                        Assert(primary.Enabled, language.Code + ": selected game enables installation");
                        ShowOffscreen(form);
                        Button browse = FindButton(form, text["browse"]);
                        Assert(browse != null && browse.Enabled, "Folder selection remains available");
                        if (output != null && width == 980) Save(form, Path.Combine(output, "launcher-" + language.Code + ".png"));
                        AssertReadable(form, 1f);
                        form.SetPreviewState(true, false);
                        Assert(primary.Text == text["play"], language.Code + ": installed state offers Play");
                        Assert(FindButton(form, text["remove"]).Enabled, "Installed state enables removal");
                        foreach (string hint in new[] { "errorGeneric", "errorCompatibility", "errorAccess", "errorPackage", "errorInstall", "errorRunning", "errorInProgress" })
                        {
                            form.SetPreviewStatus("errorTitle", hint);
                            AssertReadable(form, 1f);
                        }
                        form.SetPreviewState(true, false);
                        selector.SelectedIndex = language.Code == "en" ? 1 : 0;
                        Assert(primary.Text == new Localization(language.Code == "en" ? "ru" : "en")["play"], "Changing language updates the current state");
                        cases++;
                    }
                }
            }
            // EN: Simulated 150%/200% geometry and typography catches clipping without changing system DPI.
            // RU: Моделирование геометрии и шрифтов 150%/200% выявляет обрезку без изменения DPI Windows.
            foreach (float scale in new[] { 1.5f, 2f })
            {
                using (var form = Preview("ru"))
                {
                    form.SetPreviewState(true, true);
                    form.AutoScaleMode = AutoScaleMode.None;
                    var fonts = new Dictionary<Control, Font>();
                    RememberFonts(form, fonts);
                    form.Scale(new SizeF(scale, scale));
                    foreach (KeyValuePair<Control, Font> entry in fonts)
                        entry.Key.Font = new Font(entry.Value.FontFamily, entry.Value.Size * scale, entry.Value.Style, GraphicsUnit.Point);
                    ShowOffscreen(form);
                    AssertReadable(form, scale);
                    if (output != null) Save(form, Path.Combine(output, "launcher-ru-scale-" + (int)(scale * 100) + ".png"));
                    cases++;
                }
            }
            Console.WriteLine("UI checks passed: seven complete locales, culture fallback, action states, live language changes, " + cases + " layout cases (including simulated 150%/200%).");
            if (output != null) Console.WriteLine("Screenshots: " + output);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static MainForm Preview(string language)
    {
        return new MainForm(language, true) {
            ShowInTaskbar = false, StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
    }

    private static void ShowOffscreen(Form form)
    {
        form.Show();
        form.PerformLayout();
        Application.DoEvents();
    }

    private static void Save(Form form, string path)
    {
        Control canvas = form.Controls[0];
        using (var bitmap = new Bitmap(canvas.Width, canvas.Height))
        {
            canvas.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(path, ImageFormat.Png);
        }
    }

    private static void AssertReadable(Control parent, float scale)
    {
        foreach (Control control in parent.Controls)
        {
            if (!control.Visible) continue;
            Assert(control.Width > 0 && control.Height > 0, "Visible control has positive size: " + control.Text);
            if (parent is TableLayoutPanel && !parent.AutoSize)
                Assert(control.Bottom <= parent.ClientSize.Height && control.Right <= parent.ClientSize.Width,
                    "Control exceeds its layout cell: " + control.Text + " " + control.Bounds + " in " + parent.ClientSize);
            var label = control as Label;
            var button = control as Button;
            if ((label != null || button != null) && !string.IsNullOrEmpty(control.Text))
            {
                int padding = button != null ? (int)(16 * scale) : 0;
                Size measured = TextRenderer.MeasureText(control.Text, control.Font,
                    new Size(Math.Max(1, control.ClientSize.Width - padding), int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl);
                Assert(measured.Height <= control.ClientSize.Height + 2,
                    "Text clipped: '" + control.Text + "' needs " + measured.Height + "px; available " + control.Height + "px");
            }
            AssertReadable(control, scale);
        }
    }

    private static void RememberFonts(Control parent, Dictionary<Control, Font> fonts)
    {
        fonts.Add(parent, parent.Font);
        foreach (Control child in parent.Controls) RememberFonts(child, fonts);
    }

    private static T Find<T>(Control parent) where T : Control
    {
        foreach (Control child in parent.Controls)
        {
            T match = child as T;
            if (match != null) return match;
            match = Find<T>(child);
            if (match != null) return match;
        }
        return null;
    }

    private static Button FindButton(Control parent, string text)
    {
        foreach (Control child in parent.Controls)
        {
            Button button = child as Button;
            if (button != null && button.Text == text) return button;
            button = FindButton(child, text);
            if (button != null) return button;
        }
        return null;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
