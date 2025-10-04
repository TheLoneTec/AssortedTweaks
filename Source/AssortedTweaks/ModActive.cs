// Decompiled with JetBrains decompiler
// Type: AnotherTweaks.ModActive
// Assembly: AnotherTweaks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5472CAC4-0D69-4D05-9482-DABB8FC5A27B
// Assembly location: E:\AlternativeSteamVersions\RimWorldHSK_1.4\Mods\AnotherTweaks\Assemblies\AnotherTweaks.dll

using System;
using Verse;

namespace AssortedTweaks
{
    public class ModActive
    {
        private static bool? _shareTheLoad;
        private static bool? _coreSk;

        public static bool ShareTheLoad
        {
            get
            {
                if (!ModActive._shareTheLoad.HasValue)
                    ModActive._shareTheLoad = new bool?(LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.PackageId.Equals("Uuugggg.ShareTheLoad", StringComparison.CurrentCultureIgnoreCase) || x.PackageId.Equals("DEBUuugggg.ShareTheLoad", StringComparison.CurrentCultureIgnoreCase))));
                return ModActive._shareTheLoad.Value;
            }
        }

        public static bool CoreSK
        {
            get
            {
                if (!ModActive._coreSk.HasValue)
                    ModActive._coreSk = new bool?(LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.PackageId.Equals("skyarkhangel.HSK", StringComparison.CurrentCultureIgnoreCase))));
                return ModActive._coreSk.Value;
            }
        }

    }
}
