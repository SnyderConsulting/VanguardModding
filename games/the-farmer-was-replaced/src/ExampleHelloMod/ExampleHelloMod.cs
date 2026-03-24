using System.Reflection;
using HarmonyLib;
using TFWR.ModHarness.SDK;

namespace ExampleHelloMod;

public sealed class ExampleHelloMod : TfwrModBase, ISimulationHooks, IExecutionHooks, IFarmHooks, IGridHooks
{
    internal static IModContext ActiveContext { get; private set; }

    public override string Id => "example.hello";

    public override string Name => "Example Hello Mod";

    public override string Version => "0.1.0";

    public override void Initialize(IModContext context)
    {
        base.Initialize(context);
        ActiveContext = context;
        Context.PatchAll();
        Context.Logger.Info("Initialized");
        Context.Logger.Info($"Drop third-party mod DLLs under {Context.ModsRootDirectory}");
    }

    public override void OnSceneLoaded(SceneEvent sceneEvent)
    {
        Context.Logger.Info($"Scene loaded: {sceneEvent.Name} ({sceneEvent.LoadMode})");
    }

    public override void OnMainSimReady()
    {
        Context.Logger.Info("MainSim is ready");
        Context.WriteSnapshot("example-main-sim-ready");
    }

    public override void OnMainExecutionStarted(MainExecutionStartedEvent executionEvent)
    {
        Context.Logger.Info($"Execution started: file={executionEvent.FileName}, executionId={executionEvent.ExecutionId}");
    }

    public override void OnMainExecutionStopped(MainExecutionStoppedEvent executionEvent)
    {
        Context.Logger.Info($"Execution stopped: executionId={executionEvent.ExecutionId}, simulating={executionEvent.IsSimulating}");
    }

    public override void OnWorkspaceReady(WorkspaceEvent workspaceEvent)
    {
        Context.Logger.Info($"Workspace ready: openWindows={workspaceEvent.OpenWindowCount}, codeWindows={workspaceEvent.CodeWindowCount}");
    }

    public override void OnCodeWindowOpened(CodeWindowEvent codeWindowEvent)
    {
        Context.Logger.Info($"Code window opened: {codeWindowEvent.FileName}");
    }

    public void OnSimulationCreated(SimulationCreatedEvent simulationEvent)
    {
        Context.Logger.Info(
            $"Simulation created: type={simulationEvent.LeaderboardType}, world={simulationEvent.WorldWidth}x{simulationEvent.WorldHeight}, unlocks={simulationEvent.UnlockCount}");
    }

    public void OnSimulationRestored(SimulationRestoredEvent simulationEvent)
    {
        Context.Logger.Info(
            $"Simulation restored: type={simulationEvent.LeaderboardType}, time={simulationEvent.CurrentTimeSeconds:0.###}, executing={simulationEvent.IsExecuting}");
    }

    public void OnSimulationSpeedChanged(SimulationSpeedChangedEvent simulationEvent)
    {
        Context.Logger.Info(
            $"Simulation speed changed: {simulationEvent.PreviousSpeedFactor:0.###} -> {simulationEvent.NewSpeedFactor:0.###}");
    }

    public void OnExecutionCreated(ExecutionCreatedEvent executionEvent)
    {
        Context.Logger.Info(
            $"Execution created: id={executionEvent.ExecutionId}, states={executionEvent.StateCount}, drones={executionEvent.ActiveDroneCount}, type={executionEvent.LeaderboardType}");
    }

    public void OnExecutionStopped(ExecutionStoppedEvent executionEvent)
    {
        Context.Logger.Info(
            $"Execution stopped: id={executionEvent.ExecutionId}, ops={executionEvent.GlobalOpCount:0.###}, paused={executionEvent.WasPaused}");
    }

    public void OnUnlockChanged(UnlockChangedEvent unlockEvent)
    {
        if (unlockEvent.PreviousLevel == 0)
        {
            Context.Logger.Info($"Unlock gained: {unlockEvent.UnlockName} -> level {unlockEvent.NewLevel}");
        }
    }

    public void OnDroneAdded(DroneAddedEvent droneEvent)
    {
        Context.Logger.Info($"Drone added: id={droneEvent.DroneId}, source={droneEvent.SourceDroneId}, active={droneEvent.ActiveDroneCount}");
    }

    public void OnDroneRemoved(DroneRemovedEvent droneEvent)
    {
        Context.Logger.Info($"Drone removed: id={droneEvent.DroneId}, active={droneEvent.ActiveDroneCount}");
    }

    public void OnWorldGenerated(WorldGeneratedEvent worldEvent)
    {
        Context.Logger.Info($"World generated: {worldEvent.WorldWidth}x{worldEvent.WorldHeight}, entities={worldEvent.EntityCount}");
    }

    public void OnGridObjectChanged(GridObjectChangedEvent gridEvent)
    {
    }

    public void OnGridSwapped(GridSwapEvent swapEvent)
    {
        Context.Logger.Info(
            $"Grid swap: ({swapEvent.SourceX},{swapEvent.SourceY}) <-> ({swapEvent.DestinationX},{swapEvent.DestinationY})");
    }

    public override void Shutdown()
    {
        Context.Logger.Info("Shutdown");
        ActiveContext = null;
    }
}

[HarmonyPatch]
internal static class ExampleHelloModPatches
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method("MainSim:SetupSim");
    }

    private static void Postfix()
    {
        ExampleHelloMod.ActiveContext?.Logger.Info("Harmony patch observed MainSim.SetupSim");
    }
}
