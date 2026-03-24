#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

ensure_dotnet
"${SCRIPT_DIR}/sync-references.sh"

mkdir -p "${PLUGIN_BUILD_DIR}"
dotnet build "${PROJECT_PATH}" -c Release -o "${PLUGIN_BUILD_DIR}"

log "Plugin build output: ${PLUGIN_BUILD_DIR}"
