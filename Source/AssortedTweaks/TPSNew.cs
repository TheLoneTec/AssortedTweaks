using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AssortedTweaks
{
  public static class TPSNew
  {
    private static DateTime PrevTime;
    private static int PrevTicks = -1;
    private static int TPSActual = 0;
    private static int newGameTick = 0;
    private static float raidPoints = 0.0f;

    public static void RimWorld_GlobalControlsUtility_DoTimespeedControls_Postfix(
      float leftX,
      float width,
      ref float curBaseY)
    {
      float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
      int num = (int) Math.Round((double) tickRateMultiplier == 0.0 ? 0.0 : 60.0 * (double) tickRateMultiplier);
      if (TPSNew.PrevTicks == -1)
      {
        TPSNew.PrevTicks = GenTicks.TicksAbs;
        TPSNew.PrevTime = DateTime.Now;
      }
      else
      {
        DateTime now = DateTime.Now;
        if (now.Second != TPSNew.PrevTime.Second)
        {
          TPSNew.PrevTime = now;
          TPSNew.TPSActual = GenTicks.TicksAbs - TPSNew.PrevTicks;
          TPSNew.PrevTicks = GenTicks.TicksAbs;
        }
      }
      Rect rect = new Rect(leftX - 20f, curBaseY - 26f, (float) ((double) width + 20.0 - 7.0), 26f);
      Text.Anchor = TextAnchor.MiddleRight;
      if (Find.TickManager.TicksGame >= TPSNew.newGameTick)
      {
        Map currentMap = Find.CurrentMap;
        TPSNew.raidPoints = currentMap != null ? Mathf.Round(StorytellerUtility.DefaultThreatPointsNow((IIncidentTarget) currentMap)) : 0.0f;
        TPSNew.newGameTick = Find.TickManager.TicksGame + 2000;
      }
      string label = AssortedTweaksMod.instance.Settings.CoreSK_ShowRaidPoints ? string.Format("TPS: {0}({1}) P: {2}", (object) TPSNew.TPSActual, (object) num, (object) TPSNew.raidPoints) : string.Format("TPS: {0}({1})", (object) TPSNew.TPSActual, (object) num);
      Widgets.Label(rect, label);
      Text.Anchor = TextAnchor.UpperLeft;
      curBaseY -= 26f;
    }
  }
}
