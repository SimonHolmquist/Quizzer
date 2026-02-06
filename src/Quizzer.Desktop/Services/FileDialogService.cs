using Microsoft.Win32;

namespace Quizzer.Desktop.Services;

public sealed class FileDialogService
{
    private const string CsvFilter = "CSV (*.csv)|*.csv|Todos (*.*)|*.*";

    public string? OpenCsv(string? title = null)
    {
        var dialog = new OpenFileDialog
        {
            Filter = CsvFilter,
            DefaultExt = ".csv",
            Title = title ?? "Abrir CSV"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveCsv(string defaultFileName, string? title = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = CsvFilter,
            DefaultExt = ".csv",
            FileName = defaultFileName,
            Title = title ?? "Guardar CSV"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
