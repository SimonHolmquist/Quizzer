namespace Quizzer.Application;

public sealed record ImportRow(
    string question,
    string? option1,
    string? option2,
    string? option3,
    string? option4,
    string? option5,
    string? option6,
    string? option7,
    string? option8,
    string answer,
    string? explanation,
    string? tags,
    int? difficulty
);
