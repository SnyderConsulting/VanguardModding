#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

txt_log="${BEPINEX_ROOT}/LogOutput.txt"
log_log="${BEPINEX_ROOT}/LogOutput.log"

if [[ -f "${txt_log}" ]]; then
  exec tail -n 200 -f "${txt_log}"
fi

if [[ -f "${log_log}" ]]; then
  exec tail -n 200 -f "${log_log}"
fi

die "no BepInEx log file found yet under ${BEPINEX_ROOT}"
