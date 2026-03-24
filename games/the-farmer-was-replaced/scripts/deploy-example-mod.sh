#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

"${SCRIPT_DIR}/deploy-plugin.sh"
"${SCRIPT_DIR}/build-example-mod.sh"

mkdir -p "${EXTERNAL_MODS_DEPLOY_DIR}"
rm -f "${EXTERNAL_MODS_DEPLOY_DIR}/ExampleHelloMod.dll" "${EXTERNAL_MODS_DEPLOY_DIR}/ExampleHelloMod.pdb"
cp "${EXAMPLE_MOD_BUILD_DIR}/ExampleHelloMod.dll" "${EXTERNAL_MODS_DEPLOY_DIR}/"
if [[ -f "${EXAMPLE_MOD_BUILD_DIR}/ExampleHelloMod.pdb" ]]; then
  cp "${EXAMPLE_MOD_BUILD_DIR}/ExampleHelloMod.pdb" "${EXTERNAL_MODS_DEPLOY_DIR}/"
fi

log "Example mod deployed to ${EXTERNAL_MODS_DEPLOY_DIR}"
