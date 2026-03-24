#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

"${SCRIPT_DIR}/install-bepinex.sh"
"${SCRIPT_DIR}/deploy-plugin.sh"
ensure_steam_appid

cd "${GAME_ROOT}"
if [[ -f "${RUN_BEPINEX_SCRIPT}" ]]; then
  exec "${RUN_BEPINEX_SCRIPT}"
fi

exec "${GAME_ENTRY_PATH}"
