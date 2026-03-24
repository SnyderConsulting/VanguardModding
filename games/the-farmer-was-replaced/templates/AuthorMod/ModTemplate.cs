using System.Reflection;
using HarmonyLib;
using TFWR.ModHarness.SDK;

namespace __MOD_ROOT_NAMESPACE__;

// Add optional hook interfaces such as ISimulationHooks, IExecutionHooks, IFarmHooks, or IGridHooks when you want harness callbacks for those surfaces.
public sealed class __MOD_CLASS_NAME__ : TfwrModBase
{
    internal static IModContext ActiveContext { get; private set; }

    public override string Id => "__MOD_ID__";

    public override string Name => "__MOD_DISPLAY_NAME__";

    public override string Version => "0.1.0";

    public override void Initialize(IModContext context)
    {
        base.Initialize(context);
        ActiveContext = context;
        Context.PatchAll();
        Context.Logger.Info("Initialized");
    }

    public override void Shutdown()
    {
        ActiveContext = null;
    }
}

[HarmonyPatch]
internal static class __MOD_CLASS_NAME__Patches
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method("MainSim:SetupSim");
    }

    private static void Postfix()
    {
        __MOD_CLASS_NAME__.ActiveContext?.Logger.Info("Harmony patch observed MainSim.SetupSim");
    }
}
