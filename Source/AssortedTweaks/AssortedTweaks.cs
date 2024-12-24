using HarmonyLib;
using RimWorld;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;

namespace AssortedTweaks
{
  [StaticConstructorOnStartup]
  public static class AssortedTweaks
  {
    static AssortedTweaks()
    {
        Harmony h = new Harmony("TheLoneTec.AssortedTweaks");

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("[AssortedTweaks] Initializing...");

        h.PatchAll(Assembly.GetExecutingAssembly());
        if (AssortedTweaksMod.instance.Settings.MeatIngredients)
            MeatHasIngredients.MyPostfix();

        HarmonyMethod transpiler = new HarmonyMethod(typeof(HandleBlockingPlants), "HandleBlockingThingJob");
        h.Patch((MethodBase)AccessTools.Method(typeof(RoofUtility), "CanHandleBlockingThing"), transpiler: transpiler);
        h.Patch((MethodBase)AccessTools.Method(typeof(GenConstruct), "HandleBlockingThingJob"), transpiler: transpiler);

        stringBuilder.Append("[AssortedTweaks] General Patches Applied");
        
        stringBuilder.Append("[AssortedTweaks] Applied patches for...");

        if (ModActive.CoreSK)
        {
            MethodInfo original1 = AccessTools.Method("SK.GlobalControlsUtility_DoTimespeedControls_Patch:Postfix");
            if (original1 != (MethodInfo)null)
                h.Patch((MethodBase)original1, new HarmonyMethod(typeof(TPSPatch), "Prefix"));
            MethodInfo original2 = AccessTools.Method("SK.Patch_TickManager_TickRateMultiplier:TickRate");
            if (original2 != (MethodInfo)null)
                h.Patch((MethodBase)original2, postfix: new HarmonyMethod(typeof(CoreSK_Max4Speed), "Postfix"));
            stringBuilder.Append("CoreSK ");
        }
        if (!ModActive.ShareTheLoad)
        {
            h.Patch((MethodBase)AccessTools.Method(typeof(ItemAvailability), "ThingsAvailableAnywhere"), new HarmonyMethod(typeof(DeliverAsMuchAsPossible), "Prefix"));
            h.Patch((MethodBase)AccessTools.Method(typeof(WorkGiver_ConstructDeliverResources), "ResourceDeliverJobFor"), transpiler: new HarmonyMethod(typeof(BreakToContinue_Patch), "Transpiler"));
            stringBuilder.Append("ShareTheLoad ");
        }
        if (!ModActive.ReplaceStuff)
        {
            h.Patch((MethodBase)AccessTools.Method(typeof(TouchPathEndModeUtility), "IsCornerTouchAllowed"), new HarmonyMethod(typeof(CornerBuildable), "Prefix"));
            h.Patch((MethodBase)AccessTools.Method(typeof(TouchPathEndModeUtility), "MakesOccupiedCellsAlwaysReachableDiagonally"), new HarmonyMethod(typeof(CornerMineableOkay), "Prefix"));
            h.Patch((MethodBase)AccessTools.Method(typeof(GenPath), "ShouldNotEnterCell"), postfix: new HarmonyMethod(typeof(ShouldNotEnterCellPatch), "Postfix"));
            h.Patch((MethodBase)AccessTools.Method(typeof(HaulAIUtility), "TryFindSpotToPlaceHaulableCloseTo"), postfix: new HarmonyMethod(typeof(TryFindSpotToPlaceHaulableCloseToPatch), "Postfix"));
            stringBuilder.Append("ReplaceStuff ");
        }

        }

  }
}
