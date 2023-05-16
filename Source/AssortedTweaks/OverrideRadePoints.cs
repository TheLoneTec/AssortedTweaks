using HarmonyLib;
using RimWorld;
using Verse;

namespace AssortedTweaks
{
  [HarmonyPriority(800)]
  [HarmonyPatch(typeof (StorytellerUtility), "DefaultThreatPointsNow")]
  public class OverrideRadePoints
  {
    [HarmonyPrefix]
    public static bool RaidPoints(ref float __result)
    {
      if (!Prefs.DevMode || !AssortedTweaksMod.instance.Settings.OverrideRadePoints)
        return true;
      __result = (float)AssortedTweaksMod.instance.Settings.OverrideRadePointsValue;
      return false;
    }
  }
}
