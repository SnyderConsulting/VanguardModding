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
    internal const string PluginVersion = "0.2.0";

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
    }
}

[HarmonyPatch(typeof(MainSim), nameof(MainSim.StopMainExecution))]
internal static class MainSimStopMainExecutionPatch
{
    private static void Postfix(MainSim __instance)
    {
        Plugin.LogInfo($"MainSim.StopMainExecution: executionId={__instance.executionId}, simulating={__instance.IsSimulating()}");
    }
}

[HarmonyPatch(typeof(Workspace), "Start")]
internal static class WorkspaceStartPatch
{
    private static void Postfix(Workspace __instance)
    {
        Plugin.LogInfo($"Workspace.Start: openWindows={__instance.openWindows.Count}, codeWindows={__instance.codeWindows.Count}");
    }
}

[HarmonyPatch(typeof(Workspace), nameof(Workspace.OpenCodeWindow))]
internal static class WorkspaceOpenCodeWindowPatch
{
    private static void Postfix(string fileName)
    {
        Plugin.LogInfo($"Workspace.OpenCodeWindow: {fileName}");
    }
}
