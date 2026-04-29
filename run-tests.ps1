$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path $PSScriptRoot
$apiBaseUrl = 'http://localhost:1072'
$ownedApiProcess = $null
$powershellExe = (Get-Command powershell -ErrorAction Stop).Source
$liveE2EDomainFile = Join-Path $repoRoot 'Test\AssessmentBatchRunner\live-smoke-domains.txt'

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name,
        [Parameter(Mandatory = $true)]
        [scriptblock] $Action
    )

    Write-Host ""
    Write-Host "=== $Name ===" -ForegroundColor Cyan
    & $Action
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE."
    }
}

function Test-ApiAvailable {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Url
    )

    try {
        $response = Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec 5
        return $response.message -eq 'SecurityAssessment API is running'
    }
    catch {
        return $false
    }
}

function Wait-ApiAvailable {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Url,
        [int] $TimeoutSeconds = 120
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-ApiAvailable -Url $Url) {
            return
        }
        Start-Sleep -Seconds 2
    }

    throw "Timed out waiting for API at $Url"
}

function Ensure-LocalApi {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Url
    )

    $apiBinaryRoot = (Join-Path $repoRoot 'API\bin').ToLowerInvariant()
    $existingProcesses = Get-Process -Name 'SecurityAssessmentAPI' -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -and $_.Path.ToLowerInvariant().StartsWith($apiBinaryRoot) }

    foreach ($existingProcess in $existingProcesses) {
        if (-not $existingProcess.HasExited) {
            Stop-Process -Id $existingProcess.Id -Force
        }
    }

    $apiProject = Join-Path $repoRoot 'API\SecurityAssessmentAPI.csproj'
    $apiWorkingDirectory = Join-Path $repoRoot 'API'
    $logDirectory = Join-Path $repoRoot 'artifacts\test-run\live-api'
    New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

    $stdoutLog = Join-Path $logDirectory 'stdout.log'
    $stderrLog = Join-Path $logDirectory 'stderr.log'

    $process = Start-Process dotnet `
        -ArgumentList @('run', '--project', $apiProject, '--launch-profile', 'http', '--', '--urls', $Url) `
        -WorkingDirectory $apiWorkingDirectory `
        -RedirectStandardOutput $stdoutLog `
        -RedirectStandardError $stderrLog `
        -PassThru `
        -WindowStyle Hidden

    try {
        Wait-ApiAvailable -Url $Url
        return $process
    }
    catch {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
        throw
    }
}

function Get-LiveE2EDomain {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $domain = Get-Content $Path |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ -and -not $_.StartsWith('#') } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($domain)) {
        throw "Could not determine LIVE_E2E_DOMAIN from $Path"
    }

    return $domain
}

function Stop-OwnedApi {
    param(
        [System.Diagnostics.Process] $Process
    )

    if ($null -eq $Process) {
        return
    }

    if (-not $Process.HasExited) {
        Stop-Process -Id $Process.Id -Force
    }
}

try {
    $env:DOTNET_CLI_HOME = Join-Path $repoRoot '.dotnet-cli-home'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

    Invoke-Step -Name "API unit tests" -Action {
        Invoke-CheckedCommand {
            dotnet test (Join-Path $repoRoot 'Test\API.UnitTests\API.UnitTests.csproj') -m:1 -p:UseAppHost=false --artifacts-path (Join-Path $repoRoot 'artifacts\test-run\api-unit')
        }
    }

    Invoke-Step -Name "API integration tests" -Action {
        Invoke-CheckedCommand {
            dotnet test (Join-Path $repoRoot 'Test\API.IntegrationTests\API.IntegrationTests.csproj') -m:1 -p:UseAppHost=false --artifacts-path (Join-Path $repoRoot 'artifacts\test-run\api-integration')
        }
    }

    Invoke-Step -Name "Frontend unit tests" -Action {
        Push-Location (Join-Path $repoRoot 'Test\Frontend.UnitTests')
        try {
            Invoke-CheckedCommand {
                npm test
            }
        }
        finally {
            Pop-Location
        }
    }

    Invoke-Step -Name "E2E tests" -Action {
        if ($null -eq $ownedApiProcess) {
            $ownedApiProcess = Ensure-LocalApi -Url $apiBaseUrl
        }

        Push-Location (Join-Path $repoRoot 'Test\E2E')
        try {
            $previousLiveDomain = $env:LIVE_E2E_DOMAIN
            $previousDevApiProxy = $env:VITE_DEV_API_PROXY
            $env:LIVE_E2E_DOMAIN = Get-LiveE2EDomain -Path $liveE2EDomainFile
            $env:VITE_DEV_API_PROXY = $apiBaseUrl
            Invoke-CheckedCommand {
                npm test
            }
        }
        finally {
            $env:LIVE_E2E_DOMAIN = $previousLiveDomain
            $env:VITE_DEV_API_PROXY = $previousDevApiProxy
            Pop-Location
        }
    }

    Invoke-Step -Name "Live batch validation" -Action {
        if ($null -eq $ownedApiProcess) {
            $ownedApiProcess = Ensure-LocalApi -Url $apiBaseUrl
        }

        Invoke-CheckedCommand {
            & $powershellExe -ExecutionPolicy Bypass -File (Join-Path $repoRoot 'Test\AssessmentBatchRunner\run-live-validation.ps1') $apiBaseUrl (Join-Path $repoRoot 'Test\AssessmentBatchRunner\live-smoke-domains.txt')
        }
    }

    Invoke-Step -Name "Non-functional load smoke" -Action {
        if ($null -eq $ownedApiProcess) {
            $ownedApiProcess = Ensure-LocalApi -Url $apiBaseUrl
        }

        Invoke-CheckedCommand {
            & $powershellExe -ExecutionPolicy Bypass -File (Join-Path $repoRoot 'Test\NonFunctional\load-smoke.ps1') -ApiBaseUrl $apiBaseUrl
        }
    }

    Invoke-Step -Name "Non-functional resilience smoke" -Action {
        if ($null -eq $ownedApiProcess) {
            $ownedApiProcess = Ensure-LocalApi -Url $apiBaseUrl
        }

        Invoke-CheckedCommand {
            & $powershellExe -ExecutionPolicy Bypass -File (Join-Path $repoRoot 'Test\NonFunctional\resilience-smoke.ps1') -ApiBaseUrl $apiBaseUrl
        }
    }

    Write-Host ""
    Write-Host "All automated tests completed successfully." -ForegroundColor Green
}
finally {
    Stop-OwnedApi -Process $ownedApiProcess
}
