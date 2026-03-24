#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

MOD_NAME="${1:-}"
TARGET_DIR="${2:-}"

[[ -n "${MOD_NAME}" ]] || die "usage: $(basename "$0") <ModName> [target-dir]"
[[ "${MOD_NAME}" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]] || die "mod name must be a valid C# identifier"

ensure_game_layout
ensure_dotnet
ensure_bepinex
"${SCRIPT_DIR}/sync-references.sh"
"${SCRIPT_DIR}/build-sdk.sh"

if [[ -z "${TARGET_DIR}" ]]; then
  TARGET_DIR="${AUTHOR_PROJECTS_DIR}/${MOD_NAME}"
fi

[[ ! -e "${TARGET_DIR}" ]] || die "target already exists: ${TARGET_DIR}"
[[ -d "${AUTHOR_MOD_TEMPLATE_DIR}" ]] || die "missing template directory: ${AUTHOR_MOD_TEMPLATE_DIR}"

mkdir -p "${TARGET_DIR}/lib/game"
cp "${AUTHOR_MOD_TEMPLATE_DIR}/build.sh" "${TARGET_DIR}/build.sh"
cp "${AUTHOR_MOD_TEMPLATE_DIR}/ModTemplate.csproj" "${TARGET_DIR}/${MOD_NAME}.csproj"
cp "${AUTHOR_MOD_TEMPLATE_DIR}/ModTemplate.cs" "${TARGET_DIR}/${MOD_NAME}.cs"
cp "${SDK_BUILD_DIR}/TFWR.ModHarness.SDK.dll" "${TARGET_DIR}/lib/"
if [[ -f "${SDK_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" ]]; then
  cp "${SDK_BUILD_DIR}/TFWR.ModHarness.SDK.pdb" "${TARGET_DIR}/lib/"
fi
cp "${REFERENCE_DIR}/BepInEx/core/0Harmony.dll" "${TARGET_DIR}/lib/"
cp "${REFERENCE_DIR}/Managed/Core.dll" "${TARGET_DIR}/lib/game/"
cp "${REFERENCE_DIR}/Managed/Utils.dll" "${TARGET_DIR}/lib/game/"
cp "${REFERENCE_DIR}/Managed/Assembly-CSharp.dll" "${TARGET_DIR}/lib/game/"
find "${REFERENCE_DIR}/Managed" -maxdepth 1 -type f -name 'Unity*.dll' -exec cp {} "${TARGET_DIR}/lib/game/" \;

MOD_ID="$(printf '%s' "${MOD_NAME}" | tr '[:upper:]' '[:lower:]')"

perl -0pi -e "s/__MOD_ASSEMBLY_NAME__/${MOD_NAME}/g; s/__MOD_ROOT_NAMESPACE__/${MOD_NAME}/g; s/__MOD_CLASS_NAME__/${MOD_NAME}/g; s/__MOD_ID__/${MOD_ID}/g; s/__MOD_DISPLAY_NAME__/${MOD_NAME}/g" "${TARGET_DIR}/build.sh" "${TARGET_DIR}/${MOD_NAME}.csproj" "${TARGET_DIR}/${MOD_NAME}.cs"
chmod +x "${TARGET_DIR}/build.sh"

log "New author mod scaffold created at ${TARGET_DIR}"
log "Build with: '${TARGET_DIR}/build.sh'"
