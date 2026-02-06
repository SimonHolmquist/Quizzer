using CsvHelper;
using CsvHelper.Configuration;
using Quizzer.Application;
using Quizzer.Application.Exams.Queries;
using Quizzer.Domain.Exams;
using System.Globalization;

namespace Quizzer.Application.ImportExport.Csv;

public static class CsvExamExporter
{
    public static List<string> Export(string csvPath, IEnumerable<ExamQuestionDto> questions)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(csvPath))
        {
            errors.Add("Ruta de CSV inválida.");
            return errors;
        }

        var questionList = questions?.OrderBy(q => q.OrderIndex).ToList() ?? [];
        if (questionList.Count == 0)
        {
            errors.Add("La versión no contiene preguntas.");
            return errors;
        }

        var (Rows, Errors) = BuildRows(questionList.Select(q => (q.Text, q.Options.Select(o => (o.Text, o.IsCorrect)).ToList())));

        if (Errors.Count > 0)
            return Errors;

        WriteCsv(csvPath, Rows);
        return errors;
    }

    public static List<string> Export(ExamVersion version, string csvPath)
    {
        var errors = new List<string>();

        if (version is null)
        {
            errors.Add("ExamVersion inválida.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(csvPath))
        {
            errors.Add("Ruta de CSV inválida.");
            return errors;
        }

        var questions = version.Questions?.OrderBy(q => q.OrderIndex).ToList() ?? [];
        if (questions.Count == 0)
        {
            errors.Add("La versión no contiene preguntas.");
            return errors;
        }

        var (Rows, Errors) = BuildRows(questions.Select(q =>
            (q.Text, q.Options?.OrderBy(o => o.OrderIndex).Select(o => (o.Text, o.Id == q.CorrectOptionId)).ToList() ?? [])));

        if (Errors.Count > 0)
            return Errors;

        WriteCsv(csvPath, Rows);
        return errors;
    }

    private static (List<ImportRow> Rows, List<string> Errors) BuildRows(IEnumerable<(string Text, List<(string Text, bool IsCorrect)> Options)> questions)
    {
        var errors = new List<string>();
        var rows = new List<ImportRow>();
        var questionList = questions.ToList();

        for (var i = 0; i < questionList.Count; i++)
        {
            var (text, options) = questionList[i];
            var rowNum = i + 2; // header = 1
            var rowHasError = false;

            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add($"Fila {rowNum}: question vacío.");
                rowHasError = true;
            }

            if (options.Count < 2)
            {
                errors.Add($"Fila {rowNum}: requiere al menos 2 opciones.");
                rowHasError = true;
            }
            else if (options.Count > 8)
            {
                errors.Add($"Fila {rowNum}: máximo 8 opciones.");
                rowHasError = true;
            }

            if (options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
            {
                errors.Add($"Fila {rowNum}: opciones vacías.");
                rowHasError = true;
            }

            var correctIndex = options.FindIndex(o => o.IsCorrect);
            if (correctIndex < 0)
            {
                errors.Add($"Fila {rowNum}: opción correcta inválida.");
                rowHasError = true;
            }

            if (rowHasError)
                continue;

            var answer = (correctIndex + 1).ToString(CultureInfo.InvariantCulture);

            string? OptionAt(int index) => index < options.Count ? options[index].Text.Trim() : null;

            rows.Add(new ImportRow(
                text.Trim(),
                OptionAt(0),
                OptionAt(1),
                OptionAt(2),
                OptionAt(3),
                OptionAt(4),
                OptionAt(5),
                OptionAt(6),
                OptionAt(7),
                answer,
                null,
                null,
                null
            ));
        }

        return (rows, errors);
    }

    private static void WriteCsv(string csvPath, IReadOnlyList<ImportRow> rows)
    {
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        using var writer = new StreamWriter(csvPath);
        using var csv = new CsvWriter(writer, cfg);
        csv.WriteRecords(rows);
    }
}
