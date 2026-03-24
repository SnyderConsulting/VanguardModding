#!/usr/bin/env bash
set -euo pipefail

source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"

ensure_game_layout
ensure_dotnet

(
  cd "${HARNESS_ROOT}"
  dotnet tool restore >/dev/null
)

mkdir -p "${DECOMPILED_DIR}"

assemblies=(
  "Assembly-CSharp.dll"
  "Core.dll"
  "Utils.dll"
)

for assembly in "${assemblies[@]}"; do
  assembly_name="${assembly%.dll}"
  output_dir="${DECOMPILED_DIR}/${assembly_name}"
  rm -rf "${output_dir}"
  log "Decompiling ${assembly}"
  dotnet tool run ilspycmd -- \
    --disable-updatecheck \
    --nested-directories \
    --project \
    --outputdir "${output_dir}" \
    --referencepath "${MANAGED_DIR}" \
    "${MANAGED_DIR}/${assembly}"
done

log "Decompiled sources written to ${DECOMPILED_DIR}"
