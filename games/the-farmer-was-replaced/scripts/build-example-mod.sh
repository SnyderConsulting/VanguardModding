#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

ensure_dotnet
"${SCRIPT_DIR}/build-sdk.sh"

mkdir -p "${EXAMPLE_MOD_BUILD_DIR}"
dotnet build "${EXAMPLE_MOD_PROJECT_PATH}" -c Release -o "${EXAMPLE_MOD_BUILD_DIR}"

log "Example mod build output: ${EXAMPLE_MOD_BUILD_DIR}"
