#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="${DOTNET_ROOT}:${DOTNET_ROOT}/tools:${PATH}"

dotnet build "${SCRIPT_DIR}/__MOD_ASSEMBLY_NAME__.csproj" -c Release -o "${SCRIPT_DIR}/build"
