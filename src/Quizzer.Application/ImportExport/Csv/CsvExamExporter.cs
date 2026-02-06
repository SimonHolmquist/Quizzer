using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Quizzer.Application;
using Quizzer.Application.Exams.Queries;

namespace Quizzer.Application.ImportExport.Csv;

public static class CsvExamExporter
{
    public static void Export(string csvPath, IEnumerable<ExamQuestionDto> questions)
    {
        if (string.IsNullOrWhiteSpace(csvPath))
            throw new InvalidOperationException("Ruta de CSV inválida.");

        var rows = new List<ImportRow>();

        foreach (var question in questions.OrderBy(q => q.OrderIndex))
        {
            var options = question.Options.OrderBy(o => o.OrderIndex).ToList();
            if (options.Count < 2)
                throw new InvalidOperationException("Cada pregunta requiere 2+ opciones.");
            if (options.Count > 8)
                throw new InvalidOperationException("El exportador soporta hasta 8 opciones por pregunta.");

            var correctIndex = options.FindIndex(o => o.IsCorrect);
            if (correctIndex < 0)
                correctIndex = 0;

            var answer = (correctIndex + 1).ToString(CultureInfo.InvariantCulture);

            string? GetOption(int index) => index < options.Count ? options[index].Text : null;

            rows.Add(new ImportRow(
                question.Text,
                GetOption(0),
                GetOption(1),
                GetOption(2),
                GetOption(3),
                GetOption(4),
                GetOption(5),
                GetOption(6),
                GetOption(7),
                answer,
                null,
                null,
                null
            ));
        }

        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        using var writer = new StreamWriter(csvPath);
        using var csv = new CsvWriter(writer, cfg);
        csv.WriteHeader<ImportRow>();
        csv.NextRecord();
        csv.WriteRecords(rows);
    }
}
