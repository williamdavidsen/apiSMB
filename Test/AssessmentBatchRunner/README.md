# AssessmentBatchRunner

Small console tool for semi-automated validation against a running API.

## Usage

Start the backend API first, then run:

```powershell
dotnet run --project .\Test\AssessmentBatchRunner\AssessmentBatchRunner.csproj -- http://localhost:5555 .\Test\AssessmentBatchRunner\domains.txt
```

This tool is not a replacement for unit or integration tests. It is useful for exploratory validation and report evidence.
