using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using TFWR.ModHarness.SDK;
using UnityEngine.SceneManagement;

namespace TFWR.ModHarness;

internal sealed class ModHost
{
    private readonly Plugin _plugin;
    private readonly List<LoadedMod> _loadedMods = new List<LoadedMod>();
    private readonly HashSet<string> _resolverDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private bool _resolverAttached;

    internal ModHost(Plugin plugin)
    {
        _plugin = plugin;
        ExternalModsDirectory = Path.Combine(Paths.BepInExRootPath, "TFWR.ModHarness", "mods");
        SharedSdkDirectory = Path.Combine(Paths.BepInExRootPath, "TFWR.ModHarness", "sdk");
        DataRootDirectory = Path.Combine(Paths.BepInExRootPath, "TFWR.ModHarness", "data");
    }

    internal string ExternalModsDirectory { get; }

    internal string SharedSdkDirectory { get; }

    internal string DataRootDirectory { get; }

    internal IReadOnlyList<LoadedMod> LoadedMods => _loadedMods;

    internal void LoadMods()
    {
        Directory.CreateDirectory(ExternalModsDirectory);
        Directory.CreateDirectory(SharedSdkDirectory);
        Directory.CreateDirectory(DataRootDirectory);

        AttachAssemblyResolver();

        string[] dllPaths = Directory.GetFiles(ExternalModsDirectory, "*.dll", SearchOption.AllDirectories);
        if (dllPaths.Length == 0)
        {
            Plugin.LogInfo($"No external mods found under {ExternalModsDirectory}");
            return;
        }

        foreach (string dllPath in dllPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            RegisterResolverDirectory(Path.GetDirectoryName(dllPath));
            LoadAssemblyMods(dllPath);
        }
    }

    internal void NotifySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneEvent sceneEvent = new SceneEvent(scene.name, mode.ToString(), scene.rootCount, scene.isLoaded);
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, $"OnSceneLoaded({scene.name})", () => loadedMod.Instance.OnSceneLoaded(sceneEvent));
        }
    }

    internal void NotifyMainSimReady()
    {
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "OnMainSimReady", loadedMod.Instance.OnMainSimReady);
        }
    }

    internal void NotifyMainExecutionStarted(string fileName, int executionId, double timeFactor)
    {
        MainExecutionStartedEvent executionEvent = new MainExecutionStartedEvent(fileName, executionId, timeFactor);
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "OnMainExecutionStarted", () => loadedMod.Instance.OnMainExecutionStarted(executionEvent));
        }
    }

    internal void NotifyMainExecutionStopped(int executionId, bool isSimulating)
    {
        MainExecutionStoppedEvent executionEvent = new MainExecutionStoppedEvent(executionId, isSimulating);
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "OnMainExecutionStopped", () => loadedMod.Instance.OnMainExecutionStopped(executionEvent));
        }
    }

    internal void NotifyWorkspaceReady(int openWindowCount, int codeWindowCount)
    {
        WorkspaceEvent workspaceEvent = new WorkspaceEvent(openWindowCount, codeWindowCount);
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "OnWorkspaceReady", () => loadedMod.Instance.OnWorkspaceReady(workspaceEvent));
        }
    }

    internal void NotifyCodeWindowOpened(string fileName)
    {
        CodeWindowEvent codeWindowEvent = new CodeWindowEvent(fileName);
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "OnCodeWindowOpened", () => loadedMod.Instance.OnCodeWindowOpened(codeWindowEvent));
        }
    }

    internal void UpdateMods()
    {
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "OnUpdate", loadedMod.Instance.OnUpdate);
        }
    }

    internal void Shutdown()
    {
        foreach (LoadedMod loadedMod in _loadedMods)
        {
            SafeInvoke(loadedMod, "Shutdown", loadedMod.Instance.Shutdown);
            SafeInvoke(loadedMod, "UnpatchSelf", loadedMod.Context.UnpatchSelf);
        }
    }

    private void LoadAssemblyMods(string assemblyPath)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            IEnumerable<Type> modTypes = GetLoadableTypes(assembly)
                .Where(type => typeof(ITfwrMod).IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null);

            foreach (Type modType in modTypes)
            {
                ITfwrMod instance = (ITfwrMod)Activator.CreateInstance(modType);
                string modId = string.IsNullOrWhiteSpace(instance.Id) ? modType.FullName : instance.Id.Trim();

                if (_loadedMods.Any(mod => string.Equals(mod.Id, modId, StringComparison.OrdinalIgnoreCase)))
                {
                    Plugin.LogWarning($"Skipping duplicate mod id '{modId}' from {assemblyPath}");
                    continue;
                }

                ModContext context = new ModContext(
                    modId,
                    string.IsNullOrWhiteSpace(instance.Name) ? modType.Name : instance.Name.Trim(),
                    string.IsNullOrWhiteSpace(instance.Version) ? "0.0.0" : instance.Version.Trim(),
                    assembly,
                    assemblyPath,
                    ExternalModsDirectory,
                    SharedSdkDirectory,
                    DataRootDirectory);

                LoadedMod loadedMod = new LoadedMod(context.Id, context.Name, context.Version, instance, context, assemblyPath);
                _loadedMods.Add(loadedMod);

                SafeInvoke(loadedMod, "Initialize", () => loadedMod.Instance.Initialize(loadedMod.Context));
                Plugin.LogInfo($"Loaded external mod: {loadedMod.Id} ({loadedMod.Name} {loadedMod.Version})");
            }
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Failed to load mod assembly {assemblyPath}: {ex}");
        }
    }

    private void SafeInvoke(LoadedMod loadedMod, string operation, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Mod '{loadedMod.Id}' failed during {operation}: {ex}");
        }
    }

    private void AttachAssemblyResolver()
    {
        if (_resolverAttached)
        {
            return;
        }

        RegisterResolverDirectory(ExternalModsDirectory);
        RegisterResolverDirectory(SharedSdkDirectory);
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        _resolverAttached = true;
    }

    private void RegisterResolverDirectory(string directory)
    {
        if (!string.IsNullOrWhiteSpace(directory))
        {
            _resolverDirectories.Add(directory);
        }
    }

    private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        string fileName = $"{new AssemblyName(args.Name).Name}.dll";
        foreach (string directory in _resolverDirectories)
        {
            string candidatePath = Path.Combine(directory, fileName);
            if (File.Exists(candidatePath))
            {
                return Assembly.LoadFrom(candidatePath);
            }
        }

        return null;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null);
        }
    }
}

internal sealed class LoadedMod
{
    internal LoadedMod(string id, string name, string version, ITfwrMod instance, ModContext context, string assemblyPath)
    {
        Id = id;
        Name = name;
        Version = version;
        Instance = instance;
        Context = context;
        AssemblyPath = assemblyPath;
    }

    internal string Id { get; }

    internal string Name { get; }

    internal string Version { get; }

    internal ITfwrMod Instance { get; }

    internal ModContext Context { get; }

    internal string AssemblyPath { get; }
}

internal sealed class ModContext : IModContext
{
    private readonly ModLogger _logger;
    private readonly Assembly _assembly;
    private readonly Harmony _harmony;
    private bool _patchesApplied;

    internal ModContext(
        string id,
        string name,
        string version,
        Assembly assembly,
        string assemblyPath,
        string modsRootDirectory,
        string sdkDirectory,
        string dataRootDirectory)
    {
        Id = id;
        Name = name;
        Version = version;
        _assembly = assembly;
        AssemblyPath = assemblyPath;
        AssemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? modsRootDirectory;
        ModsRootDirectory = modsRootDirectory;
        SdkDirectory = sdkDirectory;
        DataDirectory = Path.Combine(dataRootDirectory, id);
        GameRootDirectory = Paths.GameRootPath;
        HarnessRootDirectory = Path.Combine(Paths.BepInExRootPath, "TFWR.ModHarness");
        _harmony = new Harmony($"{Plugin.PluginGuid}.{id}");
        _logger = new ModLogger(id);

        Directory.CreateDirectory(DataDirectory);
    }

    public string Id { get; }

    public string Name { get; }

    public string Version { get; }

    public string HarnessVersion => Plugin.PluginVersion;

    public string GameRootDirectory { get; }

    public string HarnessRootDirectory { get; }

    public string ModsRootDirectory { get; }

    public string SdkDirectory { get; }

    public string AssemblyPath { get; }

    public string AssemblyDirectory { get; }

    public string DataDirectory { get; }

    public IModLogger Logger => _logger;

    public void PatchAll()
    {
        if (_patchesApplied)
        {
            return;
        }

        _harmony.PatchAll(_assembly);
        _patchesApplied = true;
    }

    public void UnpatchSelf()
    {
        if (!_patchesApplied)
        {
            return;
        }

        _harmony.UnpatchSelf();
        _patchesApplied = false;
    }

    public void WriteSnapshot(string reason, bool includeHierarchy)
    {
        Plugin.WriteSnapshot($"{Id}-{reason}", includeHierarchy);
    }
}

internal sealed class ModLogger : IModLogger
{
    private readonly string _modId;

    internal ModLogger(string modId)
    {
        _modId = modId;
    }

    public void Info(string message)
    {
        Plugin.LogInfo($"[{_modId}] {message}");
    }

    public void Warning(string message)
    {
        Plugin.LogWarning($"[{_modId}] {message}");
    }

    public void Error(string message)
    {
        Plugin.LogError($"[{_modId}] {message}");
    }
}
