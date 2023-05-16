using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AssortedTweaks
{
  public static class TryFindSpotToPlaceHaulableCloseToPatch
  {
    public static bool recursive = true;

    public static void Postfix(
      ref bool __result,
      Thing haulable,
      Pawn worker,
      IntVec3 center,
      ref IntVec3 spot)
    {
      if (!AssortedTweaksMod.instance.Settings.ReplaceStuff_CornerBuildable || __result || !TryFindSpotToPlaceHaulableCloseToPatch.recursive)
        return;
      TryFindSpotToPlaceHaulableCloseToPatch.recursive = false;
      foreach (IntVec3 diagonalDirection in GenAdj.DiagonalDirections)
      {
        IntVec3 center1 = center + diagonalDirection;
        if (TryFindSpotToPlaceHaulableCloseToPatch.TryFindSpotToPlaceHaulableCloseTo(haulable, worker, center1, out spot))
        {
          __result = true;
          break;
        }
      }
      TryFindSpotToPlaceHaulableCloseToPatch.recursive = true;
    }

    public static bool TryFindSpotToPlaceHaulableCloseTo(
      Thing haulable,
      Pawn worker,
      IntVec3 center,
      out IntVec3 spot)
    {
      return HaulAIUtility.CanHaulAside(worker, haulable, out spot);
    }

    }
}
