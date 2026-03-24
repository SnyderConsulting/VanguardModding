using System.Reflection;
using HarmonyLib;
using TFWR.ModHarness.SDK;

namespace ExampleHelloMod;

public sealed class ExampleHelloMod : TfwrModBase
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
