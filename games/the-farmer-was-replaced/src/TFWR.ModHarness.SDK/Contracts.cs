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

    void OnUpdate();

    void Shutdown();
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
