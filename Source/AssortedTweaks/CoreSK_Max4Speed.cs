using Verse;

namespace AssortedTweaks
{
  internal class CoreSK_Max4Speed
  {
    public static void Postfix(ref TimeSpeed currTimeSpeed, ref float __result)
    {
      if ((double) __result != 15.0)
        return;
      int coreSkMax4Speed = AssortedTweaksMod.instance.Settings.CoreSK_Max4Speed;
      if (coreSkMax4Speed == 900 || coreSkMax4Speed <= 0)
        return;
      __result = (float) coreSkMax4Speed / 60f;
    }
  }
}
