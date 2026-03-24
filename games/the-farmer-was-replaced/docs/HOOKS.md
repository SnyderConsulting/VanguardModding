# Current Hook Surface

This document separates three things that were previously being conflated:

- game-side types we identified while reverse engineering
- host-level instrumentation patches already present in the harness
- SDK callbacks that mod authors can use directly without writing their own Harmony patch

## Current SDK Callbacks

The current public author-facing callback surface is:

- `Initialize(IModContext context)`
- `OnSceneLoaded(SceneEvent sceneEvent)`
- `OnMainSimReady()`
- `OnMainExecutionStarted(MainExecutionStartedEvent executionEvent)`
- `OnMainExecutionStopped(MainExecutionStoppedEvent executionEvent)`
- `OnWorkspaceReady(WorkspaceEvent workspaceEvent)`
- `OnCodeWindowOpened(CodeWindowEvent codeWindowEvent)`
- `OnUpdate()`
- `Shutdown()`

## Current Built-In Host Patches

The harness currently patches these game methods itself:

- `MainSim.SetupSim`
- `MainSim.StartMainExecution`
- `MainSim.StopMainExecution`
- `Workspace.Start`
- `Workspace.OpenCodeWindow`

These are the hooks that currently drive the built-in SDK callbacks listed above.

## Discovered Game-Side Targets

During reverse engineering, these types were identified as likely high-value targets for future work:

- `MainSim`
- `Simulation`
- `Execution`
- `Workspace`
- `Farm`
- `GridManager`

Not all of these are wrapped as first-class SDK callbacks yet.

At the moment:

- `MainSim` and `Workspace` are partially surfaced through the host's built-in callbacks
- `Simulation`, `Execution`, `Farm`, and `GridManager` remain discovered targets rather than dedicated SDK events
- authors can still reach those targets by adding their own Harmony patches through `Context.PatchAll()`
