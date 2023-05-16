using SK;
using Verse;

namespace AssortedTweaks
{
  public class CoreSK_Utils
  {
    public static void Set_Config_MaxRaidCount(int value)
    {
      Config named = DefDatabase<Config>.GetNamed("Config");
      if (named == null)
        return;
      named.MaxRaidCount = value;
      Log.Message(string.Format("[AnotherTweaks] CoreSK config MaxRaidCount = {0}", (object) value));
    }
  }
}
