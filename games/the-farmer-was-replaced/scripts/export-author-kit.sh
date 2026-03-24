#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

ensure_game_layout
ensure_dotnet
ensure_bepinex
"${SCRIPT_DIR}/sync-references.sh"
"${SCRIPT_DIR}/build-sdk.sh"

rm -rf "${AUTHOR_KIT_DIR}"
mkdir -p "${AUTHOR_KIT_LIB_DIR}" "${AUTHOR_KIT_GAME_LIB_DIR}" "${AUTHOR_KIT_TEMPLATE_DIR}"

cp "${SDK_BUILD_DIR}/TFWR.ModHarness.SDK.dll" "${AUTHOR_KIT_LIB_DIR}/"
if [[ -f "${SDK_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" ]]; then
  cp "${SDK_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" "${AUTHOR_KIT_LIB_DIR}/"
fi

cp "${REFERENCE_DIR}/BepInEx/core/0Harmony.dll" "${AUTHOR_KIT_LIB_DIR}/"
cp "${REFERENCE_DIR}/Managed/Core.dll" "${AUTHOR_KIT_GAME_LIB_DIR}/"
cp "${REFERENCE_DIR}/Managed/Utils.dll" "${AUTHOR_KIT_GAME_LIB_DIR}/"
cp "${REFERENCE_DIR}/Managed/Assembly-CSharp.dll" "${AUTHOR_KIT_GAME_LIB_DIR}/"
find "${REFERENCE_DIR}/Managed" -maxdepth 1 -type f -name 'Unity*.dll' -exec cp {} "${AUTHOR_KIT_GAME_LIB_DIR}/" \;
cp "${HARNESS_ROOT}/docs/AUTHORING.md" "${AUTHOR_KIT_DIR}/README.md"
cp "${AUTHOR_MOD_TEMPLATE_DIR}/build.sh" "${AUTHOR_KIT_TEMPLATE_DIR}/"
cp "${AUTHOR_MOD_TEMPLATE_DIR}/ModTemplate.csproj" "${AUTHOR_KIT_TEMPLATE_DIR}/"
cp "${AUTHOR_MOD_TEMPLATE_DIR}/ModTemplate.cs" "${AUTHOR_KIT_TEMPLATE_DIR}/"
chmod +x "${AUTHOR_KIT_TEMPLATE_DIR}/build.sh"

log "Author kit exported to ${AUTHOR_KIT_DIR}"
