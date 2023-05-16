using Verse;

namespace AssortedTweaks
{
  public static class ShouldNotEnterCellPatch
  {
    public static void Postfix(ref bool __result, Pawn pawn, Map map, IntVec3 dest)
    {
      if (!AssortedTweaksMod.instance.Settings.ReplaceStuff_CornerBuildable || pawn == null || __result || !pawn.IsFreeColonist || !dest.InBounds(map))
        return;
      foreach (IntVec3 cardinalDirection in GenAdj.CardinalDirections)
      {
        IntVec3 c = dest + cardinalDirection;
        if (c.InBounds(map))
        {
          Building building = map.edificeGrid[c];
          if ((building != null ? (!building.BlocksPawn(pawn) ? 1 : 0) : 1) != 0)
            return;
        }
      }
      foreach (IntVec3 diagonalDirection in GenAdj.DiagonalDirections)
      {
        IntVec3 c = dest + diagonalDirection;
        if (c.InBounds(map))
        {
          Building building = map.edificeGrid[c];
          if ((building != null ? (!building.BlocksPawn(pawn) ? 1 : 0) : 1) != 0)
          {
            __result = true;
            break;
          }
        }
      }
    }
  }
}
