# Current Hook Surface

This document lists the hook surface that is actually exposed by the harness.

All harness callbacks are delivered on the Unity main thread. That includes hooks that originate from the simulation thread, such as farm and grid mutations.

## Core Mod Lifecycle

Every mod implements `ITfwrMod`, typically through `TfwrModBase`:

- `Initialize(IModContext context)`
- `OnSceneLoaded(SceneEvent sceneEvent)`
- `OnMainSimReady()`
- `OnMainExecutionStarted(MainExecutionStartedEvent executionEvent)`
- `OnMainExecutionStopped(MainExecutionStoppedEvent executionEvent)`
- `OnWorkspaceReady(WorkspaceEvent workspaceEvent)`
- `OnCodeWindowOpened(CodeWindowEvent codeWindowEvent)`
- `OnUpdate()`
- `Shutdown()`

## Optional Hook Interfaces

Implement these interfaces only when you want those callbacks:

### `ISimulationHooks`

- `OnSimulationCreated(SimulationCreatedEvent simulationEvent)`
- `OnSimulationRestored(SimulationRestoredEvent simulationEvent)`
- `OnSimulationSpeedChanged(SimulationSpeedChangedEvent simulationEvent)`

### `IExecutionHooks`

- `OnExecutionCreated(ExecutionCreatedEvent executionEvent)`
- `OnExecutionStopped(ExecutionStoppedEvent executionEvent)`

### `IFarmHooks`

- `OnUnlockChanged(UnlockChangedEvent unlockEvent)`
- `OnDroneAdded(DroneAddedEvent droneEvent)`
- `OnDroneRemoved(DroneRemovedEvent droneEvent)`

### `IGridHooks`

- `OnWorldGenerated(WorldGeneratedEvent worldEvent)`
- `OnGridObjectChanged(GridObjectChangedEvent gridEvent)`
- `OnGridSwapped(GridSwapEvent swapEvent)`

## Built-In Host Patches

The harness currently patches these game methods itself:

- `MainSim.SetupSim`
- `MainSim.StartMainExecution`
- `MainSim.StopMainExecution`
- `MainSim.RestoreMainSim`
- `Workspace.Start`
- `Workspace.OpenCodeWindow`
- `Simulation..ctor(...)`
- `Simulation.ChangeExecutionSpeed`
- `Simulation.StartProgramExecution`
- `Simulation.StopProgramExecution`
- `Farm.Unlock`
- `Farm.AddDrone`
- `Farm.RemoveDrone`
- `Farm.RemoveSpawnedDrones`
- `GridManager.GenerateWorld`
- `GridManager.SetGround`
- `GridManager.SetEntity`
- `GridManager.RemoveEntity`
- `GridManager.Swap`

These are the patches that currently drive the public hook surface above.
