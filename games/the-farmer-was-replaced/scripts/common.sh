#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
HARNESS_ROOT="$(cd -- "${SCRIPT_DIR}/.." && pwd)"

DEFAULT_GAME_ROOT="${HOME}/Library/Application Support/Steam/steamapps/common/The Farmer Was Replaced"
GAME_ROOT="${TFWR_GAME_ROOT:-$DEFAULT_GAME_ROOT}"
GAME_APP_NAME="${TFWR_GAME_APP_NAME:-TheFarmerWasReplaced.app}"
GAME_APP_PATH="${GAME_ROOT}/${GAME_APP_NAME}"
GAME_DATA_DIR="${GAME_APP_PATH}/Contents/Resources/Data"
MANAGED_DIR="${GAME_DATA_DIR}/Managed"

DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export DOTNET_ROOT
export PATH="${DOTNET_ROOT}:${DOTNET_ROOT}/tools:${PATH}"

BEPINEX_VERSION="${TFWR_BEPINEX_VERSION:-5.4.23.4}"
BEPINEX_ARCHIVE="BepInEx_macos_x64_${BEPINEX_VERSION}.zip"
BEPINEX_URL="https://github.com/BepInEx/BepInEx/releases/download/v${BEPINEX_VERSION}/${BEPINEX_ARCHIVE}"
BEPINEX_ROOT="${GAME_ROOT}/BepInEx"
BEPINEX_CORE_DIR="${BEPINEX_ROOT}/core"
RUN_BEPINEX_SCRIPT="${GAME_ROOT}/run_bepinex.sh"
STEAM_APP_ID="${TFWR_STEAM_APP_ID:-2060160}"
STEAM_APP_ID_FILE="${GAME_ROOT}/steam_appid.txt"

ARTIFACTS_DIR="${HARNESS_ROOT}/artifacts"
CACHE_DIR="${ARTIFACTS_DIR}/cache"
BUILD_DIR="${ARTIFACTS_DIR}/build"
DEPLOY_DIR="${ARTIFACTS_DIR}/deploy"
REFERENCE_DIR="${HARNESS_ROOT}/references"
DECOMPILED_DIR="${HARNESS_ROOT}/decompiled"
TEMPLATES_DIR="${HARNESS_ROOT}/templates"
AUTHOR_MOD_TEMPLATE_DIR="${TEMPLATES_DIR}/AuthorMod"
PROJECT_PATH="${HARNESS_ROOT}/src/TFWR.ModHarness/TFWR.ModHarness.csproj"
SDK_PROJECT_PATH="${HARNESS_ROOT}/src/TFWR.ModHarness.SDK/TFWR.ModHarness.SDK.csproj"
EXAMPLE_MOD_PROJECT_PATH="${HARNESS_ROOT}/src/ExampleHelloMod/ExampleHelloMod.csproj"
PLUGIN_NAME="TFWR.ModHarness"
PLUGIN_BUILD_DIR="${BUILD_DIR}/${PLUGIN_NAME}"
SDK_BUILD_DIR="${BUILD_DIR}/TFWR.ModHarness.SDK"
EXAMPLE_MOD_BUILD_DIR="${BUILD_DIR}/ExampleHelloMod"
AUTHOR_KIT_DIR="${ARTIFACTS_DIR}/author-kit"
AUTHOR_KIT_LIB_DIR="${AUTHOR_KIT_DIR}/lib"
AUTHOR_KIT_GAME_LIB_DIR="${AUTHOR_KIT_LIB_DIR}/game"
AUTHOR_KIT_TEMPLATE_DIR="${AUTHOR_KIT_DIR}/template"
AUTHOR_PROJECTS_DIR="${HARNESS_ROOT}/author-projects"
PLUGIN_DEPLOY_DIR="${GAME_ROOT}/BepInEx/plugins/${PLUGIN_NAME}"
HARNESS_RUNTIME_ROOT="${GAME_ROOT}/BepInEx/TFWR.ModHarness"
EXTERNAL_MODS_DEPLOY_DIR="${HARNESS_RUNTIME_ROOT}/mods"
SDK_DEPLOY_DIR="${HARNESS_RUNTIME_ROOT}/sdk"

die() {
  printf '%s\n' "error: $*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "missing required command: $1"
}

ensure_game_layout() {
  [[ -d "${GAME_ROOT}" ]] || die "game root not found: ${GAME_ROOT}"
  [[ -d "${GAME_APP_PATH}" ]] || die "game app not found: ${GAME_APP_PATH}"
  [[ -d "${MANAGED_DIR}" ]] || die "managed assemblies not found: ${MANAGED_DIR}"
}

ensure_dotnet() {
  require_command dotnet
}

ensure_bepinex() {
  [[ -f "${BEPINEX_CORE_DIR}/BepInEx.dll" ]] || die "BepInEx is not installed. Run scripts/install-bepinex.sh first."
  [[ -f "${BEPINEX_CORE_DIR}/0Harmony.dll" ]] || die "Harmony was not found in ${BEPINEX_CORE_DIR}"
}

ensure_steam_appid() {
  printf '%s\n' "${STEAM_APP_ID}" > "${STEAM_APP_ID_FILE}"
}

log() {
  printf '%s\n' "$*"
}
