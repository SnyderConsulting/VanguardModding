#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

"${SCRIPT_DIR}/install-dotnet.sh"
"${SCRIPT_DIR}/install-bepinex.sh"
"${SCRIPT_DIR}/sync-references.sh"
"${SCRIPT_DIR}/decompile.sh"
"${SCRIPT_DIR}/build-sdk.sh"
"${SCRIPT_DIR}/build-plugin.sh"
"${SCRIPT_DIR}/deploy-plugin.sh"
