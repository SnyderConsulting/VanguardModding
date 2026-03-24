#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
HARNESS_ROOT="$(cd -- "${SCRIPT_DIR}/.." && pwd)"

DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export DOTNET_ROOT
export PATH="${DOTNET_ROOT}:${DOTNET_ROOT}/tools:${PATH}"

BEPINEX_VERSION="${TFWR_BEPINEX_VERSION:-5.4.23.4}"
STEAM_APP_ID="${TFWR_STEAM_APP_ID:-2060160}"

detect_host_platform() {
  case "$(uname -s)" in
    Darwin)
      printf '%s\n' "macos"
      ;;
    Linux)
      printf '%s\n' "linux"
      ;;
    MINGW*|MSYS*|CYGWIN*)
      printf '%s\n' "windows"
      ;;
    *)
      printf '%s\n' "unknown"
      ;;
  esac
}

HOST_PLATFORM="${TFWR_HOST_PLATFORM:-$(detect_host_platform)}"

case "${HOST_PLATFORM}" in
  macos)
    DEFAULT_GAME_ROOT="${HOME}/Library/Application Support/Steam/steamapps/common/The Farmer Was Replaced"
    DEFAULT_GAME_ENTRY_NAME="TheFarmerWasReplaced.app"
    DEFAULT_BEPINEX_ARCHIVE="BepInEx_macos_x64_${BEPINEX_VERSION}.zip"
    DEFAULT_RUN_BEPINEX_SCRIPT="run_bepinex.sh"
    ;;
  linux)
    DEFAULT_GAME_ROOT="${HOME}/.steam/steam/steamapps/common/The Farmer Was Replaced"
    DEFAULT_GAME_ENTRY_NAME="TheFarmerWasReplaced.x86_64"
    DEFAULT_BEPINEX_ARCHIVE="BepInEx_linux_x64_${BEPINEX_VERSION}.zip"
    DEFAULT_RUN_BEPINEX_SCRIPT="run_bepinex.sh"
    ;;
  *)
    DEFAULT_GAME_ROOT=""
    DEFAULT_GAME_ENTRY_NAME=""
    DEFAULT_BEPINEX_ARCHIVE=""
    DEFAULT_RUN_BEPINEX_SCRIPT=""
    ;;
esac

GAME_ROOT="${TFWR_GAME_ROOT:-$DEFAULT_GAME_ROOT}"
GAME_ENTRY_NAME="${TFWR_GAME_ENTRY_NAME:-$DEFAULT_GAME_ENTRY_NAME}"
GAME_ENTRY_PATH="${TFWR_GAME_ENTRY_PATH:-}"

if [[ -z "${GAME_ENTRY_PATH}" && -n "${GAME_ROOT}" && -n "${GAME_ENTRY_NAME}" ]]; then
  GAME_ENTRY_PATH="${GAME_ROOT}/${GAME_ENTRY_NAME}"
fi

if [[ -n "${TFWR_GAME_DATA_DIR:-}" ]]; then
  GAME_DATA_DIR="${TFWR_GAME_DATA_DIR}"
elif [[ "${HOST_PLATFORM}" == "macos" && -n "${GAME_ENTRY_PATH}" ]]; then
  GAME_DATA_DIR="${GAME_ENTRY_PATH}/Contents/Resources/Data"
elif [[ "${HOST_PLATFORM}" == "linux" && -n "${GAME_ROOT}" ]]; then
  GAME_DATA_DIR="${GAME_ROOT}/TheFarmerWasReplaced_Data"
else
  GAME_DATA_DIR=""
fi

MANAGED_DIR="${TFWR_MANAGED_DIR:-${GAME_DATA_DIR:+${GAME_DATA_DIR}/Managed}}"
GAME_APP_NAME="${GAME_ENTRY_NAME}"
GAME_APP_PATH="${GAME_ENTRY_PATH}"

BEPINEX_ARCHIVE="${TFWR_BEPINEX_ARCHIVE:-$DEFAULT_BEPINEX_ARCHIVE}"
BEPINEX_URL="${TFWR_BEPINEX_URL:-}"
if [[ -z "${BEPINEX_URL}" && -n "${BEPINEX_ARCHIVE}" ]]; then
  BEPINEX_URL="https://github.com/BepInEx/BepInEx/releases/download/v${BEPINEX_VERSION}/${BEPINEX_ARCHIVE}"
fi
BEPINEX_ROOT="${GAME_ROOT:+${GAME_ROOT}/BepInEx}"
BEPINEX_CORE_DIR="${BEPINEX_ROOT:+${BEPINEX_ROOT}/core}"
RUN_BEPINEX_SCRIPT="${TFWR_RUN_BEPINEX_SCRIPT:-}"
if [[ -z "${RUN_BEPINEX_SCRIPT}" && -n "${GAME_ROOT}" && -n "${DEFAULT_RUN_BEPINEX_SCRIPT}" ]]; then
  RUN_BEPINEX_SCRIPT="${GAME_ROOT}/${DEFAULT_RUN_BEPINEX_SCRIPT}"
fi
STEAM_APP_ID_FILE="${GAME_ROOT:+${GAME_ROOT}/steam_appid.txt}"

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
PLUGIN_DEPLOY_DIR="${GAME_ROOT:+${GAME_ROOT}/BepInEx/plugins/${PLUGIN_NAME}}"
HARNESS_RUNTIME_ROOT="${GAME_ROOT:+${GAME_ROOT}/BepInEx/TFWR.ModHarness}"
EXTERNAL_MODS_DEPLOY_DIR="${HARNESS_RUNTIME_ROOT:+${HARNESS_RUNTIME_ROOT}/mods}"
SDK_DEPLOY_DIR="${HARNESS_RUNTIME_ROOT:+${HARNESS_RUNTIME_ROOT}/sdk}"

die() {
  printf '%s\n' "error: $*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "missing required command: $1"
}

ensure_game_layout() {
  [[ -n "${GAME_ROOT}" ]] || die "game root is not configured. Set TFWR_GAME_ROOT."
  [[ -n "${GAME_ENTRY_PATH}" ]] || die "game entry path is not configured. Set TFWR_GAME_ENTRY_PATH or TFWR_GAME_ENTRY_NAME."
  [[ -n "${MANAGED_DIR}" ]] || die "managed assemblies path is not configured. Set TFWR_MANAGED_DIR or TFWR_GAME_DATA_DIR."
  [[ -d "${GAME_ROOT}" ]] || die "game root not found: ${GAME_ROOT}"
  [[ -e "${GAME_ENTRY_PATH}" ]] || die "game entry not found: ${GAME_ENTRY_PATH}"
  [[ -d "${MANAGED_DIR}" ]] || die "managed assemblies not found: ${MANAGED_DIR}"
}

ensure_dotnet() {
  require_command dotnet
}

ensure_bepinex() {
  [[ -n "${BEPINEX_CORE_DIR}" ]] || die "BepInEx core path is not configured."
  [[ -f "${BEPINEX_CORE_DIR}/BepInEx.dll" ]] || die "BepInEx is not installed. Run scripts/install-bepinex.sh first."
  [[ -f "${BEPINEX_CORE_DIR}/0Harmony.dll" ]] || die "Harmony was not found in ${BEPINEX_CORE_DIR}"
}

ensure_steam_appid() {
  [[ -n "${STEAM_APP_ID_FILE}" ]] || die "steam app id path is not configured."
  printf '%s\n' "${STEAM_APP_ID}" > "${STEAM_APP_ID_FILE}"
}

log() {
  printf '%s\n' "$*"
}
