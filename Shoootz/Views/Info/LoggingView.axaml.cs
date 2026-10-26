using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Controls.ContentDialog;
using Controls.JsonEditor;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Localization;
using Shoootz.Models.Udp;

namespace Shoootz.Views.Info;

/// <inheritdoc/>
public partial class LoggingView : UserControl
{
    public LoggingView()
    {
        InitializeComponent();
    }

    private static JsonEditorControl BuildTextEditor(string content)
    {
        return new JsonEditorControl { IsReadOnly = true, Text = content };
    }

    private static string PrettyPrintJson(string raw)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return raw;
        }
    }

    private void OnLogEntrySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox { SelectedItem: UdpRawModel entry } listBox)
        {
            return;
        }

        listBox.SelectedItem = null;
        _ = OpenRawDataDialogAsync(entry);
    }

    private async Task OpenRawDataDialogAsync(UdpRawModel entry)
    {
        string prettyJson = PrettyPrintJson(entry.RawData);

        await DialogService.ShowAsync(
            this,
            window =>
                new ContentDialog
                {
                    CopyText = prettyJson,
                    DialogContent = BuildTextEditor(prettyJson),
                    DialogTitle =
                        $"{LocalizationService.Instance[nameof(Messages.LoggingRawTitle)]} – {entry.Timestamp:HH:mm:ss.fff}",
                    Height = 650,
                    Width = 800,
                }.ShowDialog(window)
        );
    }
}
