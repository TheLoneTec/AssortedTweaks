using Verse;

namespace AssortedTweaks
{
  public static class CornerMineableOkay
  {
    public static bool Prefix(ref bool __result, ThingDef def)
    {
      if (!AssortedTweaksMod.instance.Settings.ReplaceStuff_CornerBuildable)
        return true;
      ThingDef thingDef = def.IsFrame || def.IsBlueprint ? def.entityDefToBuild as ThingDef : def;
      __result = thingDef != null && thingDef.category == ThingCategory.Building && thingDef.holdsRoof;
      return false;
    }
  }
}
