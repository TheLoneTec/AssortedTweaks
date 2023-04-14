using HarmonyLib;
using RimWorld;
using System.Reflection;
using System.Text;
using Verse;

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

        stringBuilder.Append("[AssortedTweaks] General Patches Applied");
        
        stringBuilder.Append("[AssortedTweaks] Applied patches for...");

        if (!ModActive.ShareTheLoad)
        {
            h.Patch((MethodBase)AccessTools.Method(typeof(ItemAvailability), "ThingsAvailableAnywhere"), new HarmonyMethod(typeof(DeliverAsMuchAsPossible), "Prefix"));
            h.Patch((MethodBase)AccessTools.Method(typeof(WorkGiver_ConstructDeliverResources), "ResourceDeliverJobFor"), transpiler: new HarmonyMethod(typeof(BreakToContinue_Patch), "Transpiler"));
            stringBuilder.Append("ShareTheLoad ");
        }
    }

  }
}
