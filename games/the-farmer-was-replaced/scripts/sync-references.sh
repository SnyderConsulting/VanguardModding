#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

require_command rsync
ensure_game_layout
ensure_bepinex

mkdir -p "${REFERENCE_DIR}/Managed" "${REFERENCE_DIR}/BepInEx/core"

rsync -a --delete "${MANAGED_DIR}/" "${REFERENCE_DIR}/Managed/"
rsync -a --delete "${BEPINEX_CORE_DIR}/" "${REFERENCE_DIR}/BepInEx/core/"

log "References synced to ${REFERENCE_DIR}"
