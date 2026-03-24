# Current Hook Surface

This document separates two things that should stay aligned:

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
