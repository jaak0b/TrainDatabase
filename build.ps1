[CmdletBinding()]
Param(
    [Parameter(Position = 0, Mandatory = $false, ValueFromRemainingArguments = $true)]
    [string[]] $BuildArguments
)

Write-Output "PowerShell $($PSVersionTable.PSEdition) version $($PSVersionTable.PSVersion)"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSDefaultParameterValues['*:ErrorAction'] = 'Stop'

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

$BuildProjectFile = "$PSScriptRoot\_build\_build.csproj"

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_NOLOGO = 1

ExecSafe { & dotnet tool restore }
ExecSafe { & dotnet build $BuildProjectFile /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet }
ExecSafe { & dotnet run --project $BuildProjectFile --no-build -- $BuildArguments }
