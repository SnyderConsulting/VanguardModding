# Authoring Mods Against TFWR.ModHarness

The harness is the host. Third-party mods are normal `.dll` files that implement `TFWR.ModHarness.SDK.ITfwrMod` and are dropped under `BepInEx/TFWR.ModHarness/mods`.

The current automation was validated against the macOS Steam build, but the SDK and external mod model are not inherently macOS-specific.

## What the harness provides

- A single BepInEx host plugin installed once per game install
- A small SDK with lifecycle callbacks, logging, data directories, and snapshot helpers
- Built-in lifecycle callbacks plus optional Simulation, Execution, Farm, and Grid hook interfaces
- Harmony patch support through `IModContext.PatchAll()`
- A dedicated runtime folder for third-party mods, shared SDK binaries, and per-mod data

## Recommended author workflow

1. Run `./scripts/bootstrap.sh` once to install the local toolchain, BepInEx, and synced references.
2. Run `./scripts/new-mod.sh MyCoolMod` to create a standalone author project.
3. Build the project with `./build.sh`.
4. Copy the built `.dll` into `BepInEx/TFWR.ModHarness/mods`, or use the generated project as the basis for your own packaging script.

## Scaffolded project layout

Each generated author project contains:

- `MyCoolMod.csproj`: references the harness SDK and Harmony
- `MyCoolMod.cs`: minimal mod entry point plus a sample Harmony patch
- `build.sh`: local build helper that uses the same user-space .NET install as the harness scripts
- `lib/TFWR.ModHarness.SDK.dll`: compile-time SDK reference
- `lib/0Harmony.dll`: compile-time Harmony reference
- `lib/game/`: optional compile-time game assemblies copied from the local install

## When to reference game assemblies

You only need `lib/game/*.dll` when your mod wants compile-time access to shipped game types such as `MainSim` or `Workspace`.

If you want to stay resilient to minor game updates, you can also patch by string with `AccessTools.Method("TypeName:MethodName")` and avoid compile-time references to the game's own assemblies.

## Lifecycle contract

- `Initialize(IModContext context)`: first entry point for startup work
- `OnSceneLoaded(SceneEvent sceneEvent)`: called on Unity scene load
- `OnMainSimReady()`: called after `MainSim.SetupSim`
- `OnMainExecutionStarted(MainExecutionStartedEvent executionEvent)`: called when `MainSim.StartMainExecution` begins
- `OnMainExecutionStopped(MainExecutionStoppedEvent executionEvent)`: called after `MainSim.StopMainExecution`
- `OnWorkspaceReady(WorkspaceEvent workspaceEvent)`: called after `Workspace.Start`
- `OnCodeWindowOpened(CodeWindowEvent codeWindowEvent)`: called after `Workspace.OpenCodeWindow`
- `OnUpdate()`: called every frame
- `Shutdown()`: called when the host is unloading

Optional grouped interfaces:

- `ISimulationHooks`: simulation creation, restore, and speed changes
- `IExecutionHooks`: execution lifecycle events beyond the main code-window run callback
- `IFarmHooks`: unlock and drone lifecycle events
- `IGridHooks`: world generation, grid-object mutation, and swap events

Harness callbacks are delivered on the Unity main thread, including farm/grid hooks that originate from the background simulation loop.

## Harmony usage

Call `Context.PatchAll()` in `Initialize()` if your assembly contains `[HarmonyPatch]` classes. The host allocates a dedicated Harmony id per mod and automatically calls `UnpatchSelf()` during shutdown.

## Hook Surface Notes

The current public hook surface is documented in `docs/HOOKS.md`.
