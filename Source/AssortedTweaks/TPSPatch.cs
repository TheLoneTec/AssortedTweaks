using Verse;

namespace AssortedTweaks
{
  public class TPSPatch
  {
    public static bool Prefix(float leftX, float width, ref float curBaseY)
    {
      if (!AssortedTweaksMod.instance.Settings.CoreSK_ShowTPSInRegularGame && !Prefs.DevMode)
        return false;
      TPSNew.RimWorld_GlobalControlsUtility_DoTimespeedControls_Postfix(leftX, width, ref curBaseY);
      return false;
    }
  }
}
