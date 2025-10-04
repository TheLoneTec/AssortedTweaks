using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace AssortedTweaks
{
  internal class CornerBuildable
  {
    public static bool Prefix(ref bool __result, IntVec3 adjCardinalX, IntVec3 adjCardinalZ, PathingContext pc)
    {
      if (!AssortedTweaksMod.instance.Settings.ReplaceStuff_CornerBuildable || !pc.map.thingGrid.ThingsAt(new IntVec3(adjCardinalX.x, 0, adjCardinalZ.z)).Any<Thing>((Func<Thing, bool>) (t => TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(t.def))))
        return true;
      __result = true;
      return false;
    }
  }
}
