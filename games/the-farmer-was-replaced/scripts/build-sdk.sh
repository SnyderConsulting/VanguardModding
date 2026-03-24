#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

ensure_dotnet

mkdir -p "${SDK_BUILD_DIR}"
dotnet build "${SDK_PROJECT_PATH}" -c Release -o "${SDK_BUILD_DIR}"

log "SDK build output: ${SDK_BUILD_DIR}"
