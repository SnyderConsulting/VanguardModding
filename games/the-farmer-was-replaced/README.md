# The Farmer Was Replaced

This folder contains Vanguard Modding's first published game harness: a BepInEx-based runtime and SDK for **The Farmer Was Replaced**.

Platform note:

- The harness itself is not tied to macOS, but the current automation has only been validated against the macOS Steam build so far.
- Override `TFWR_GAME_ROOT`, `TFWR_GAME_ENTRY_PATH`, `TFWR_GAME_DATA_DIR`, `TFWR_MANAGED_DIR`, `TFWR_BEPINEX_ARCHIVE`, or `TFWR_BEPINEX_URL` if your install layout differs.

What this harness provides:

- a shared runtime host for third-party mods
- a small SDK for external mod DLLs
- Harmony patch support through the harness
- author scaffolding and an exported author kit
- reverse-engineering/bootstrap scripts for setting up the modding surface

## For Players

If you only want compatible mods to work, start here:

1. Run `./scripts/bootstrap.sh` once.
2. Put compatible mod DLLs under `BepInEx/TFWR.ModHarness/mods` inside the game install.
3. Launch the game with `./scripts/run-game.sh`.
4. If you need logs, use `./scripts/tail-log.sh`.

Player notes:

- This repo is source-first. It sets up the harness and host plugin rather than acting like a one-click mod manager.
- Compatible mods are normal DLLs loaded from `BepInEx/TFWR.ModHarness/mods`.
- If a mod author ships dependency DLLs, keep them beside the mod DLL.
- `run-game.sh` writes `steam_appid.txt` with app id `2060160` so the game can launch outside the Steam bootstrapper.

More detail for players:

- `docs/PLAYERS.md`
- `docs/HOOKS.md`

## For Mod Authors

If you want to build mods against the harness:

1. Run `./scripts/bootstrap.sh`.
2. Create a standalone mod project with `./scripts/new-mod.sh MyMod`.
3. Build the generated project with its local `./build.sh`.
4. Drop the built DLL into `BepInEx/TFWR.ModHarness/mods`, or package it however you prefer.

Author notes:

- `./scripts/export-author-kit.sh` packages the SDK, Harmony, template, and local game references into `artifacts/author-kit`.
- `src/TFWR.ModHarness/` is the runtime host plugin.
- `src/TFWR.ModHarness.SDK/` is the author-facing contract surface.
- `templates/AuthorMod/` is the standalone starter template.

More detail for authors:

- `docs/AUTHORING.md`
- `docs/HOOKS.md`

## Repository Layout

- `scripts/`: install, build, deploy, decompile, and run helpers
- `src/TFWR.ModHarness/`: BepInEx host plugin that discovers external mods
- `src/TFWR.ModHarness.SDK/`: minimal SDK external mods compile against
- `src/ExampleHelloMod/`: sample third-party mod
- `templates/AuthorMod/`: standalone author template
- `docs/`: player and author guides

Generated locally, not committed:

- `artifacts/`: build output, caches, and exported author kits
- `references/`: synced game and BepInEx assemblies
- `decompiled/`: decompiled managed source
- `author-projects/`: generated standalone mod projects

## Current Findings

- The currently analyzed build is the macOS Steam build.
- That analyzed build is a Unity 6.0.0f1 Mono build, not IL2CPP.
- The main gameplay code lives in `Core.dll`, with support code in `Utils.dll`.
- The current author-facing hook surface is documented in `docs/HOOKS.md`.
- Additional game-side targets that were identified during reverse engineering include `Simulation`, `Execution`, `Farm`, and `GridManager`, but they are not yet wrapped as first-class SDK callbacks.
