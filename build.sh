#!/usr/bin/env bash

bold=$(tput bold 2>/dev/null || echo '')
normal=$(tput sgr0 2>/dev/null || echo '')

echo "${bold}Bash version ${BASH_VERSION}${normal}"

set -eo pipefail
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)

BUILD_PROJECT_FILE="$SCRIPT_DIR/_build/_build.csproj"

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

dotnet tool restore
dotnet build "$BUILD_PROJECT_FILE" /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
dotnet run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
