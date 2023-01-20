using RimWorld;
using Verse;

namespace AssortedTweaks
{
    [DefOf]
    public static class ThingCategoryDefOfLocal
    {
        public static ThingCategoryDef Preserves;

        static ThingCategoryDefOfLocal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOfLocal));
        }
    }
}