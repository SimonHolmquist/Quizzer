$baseDir = $env:LOCALAPPDATA
if (-not $baseDir) {
    Write-Error "LOCALAPPDATA no está definido."
    exit 1
}

$dbPath = Join-Path $baseDir "Quizzer\\quizzer.db"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "DB eliminada: $dbPath"
} else {
    Write-Host "DB no encontrada: $dbPath"
}
