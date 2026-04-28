$ErrorActionPreference = 'Stop'

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

$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.dotnet-cli-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

Invoke-Step -Name "API unit tests" -Action {
    Invoke-CheckedCommand {
        dotnet test .\Test\API.UnitTests\API.UnitTests.csproj -m:1 -p:UseAppHost=false --artifacts-path .\artifacts\test-run\api-unit
    }
}

Invoke-Step -Name "API integration tests" -Action {
    Invoke-CheckedCommand {
        dotnet test .\Test\API.IntegrationTests\API.IntegrationTests.csproj -m:1 -p:UseAppHost=false --artifacts-path .\artifacts\test-run\api-integration
    }
}

Invoke-Step -Name "Frontend unit tests" -Action {
    Push-Location .\Test\Frontend.UnitTests
    try {
        Invoke-CheckedCommand {
            npm test
        }
    }
    finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "All automated tests completed successfully." -ForegroundColor Green
