using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using Verse;
using System.Linq;
using AlienRace;
using UnityEngine;
using System.Reflection;
using System.Runtime;
using System.Diagnostics;
using Verse.AI;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using Random = UnityEngine.Random;
using System.Security.Cryptography;
using System.Text;
using AssortedTweaks;
using Mono.Cecil;
using SK;

namespace AssortedTweaks
{
    [HarmonyPatch(typeof(BillUtility), "MakeNewBill")]
    public class DefaultRadius
    {
        public static void Postfix(ref Bill __result)
        {
            if ((double)AssortedTweaksMod.instance.Settings.DefaultRadius >= 100.0 || !(__result is Bill_Production billProduction) || (double)billProduction.ingredientSearchRadius < 100.0)
                return;
            billProduction.ingredientSearchRadius = AssortedTweaksMod.instance.Settings.DefaultRadius;
        }
    }
    /*
    [HarmonyPatch(typeof(Thing), "Ingested")]
    public class Ingested_Patch
    {
        public static void Postfix(ref float __result, ref Thing __instance, Pawn ingester, float nutritionWanted)
        {
            if (!AssortedTweaksMod.instance.Settings.MeatIngredients)
                return;
            return;

        }
    }*/

    [HarmonyPatch(typeof(CompIngredients), "MergeIngredients", new System.Type[] { typeof(List<ThingDef>), typeof(List<ThingDef>), typeof(bool), typeof(ThingDef) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public class MergeIngredients_Patch
    {
        public static void Prefix(List<ThingDef> destIngredients, List<ThingDef> srcIngredients, bool lostImportantIngredients, ThingDef defToMake = null)
        {
            if (!AssortedTweaksMod.instance.Settings.MeatIngredients)
                return;
            foreach (var scr in srcIngredients)
            {
                List<ThingDef> toRemove = destIngredients.Where(i => /*i.race != null &&*/ scr.defName == i.defName && scr.label.CapitalizeFirst() == i.label.CapitalizeFirst()).ToList();
                foreach (var item in toRemove)
                {
                    destIngredients.Remove(item);
                }
            }/*
            CompIngredients tmp = new CompIngredients();
            tmp.ingredients = srcIngredients;
            foreach (var item in tmp.ingredients)
            {
                if (item.IsMeat || (item.ingestible != null && item.ingestible.sourceDef != null))
                {
                    CorrectIngredients.correctIngredients(ref tmp);
                    srcIngredients = tmp.ingredients;
                }
            }*/
        }
    }

    [HarmonyPatch(typeof(CompIngredients), "AllowStackWith", new System.Type[] { typeof(Thing) },
        new ArgumentType[] { ArgumentType.Normal })]
    public class AllowStackWith_Patch
    {/*
        public static bool Prefix(bool __result, CompIngredients __instance, Thing otherStack,  object[] __state)
        {
            CompIngredients compIngredients = otherStack.TryGetComp<CompIngredients>();

            if (__instance.Props.performMergeCompatibilityChecks)
                __state.AddItem(true);
            else
                __state.AddItem(false);
            if (compIngredients.Props.performMergeCompatibilityChecks)
                __state.AddItem(true);
            else
                __state.AddItem(false);

            string reason = "";
            if (FoodUtilities.CanBeStacked(otherStack,out reason))
            {
                __instance.Props.performMergeCompatibilityChecks = false;
                compIngredients.Props.performMergeCompatibilityChecks = false;
            }
            return true;
        }*/

        public static void Postfix(ref bool __result, CompIngredients __instance, Thing otherStack/*, object[] __state*/)
        {
            //CompIngredients compIngredients = otherStack.TryGetComp<CompIngredients>();
            string reason = "";
            if (!FoodUtilities.CanBeStacked(otherStack, out reason))
            {
                if (reason != "")
                    __result = false;
            }
            string tmp = "";
            if (!FoodUtilities.CanBeStacked(__instance.parent, out tmp))
            {
                if (tmp != "")
                    __result = false;
            }
            if (reason == tmp)
                __result = true;
            //__instance.Props.performMergeCompatibilityChecks = (bool)__state[0];
            //compIngredients.Props.performMergeCompatibilityChecks = (bool)__state[1];
        }
    }
    
    [HarmonyPatch(typeof(CompIngredients), "CompInspectStringExtra")]
    public class CompInspectStringExtra_Patch
    {
        public static void Postfix(ref string __result, CompIngredients __instance)
        {
            //Log.Message("Current result: " + __result);
            StringBuilder stringBuilder = new StringBuilder();
            if (__instance.ingredients.Count > 0)
            {
                stringBuilder.Append("Ingredients".Translate() + ": ");
                string reason = "";
                stringBuilder.Append(__instance.GetIngredientsString(includeMergeCompatibility: !FoodUtilities.CanBeStacked(__instance.parent, out reason), out var hasMergeCompatibilityIngredients));
                //if (DebugSettings.godMode)
                    //Log.Message("stringBuilder: " + stringBuilder.ToString() + ". reason is: " + reason);
                if (hasMergeCompatibilityIngredients)
                {
                    //Log.Message("hasMergeCompatibilityIngredients");
                    if (reason != "")
                    {
                        stringBuilder.Append(" (* " + "OnlyStacksWithCompatibleMeals".Translate().Resolve() + ": " + reason + ")");
                    }
                    else
                    {
                        stringBuilder.Append(" (* " + "OnlyStacksWithCompatibleMeals".Translate().Resolve() + ")");
                    }
                }
            }
            
            if (ModsConfig.IdeologyActive)
            {
                stringBuilder.AppendLineIfNotEmpty().Append(GetFoodKindInspectString(__instance));
            }

            //if (DebugSettings.godMode)
                //Log.Message("CompInspectStringExtra: " + stringBuilder.ToString());
            __result = stringBuilder.ToString();
            //return;
        }
        
        private static string GetFoodKindInspectString(CompIngredients comp)
        {
            //if (DebugSettings.godMode)
                //Log.Message("GetFoodKindInspectString Entered");
            if (FoodUtility.GetFoodKind(comp.parent) == FoodKind.NonMeat)
            {
                return "MealKindVegetarian".Translate().Colorize(Color.green);
            }

            if (FoodUtility.GetFoodKind(comp.parent) == FoodKind.Meat)
            {
                return "MealKindMeat".Translate().Colorize(ColorLibrary.RedReadable);
            }

            return "MealKindAny".Translate().Colorize(Color.white);
        }
    }

    [HarmonyPatch(typeof(CompIngredients), "GetIngredientsString")]
    public class GetIngredientsString_Patch
    {
        public static bool Prefix(ref string __result,CompIngredients __instance, bool includeMergeCompatibility, out bool hasMergeCompatibilityIngredients)
        {
            //Log.Message("GetIngredientsString: " + __result);
            StringBuilder stringBuilder = new StringBuilder();
            hasMergeCompatibilityIngredients = false;
            for (int i = 0; i < __instance.ingredients.Count; i++)
            {
                ThingDef thingDef = __instance.ingredients[i];
                stringBuilder.Append(thingDef.LabelCap.Resolve());
                string reason = "";
                if (includeMergeCompatibility && __instance.Props.performMergeCompatibilityChecks)
                {
                    if (!FoodUtilities.CanIngredientBeStacked(thingDef, out reason))
                    {
                        //Log.Message("adding asterix");
                        stringBuilder.Append("*");
                        hasMergeCompatibilityIngredients = true;
                    }
                }

                if (i < __instance.ingredients.Count - 1)
                {
                    stringBuilder.Append(", ");
                }
            }

            __result = stringBuilder.ToString();
            //if (DebugSettings.godMode)
                //Log.Message("GetIngredientsString: " + __result);
            return false;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "IsVeneratedAnimalMeatOrCorpse")]
    public class IsVeneratedAnimalMeatOrCorpse_Patch
    {
        public static void Postfix(ref bool __result, ThingDef foodDef, Pawn ingester, Thing source)
        {
            if (!AssortedTweaksMod.instance.Settings.MeatIngredients)
                return;

            if (ingester == null)
            {
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                    Log.Warning("Pawn is Null");
                return;
            }

            if (ingester.RaceProps == null)
            {
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                    Log.Warning(ingester.Name + "'s RaceProps is Null. Pawn with Invalid race.");
                return;
            }

            if (ingester.Ideo == null)
            {
                return;
            }

            if (foodDef == null)
            {
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                    Log.Warning("foodDef is Null");
                return;
            }

            if (foodDef.ingestible == null)
                return;

            if (foodDef.ingestible.sourceDef == null)
                return;

            if (ingester.def == null)
            {
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                    Log.Warning(ingester.Name + "'s def is Null");
                return;
            }

            if (source != null && source.def.IsCorpse && (source as Corpse).InnerPawn != null)
            {
                __result = ingester.Ideo.IsVeneratedAnimal(((Corpse)source).InnerPawn);
                return;
            }

            if (ingester.RaceProps.Humanlike && ingester.def != null && foodDef.IsIngestible)
            {
                __result = ingester.Ideo.IsVeneratedAnimal(foodDef.ingestible.sourceDef);
                return;
            }
        }
    }

    /*
     * Test to try and stop people using body parts from a difference race.
     * 
     */
    [HarmonyPatch(typeof(WorkGiver_DoBill), "StartOrResumeBillJob")]
    public class StartOrResumeBillJob_Patch
    {
        public static bool Prefix(ref Job __result, WorkGiver_DoBill __instance, Pawn pawn, IBillGiver giver, bool forced = false)
        {
            if (!AssortedTweaksMod.instance.Settings.RaceBodyPartsMatter)
                return true;
            if (pawn == null || giver == null)
                return true;
            bool flag1 = FloatMenuMakerMap.makingFor == pawn;
            if (flag1)
            {
                for (int index = 0; index < giver.BillStack.Count; ++index)
                {
                    Bill bill1 = giver.BillStack[index];

                    if (bill1 is Bill_Medical billMedical)
                    {
                        foreach (var item in billMedical.uniqueRequiredIngredients)
                        {
                            CompIngredients comp = item.TryGetComp<CompIngredients>();
                            if (item.HasThingCategory(ThingCategoryDefOfLocal.BodyPartsNatural) && comp != null && !comp.ingredients.Where(d => d.defName == pawn.def.defName).EnumerableNullOrEmpty())
                            {
                                JobFailReason.Is((string)"ModIncompatibleWith".Translate(item.def.label.Translate()));
                                __result = null;
                                return false;
                            }
                        }
                    }

                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "ButcherProducts", new System.Type[] { typeof(Pawn), typeof(float) })]
    public static class Pawn_ButcherProducts_RegularMeatPatch
    {
        public static void Postfix(ref IEnumerable<Thing> __result, Pawn __instance)
        {
            if (!AssortedTweaksMod.instance.Settings.MeatIngredients && __result == null || !__result.Any<Thing>())
                return;
            List<Thing> list = __result.ToList<Thing>();
            Thing thing1 = list.Find((Predicate<Thing>)(x => x.def.IsIngestible && x.def.ingestible.foodType == FoodTypeFlags.Meat));
            if (thing1 == null || thing1.def.ingestible.ateEvent != null)
                return;
            ThingWithComps thing2 = (ThingWithComps)ThingMaker.MakeThing(__instance.RaceProps.meatDef);
            thing2.stackCount = thing1.stackCount;

            CompIngredients comp = thing2.TryGetComp<CompIngredients>();
            if (comp != null)
            {
                comp.ingredients.Clear();
                if (__instance.def is ThingDef_AlienRace)
                {
                    ThingDef alien = CorrectIngredients.GetInfo(__instance.def);
                    //alien.descriptionHyperlinks.Add(new DefHyperlink(__instance.def));
                    alien.ingestible = thing2.def.ingestible;
                    //alien.ingredient = __instance.def.ingredient;
                    alien.category = ThingCategory.Item;
                    if (!alien.thingCategories.NullOrEmpty())
                    {
                        alien.thingCategories.Add(ThingCategoryDefOf.MeatRaw);
                    }
                    else
                    {
                        alien.thingCategories = new List<ThingCategoryDef>();
                        alien.thingCategories.Add(ThingCategoryDefOf.MeatRaw);
                    }
                    alien.ingestible.foodType = FoodTypeFlags.Meat;
                    alien.ingestible.sourceDef = __instance.def as ThingDef_AlienRace;

                    comp.RegisterIngredient(alien);
                    thing2.def.ingestible.sourceDef = __instance.def as ThingDef_AlienRace;
                }
                else
                {
                    ThingDef pawn = CorrectIngredients.GetInfo(__instance.def);
                    //alien.descriptionHyperlinks.Add(new DefHyperlink(__instance.def));
                    pawn.ingestible = thing2.def.ingestible;
                    //pawn.ingredient = __instance.def.ingredient;
                    pawn.category = ThingCategory.Item;
                    if (!pawn.thingCategories.NullOrEmpty())
                    {
                        pawn.thingCategories.Add(ThingCategoryDefOf.MeatRaw);
                    }
                    else
                    {
                        pawn.thingCategories = new List<ThingCategoryDef>();
                        pawn.thingCategories.Add(ThingCategoryDefOf.MeatRaw);
                    }
                    pawn.ingestible.foodType = FoodTypeFlags.Meat;
                    pawn.ingestible.sourceDef = __instance.def;

                    comp.RegisterIngredient(pawn);
                    thing2.def.ingestible.sourceDef = __instance.def;
                }
            }

            list.Remove(thing1);
            list.Insert(0, (Thing)thing2);
            __result = (IEnumerable<Thing>)list;

        }

    }

    [HarmonyPatch(typeof(GenSpawn), "Spawn", new System.Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool)})]
    public class CorrectIngredients
    {
        //public static Dictionary<Faction, ThingDef> factionMainMeatSource = new Dictionary<Faction, ThingDef>();
        public static Thing currentPawn = null;
        public static bool isCannible = false;
        public static string thingName = "";
        public static int recurring = 0;
        public static bool endOfSearch = false;

        public static void Postfix(ref Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode, bool respawningAfterLoad, bool forbidLeavings)
        {
            if (!AssortedTweaksMod.instance.Settings.MeatIngredients)
                return;

            CompIngredients comp = null;
            // Keep Track of food items and their ingredients added as well as factions. That way if its a same faction member with the same food item
            // can assign the same ingredients, saving time and resources. It also makes more sense an entire caravan or raid has the same meat types.

            try
            {

                if (__result is Pawn pawn)
                {
                    currentPawn = __result;

                    //Log.Message("Check is cannibal for: " + __result.def.defName);
                    if ((currentPawn as Pawn).story != null && (currentPawn as Pawn).story.traits.HasTrait(TraitDef.Named("Cannibal"))
                        || ModLister.IdeologyInstalled && (currentPawn as Pawn).Ideo != null && (currentPawn as Pawn).Ideo.HasHumanMeatEatingRequiredPrecept())
                    {
                        isCannible = true;
                    }
                    if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                        Log.Message("CorrectedIngredients Entered for : " + __result.def.defName);
                    //if (pawn.Faction != null && !factionMainMeatSource.ContainsKey(pawn.Faction))
                    //    factionMainMeatSource.Add(pawn.Faction, null);
                    //if (pawn.RaceProps.Humanlike && pawn.Faction != null && pawn.Faction != factionGroup)
                    //    factionGroup = pawn.Faction;
                    List<Thing> list = null;
                    if (pawn.inventory != null)
                        list = pawn.inventory.innerContainer.ToList();
                    List<Thing> carrying = null;
                    if (pawn.carryTracker != null)
                        carrying = pawn.carryTracker.innerContainer.ToList();

                    if (!carrying.NullOrEmpty())
                    {
                        //Log.Message("Starting carryTracker Check");
                        foreach (var thing in carrying)
                        {
                            comp = thing.TryGetComp<CompIngredients>();
                            if (comp != null)
                            {
                                correctIngredients(ref comp, loc, map);
                            }
                        }
                    }

                    if (!list.NullOrEmpty())
                    {
                        //Log.Message("Starting inventory Check. Inventory Size: " + list.Count);
                        foreach (var thing in list)
                        {
                            comp = thing.TryGetComp<CompIngredients>();
                            if (comp != null)
                            {
                                //Log.Message("Correcting Ingredients For: "  + thing.def.defName);
                                correctIngredients(ref comp, loc, map);
                            }
                        }
                    }
                }
                else if (__result.def.category == ThingCategory.Item && __result.def.IsIngestible && __result.TryGetComp<CompIngredients>() != null)
                {
                    comp = __result.TryGetComp<CompIngredients>();
                    if (comp != null)
                    {
                        correctIngredients(ref comp, loc, map);
                    }
                }
                isCannible = false;
            } catch (Exception e)
            {
                Log.Warning("Assorted Tweaks Encountered an Error (enable \"Show Debug Messages\" in mod options and godMode for more info):" + e.Message + " found at " + e.Source + Environment.NewLine + e.StackTrace);
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages && __result != null & __result.TryGetComp<CompIngredients>() != null)
                {
                    if (__result != null && __result.def != null)
                        Log.Message("Item is: " + 
                            (__result.def.defName != null ? __result.def.defName : "Null") + ". Location: " + 
                            (__result.PositionHeld != null ? __result.PositionHeld.ToString() : "Null") +
                            (__result.def.modContentPack != null ? __result.def.modContentPack.Name : "Null"));
                    foreach (var item in __result.TryGetComp<CompIngredients>().ingredients)
                    {
                        if (item != null)
                        {
                            Log.Message("Ingredient: " + item.defName + ". From Mod: " + (__result.def.modContentPack != null ? __result.def.modContentPack.Name : "Null"));
                        }
                        else
                        {
                            Log.Message("Ingredient: Null Ingredient");
                        }
                    }
                }
                return;
            }
        }

        public static void correctIngredients(ref CompIngredients comp, IntVec3 loc, Map map)
        {
            if (comp != null)
            {
                List<ThingDef> newIngredients = new List<ThingDef>();
                List<ThingDef> oldIngredients = new List<ThingDef>();

                if (!comp.ingredients.NullOrEmpty())
                {
                    if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)  
                        Log.Message("Has Ingredient Comp: " + comp.parent.def.defName + " and isnt a meal");
                    foreach (var item in comp.ingredients.Where(i => i.race != null))
                    {
                        ThingDef newIngredient = null;
                        if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                        {
                            Log.Message("ingredient is: " + item.defName);
                            Log.Message("category is: " + item.category);
                            if (item.thingCategories != null)
                                Log.Message("thingCategory count is: " + item.thingCategories.Count);
                            Log.Message("Is Ingestible? " + item.IsIngestible);
                            if (item.IsIngestible)
                            {
                                Log.Message("sourceDef is: " + item.ingestible.sourceDef.defName);
                                Log.Message("foodType is: " + item.ingestible.foodType);
                            }
                        }

                        if (item.category != ThingCategory.Item || (item.thingCategories != null && !item.thingCategories.Contains(ThingCategoryDefOf.MeatRaw)) || !item.IsIngestible ||
                            item.ingestible.sourceDef == null || !(item.ingestible.foodType == FoodTypeFlags.Meat))
                        {
                            oldIngredients.Add(item);
                            ThingDef originalRaceIngredient = DefDatabase<ThingDef>.AllDefs.Where(i => i.defName == item.defName && i.label == item.label).First();

                            if (originalRaceIngredient == null)
                            {
                                newIngredients.Add(newIngredient);
                                return;
                            }

                            newIngredient = GetInfo(originalRaceIngredient);
                            SetAsMeat(ref newIngredient, originalRaceIngredient, item);
                            newIngredients.Add(newIngredient);
                        }
                    }
                }
                else
                {
                    //List<ThingDef> possibleIngredients = new List<ThingDef>();
                    //Log.Message("Beginning Generation of " + comp.parent.def.defName + ". Held By " + (comp.parent.ParentHolder.ParentHolder is Pawn && (comp.parent.ParentHolder.ParentHolder as Pawn).Name != null ? (comp.parent.ParentHolder.ParentHolder as Pawn).Name.ToStringFull : "None"));
                    GenerateIngredients(comp.parent.def,ref newIngredients, loc, map);
                }


                #region
                /*
                else
                {
                    //Method to add ingredients to meals with none
                    ThingDef food = comp.parent.def;

                    // Call New Food item Method Here

                    Log.Message("Food Checked is: " + food.defName);
                    if (food.thingCategories.Contains(ThingCategoryDefOf.MeatRaw))
                    {
                        Log.Message(food.defName + " is Raw Meat");
                        ThingDef pawn = null;
                        IEnumerable<ThingDef> choices = null;
                        if (food.thingCategories.Contains(ThingCategoryDefOfLocal.Preserves))
                        {
                            Log.Message("Checking Preserves");
                            if (food.label.Contains("prime meat"))
                            {
                                Log.Message("Preserve is Prime");
                                // Problem Here
                                choices = DefDatabase<ThingDef>.AllDefs.Where(p => p.race != null
                                && !p.race.Humanlike && (p.race.meatLabel != null && p.race.meatLabel == "prime meat") || (p.race.useMeatFrom != null && p.race.useMeatFrom.defName == "Muffalo"));
                                Log.Message("Attempted to Find Choices");
                                if (!choices.EnumerableNullOrEmpty())
                                    pawn = choices.RandomElement();
                            }
                            else
                            {
                                Log.Message("Preserve is Raw or Fish or Humanoid or Insect");
                                // Problem Here
                                choices = DefDatabase<ThingDef>.AllDefs.Where(p => p.race != null
                                 && (p.race.meatLabel != null && p.race.meatLabel == "Raw Meat" || p.race.meatLabel == "Fish meat" || p.race.meatLabel == "Human Meat" || p.race.meatLabel == "Insect Meat")
                                 || (p.race.useMeatFrom != null && p.race.useMeatFrom.defName == "Elephant" || p.race.useMeatFrom.defName == "ThingDefFishSduiggles"
                                 || p.race.useMeatFrom.defName == "Human" || p.race.useMeatFrom.defName == "Megaspider"));
                                Log.Message("Attempted to Find Choices");
                                if (!choices.EnumerableNullOrEmpty())
                                    pawn = choices.RandomElement();
                            } 
                        }
                        else
                        {
                            Log.Message("Finding Exact Meat...");
                            pawn = DefDatabase<ThingDef>.AllDefs.Where(p => p.race != null && p.race.meatDef != null && p.race.meatDef.defName == food.defName).RandomElement();
                        }
                        ThingDef newIngredient = GetInfo(pawn);
                        SetAsMeat(ref newIngredient, pawn, null);
                        //newIngredient.ingestible.sourceDef = pawn;
                        //food.ingestible.sourceDef = pawn;
                        Log.Message("New Ingredient set as Meat: " + newIngredient + ". " + food.defName + " sourceDef, is set to: " + pawn.defName);
                        if (pawn is ThingDef_AlienRace)
                        {
                            newIngredients.Add(newIngredient as ThingDef_AlienRace);
                        }
                        else
                        {
                            newIngredients.Add(newIngredient);
                        }

                    }
                    else if (food.ingestible.IsMeal || food.thingCategories.Contains(ThingCategoryDefOf.FoodMeals))
                    {
                        Log.Message(food.defName + " is Meal");
                        List<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefs.Where(recipe1 => recipe1.products.Any(i => i.thingDef.IsIngestible)).ToList();
                        List<ThingDef> products = new List<ThingDef>();

                        RecipeDef matchingRecipe = recipes.Where(recipe2 => !recipe2.products.Where(p => p.thingDef.defName == food.defName).EnumerableNullOrEmpty()).First();
                        {
                            Log.Message("Recipe Found: " + (matchingRecipe != null ? matchingRecipe.defName : "Null"));
                            foreach (var item in matchingRecipe.ingredients)
                            {
                                ThingDef ingredient = item.filter.AllowedThingDefs.RandomElement();
                                Log.Message("Random Ingredient Found: " + ingredient.defName + ". Is Meal: " + ingredient.ingestible.IsMeal);
                                if (ingredient.IsMeat)
                                {
                                    ThingDef pawn = null;
                                    if (ingredient.defName == "soylentgreen")
                                    {
                                        // Problem Here
                                        pawn = DefDatabase<ThingDef>.AllDefs.Where(p => p.race != null && p.race.meatLabel == "Human Meat" || p.race.useMeatFrom.defName == "Human").RandomElement();
                                    }
                                    else
                                    {
                                        pawn = DefDatabase<ThingDef>.AllDefs.Where(p => p.race != null && p.race.IsFlesh && p.race.meatDef.defName == ingredient.defName).RandomElement();
                                    }

                                    if (pawn != null)
                                    {
                                        ThingDef newIngredient = GetInfo(pawn);
                                        SetAsMeat(ref newIngredient, pawn, null);

                                        if (pawn is ThingDef_AlienRace)
                                        {
                                            newIngredients.Add(newIngredient as ThingDef_AlienRace);
                                        }
                                        else
                                        {
                                            newIngredients.Add(newIngredient);
                                        }
                                        Log.Message(ingredient.defName + " Replaced with " + newIngredient.defName + " and Added to Comp");
                                        //food.ingestible.sourceDef = pawn;
                                    }
                                }
                                else
                                {
                                    newIngredients.Add(ingredient);
                                    Log.Message(ingredient.defName + " Added");
                                }
                            }
                        }
                    }
                }*/
                #endregion

                if (!oldIngredients.NullOrEmpty())
                {
                    foreach (var item in oldIngredients)
                    {
                        comp.ingredients.Remove(item);
                    }
                }
                if (!newIngredients.NullOrEmpty())
                {
                    foreach (var item in newIngredients)
                    {
                        comp.ingredients.Add(item);
                        // would need to harmony patch DrawStatsReport with the thing in the result, to change the special display stats.
                        // or transplier the specialDisplayStats in the ThignDef method
                    }
                    //savedFoods.Add(comp.parent.def);
                }
            }
        }
        
        public static void GenerateIngredients(ThingDef food, ref List<ThingDef> newIngredients, IntVec3 loc, Map map = null)
        {/*
            if (food.defName == thingName)
            {
                recurring++;
            }
            else
            {
                thingName = food.defName;
                recurring = 0;
            }

            if (recurring > 5)
            {
                recurring = 0;
                newIngredients.Add(food);
                return;
            }*/

            //Log.Message("Checking " + food.defName);

            if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
            {
                Log.Message("Generate Ingredients Debug: food is " + (food != null ? food.defName : " Null."));
                Log.Message("location is: " +(loc != null ? loc.ToString() :"Null"));
            }

            CompProperties_Rottable rot = food.GetCompProperties<CompProperties_Rottable>();
            if (rot != null && rot.daysToRotStart == 0)
                return;

            if (food.thingCategories != null && !food.thingCategories.Where(b => b.defName == "BodyPartsNatural").EnumerableNullOrEmpty())
            {
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                    Log.Message("Found Natural BodyPart");
                Pawn pawn = null;
                ThingDef pawnDef = null;
                if (loc != null && map != null)
                {
                    Pawn pawnFound = loc.GetFirstPawn(map);
                    if (pawnFound != null && pawnFound.CurJob.def.defName == "DoBill" && pawnFound.CurJob.workGiverDef.defName == "DoBillsMedicalHumanOperation")
                    {
                        //Log.Message("targetA is: " + pawnFound.CurJob.targetA);
                        pawn = (pawnFound.CurJob.targetA.Pawn);
                    }
                    else
                    {
                        CellRect rect = CellRect.CenteredOn(loc, 1);
                        foreach (var pos in rect)
                        {
                            Pawn pawnFound2 = pos.GetFirstPawn(map);
                            if (pawnFound2 != null && pawnFound2.CurJob.def.defName == "DoBill" && pawnFound2.CurJob.workGiverDef.defName == "DoBillsMedicalHumanOperation")
                            {
                                //Log.Message("targetA is: " + pawnFound2.CurJob.targetA);
                                pawn = (pawnFound2.CurJob.targetA.Pawn);
                            }
                        }
                    }
                    if (pawn != null)
                        pawnDef = pawn.def;
                }

                if (pawnDef != null)
                {
                    ThingDef newIngredient = null;
                    newIngredient = GetInfo(pawnDef);
                    SetAsMeat(ref newIngredient, pawnDef);
                    newIngredients.Add(newIngredient);
                    return;
                }
                else
                {
                    if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                        Log.Message("No Pawn Found");
                    ThingDef human = DefDatabase<ThingDef>.AllDefsListForReading.Find(x => x.defName == "Human");
                    if (human == null && DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                        Log.Message("Couldnt Find Human Meat");
                    ThingDef meat = ConvertMeatToRandomRace(human.race.meatDef);
                    if (meat != null)
                    {
                        if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                        {
                            if (meat.IsIngestible && meat.ingestible.sourceDef != null)
                            {
                                Log.Message("Assigned source is: " + meat.ingestible.sourceDef.defName);
                            }
                            else
                            {
                                Log.Message("Assigned source is: Null");
                            }
                        }
                        newIngredients.Add(meat);
                    }
                    else
                    {
                        if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                        {
                            Log.Message("couldnt find random meat");
                        }
                    }
                    return;
                }
            }

            foreach (var item in GetPossibleIngredients(food))
            {
                if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                    Log.Message("Checking Possible Ingredients");
                //string race = "None";
                //if (item.IsIngestible && item.ingestible.sourceDef != null)
                //    race = item.ingestible.sourceDef.defName;
                //Log.Message("Ingredients: " + item.defName + ". item Race is " + race);
                if (!endOfSearch)
                {
                    if (item.IsIngestible && item.ingestible.sourceDef != null)
                    {
                        ThingDef possibleMeat = ConvertMeatToRandomRace(item);
                        if (!newIngredients.Contains(possibleMeat))
                        {
                            //Log.Message("Adding Meat Source: " + possibleMeat.defName);
                            newIngredients.Add(possibleMeat);
                        }
                    }
                    else if (item.IsIngestible && item.ingestible.IsMeal)
                    {
                        //Log.Message("Is Meal: " + item.defName);
                        foreach (var innerItem in GetPossibleIngredients(item))
                        {
                            GenerateIngredients(innerItem, ref newIngredients, loc, map);
                        }
                    }
                    else if (item.IsIngestible)
                    {
                        foreach (var innerItem in GetPossibleIngredients(item))
                        {
                            if (!newIngredients.Contains(innerItem))
                            {
                                //Log.Message("Adding Ingestible: " + item.defName);
                                newIngredients.Add(innerItem);
                            }
                        }
                    }
                }
                else
                {
                    newIngredients.Add(item);
                    endOfSearch = false;
                }
            }
            //Log.Message("Exited GetPossibleIngredients Loop");
        }

        public static ThingDef ConvertMeatToRandomRace(ThingDef meat)
        {
            if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                Log.Message("Trying to convert: " + (meat.defName != null ? meat.defName : "Null"));
            if (meat.IsIngestible && meat.ingestible.sourceDef != null)
            {
                if (meat.ingestible.foodType == FoodTypeFlags.Meat && meat.thingCategories.Contains(ThingCategoryDefOf.MeatRaw) && !meat.defName.ToLower().Contains("meat"))
                    return meat;

                IEnumerable<ThingDef> pawn = null;
                //Log.Message("About to check if not corpse");
                if (!meat.IsCorpse)
                {
                    pawn = DefDatabase<ThingDef>.AllDefs.Where(i => i.race != null && i.race.meatDef == meat
                    && !i.defName.ToLower().Contains("droid") && !i.defName.ToLower().Contains("skynet") && !i.defName.ToLower().Contains("void") &&
                    (!i.statBases.Where(s => s.stat.defName == "MeatAmount").EnumerableNullOrEmpty() ? i.statBases.Find(s => s.stat.defName == "MeatAmount").value != 0 : true));
                }


                if (pawn != null || meat.IsCorpse)
                {
                    ThingDef randomPawn = null;
                    if (meat.IsCorpse)
                    {
                        if (meat.race != null && meat.race.FleshType != FleshTypeDefOf.Mechanoid)
                            meat = DefDatabase<ThingDef>.AllDefs.Where(d => d.defName == "Elephant").First().race.corpseDef;
                        //Log.Message("Is Corpse");
                        randomPawn = meat.ingestible.sourceDef;
                    }
                    else
                    {
                        randomPawn = pawn.RandomElement();
                    }
                    //Log.Message("Selection Count: " + (pawn != null ? pawn.Count().ToString() : "0"));
                    ThingDef newIngredient = null;
                    //Log.Message("randomPawn is: " + randomPawn.defName);
                    if (randomPawn == null)
                        randomPawn = DefDatabase<ThingDef>.AllDefs.Where(i => i.race != null && !i.race.IsMechanoid && !i.defName.ToLower().Contains("droid") && !i.defName.ToLower().Contains("skynet") && !i.defName.ToLower().Contains("void") &&
                    (!i.statBases.Where(s => s.stat.defName == "MeatAmount").EnumerableNullOrEmpty() ? i.statBases.Find(s => s.stat.defName == "MeatAmount").value != 0 : true)).RandomElement();
                    newIngredient = GetInfo(randomPawn);
                    //Log.Message("Info Aquired");
                    SetAsMeat(ref newIngredient, randomPawn, meat);
                    //Log.Message("Converted: " + meat.defName + " to " + newIngredient.defName);
                    return newIngredient;
                }
            }
            return meat;
        }

        public static List<ThingDef> GetPossibleIngredients(ThingDef food)
        {
            List<ThingDef> possibleIngredients = new List<ThingDef>();
            List<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefs.Where(recipe1 => recipe1.products.Any(i => i.thingDef.IsIngestible) && !recipe1.defName.Contains("Glycerol")).ToList();

            if (DebugSettings.godMode && AssortedTweaksMod.instance.Settings.ShowDebugMessages)
                Log.Message("Looking for Recipes for " + food.defName);
            RecipeDef matchingRecipe = recipes.Where(recipe2 => !recipe2.products.Where(p => p.thingDef.defName == food.defName).EnumerableNullOrEmpty()).RandomElementWithFallback();

            //Log.Message("Get Possible Ingredients... ");
            if (food.IsIngestible && food.ingestible.sourceDef != null)
            {
                possibleIngredients.Add(food);
                return possibleIngredients;
            }

            if (matchingRecipe == null)
            {
                //Log.Warning("Couldnt find recipe for " + food.defName);

                /*
                else if (food.IsIngestible && food.ingestible.sourceDef != null)
                {
                    
                    ThingDef possibleMeat = ConvertMeatToRandomRace(food);
                    if (!possibleIngredients.Contains(possibleMeat))
                    {
                        Log.Message("Adding Meat Source: " + possibleMeat.defName);
                        possibleIngredients.Add(possibleMeat);
                    }
                return possibleIngredients;
                }*/
                // the above has been moved down
                
                if (food.IsIngestible && food.ingestible.IsMeal && food.defName.Contains("NutrientPaste")) 
                {
                    IEnumerable<ThingDef> randomIngredient = DefDatabase<ThingDef>.AllDefs.Where(d => (d.IsWithinCategory(ThingCategoryDefOf.PlantFoodRaw) || d.IsWithinCategory(ThingCategoryDefOf.MeatRaw))
                        && !d.defName.ToLower().Contains("skynet") && !d.defName.ToLower().Contains("droid") && !d.defName.ToLower().Contains("egg")
                        && !d.defName.ToLower().Contains("human") && !d.defName.ToLower().Contains("robot") && !d.defName.ToLower().Contains("soylentgreen")
                        && !d.defName.ToLower().Contains("salt") && !d.defName.Contains("Canned") && (d.race == null || !d.race.IsMechanoid));
                    if (!randomIngredient.EnumerableNullOrEmpty())
                    {
                        //foreach (var item in randomIngredient)
                        //{
                        //    Log.Message("Possible ingredient: " + item.defName);
                        //}
                        ThingDef ing = randomIngredient.RandomElement();
                        if (ing.IsIngestible && ing.ingestible.sourceDef != null) 
                        {
                            ThingDef possibleMeat = ConvertMeatToRandomRace(ing);
                            if (!possibleIngredients.Contains(possibleMeat))
                            {
                                //Log.Message("Adding Meat Source: " + possibleMeat.defName);
                                possibleIngredients.Add(possibleMeat);
                            }
                        }
                        else
                        {
                            possibleIngredients.Add(ing);
                        }
                        //endOfSearch = true;
                        return possibleIngredients;
                    }
                }

                if (food.thingCategories != null && food.thingCategories.Find(c => c.defName == "MeatSubRaw" || c.defName == "CookingSupplies"
                || c.defName == "AnimalProductRaw" || c.defName == "VegetableOrFruit" || c.defName == "BasicPlantFoodRaw"
                || c.defName == "ExtraPlantFoodRaw" || c.defName == "LuxuryStuffs") != null
                    && food.thingCategories.Find(cat => cat.defName == "Preserves") == null && !food.ingestible.IsMeal)
                {
                    if (food.ingestible.JoyKind == null || food.ingestible.JoyKind.defName != "Chemical")
                    {
                        //Log.Message("Adding ingredient: " + food.defName);
                        possibleIngredients.Add(food);
                        endOfSearch = true;
                        return possibleIngredients;
                    }
                }
                else
                {
                    if (food.IsWithinCategory(ThingCategoryDefOf.PlantFoodRaw) || food.IsWithinCategory(ThingCategoryDefOf.MeatRaw)
                        && food.IsIngestible && food.ingestible.foodType != FoodTypeFlags.Meal && !food.HasComp(typeof(CompProperties_Ingredients)))
                    {
                        
                        //Log.Message("Adding Random Ingredient");
                        if (food.ingestible.sourceDef != null)
                        {
                            ThingDef temp = ConvertMeatToRandomRace(food);
                            //Log.Message("Convert to Race");
                            if (temp != null)
                            {
                                possibleIngredients.Add(temp);
                            }
                            else
                            {
                                //Log.Message("Add Other");
                                possibleIngredients.Add(food);
                            }

                        }
                        else
                        {
                            //Log.Message("Add Vegetable/Fruit");
                            possibleIngredients.Add(food);
                        }
                    }
                }
                if (possibleIngredients.NullOrEmpty() && food.IsIngestible && food.GetCompProperties<CompProperties_Ingredients>() == null)
                {
                    possibleIngredients.Add(food);
                    endOfSearch = true;
                }

                return possibleIngredients;
            }
            //Log.Message("Total Ingredients to find for recipe: " + matchingRecipe.ingredients.Count);
            foreach (var item in matchingRecipe.ingredients)
            {
                //item.filter.DisplayRootCategory.ChildCategoryNodes.RandomElement().catDef
                //Log.Message("checking ThingCount: " + item.Summary);
                if (matchingRecipe.ingredients.Count == 1 && item.filter.AllowedThingDefs.Any(f => f.defName.ToLower().Contains("flour") || f.defName.ToLower().Contains("milk")))
                {
                    possibleIngredients.Add(food);
                    endOfSearch = true;
                    return possibleIngredients;
                }

                foreach (var item2 in item.filter.AllowedThingDefs)
                {
                    //Log.Message("Allowed: " + item2.defName);
                }
                if (!item.filter.AllowedThingDefs.EnumerableNullOrEmpty())
                {
                    ThingDef randomIngredient = null;
                    if (!item.filter.AllowedThingDefs.Where(i => i.label.ToLower().Contains("prime meat")).EnumerableNullOrEmpty())
                    {
                        //Log.Message("Prime");
                        // Need to seperate code to grab race meat.
                        float chance = 0.4f;
                        if (Random.Range(0f, 1f) > chance)
                        {
                            randomIngredient = ThingDefOf.Muffalo.race.meatDef;
                        }
                        else
                        {
                            randomIngredient = item.filter.AllowedThingDefs.Where(meat => !meat.defName.ToLower().Contains("canned")).RandomElementWithFallback();
                        }
                    }
                    else if (item.filter.AllowedThingDefs.Where(d => !d.defName.ToLower().Contains("egg")).EnumerableNullOrEmpty())
                    {
                        //Log.Message("Egg");
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => d.IsIngestible && !d.defName.ToLower().Contains("skynet")
                        && !d.defName.ToLower().Contains("droid") && !d.defName.ToLower().Contains("human") && !d.defName.ToLower().Contains("robot")
                        && !d.defName.ToLower().Contains("soylentgreen") && !d.defName.ToLower().Contains("salt")).RandomElementWithFallback();
                    }
                    else if (isCannible && item.filter.AllowedThingDefs.Contains(ThingDefOf.Meat_Human))
                    {
                        //Log.Message("Human");
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => d.IsIngestible && d.defName.ToLower().Contains("human")
                        || d.defName.ToLower().Contains("soylentgreen")).RandomElementWithFallback();
                    }
                    else
                    {
                        //Log.Message("Other");
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => d.IsIngestible && !d.defName.ToLower().Contains("skynet") && !d.defName.ToLower().Contains("droid")
                        && !d.defName.ToLower().Contains("egg") && !d.defName.ToLower().Contains("human") && !d.defName.ToLower().Contains("robot")
                        && !d.defName.ToLower().Contains("soylentgreen") && !d.defName.ToLower().Contains("salt") && (d.race == null || !d.race.IsMechanoid)).RandomElementWithFallback();
                    }

                    if (randomIngredient != null)
                    {
                        //Log.Message("Ingredient Found");
                        //Log.Message("Ingredient is: " + randomIngredient.defName + " Is Corpse: " + randomIngredient.IsCorpse);
                        if (randomIngredient.IsIngestible && randomIngredient.ingestible.sourceDef != null)
                        {
                                possibleIngredients.Add(ConvertMeatToRandomRace(randomIngredient));
                        }
                        else
                        {
                            if (randomIngredient.IsIngestible)
                                possibleIngredients.Add(randomIngredient);
                        }
                    }
                    else
                    {
                        randomIngredient = item.filter.AllowedThingDefs.Where(i => i.IsIngestible && i.IsRawFood() && i.ingestible.JoyKind != null
                            && i.ingestible.JoyKind.defName != "Chemical" && !i.defName.Contains("Glycerol")).RandomElementWithFallback();
                        //Log.Message("randomIngredient Assigned: " + (randomIngredient != null ? randomIngredient.defName : "Is Null"));
                        //Log.Message("randomIngredient is null");
                    }

                    /*
                    if (food.defName.Contains("Prime"))
                    {
                        // Need to seperate code to grab race meat.
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => d.label == "Prime Meat").RandomElement();
                    }
                    else if (item.filter.AllowedThingDefs.Where(d => !d.defName.Contains("Egg")).EnumerableNullOrEmpty())
                    {
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => !d.defName.Contains("Android")
                        && !d.defName.Contains("Skynet") && !d.defName.Contains("Droid")).RandomElement();
                    }
                    else
                    {
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => !d.defName.Contains("Android")
                        && !d.defName.Contains("Skynet") && !d.defName.Contains("Droid") && !d.defName.Contains("Egg")).RandomElement();
                    }*/

                }
            }
            //string possible = "Possible Ingredients: ";
            //foreach (var item in possibleIngredients)
            //{
            //    possible += item.defName + ", ";
            //}
            //Log.Message(possible);
            //Log.Message("Reached the end. ingredient count is: " + (possibleIngredients == null ? "Null" : possibleIngredients.Count.ToString()));
            return possibleIngredients;
        }

        public static ThingDef GetInfo(ThingDef originalRace)
        {
            ThingDef newIngredient = new ThingDef()
            {
                defName = originalRace.defName,
                label = originalRace.label,
                description = originalRace.description,
                thingClass = originalRace.thingClass,
                race = originalRace.race,
                statBases = originalRace.statBases,
                tools = originalRace.tools,
                modContentPack = originalRace.modContentPack
                //ingredient = originalRaceIngredient.ingredient
            };
            return newIngredient;
        }

        public static void SetAsMeat(ref ThingDef newIngredient, ThingDef originalRace, ThingDef originalIngredient = null)
        {
            newIngredient.category = ThingCategory.Item;
            if (!newIngredient.thingCategories.NullOrEmpty())
            {
                newIngredient.thingCategories.Add(ThingCategoryDefOf.MeatRaw);
            }
            else
            {
                newIngredient.thingCategories = new List<ThingCategoryDef>();
                newIngredient.thingCategories.Add(ThingCategoryDefOf.MeatRaw);
            }
            if (originalRace.race != null)
            {
                ThingWithComps thing2 = (ThingWithComps)ThingMaker.MakeThing(originalRace.race.meatDef);
                newIngredient.ingestible = thing2.def.ingestible;
            }

            if (originalRace.race != null && originalRace.race.Humanlike)
            {
                newIngredient.ingestible.sourceDef = originalRace as ThingDef_AlienRace;
            }
            else
            {
                newIngredient.ingestible.sourceDef = originalRace;
            }
            newIngredient.ingestible.foodType = FoodTypeFlags.Meat;
            //newIngredient.ingestible.sourceDef = originalRace;
        }
    }

    public class MeatHasIngredients
    {
        /*
        public static List<ThingDef> primeMeatPawns = new List<ThingDef>();
        public static List<ThingDef> rawMeatPawns = new List<ThingDef>();
        public static List<ThingDef> fishMeatPawns = new List<ThingDef>();
        public static List<ThingDef> insectMeatPawns = new List<ThingDef>();
        public static List<ThingDef> humanMeatPawns = new List<ThingDef>();
        */
        public static void MyPostfix()
        {
            foreach (var meat in DefDatabase<ThingDef>.AllDefs.Where(d => (d.IsIngestible && d.ingestible.foodType == FoodTypeFlags.Meat && d.category == ThingCategory.Item)/*
            || d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Where(b => b.parent == ThingCategoryDefOf.BodyParts).EnumerableNullOrEmpty()*/))
            {
                if (meat.GetCompProperties<CompProperties_Ingredients>() == null)
                {
                    CompProperties_Ingredients comp = new CompProperties_Ingredients();
                    comp.splitTransferableFoodKind = true;
                    meat.comps.Add(comp);
                }
                /*
                if (!meat.IsIngestible && meat.thingCategories != null && meat.thingCategories.Where(b => b.parent == ThingCategoryDefOf.BodyParts).EnumerableNullOrEmpty())
                {
                    //ThingWithComps thing1 = (ThingWithComps)ThingMaker.MakeThing(DefDatabase<ThingDef>.AllDefsListForReading.Find(d => d.defName == "Human").race.meatDef);
                    meat.ingestible = new IngestibleProperties()
                    {
                        parent = meat,
                        foodType = FoodTypeFlags.Meat,
                        preferability = FoodPreferability.RawBad
                    };
                }*/
            }
        }
    }

    /*
     * vanilla ui code is broken atm, unless visible is set to true, the rect gets created as a blank if a trainability is disabled.
     * but the height of the window isnt adjusted to account for this, so it ends up not being visible as its below a blank row.
     */

    [HarmonyPatch(typeof(Pawn_TrainingTracker), "CanAssignToTrain",new System.Type[] { typeof(TrainableDef), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
    public class CanAssignToTrain_Patch
    {
        public static void Postfix(ref AcceptanceReport __result, ref Pawn_TrainingTracker __instance, TrainableDef td,out bool visible)
        {
            visible = true;
        }
    }

    [HarmonyPatch(typeof(DebugActionsUtility), "PointsOptions")]
    public class MoreDevRaidpoints
    {
        [HarmonyPostfix]
        public static void PointsOptions_Postfix(bool extended, ref IEnumerable<float> __result)
        {
            List<float> list = __result.ToList<float>();
            for (int index = 11000; index <= 30000; index += 1000)
                list.Add((float)index);
            __result = (IEnumerable<float>)list;
        }
    }

    /*
     * Grab tweakValues
     */
    /*
     [HarmonyPatch(typeof(EditWindow_TweakValues), "FindAllTweakables")]
     public class FindAllTweakables_Patch
     {
         public static void Postfix(ref IEnumerable<FieldInfo> __result)
         {
             *//*
             TweakValue_DoWindowContents_Patch.tweakValueFields = __result.Select<FieldInfo, TweakValue_DoWindowContents_Patch.TweakInfo>((Func<FieldInfo, TweakValue_DoWindowContents_Patch.TweakInfo>)(field => new TweakValue_DoWindowContents_Patch.TweakInfo()
             {
                 field = field,
                 tweakValue = field.TryGetAttribute<TweakValue>(),
                 initial = TweakValue_DoWindowContents_Patch.GetAsFloat(field)
             })).OrderBy<TweakValue_DoWindowContents_Patch.TweakInfo, string>((Func<TweakValue_DoWindowContents_Patch.TweakInfo, string>)(ti => string.Format("{0}.{1}", (object)ti.tweakValue.category, (object)ti.field.DeclaringType.Name))).ToList<TweakValue_DoWindowContents_Patch.TweakInfo>();

             if (TweakValue_DoWindowContents_Patch.tweakValueFields != null)
             {
                 TweakValue_DoWindowContents_Patch.categories = new List<string>();
                 foreach (var item in TweakValue_DoWindowContents_Patch.tweakValueFields)
                 {
                     if (item.tweakValue != null && !categories.Contains(item.tweakValue.category))
                         TweakValue_DoWindowContents_Patch.categories.Add(item.tweakValue.category);
                     if (item.tweakValue == null)
                         Log.Message("Null TweakValue");
                 }
             }
             */ /*
            int count = 0;
            tweakValueFields = (from ti in __result.Select(delegate (FieldInfo field)
            {
                Log.Message("Entered Here " + count);
                if (field.Name.ToLower().Contains("minticks")) 
                {
                    return new TweakInfo();
                }
                TweakInfo result = default(TweakInfo);
                result.field = field;
                result.tweakValue = field.TryGetAttribute<TweakValue>();
                result.initial = GetAsFloat(field);
                count++;
                return result;
            })
                                orderby $"{ti.tweakValue.category}.{ti.field.DeclaringType.Name}"
                                select ti).ToList();
            TweakValue_DoWindowContents_Patch.categories = new List<string>();
            Log.Message("Reached Here 1");
            foreach (var item in tweakValueFields)
            {
                if (item.tweakValue != null && !categories.Contains(item.tweakValue.category))
                    categories.Add(item.tweakValue.category);
                if (item.tweakValue == null)
                    Log.Message("Null TweakValue: " + item.ToString());
            }
            Log.Message("Reached Here 2");
            selected = categories.First();
        }
    }
    */
    /*
     * Improve performance of tweaks menu
     */
    /*
   [HarmonyPatch(typeof(EditWindow_TweakValues), "DoWindowContents", new System.Type[] { typeof(Rect)})]
   public class TweakValue_DoWindowContents_Patch
   {
       private static Vector2 scrollPosition;
       public static List<TweakValue_DoWindowContents_Patch.TweakInfo> tweakValueFields;
       public static List<string> categories;
       public static string selected;

       public static bool Prefix(EditWindow_TweakValues __instance, Rect inRect)
       {
           if (tweakValueFields != null)
           {
               Text.Font = GameFont.Small;
               Rect outRect;
               //Rect header = inRect.ContractedBy(18f);
               //selected = "";
               Rect rect1 = outRect = inRect.ContractedBy(4f); //4f original
               rect1.xMax -= 33f;
               Rect rectSelectable = new Rect(0.0f, 0.0f, EditWindow_TweakValues.CategoryWidth, Text.CalcHeight("test", 1000f));
               if (Widgets.ButtonText(rectSelectable, selected, true, true, Color.white))
               {
                   List<FloatMenuOption> options = new List<FloatMenuOption>();
                   foreach (string field in categories)
                   {
                       options.Add(new FloatMenuOption((string)field, (Action)(() =>
                       {
                           selected = field;
                           //__instance.DoWindowContents(inRect);
                           //this.Setup();
                       })));
                   }
               Find.WindowStack.Add((Window)new FloatMenu(options));
               }
               if (!selected.NullOrEmpty())
               {
                   Rect rect2 = new Rect(0.0f, 0.0f, EditWindow_TweakValues.CategoryWidth, Text.CalcHeight("test", 1000f));
                   Rect rect3 = new Rect(rect2.xMax, 0.0f, EditWindow_TweakValues.TitleWidth, rect2.height);
                   Rect rect4 = new Rect(rect3.xMax, 0.0f, EditWindow_TweakValues.NumberWidth, rect2.height);
                   Rect rect5 = new Rect(rect4.xMax, 0.0f, rect1.width - rect4.xMax, rect2.height);
                   ref Vector2 local = ref scrollPosition;
                   Rect viewRect = new Rect(0.0f, -20f, rect1.width, rect2.height * (float)tweakValueFields.Count);
                   Widgets.BeginScrollView(outRect, ref local, viewRect);
                   foreach (TweakInfo tweakValueField in tweakValueFields.Where(f => f.tweakValue.category == selected))
                   {
                       Widgets.Label(rect2, tweakValueField.tweakValue.category);
                       Widgets.Label(rect3, string.Format("{0}.{1}", (object)tweakValueField.field.DeclaringType.Name, (object)tweakValueField.field.Name));
                       float input;
                       bool flag;
                       if (tweakValueField.field.FieldType == typeof(float) || tweakValueField.field.FieldType == typeof(int) || tweakValueField.field.FieldType == typeof(ushort))
                       {
                           double asFloat = (double)GetAsFloat(tweakValueField.field);
                           input = Widgets.HorizontalSlider(rect5, GetAsFloat(tweakValueField.field), tweakValueField.tweakValue.min, tweakValueField.tweakValue.max);
                           SetFromFloat(tweakValueField.field, input);
                           double num = (double)input;
                           flag = asFloat != num;
                       }
                       else if (tweakValueField.field.FieldType == typeof(bool))
                       {
                           int num1;
                           bool checkOn = (num1 = (bool)tweakValueField.field.GetValue((object)null) ? 1 : 0) != 0;
                           Widgets.Checkbox(rect5.xMin, rect5.yMin, ref checkOn);
                           tweakValueField.field.SetValue((object)null, (object)checkOn);
                           input = checkOn ? 1f : 0.0f;
                           int num2 = checkOn ? 1 : 0;
                           flag = num1 != num2;
                       }
                       else
                       {
                           Log.ErrorOnce(string.Format("Attempted to tweakvalue unknown field type {0}", (object)tweakValueField.field.FieldType), 83944645);
                           flag = false;
                           input = tweakValueField.initial;
                       }
                       if ((double)input != (double)tweakValueField.initial)
                       {
                           GUI.color = Color.red;
                           Text.WordWrap = false;
                           Widgets.Label(rect4, string.Format("{0} -> {1}", (object)tweakValueField.initial, (object)input));
                           Text.WordWrap = true;
                           GUI.color = Color.white;
                           if (Widgets.ButtonInvisible(rect4))
                           {
                               flag = true;
                               if (tweakValueField.field.FieldType == typeof(float) || tweakValueField.field.FieldType == typeof(int) || tweakValueField.field.FieldType == typeof(ushort))
                                   SetFromFloat(tweakValueField.field, tweakValueField.initial);
                               else if (tweakValueField.field.FieldType == typeof(bool))
                                   tweakValueField.field.SetValue((object)null, (object)((double)tweakValueField.initial != 0.0));
                               else
                                   Log.ErrorOnce(string.Format("Attempted to tweakvalue unknown field type {0}", (object)tweakValueField.field.FieldType), 83944646);
                           }
                       }
                       else
                           Widgets.Label(rect4, string.Format("{0}", (object)tweakValueField.initial));
                       if (flag)
                       {
                           MethodInfo method = tweakValueField.field.DeclaringType.GetMethod(tweakValueField.field.Name + "_Changed", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                           if (method != (MethodInfo)null)
                               method.Invoke((object)null, (object[])null);
                       }
                       rect2.y += rect2.height;
                       rect3.y += rect2.height;
                       rect4.y += rect2.height;
                       rect5.y += rect2.height;
                   }
                   Widgets.EndScrollView();
               }
               return false;

           }
           return true;
       }

       public void SelectCategory(string s)
       {
           selected = s;
       }

       private static bool ShowThingButton(Rect containerRect, bool withBackButtonOffset = false)
       {
           float x = (float)((double)containerRect.x + (double)containerRect.width - 14.0 - 200.0 - 16.0);
           if (withBackButtonOffset)
               x -= 136f;
           return Widgets.ButtonText(new Rect(x, containerRect.y + 18f, 200f, 40f), (string)"Select_Thing".Translate());
       }

       public static float GetAsFloat(FieldInfo field)
       {
           if (field.FieldType == typeof(float))
               return (float)field.GetValue((object)null);
           if (field.FieldType == typeof(bool))
               return (bool)field.GetValue((object)null) ? 1f : 0.0f;
           if (field.FieldType == typeof(int))
               return (float)(int)field.GetValue((object)null);
           if (field.FieldType == typeof(ushort))
               return (float)(ushort)field.GetValue((object)null);
           Log.ErrorOnce(string.Format("Attempted to return unknown field type {0} as a float", (object)field.FieldType), 83944644);
           return 0.0f;
       }

       public static void SetFromFloat(FieldInfo field, float input)
       {
           if (field.FieldType == typeof(float))
               field.SetValue((object)null, (object)input);
           else if (field.FieldType == typeof(bool))
               field.SetValue((object)null, (object)((double)input != 0.0));
           else if (field.FieldType == typeof(int))
               field.SetValue((object)field, (object)(int)input);
           else if (field.FieldType == typeof(ushort))
               field.SetValue((object)field, (object)(ushort)input);
           else
               Log.ErrorOnce(string.Format("Attempted to set unknown field type {0} from a float", (object)field.FieldType), 83944645);
       }

       public struct TweakInfo
       {
           public FieldInfo field;
           public TweakValue tweakValue;
           public float initial;
       }
       
    }*/

}
