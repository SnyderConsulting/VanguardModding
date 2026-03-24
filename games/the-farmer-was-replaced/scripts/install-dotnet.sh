#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
TMP_INSTALLER="$(mktemp -t tfwr-dotnet-install.XXXXXX.sh)"
trap 'rm -f "${TMP_INSTALLER}"' EXIT

curl -fsSL https://dot.net/v1/dotnet-install.sh -o "${TMP_INSTALLER}"
bash "${TMP_INSTALLER}" --channel "${TFWR_DOTNET_SDK_CHANNEL:-10.0}" --install-dir "${HOME}/.dotnet"
bash "${TMP_INSTALLER}" --channel "${TFWR_DOTNET_RUNTIME_CHANNEL:-8.0}" --runtime dotnet --install-dir "${HOME}/.dotnet"

printf '%s\n' "dotnet installed to ${HOME}/.dotnet"
