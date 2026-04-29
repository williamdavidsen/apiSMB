param(
  [string]$ApiBaseUrl = 'http://localhost:1071',
  [string]$MixedDomainFile = (Join-Path $PSScriptRoot '..\AssessmentBatchRunner\live-smoke-domains.txt'),
  [string]$FakeDomainFile = (Join-Path $PSScriptRoot '..\AssessmentBatchRunner\weak-domains.txt'),
  [int]$TimeoutSeconds = 30
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http

function Read-DomainList {
  param(
    [Parameter(Mandatory = $true)]
    [string] $Path
  )

  return @(Get-Content $Path | ForEach-Object { "$_".Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') })
}

$realDomains = Read-DomainList -Path $MixedDomainFile
$fakeDomains = Read-DomainList -Path $FakeDomainFile | Select-Object -First 5
$domainTargets = @(
  $realDomains | ForEach-Object {
    [pscustomobject]@{
      Domain = $_
      ExpectedOutcome = 'LIVE_SUCCESS'
    }
  }
  $fakeDomains | ForEach-Object {
    [pscustomobject]@{
      Domain = $_
      ExpectedOutcome = 'HANDLED_NEGATIVE'
    }
  }
)

$handler = [System.Net.Http.HttpClientHandler]::new()
$client = [System.Net.Http.HttpClient]::new($handler)
$client.BaseAddress = [Uri]($ApiBaseUrl.TrimEnd('/') + '/')
$client.Timeout = [TimeSpan]::FromSeconds($TimeoutSeconds)

$results = foreach ($target in $domainTargets) {
  try {
    $payload = @{ domain = $target.Domain } | ConvertTo-Json
    $content = [System.Net.Http.StringContent]::new($payload, [System.Text.Encoding]::UTF8, 'application/json')
    $response = $client.PostAsync('api/assessment/check', $content).GetAwaiter().GetResult()
    $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    $parsedBody = $null
    try {
      $parsedBody = $body | ConvertFrom-Json
    }
    catch {
      $parsedBody = $null
    }

    [pscustomobject]@{
      Domain = $target.Domain
      ExpectedOutcome = $target.ExpectedOutcome
      StatusCode = [int]$response.StatusCode
      Success = $response.IsSuccessStatusCode
      AssessmentStatus = if ($parsedBody) { "$($parsedBody.status)" } else { '' }
      OverallScore = if ($parsedBody -and $null -ne $parsedBody.overallScore) { [int]$parsedBody.overallScore } else { -1 }
      Body = $body
    }
  }
  catch {
    [pscustomobject]@{
      Domain = $target.Domain
      ExpectedOutcome = $target.ExpectedOutcome
      StatusCode = 0
      Success = $false
      AssessmentStatus = ''
      OverallScore = -1
      Body = $_.Exception.Message
    }
  }
}

$results | Select-Object Domain, ExpectedOutcome, StatusCode, Success, AssessmentStatus, OverallScore | Format-Table -AutoSize

$transportFailures = $results | Where-Object { $_.StatusCode -eq 0 }
if ($transportFailures.Count -gt 0) {
  throw "Resilience smoke failed because one or more requests did not return a handled HTTP response."
}

$realFailures = $results | Where-Object { $_.ExpectedOutcome -eq 'LIVE_SUCCESS' -and -not $_.Success }
if ($realFailures.Count -gt 0) {
  $summary = ($realFailures | ForEach-Object { "$($_.Domain) => HTTP_$($_.StatusCode)" }) -join '; '
  throw "Resilience smoke failed because expected-live domains did not succeed: $summary"
}

$negativeFailures = $results | Where-Object {
  $_.ExpectedOutcome -eq 'HANDLED_NEGATIVE' -and (
    -not $_.Success -or
    $_.AssessmentStatus -notin @('PARTIAL', 'FAIL', 'ERROR') -or
    $_.OverallScore -ne 0
  )
}

if ($negativeFailures.Count -gt 0) {
  $summary = ($negativeFailures | ForEach-Object {
    "$($_.Domain) => HTTP_$($_.StatusCode), Status=$($_.AssessmentStatus), Score=$($_.OverallScore)"
  }) -join '; '
  throw "Resilience smoke failed because reserved negative domains did not produce the expected degraded handled result: $summary"
}
