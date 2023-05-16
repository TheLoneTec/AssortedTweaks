using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace AssortedTweaks
{
  internal class CornerBuildable
  {
    public static bool Prefix(ref bool __result, int cornerX, int cornerZ, PathingContext pc)
    {
      if (!AssortedTweaksMod.instance.Settings.ReplaceStuff_CornerBuildable || !pc.map.thingGrid.ThingsAt(new IntVec3(cornerX, 0, cornerZ)).Any<Thing>((Func<Thing, bool>) (t => TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(t.def))))
        return true;
      __result = true;
      return false;
    }
  }
}
