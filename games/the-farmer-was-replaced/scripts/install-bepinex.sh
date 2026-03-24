#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

require_command curl
require_command unzip
require_command rsync
ensure_game_layout

mkdir -p "${CACHE_DIR}"

archive_path="${CACHE_DIR}/${BEPINEX_ARCHIVE}"
extract_dir="${CACHE_DIR}/bepinex-${BEPINEX_VERSION}"

if [[ ! -f "${archive_path}" ]]; then
  log "Downloading ${BEPINEX_ARCHIVE}"
  curl -fL "${BEPINEX_URL}" -o "${archive_path}"
fi

rm -rf "${extract_dir}"
mkdir -p "${extract_dir}"
unzip -oq "${archive_path}" -d "${extract_dir}"
rsync -a "${extract_dir}/" "${GAME_ROOT}/"

if [[ -f "${RUN_BEPINEX_SCRIPT}" ]]; then
  chmod u+x "${RUN_BEPINEX_SCRIPT}"
  if grep -q '^executable_name=' "${RUN_BEPINEX_SCRIPT}"; then
    sed -i.bak "s#^executable_name=.*#executable_name=\"${GAME_APP_NAME}\"#" "${RUN_BEPINEX_SCRIPT}"
  fi
fi

log "BepInEx ${BEPINEX_VERSION} installed into ${GAME_ROOT}"
