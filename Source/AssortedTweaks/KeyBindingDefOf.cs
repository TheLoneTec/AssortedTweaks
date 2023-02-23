using RimWorld;
using Verse;

namespace AssortedTweaks
{
  [DefOf]
  public static class KeyBindingDefOf
  {
    public static KeyBindingDef Dev_ToggleDevMode;

    static KeyBindingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof (KeyBindingDefOf));
  }
}
