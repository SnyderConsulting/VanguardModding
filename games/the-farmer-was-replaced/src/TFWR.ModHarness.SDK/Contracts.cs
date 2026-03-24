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
