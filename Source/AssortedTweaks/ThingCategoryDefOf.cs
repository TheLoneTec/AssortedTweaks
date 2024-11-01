using RimWorld;
using Verse;

namespace AssortedTweaks
{
    [DefOf]
    public static class ThingCategoryDefOfLocal
    {
        public static ThingCategoryDef Preserves;
        public static ThingCategoryDef BodyPartsNatural;

        static ThingCategoryDefOfLocal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOfLocal));
        }
    }
}