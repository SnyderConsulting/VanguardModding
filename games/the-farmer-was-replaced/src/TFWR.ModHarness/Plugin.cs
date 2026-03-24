using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TFWR.ModHarness.SDK;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TFWR.ModHarness;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    internal const string PluginGuid = "com.vanguardmodding.tfwr.modharness";
    internal const string PluginName = "TFWR Mod Harness";
    internal const string PluginVersion = "0.3.0";

    private Harmony _harmony;

    internal static Plugin Instance { get; private set; }

    internal static ManualLogSource LogSource => Instance.Logger;

    internal static ModHost Host { get; private set; }

    internal static string SnapshotDirectory => Path.Combine(Paths.ConfigPath, "TFWR.ModHarness", "snapshots");

    private void Awake()
    {
        Instance = this;
        Directory.CreateDirectory(SnapshotDirectory);

        Host = new ModHost(this);
        Host.LoadMods();

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        GameObject runtimeHost = new GameObject("TFWR.ModHarness.Runtime");
        runtimeHost.hideFlags = HideFlags.HideAndDontSave;
        UnityEngine.Object.DontDestroyOnLoad(runtimeHost);
        runtimeHost.AddComponent<HarnessBehaviour>();

        LogSource.LogInfo($"{PluginName} {PluginVersion} loaded");
    }

    private void OnDestroy()
    {
        Host?.Shutdown();
        _harmony?.UnpatchSelf();
    }

    internal static void WriteSnapshot(string reason, bool includeHierarchy)
    {
        try
        {
            Directory.CreateDirectory(SnapshotDirectory);

            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            string safeReason = SanitizeFileName(reason);
            string suffix = includeHierarchy ? "state-hierarchy" : "state";
            string snapshotPath = Path.Combine(SnapshotDirectory, $"{timestamp}-{safeReason}-{suffix}.txt");

            File.WriteAllText(snapshotPath, BuildSnapshot(reason, includeHierarchy), Encoding.UTF8);
            LogSource.LogInfo($"Snapshot written: {snapshotPath}");
        }
        catch (Exception ex)
        {
            LogSource.LogError($"Failed to write snapshot for '{reason}': {ex}");
        }
    }

    internal static void LogInfo(string message)
    {
        LogSource.LogInfo(message);
    }

    internal static void LogWarning(string message)
    {
        LogSource.LogWarning(message);
    }

    internal static void LogError(string message)
    {
        LogSource.LogError(message);
    }

    private static string BuildSnapshot(string reason, bool includeHierarchy)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"reason: {reason}");
        builder.AppendLine($"time: {DateTime.Now:O}");
        builder.AppendLine($"activeScene: {SceneManager.GetActiveScene().name}");
        builder.AppendLine($"sceneCount: {SceneManager.sceneCount}");
        builder.AppendLine($"externalModsDirectory: {Host?.ExternalModsDirectory}");
        builder.AppendLine($"loadedMods: {Host?.LoadedMods.Count ?? 0}");
        if (Host != null)
        {
            builder.AppendLine("mods:");
            foreach (LoadedMod loadedMod in Host.LoadedMods.OrderBy(mod => mod.Id, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"  - {loadedMod.Id} | {loadedMod.Name} | {loadedMod.Version}");
            }
        }
        builder.AppendLine("scenes:");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            builder.AppendLine($"  - {scene.name} (loaded={scene.isLoaded}, roots={scene.rootCount})");
        }

        MainSim mainSim = MainSim.Inst;
        builder.AppendLine($"mainSimPresent: {mainSim != null}");
        if (mainSim != null)
        {
            try
            {
                Vector2Int worldSize = mainSim.GetWorldSize();
                Dictionary<string, int> unlocks = mainSim.GetUnlocks();

                builder.AppendLine($"isExecuting: {mainSim.IsExecuting()}");
                builder.AppendLine($"isSimulating: {mainSim.IsSimulating()}");
                builder.AppendLine($"timeFactor: {mainSim.TimeFactor}");
                builder.AppendLine($"executionId: {mainSim.executionId}");
                builder.AppendLine($"currentTimeSeconds: {mainSim.GetCurrentTime().Seconds}");
                builder.AppendLine($"worldSize: {worldSize.x}x{worldSize.y}");
                builder.AppendLine($"hoveredCell: {mainSim.hoveredCell.x},{mainSim.hoveredCell.y}");
                builder.AppendLine($"unlockCount: {unlocks.Count}");
                builder.AppendLine("unlocks:");
                foreach (KeyValuePair<string, int> unlock in unlocks.OrderBy(pair => pair.Key))
                {
                    builder.AppendLine($"  - {unlock.Key}={unlock.Value}");
                }

                Workspace workspace = mainSim.workspace;
                builder.AppendLine($"workspacePresent: {workspace != null}");
                if (workspace != null)
                {
                    builder.AppendLine($"openWindows: {workspace.openWindows.Count}");
                    builder.AppendLine($"codeWindows: {workspace.codeWindows.Count}");
                    builder.AppendLine("windowNames:");
                    foreach (string name in workspace.openWindows.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
                    {
                        builder.AppendLine($"  - {name}");
                    }
                }
            }
            catch (Exception ex)
            {
                builder.AppendLine($"mainSimStateError: {ex.GetType().Name}: {ex.Message}");
            }
        }

        builder.AppendLine("assemblies:");
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(assembly => assembly.GetName().Name))
        {
            builder.AppendLine($"  - {assembly.GetName().Name}");
        }

        if (includeHierarchy)
        {
            builder.AppendLine("hierarchy:");
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                builder.AppendLine($"scene {scene.name}:");
                foreach (GameObject rootObject in scene.GetRootGameObjects().OrderBy(gameObject => gameObject.name, StringComparer.Ordinal))
                {
                    AppendTransform(builder, rootObject.transform, 1);
                }
            }
        }

        return builder.ToString();
    }

    private static void AppendTransform(StringBuilder builder, Transform current, int depth)
    {
        builder.Append(' ', depth * 2);
        builder.AppendLine($"- {current.name} [{current.GetType().Name}]");
        for (int i = 0; i < current.childCount; i++)
        {
            AppendTransform(builder, current.GetChild(i), depth + 1);
        }
    }

    private static string SanitizeFileName(string value)
    {
        StringBuilder builder = new StringBuilder(value.Length);
        HashSet<char> invalidChars = Path.GetInvalidFileNameChars().ToHashSet();

        foreach (char character in value)
        {
            builder.Append(invalidChars.Contains(character) ? '_' : character);
        }

        return builder.ToString();
    }
}

internal sealed class HarnessBehaviour : MonoBehaviour
{
    private bool _capturedInitialSnapshot;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        Plugin.LogInfo("Harness runtime active. F6 dumps state, F7 dumps state plus hierarchy.");
    }

    private void Update()
    {
        if (!_capturedInitialSnapshot && SceneManager.sceneCount > 0)
        {
            _capturedInitialSnapshot = true;
            Plugin.WriteSnapshot("initial-frame", includeHierarchy: false);
        }

        Plugin.Host?.UpdateMods();

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Plugin.WriteSnapshot("manual-f6", includeHierarchy: false);
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            Plugin.WriteSnapshot("manual-f7", includeHierarchy: true);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Plugin.LogInfo($"Scene loaded: {scene.name} ({mode})");
        Plugin.WriteSnapshot($"scene-{scene.name}", includeHierarchy: false);
        Plugin.Host?.NotifySceneLoaded(scene, mode);
    }
}

[HarmonyPatch(typeof(MainSim), nameof(MainSim.SetupSim))]
internal static class MainSimSetupSimPatch
{
    private static void Postfix(MainSim __instance)
    {
        Vector2Int worldSize = __instance.GetWorldSize();
        Plugin.LogInfo($"MainSim.SetupSim completed: world={worldSize.x}x{worldSize.y}, unlocks={__instance.GetUnlocks().Count}");
        Plugin.WriteSnapshot("setup-sim", includeHierarchy: false);
        Plugin.Host?.NotifyMainSimReady();
    }
}

[HarmonyPatch(typeof(MainSim), nameof(MainSim.StartMainExecution))]
internal static class MainSimStartMainExecutionPatch
{
    private static void Prefix(MainSim __instance, CodeWindow cw)
    {
        string fileName = cw != null ? cw.fileName : "<null>";
        Plugin.LogInfo($"MainSim.StartMainExecution: file={fileName}, executionId={__instance.executionId}, timeFactor={__instance.TimeFactor}");
        Plugin.Host?.NotifyMainExecutionStarted(fileName, __instance.executionId, __instance.TimeFactor);
    }
}

[HarmonyPatch(typeof(MainSim), nameof(MainSim.StopMainExecution))]
internal static class MainSimStopMainExecutionPatch
{
    private static void Postfix(MainSim __instance)
    {
        Plugin.LogInfo($"MainSim.StopMainExecution: executionId={__instance.executionId}, simulating={__instance.IsSimulating()}");
        Plugin.Host?.NotifyMainExecutionStopped(__instance.executionId, __instance.IsSimulating());
    }
}

[HarmonyPatch(typeof(Workspace), "Start")]
internal static class WorkspaceStartPatch
{
    private static void Postfix(Workspace __instance)
    {
        Plugin.LogInfo($"Workspace.Start: openWindows={__instance.openWindows.Count}, codeWindows={__instance.codeWindows.Count}");
        Plugin.Host?.NotifyWorkspaceReady(__instance.openWindows.Count, __instance.codeWindows.Count);
    }
}

[HarmonyPatch(typeof(Workspace), nameof(Workspace.OpenCodeWindow))]
internal static class WorkspaceOpenCodeWindowPatch
{
    private static void Postfix(string fileName)
    {
        Plugin.LogInfo($"Workspace.OpenCodeWindow: {fileName}");
        Plugin.Host?.NotifyCodeWindowOpened(fileName);
    }
}

[HarmonyPatch(typeof(MainSim), nameof(MainSim.RestoreMainSim))]
internal static class MainSimRestoreMainSimPatch
{
    private static void Postfix(Simulation ___sim)
    {
        if (___sim?.farm?.grid == null)
        {
            return;
        }

        Vector2Int worldSize = ___sim.farm.grid.WorldSize;
        Plugin.LogInfo($"MainSim.RestoreMainSim: world={worldSize.x}x{worldSize.y}, executing={___sim.IsExecuting()}");
        Plugin.Host?.NotifySimulationRestored(
            ___sim.leaderboardType.ToString(),
            ___sim.CurrentTime.Seconds,
            ___sim.IsExecuting(),
            ___sim.Paused,
            worldSize.x,
            worldSize.y);
    }
}

[HarmonyPatch]
internal static class SimulationConstructorPatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Constructor(
            typeof(Simulation),
            new[]
            {
                typeof(MainSim),
                typeof(IEnumerable<string>),
                typeof(ItemBlock),
                typeof(string),
                typeof(string),
                typeof(LeaderboardType),
                typeof(int),
                typeof(List<SFO>),
                typeof(List<SFO>),
                typeof(bool)
            });
    }

    private static void Postfix(
        Simulation __instance,
        string leaderboardName,
        string steamLeaderboardName,
        LeaderboardType leaderboardType,
        bool resetUnlocks)
    {
        if (__instance?.farm?.grid == null)
        {
            return;
        }

        Vector2Int worldSize = __instance.farm.grid.WorldSize;
        Plugin.LogInfo($"Simulation created: type={leaderboardType}, world={worldSize.x}x{worldSize.y}, unlocks={__instance.farm.GetUnlocks().Count}");
        Plugin.Host?.NotifySimulationCreated(
            leaderboardType.ToString(),
            leaderboardName,
            steamLeaderboardName,
            resetUnlocks,
            __instance.farm.GetUnlocks().Count,
            worldSize.x,
            worldSize.y);
    }
}

[HarmonyPatch(typeof(Simulation), nameof(Simulation.ChangeExecutionSpeed))]
internal static class SimulationChangeExecutionSpeedPatch
{
    private static void Prefix(Simulation __instance, out SimulationSpeedChangeState __state)
    {
        __state = new SimulationSpeedChangeState(__instance.SpeedFactor, __instance.OpDuration.Seconds);
    }

    private static void Postfix(Simulation __instance, SimulationSpeedChangeState __state)
    {
        if (__state == null)
        {
            return;
        }

        Plugin.LogInfo($"Simulation.ChangeExecutionSpeed: {__state.PreviousSpeedFactor} -> {__instance.SpeedFactor}");
        Plugin.Host?.NotifySimulationSpeedChanged(
            __instance.leaderboardType.ToString(),
            __state.PreviousSpeedFactor,
            __instance.SpeedFactor,
            __state.PreviousOpDurationSeconds,
            __instance.OpDuration.Seconds);
    }
}

[HarmonyPatch(typeof(Simulation), nameof(Simulation.StartProgramExecution))]
internal static class SimulationStartProgramExecutionPatch
{
    private static void Postfix(Simulation __instance, Execution execution)
    {
        if (execution == null)
        {
            return;
        }

        int activeDroneCount = __instance.farm?.drones?.Count(drone => drone != null) ?? 0;
        Plugin.LogInfo($"Simulation.StartProgramExecution: executionId={execution.Id}, states={execution.States.Count}, drones={activeDroneCount}");
        Plugin.Host?.NotifyExecutionCreated(
            execution.Id,
            __instance.leaderboardType.ToString(),
            execution.States.Count,
            activeDroneCount,
            __instance.singleDrone,
            __instance.stepByStepMode,
            __instance.CurrentTime.Seconds);
    }
}

[HarmonyPatch(typeof(Simulation), nameof(Simulation.StopProgramExecution))]
internal static class SimulationStopProgramExecutionPatch
{
    private static void Prefix(Simulation __instance, out ExecutionStopState __state)
    {
        Execution execution = __instance.Execution;
        __state = execution == null
            ? null
            : new ExecutionStopState(
                execution.Id,
                __instance.leaderboardType.ToString(),
                execution.States.Count,
                __instance.farm?.drones?.Count(drone => drone != null) ?? 0,
                execution.GlobalOpCount,
                __instance.Paused,
                __instance.CurrentTime.Seconds);
    }

    private static void Postfix(ExecutionStopState __state)
    {
        if (__state == null)
        {
            return;
        }

        Plugin.LogInfo($"Simulation.StopProgramExecution: executionId={__state.ExecutionId}, states={__state.StateCount}, globalOps={__state.GlobalOpCount}");
        Plugin.Host?.NotifyExecutionStopped(
            __state.ExecutionId,
            __state.LeaderboardType,
            __state.StateCount,
            __state.ActiveDroneCount,
            __state.GlobalOpCount,
            __state.WasPaused,
            __state.CurrentTimeSeconds);
    }
}

[HarmonyPatch(typeof(Farm), nameof(Farm.Unlock))]
internal static class FarmUnlockPatch
{
    private static void Prefix(Farm __instance, string s, out int __state)
    {
        __state = string.IsNullOrWhiteSpace(s) ? 0 : __instance.NumUnlocked(s.ToLowerInvariant());
    }

    private static void Postfix(Farm __instance, string s, int __state)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return;
        }

        string unlockName = s.ToLowerInvariant();
        int newLevel = __instance.NumUnlocked(unlockName);
        if (newLevel == __state)
        {
            return;
        }

        Plugin.LogInfo($"Farm.Unlock: {unlockName} {__state} -> {newLevel}");
        Plugin.Host?.NotifyUnlockChanged(unlockName, __state, newLevel);
    }
}

[HarmonyPatch(typeof(Farm), nameof(Farm.AddDrone))]
internal static class FarmAddDronePatch
{
    private static void Postfix(Farm __instance, int droneId, int __result)
    {
        int activeDroneCount = __instance.drones.Count(drone => drone != null);
        Plugin.LogInfo($"Farm.AddDrone: source={droneId}, new={__result}, active={activeDroneCount}");
        Plugin.Host?.NotifyDroneAdded(__result, droneId, activeDroneCount, __instance.mainDroneId, __instance.droneGeneration);
    }
}

[HarmonyPatch(typeof(Farm), nameof(Farm.RemoveDrone))]
internal static class FarmRemoveDronePatch
{
    private static void Prefix(Farm __instance, int droneId, out DroneRemovalState __state)
    {
        bool exists = droneId >= 0 && droneId < __instance.drones.Count && __instance.drones[droneId] != null;
        __state = exists ? new DroneRemovalState(droneId, droneId == __instance.mainDroneId, __instance.droneGeneration) : null;
    }

    private static void Postfix(Farm __instance, DroneRemovalState __state)
    {
        if (__state == null)
        {
            return;
        }

        int activeDroneCount = __instance.drones.Count(drone => drone != null);
        Plugin.LogInfo($"Farm.RemoveDrone: drone={__state.DroneId}, active={activeDroneCount}");
        Plugin.Host?.NotifyDroneRemoved(__state.DroneId, __state.WasMainDrone, activeDroneCount, __instance.mainDroneId, __state.DroneGeneration);
    }
}

[HarmonyPatch(typeof(Farm), nameof(Farm.RemoveSpawnedDrones))]
internal static class FarmRemoveSpawnedDronesPatch
{
    private static void Prefix(Farm __instance, out SpawnedDroneRemovalState __state)
    {
        __state = new SpawnedDroneRemovalState(__instance.droneGeneration);
        for (int i = 0; i < __instance.drones.Count; i++)
        {
            if (i != __instance.mainDroneId && __instance.drones[i] != null)
            {
                __state.RemovedDroneIds.Add(i);
            }
        }
    }

    private static void Postfix(Farm __instance, SpawnedDroneRemovalState __state)
    {
        if (__state == null || __state.RemovedDroneIds.Count == 0)
        {
            return;
        }

        int activeDroneCount = __instance.drones.Count(drone => drone != null);
        foreach (int droneId in __state.RemovedDroneIds)
        {
            Plugin.LogInfo($"Farm.RemoveSpawnedDrones: drone={droneId}, active={activeDroneCount}");
            Plugin.Host?.NotifyDroneRemoved(droneId, false, activeDroneCount, __instance.mainDroneId, __state.DroneGeneration);
        }
    }
}

[HarmonyPatch(typeof(GridManager), nameof(GridManager.GenerateWorld))]
internal static class GridManagerGenerateWorldPatch
{
    private static void Postfix(GridManager __instance, bool shrinkFarm)
    {
        Vector2Int worldSize = __instance.WorldSize;
        Plugin.LogInfo($"GridManager.GenerateWorld: world={worldSize.x}x{worldSize.y}, grounds={__instance.grounds.Count}, entities={__instance.entities.Count}");
        Plugin.Host?.NotifyWorldGenerated(worldSize.x, worldSize.y, __instance.grounds.Count, __instance.entities.Count, shrinkFarm);
    }
}

[HarmonyPatch(typeof(GridManager), nameof(GridManager.SetGround))]
internal static class GridManagerSetGroundPatch
{
    private static void Prefix(GridManager __instance, Vector2Int pos, out string __state)
    {
        __state = __instance.grounds.TryGetValue(pos, out FarmObject previousGround) ? previousGround?.objectSO?.objectName : string.Empty;
    }

    private static void Postfix(Vector2Int pos, string newGround, string __state)
    {
        Plugin.Host?.NotifyGridObjectChanged(
            GridObjectLayer.Ground,
            GridObjectChangeKind.Set,
            pos.x,
            pos.y,
            newGround,
            __state,
            regrowGrass: false);
    }
}

[HarmonyPatch(typeof(GridManager), nameof(GridManager.SetEntity))]
internal static class GridManagerSetEntityPatch
{
    private static void Prefix(GridManager __instance, Vector2Int pos, out string __state)
    {
        __state = __instance.entities.TryGetValue(pos, out FarmObject previousEntity) ? previousEntity?.objectSO?.objectName : string.Empty;
    }

    private static void Postfix(Vector2Int pos, string newObject, string __state)
    {
        Plugin.Host?.NotifyGridObjectChanged(
            GridObjectLayer.Entity,
            GridObjectChangeKind.Set,
            pos.x,
            pos.y,
            newObject,
            __state,
            regrowGrass: false);
    }
}

[HarmonyPatch(typeof(GridManager), nameof(GridManager.RemoveEntity))]
internal static class GridManagerRemoveEntityPatch
{
    private static void Prefix(GridManager __instance, Vector2Int pos, bool regrowGrass, out GridObjectRemovalState __state)
    {
        __state = __instance.entities.TryGetValue(pos, out FarmObject previousEntity)
            ? new GridObjectRemovalState(previousEntity?.objectSO?.objectName, regrowGrass)
            : null;
    }

    private static void Postfix(Vector2Int pos, GridObjectRemovalState __state)
    {
        if (__state == null)
        {
            return;
        }

        Plugin.Host?.NotifyGridObjectChanged(
            GridObjectLayer.Entity,
            GridObjectChangeKind.Removed,
            pos.x,
            pos.y,
            string.Empty,
            __state.PreviousObjectName,
            __state.RegrowGrass);
    }
}

[HarmonyPatch(typeof(GridManager), nameof(GridManager.Swap))]
internal static class GridManagerSwapPatch
{
    private static void Prefix(GridManager __instance, Vector2Int pos, GridDirection dir, out GridSwapState __state)
    {
        Vector2Int destination = pos + dir.GetDirectionVector();
        __state = new GridSwapState(
            destination.x,
            destination.y,
            __instance.entities.TryGetValue(pos, out FarmObject sourceObject) ? sourceObject?.objectSO?.objectName : string.Empty,
            __instance.entities.TryGetValue(destination, out FarmObject destinationObject) ? destinationObject?.objectSO?.objectName : string.Empty);
    }

    private static void Postfix(Vector2Int pos, bool __result, GridSwapState __state)
    {
        if (!__result || __state == null)
        {
            return;
        }

        Plugin.Host?.NotifyGridSwapped(
            pos.x,
            pos.y,
            __state.DestinationX,
            __state.DestinationY,
            __state.SourceObjectName,
            __state.DestinationObjectName);
    }
}

internal sealed class SimulationSpeedChangeState
{
    internal SimulationSpeedChangeState(double previousSpeedFactor, double previousOpDurationSeconds)
    {
        PreviousSpeedFactor = previousSpeedFactor;
        PreviousOpDurationSeconds = previousOpDurationSeconds;
    }

    internal double PreviousSpeedFactor { get; }

    internal double PreviousOpDurationSeconds { get; }
}

internal sealed class ExecutionStopState
{
    internal ExecutionStopState(
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

    internal int ExecutionId { get; }

    internal string LeaderboardType { get; }

    internal int StateCount { get; }

    internal int ActiveDroneCount { get; }

    internal double GlobalOpCount { get; }

    internal bool WasPaused { get; }

    internal double CurrentTimeSeconds { get; }
}

internal sealed class DroneRemovalState
{
    internal DroneRemovalState(int droneId, bool wasMainDrone, int droneGeneration)
    {
        DroneId = droneId;
        WasMainDrone = wasMainDrone;
        DroneGeneration = droneGeneration;
    }

    internal int DroneId { get; }

    internal bool WasMainDrone { get; }

    internal int DroneGeneration { get; }
}

internal sealed class SpawnedDroneRemovalState
{
    internal SpawnedDroneRemovalState(int droneGeneration)
    {
        DroneGeneration = droneGeneration;
        RemovedDroneIds = new List<int>();
    }

    internal int DroneGeneration { get; }

    internal List<int> RemovedDroneIds { get; }
}

internal sealed class GridObjectRemovalState
{
    internal GridObjectRemovalState(string previousObjectName, bool regrowGrass)
    {
        PreviousObjectName = previousObjectName ?? string.Empty;
        RegrowGrass = regrowGrass;
    }

    internal string PreviousObjectName { get; }

    internal bool RegrowGrass { get; }
}

internal sealed class GridSwapState
{
    internal GridSwapState(int destinationX, int destinationY, string sourceObjectName, string destinationObjectName)
    {
        DestinationX = destinationX;
        DestinationY = destinationY;
        SourceObjectName = sourceObjectName ?? string.Empty;
        DestinationObjectName = destinationObjectName ?? string.Empty;
    }

    internal int DestinationX { get; }

    internal int DestinationY { get; }

    internal string SourceObjectName { get; }

    internal string DestinationObjectName { get; }
}
