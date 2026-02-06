using CsvHelper;
using CsvHelper.Configuration;
using Quizzer.Application;
using Quizzer.Domain.Exams;
using System.Globalization;

namespace Quizzer.Application.ImportExport.Csv;

public static class CsvExamExporter
{
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

        var rows = new List<ImportRow>();

        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var rowNum = i + 2; // header = 1
            var rowHasError = false;

            if (string.IsNullOrWhiteSpace(question.Text))
            {
                errors.Add($"Fila {rowNum}: question vacío.");
                rowHasError = true;
            }

            var options = question.Options?.OrderBy(o => o.OrderIndex).ToList() ?? [];
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

            var correctIndex = options.FindIndex(o => o.Id == question.CorrectOptionId);
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
                question.Text.Trim(),
                OptionAt(0),
                OptionAt(1),
                OptionAt(2),
                OptionAt(3),
                OptionAt(4),
                OptionAt(5),
                OptionAt(6),
                OptionAt(7),
                answer,
                question.Explanation?.Trim(),
                null,
                question.Difficulty
            ));
        }

        if (errors.Count > 0)
            return errors;

        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        using var writer = new StreamWriter(csvPath);
        using var csv = new CsvWriter(writer, cfg);
        csv.WriteRecords(rows);

        return errors;
    }
}
