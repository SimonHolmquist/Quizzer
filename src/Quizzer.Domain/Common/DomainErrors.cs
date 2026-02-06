namespace Quizzer.Domain.Common;

public static class DomainErrors
{
    public const string InvalidCorrectAnswer = "La pregunta debe tener exactamente una opción correcta.";
    public const string AtLeastTwoOptions = "La pregunta debe tener al menos 2 opciones.";
}
