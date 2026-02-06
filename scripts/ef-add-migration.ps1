param([string]$Name = ""Init"")
dotnet ef migrations add $Name --project src/Quizzer.Infrastructure --startup-project src/Quizzer.Desktop
