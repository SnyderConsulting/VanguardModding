using System;

namespace TFWR.ModHarness.SDK;

public interface IModLogger
{
    void Info(string message);

    void Warning(string message);

    void Error(string message);
}

public interface IModContext
{
    string Id { get; }

    string Name { get; }

    string Version { get; }

    string HarnessVersion { get; }

    string GameRootDirectory { get; }

    string HarnessRootDirectory { get; }

    string ModsRootDirectory { get; }

    string SdkDirectory { get; }

    string AssemblyPath { get; }

    string AssemblyDirectory { get; }

    string DataDirectory { get; }

    IModLogger Logger { get; }

    void PatchAll();

    void UnpatchSelf();

    void WriteSnapshot(string reason, bool includeHierarchy = false);
}

public interface ITfwrMod
{
    string Id { get; }

    string Name { get; }

    string Version { get; }

    void Initialize(IModContext context);

    void OnSceneLoaded(SceneEvent sceneEvent);

    void OnMainSimReady();

    void OnMainExecutionStarted(MainExecutionStartedEvent executionEvent);

    void OnMainExecutionStopped(MainExecutionStoppedEvent executionEvent);

    void OnWorkspaceReady(WorkspaceEvent workspaceEvent);

    void OnCodeWindowOpened(CodeWindowEvent codeWindowEvent);

    void OnUpdate();

    void Shutdown();
}

public interface ISimulationHooks
{
    void OnSimulationCreated(SimulationCreatedEvent simulationEvent);

    void OnSimulationRestored(SimulationRestoredEvent simulationEvent);

    void OnSimulationSpeedChanged(SimulationSpeedChangedEvent simulationEvent);
}

public interface IExecutionHooks
{
    void OnExecutionCreated(ExecutionCreatedEvent executionEvent);

    void OnExecutionStopped(ExecutionStoppedEvent executionEvent);
}

public interface IFarmHooks
{
    void OnUnlockChanged(UnlockChangedEvent unlockEvent);

    void OnDroneAdded(DroneAddedEvent droneEvent);

    void OnDroneRemoved(DroneRemovedEvent droneEvent);
}

public interface IGridHooks
{
    void OnWorldGenerated(WorldGeneratedEvent worldEvent);

    void OnGridObjectChanged(GridObjectChangedEvent gridEvent);

    void OnGridSwapped(GridSwapEvent swapEvent);
}

public abstract class TfwrModBase : ITfwrMod
{
    public abstract string Id { get; }

    public abstract string Name { get; }

    public abstract string Version { get; }

    protected IModContext Context { get; private set; }

    public virtual void Initialize(IModContext context)
    {
        Context = context;
    }

    public virtual void OnSceneLoaded(SceneEvent sceneEvent)
    {
    }

    public virtual void OnMainSimReady()
    {
    }

    public virtual void OnMainExecutionStarted(MainExecutionStartedEvent executionEvent)
    {
    }

    public virtual void OnMainExecutionStopped(MainExecutionStoppedEvent executionEvent)
    {
    }

    public virtual void OnWorkspaceReady(WorkspaceEvent workspaceEvent)
    {
    }

    public virtual void OnCodeWindowOpened(CodeWindowEvent codeWindowEvent)
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void Shutdown()
    {
    }
}

public sealed class SceneEvent
{
    public SceneEvent(string name, string loadMode, int rootCount, bool isLoaded)
    {
        Name = name ?? string.Empty;
        LoadMode = loadMode ?? string.Empty;
        RootCount = rootCount;
        IsLoaded = isLoaded;
    }

    public string Name { get; }

    public string LoadMode { get; }

    public int RootCount { get; }

    public bool IsLoaded { get; }
}

public sealed class MainExecutionStartedEvent
{
    public MainExecutionStartedEvent(string fileName, int executionId, double timeFactor)
    {
        FileName = fileName ?? string.Empty;
        ExecutionId = executionId;
        TimeFactor = timeFactor;
    }

    public string FileName { get; }

    public int ExecutionId { get; }

    public double TimeFactor { get; }
}

public sealed class MainExecutionStoppedEvent
{
    public MainExecutionStoppedEvent(int executionId, bool isSimulating)
    {
        ExecutionId = executionId;
        IsSimulating = isSimulating;
    }

    public int ExecutionId { get; }

    public bool IsSimulating { get; }
}

public sealed class WorkspaceEvent
{
    public WorkspaceEvent(int openWindowCount, int codeWindowCount)
    {
        OpenWindowCount = openWindowCount;
        CodeWindowCount = codeWindowCount;
    }

    public int OpenWindowCount { get; }

    public int CodeWindowCount { get; }
}

public sealed class CodeWindowEvent
{
    public CodeWindowEvent(string fileName)
    {
        FileName = fileName ?? string.Empty;
    }

    public string FileName { get; }
}

public sealed class SimulationCreatedEvent
{
    public SimulationCreatedEvent(
        string leaderboardType,
        string leaderboardName,
        string steamLeaderboardName,
        bool resetUnlocks,
        int unlockCount,
        int worldWidth,
        int worldHeight)
    {
        LeaderboardType = leaderboardType ?? string.Empty;
        LeaderboardName = leaderboardName ?? string.Empty;
        SteamLeaderboardName = steamLeaderboardName ?? string.Empty;
        ResetUnlocks = resetUnlocks;
        UnlockCount = unlockCount;
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
    }

    public string LeaderboardType { get; }

    public string LeaderboardName { get; }

    public string SteamLeaderboardName { get; }

    public bool ResetUnlocks { get; }

    public int UnlockCount { get; }

    public int WorldWidth { get; }

    public int WorldHeight { get; }
}

public sealed class SimulationRestoredEvent
{
    public SimulationRestoredEvent(string leaderboardType, double currentTimeSeconds, bool isExecuting, bool isPaused, int worldWidth, int worldHeight)
    {
        LeaderboardType = leaderboardType ?? string.Empty;
        CurrentTimeSeconds = currentTimeSeconds;
        IsExecuting = isExecuting;
        IsPaused = isPaused;
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
    }

    public string LeaderboardType { get; }

    public double CurrentTimeSeconds { get; }

    public bool IsExecuting { get; }

    public bool IsPaused { get; }

    public int WorldWidth { get; }

    public int WorldHeight { get; }
}

public sealed class SimulationSpeedChangedEvent
{
    public SimulationSpeedChangedEvent(
        string leaderboardType,
        double previousSpeedFactor,
        double newSpeedFactor,
        double previousOpDurationSeconds,
        double newOpDurationSeconds)
    {
        LeaderboardType = leaderboardType ?? string.Empty;
        PreviousSpeedFactor = previousSpeedFactor;
        NewSpeedFactor = newSpeedFactor;
        PreviousOpDurationSeconds = previousOpDurationSeconds;
        NewOpDurationSeconds = newOpDurationSeconds;
    }

    public string LeaderboardType { get; }

    public double PreviousSpeedFactor { get; }

    public double NewSpeedFactor { get; }

    public double PreviousOpDurationSeconds { get; }

    public double NewOpDurationSeconds { get; }
}

public sealed class ExecutionCreatedEvent
{
    public ExecutionCreatedEvent(
        int executionId,
        string leaderboardType,
        int stateCount,
        int activeDroneCount,
        bool isSingleDrone,
        bool isStepByStepMode,
        double currentTimeSeconds)
    {
        ExecutionId = executionId;
        LeaderboardType = leaderboardType ?? string.Empty;
        StateCount = stateCount;
        ActiveDroneCount = activeDroneCount;
        IsSingleDrone = isSingleDrone;
        IsStepByStepMode = isStepByStepMode;
        CurrentTimeSeconds = currentTimeSeconds;
    }

    public int ExecutionId { get; }

    public string LeaderboardType { get; }

    public int StateCount { get; }

    public int ActiveDroneCount { get; }

    public bool IsSingleDrone { get; }

    public bool IsStepByStepMode { get; }

    public double CurrentTimeSeconds { get; }
}

public sealed class ExecutionStoppedEvent
{
    public ExecutionStoppedEvent(
        int executionId,
        string leaderboardType,
        int stateCount,
        int activeDroneCount,
        double globalOpCount,
        bool wasPaused,
        double currentTimeSeconds)
    {
        ExecutionId = executionId;
        LeaderboardType = leaderboardType ?? string.Empty;
        StateCount = stateCount;
        ActiveDroneCount = activeDroneCount;
        GlobalOpCount = globalOpCount;
        WasPaused = wasPaused;
        CurrentTimeSeconds = currentTimeSeconds;
    }

    public int ExecutionId { get; }

    public string LeaderboardType { get; }

    public int StateCount { get; }

    public int ActiveDroneCount { get; }

    public double GlobalOpCount { get; }

    public bool WasPaused { get; }

    public double CurrentTimeSeconds { get; }
}

public sealed class UnlockChangedEvent
{
    public UnlockChangedEvent(string unlockName, int previousLevel, int newLevel)
    {
        UnlockName = unlockName ?? string.Empty;
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
    }

    public string UnlockName { get; }

    public int PreviousLevel { get; }

    public int NewLevel { get; }
}

public sealed class DroneAddedEvent
{
    public DroneAddedEvent(int droneId, int sourceDroneId, int activeDroneCount, int mainDroneId, int droneGeneration)
    {
        DroneId = droneId;
        SourceDroneId = sourceDroneId;
        ActiveDroneCount = activeDroneCount;
        MainDroneId = mainDroneId;
        DroneGeneration = droneGeneration;
    }

    public int DroneId { get; }

    public int SourceDroneId { get; }

    public int ActiveDroneCount { get; }

    public int MainDroneId { get; }

    public int DroneGeneration { get; }
}

public sealed class DroneRemovedEvent
{
    public DroneRemovedEvent(int droneId, bool wasMainDrone, int activeDroneCount, int mainDroneId, int droneGeneration)
    {
        DroneId = droneId;
        WasMainDrone = wasMainDrone;
        ActiveDroneCount = activeDroneCount;
        MainDroneId = mainDroneId;
        DroneGeneration = droneGeneration;
    }

    public int DroneId { get; }

    public bool WasMainDrone { get; }

    public int ActiveDroneCount { get; }

    public int MainDroneId { get; }

    public int DroneGeneration { get; }
}

public sealed class WorldGeneratedEvent
{
    public WorldGeneratedEvent(int worldWidth, int worldHeight, int groundCount, int entityCount, bool shrinkFarm)
    {
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
        GroundCount = groundCount;
        EntityCount = entityCount;
        ShrinkFarm = shrinkFarm;
    }

    public int WorldWidth { get; }

    public int WorldHeight { get; }

    public int GroundCount { get; }

    public int EntityCount { get; }

    public bool ShrinkFarm { get; }
}

public enum GridObjectLayer
{
    Ground,
    Entity
}

public enum GridObjectChangeKind
{
    Set,
    Removed
}

public sealed class GridObjectChangedEvent
{
    public GridObjectChangedEvent(
        GridObjectLayer layer,
        GridObjectChangeKind changeKind,
        int x,
        int y,
        string objectName,
        string previousObjectName,
        bool regrowGrass)
    {
        Layer = layer;
        ChangeKind = changeKind;
        X = x;
        Y = y;
        ObjectName = objectName ?? string.Empty;
        PreviousObjectName = previousObjectName ?? string.Empty;
        RegrowGrass = regrowGrass;
    }

    public GridObjectLayer Layer { get; }

    public GridObjectChangeKind ChangeKind { get; }

    public int X { get; }

    public int Y { get; }

    public string ObjectName { get; }

    public string PreviousObjectName { get; }

    public bool RegrowGrass { get; }
}

public sealed class GridSwapEvent
{
    public GridSwapEvent(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        string sourceObjectName,
        string destinationObjectName)
    {
        SourceX = sourceX;
        SourceY = sourceY;
        DestinationX = destinationX;
        DestinationY = destinationY;
        SourceObjectName = sourceObjectName ?? string.Empty;
        DestinationObjectName = destinationObjectName ?? string.Empty;
    }

    public int SourceX { get; }

    public int SourceY { get; }

    public int DestinationX { get; }

    public int DestinationY { get; }

    public string SourceObjectName { get; }

    public string DestinationObjectName { get; }
}
