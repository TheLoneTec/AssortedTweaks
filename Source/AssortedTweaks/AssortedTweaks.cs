using HarmonyLib;
using System.Reflection;
using Verse;

namespace AssortedTweaks
{
  [StaticConstructorOnStartup]
  public static class AssortedTweaks
  {
    static AssortedTweaks()
    {
        new Harmony("TheLoneTec.AssortedTweaks").PatchAll(Assembly.GetExecutingAssembly());
        if (AssortedTweaksMod.instance.Settings.MeatIngredients)
            MeatHasIngredients.MyPostfix();
    }

  }
}
