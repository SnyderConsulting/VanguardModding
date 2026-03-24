#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

"${SCRIPT_DIR}/install-bepinex.sh"
"${SCRIPT_DIR}/deploy-plugin.sh"
ensure_steam_appid

[[ -f "${RUN_BEPINEX_SCRIPT}" ]] || die "missing run_bepinex.sh in ${GAME_ROOT}"

cd "${GAME_ROOT}"
exec "${RUN_BEPINEX_SCRIPT}"
