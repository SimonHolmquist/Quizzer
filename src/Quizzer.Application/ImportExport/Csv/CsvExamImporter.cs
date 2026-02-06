using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Quizzer.Application.ImportExport.Csv;

public static class CsvExamImporter
{
    public static (List<(string Question, List<string> Options, int CorrectIndex, string? Explanation, string? Tags, int? Difficulty)> Items, List<string> Errors)
        Parse(string csvPath)
    {
        var errors = new List<string>();
        var items = new List<(string, List<string>, int, string?, string?, int?)>();

        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, cfg);

        var rows = csv.GetRecords<ImportRow>().ToList();
        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var rowNum = i + 2; // header = 1

            if (string.IsNullOrWhiteSpace(r.question))
            {
                errors.Add($"Fila {rowNum}: question vacío.");
                continue;
            }

            var opts = new[]
            {
                r.option1, r.option2, r.option3, r.option4, r.option5, r.option6, r.option7, r.option8
            }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()).ToList();

            if (opts.Count < 2)
            {
                errors.Add($"Fila {rowNum}: requiere al menos 2 opciones.");
                continue;
            }

            var correct = ParseAnswer(r.answer, opts.Count);
            if (correct is null)
            {
                errors.Add($"Fila {rowNum}: answer inválido '{r.answer}'. Use 1..{opts.Count} o A..{(char)('A' + opts.Count - 1)}.");
                continue;
            }

            items.Add((r.question.Trim(), opts, correct.Value, r.explanation?.Trim(), r.tags?.Trim(), r.difficulty));
        }

        return (items, errors);
    }

    private static int? ParseAnswer(string raw, int optionCount)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();

        if (int.TryParse(raw, out var idx))
        {
            idx -= 1;
            return idx >= 0 && idx < optionCount ? idx : null;
        }

        var c = char.ToUpperInvariant(raw[0]);
        if (c >= 'A' && c < 'A' + optionCount)
            return c - 'A';

        return null;
    }
}
