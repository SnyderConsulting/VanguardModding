# Using TFWR.ModHarness As A Player

This harness is the shared runtime layer for mods that target **The Farmer Was Replaced**.

It does not add gameplay on its own. You use it because another mod depends on it, or because you want a stable way to load compatible external mods.

## Basic Setup

1. Make sure the game is installed locally.
2. Run `./scripts/bootstrap.sh` from this game folder once.
3. Place compatible mod DLLs under `BepInEx/TFWR.ModHarness/mods` in the game install.
4. Start the game with `./scripts/run-game.sh`.

## Where Mods Go

The harness loads third-party mod DLLs from:

- `BepInEx/TFWR.ModHarness/mods`

If a mod ships extra dependency DLLs, keep them in the same mod folder.

## Useful Commands

- `./scripts/run-game.sh`: launches the game through the harness
- `./scripts/tail-log.sh`: follows the BepInEx log output
- `./scripts/deploy-example-mod.sh`: installs the sample external mod for testing

## Troubleshooting

- If the game was installed somewhere else, set `TFWR_GAME_ROOT` before running the scripts.
- If a mod does not load, check `./scripts/tail-log.sh` first.
- If the game was launched directly through Steam before setup, re-run `./scripts/bootstrap.sh` and then `./scripts/run-game.sh`.
