#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

"${SCRIPT_DIR}/build-plugin.sh"

mkdir -p "${PLUGIN_DEPLOY_DIR}"
mkdir -p "${SDK_DEPLOY_DIR}" "${EXTERNAL_MODS_DEPLOY_DIR}"
rm -f "${PLUGIN_DEPLOY_DIR}/TFWR.ModHarness.dll" "${PLUGIN_DEPLOY_DIR}/TFWR.ModHarness.pdb" "${PLUGIN_DEPLOY_DIR}/TFWR.ModHarness.SDK.dll" "${PLUGIN_DEPLOY_DIR}/TFWR.ModHarness.SDK.pdb"
rm -f "${SDK_DEPLOY_DIR}/TFWR.ModHarness.SDK.dll" "${SDK_DEPLOY_DIR}/TFWR.ModHarness.SDK.pdb"
cp "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.dll" "${PLUGIN_DEPLOY_DIR}/"
if [[ -f "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.pdb" ]]; then
  cp "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.pdb" "${PLUGIN_DEPLOY_DIR}/"
fi
if [[ -f "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.SDK.dll" ]]; then
  cp "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.SDK.dll" "${PLUGIN_DEPLOY_DIR}/"
  cp "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.SDK.dll" "${SDK_DEPLOY_DIR}/"
fi
if [[ -f "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" ]]; then
  cp "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" "${PLUGIN_DEPLOY_DIR}/"
  cp "${PLUGIN_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" "${SDK_DEPLOY_DIR}/"
fi

log "Plugin deployed to ${PLUGIN_DEPLOY_DIR}"
