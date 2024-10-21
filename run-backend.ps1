$dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue

if (-not $dockerProcess) {
    Write-Output "Docker Desktop is not running. Starting Docker Desktop..."
    Start-Process -FilePath "C:\Program Files\Docker\Docker\Docker Desktop.exe"

    Write-Output "Waiting for Docker to initialize..."
    while ($true) {

        $dockerBackendProcess = Get-Process -Name "com.docker.backend" -ErrorAction SilentlyContinue
        if ($dockerBackendProcess) {
            Write-Output "Docker is now running."
            break
        }
        Start-Sleep -Seconds 5
    }
}

$composeStatus = docker-compose ps --services --filter "status=running"

if (-not $composeStatus) {
    Write-Output "Docker services are not running. Starting Docker Compose..."
    docker-compose up -d
}

dotnet run --project ./server/api