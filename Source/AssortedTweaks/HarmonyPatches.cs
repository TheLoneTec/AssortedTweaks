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
                List<ThingDef> toRemove = destIngredients.Where(i => /*i.race != null &&*/ scr.defName == i.defName && scr.label == i.label).ToList();
                foreach (var item in toRemove)
                {
                    destIngredients.Remove(item);
                }
            }
            CompIngredients tmp = new CompIngredients();
            tmp.ingredients = srcIngredients;
            CorrectIngredients.correctIngredients(ref tmp);
            srcIngredients = tmp.ingredients;
        }
    }

        [HarmonyPatch(typeof(FoodUtility), "IsVeneratedAnimalMeatOrCorpse")]
    public class IsVeneratedAnimalMeatOrCorpse_Patch
    {
        public static void Postfix(ref bool __result, ThingDef foodDef, Pawn ingester)
        {
            if (!AssortedTweaksMod.instance.Settings.MeatIngredients)
                return;

            if (foodDef.ingestible == null)
                return;

            if (foodDef.ingestible.sourceDef == null)
                return;

            if (ingester.RaceProps.Humanlike && ingester.def != null && foodDef.IsIngestible)
            {

                __result = ingester.Ideo.IsVeneratedAnimal(foodDef.ingestible.sourceDef);
            }
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

    [HarmonyPatch(typeof(GenSpawn), "Spawn", new System.Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
    public class CorrectIngredients
    {
        //public static Dictionary<Faction, ThingDef> factionMainMeatSource = new Dictionary<Faction, ThingDef>();
        public static Thing currentPawn = null;
        public static bool isCannible = false;
        public static string thingName = "";
        public static int recurring = 0;

        public static void Postfix(ref Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode, bool respawningAfterLoad)
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
                    if ((currentPawn as Pawn).story != null && (currentPawn as Pawn).story.traits.HasTrait(TraitDefOf.Cannibal)
                        || ModLister.IdeologyInstalled && (currentPawn as Pawn).Ideo != null && (currentPawn as Pawn).Ideo.HasHumanMeatEatingRequiredPrecept())
                    {
                        isCannible = true;
                    }
                    //Log.Message("CorrectedIngredients Entered for : " + __result.def.defName);
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
                                correctIngredients(ref comp);
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
                                correctIngredients(ref comp);
                            }
                        }
                    }
                }
                else if (__result.def.category == ThingCategory.Item && __result.def.IsIngestible && __result.TryGetComp<CompIngredients>() != null)
                {
                    comp = __result.TryGetComp<CompIngredients>();
                    if (comp != null)
                    {
                        correctIngredients(ref comp);
                    }
                }
                isCannible = false;
            } catch (Exception e)
            {
                Log.Warning("Assorted Tweaks Encountered an Error: " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void correctIngredients(ref CompIngredients comp)
        {
            if (comp != null)
            {
                List<ThingDef> newIngredients = new List<ThingDef>();
                List<ThingDef> oldIngredients = new List<ThingDef>();

                if (!comp.ingredients.NullOrEmpty())
                {
                    Log.Message("Has Ingredient Comp: " + comp.parent.def.defName + " and isnt a meal");
                    foreach (var item in comp.ingredients.Where(i => i.race != null))
                    {
                        ThingDef newIngredient = null;
                        if ((item.category == ThingCategory.Item) || !item.thingCategories.Contains(ThingCategoryDefOf.MeatRaw) || !item.IsIngestible ||
                            item.ingestible.sourceDef == null || !(item.ingestible.foodType == FoodTypeFlags.Meat))
                        {
                            oldIngredients.Add(item);
                            ThingDef originalRaceIngredient = DefDatabase<ThingDef>.AllDefs.Where(i => i.race == item.race).First();
                            newIngredient = GetInfo(originalRaceIngredient);
                            SetAsMeat(ref newIngredient, originalRaceIngredient, item);
                            newIngredients.Add(newIngredient);
                        }
                    }
                }
                else
                {
                    //List<ThingDef> possibleIngredients = new List<ThingDef>();
                    Log.Message("Beginning Generation of " + comp.parent.def.defName + ". Held By " + (comp.parent.ParentHolder.ParentHolder is Pawn && (comp.parent.ParentHolder.ParentHolder as Pawn).Name != null ? (comp.parent.ParentHolder.ParentHolder as Pawn).Name.ToStringFull : "None"));
                    GenerateIngredients(comp.parent.def,ref newIngredients);
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
        
        public static void GenerateIngredients(ThingDef food, ref List<ThingDef> newIngredients)
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
            
            Log.Message("Checking " + food.defName);
            CompProperties_Rottable rot = food.GetCompProperties<CompProperties_Rottable>();
            if (rot != null && rot.daysToRotStart == 0)
                return;
            foreach (var item in GetPossibleIngredients(food))
            {
                //Log.Message("Entered For Loop");
                //string race = "None";
                //if (item.IsIngestible && item.ingestible.sourceDef != null)
                //    race = item.ingestible.sourceDef.defName;
                //Log.Message("Ingredients: " + item.defName + ". item Race is " + race);
                if (item.IsIngestible && item.ingestible.sourceDef != null)
                {
                    ThingDef possibleMeat = ConvertMeatToRandomRace(item);
                    if (!newIngredients.Contains(possibleMeat))
                    {
                        Log.Message("Adding Meat Source: " + possibleMeat.defName);
                        newIngredients.Add(possibleMeat);
                    }
                }
                else if (item.IsIngestible && item.ingestible.IsMeal)
                {
                    Log.Message("Is Meal: " + item.defName);
                    foreach (var innerItem in GetPossibleIngredients(item))
                    {
                        GenerateIngredients(innerItem, ref newIngredients);
                    }
                }
                else if(item.IsIngestible)
                {
                    foreach (var innerItem in GetPossibleIngredients(item))
                    {
                        if (!newIngredients.Contains(innerItem))
                        {
                            Log.Message("Adding Ingestible: " + item.defName);
                            newIngredients.Add(innerItem);
                        }
                    }      
                }
            }
            Log.Message("Exited GetPossibleIngredients Loop");
        }

        public static ThingDef ConvertMeatToRandomRace(ThingDef meat)
        {
            //Log.Message("Trying to convert: " + meat.defName);
            if (meat.IsIngestible && meat.ingestible.sourceDef != null)
            {
                if (meat.ingestible.foodType == FoodTypeFlags.Meat && meat.thingCategories.Contains(ThingCategoryDefOf.MeatRaw) && !meat.defName.ToLower().Contains("meat"))
                    return meat;

                IEnumerable<ThingDef> pawn = null;
                Log.Message("About to check if not corpse");
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
                        if (meat.defName.Contains("Mech") || meat.race != null && meat.race.FleshType != FleshTypeDefOf.Mechanoid)
                            meat = DefDatabase<ThingDef>.AllDefs.Where(d => d.defName == "Elephant").First().race.corpseDef;
                        Log.Message("Is Corpse");
                        randomPawn = meat.ingestible.sourceDef;
                    }
                    else
                    {
                        randomPawn = pawn.RandomElement();
                    }
                    Log.Message("Selection Count: " + (pawn != null ? pawn.Count().ToString() : "0"));
                    ThingDef newIngredient = null;
                    Log.Message("randomPawn is: " + randomPawn.defName);
                    if (randomPawn == null)
                        randomPawn = DefDatabase<ThingDef>.AllDefs.Where(i => i.race != null && !i.race.IsMechanoid && !i.defName.ToLower().Contains("droid") && !i.defName.ToLower().Contains("skynet") && !i.defName.ToLower().Contains("void") &&
                    (!i.statBases.Where(s => s.stat.defName == "MeatAmount").EnumerableNullOrEmpty() ? i.statBases.Find(s => s.stat.defName == "MeatAmount").value != 0 : true)).RandomElement();
                    newIngredient = GetInfo(randomPawn);
                    Log.Message("Info Aquired");
                    SetAsMeat(ref newIngredient, randomPawn, meat);
                    Log.Message("Converted: " + meat.defName + " to " + newIngredient.defName);
                    return newIngredient;
                }
            }
            return meat;
        }

        public static List<ThingDef> GetPossibleIngredients(ThingDef food)
        {
            List<ThingDef> possibleIngredients = new List<ThingDef>();
            List<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefs.Where(recipe1 => recipe1.products.Any(i => i.thingDef.IsIngestible)).ToList();

            Log.Message("Looking for Recipes for " + food.defName);
            RecipeDef matchingRecipe = recipes.Where(recipe2 => !recipe2.products.Where(p => p.thingDef.defName == food.defName).EnumerableNullOrEmpty()).RandomElementWithFallback();

            Log.Message("Get Possible Ingredients... ");
            if (food.IsIngestible && food.ingestible.sourceDef != null)
            {
                possibleIngredients.Add(food);
                return possibleIngredients;
            }

            if (matchingRecipe == null)
            {
                Log.Warning("Couldnt find recipe for " + food.defName);

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
                    IEnumerable<ThingDef> randomIngredient = DefDatabase<ThingDef>.AllDefs.Where(d => d.IsWithinCategory(ThingCategoryDefOf.PlantFoodRaw) || d.IsWithinCategory(ThingCategoryDefOf.MeatRaw));
                    if (!randomIngredient.EnumerableNullOrEmpty())
                    {
                        ThingDef ing = randomIngredient.RandomElement();
                        if (ing.IsIngestible && ing.ingestible.sourceDef != null) 
                        {
                            ThingDef possibleMeat = ConvertMeatToRandomRace(ing);
                            if (!possibleIngredients.Contains(possibleMeat))
                            {
                                Log.Message("Adding Meat Source: " + possibleMeat.defName);
                                possibleIngredients.Add(possibleMeat);
                            }
                        }
                        else
                        {
                            possibleIngredients.Add(ing);
                        }

                    }
                }

                if (food.thingCategories != null && food.thingCategories.Find(c => c.defName == "MeatSubRaw" || c.defName == "CookingSupplies"
                || c.defName == "AnimalProductRaw" || c.defName == "VegetableOrFruit" || c.defName == "BasicPlantFoodRaw"
                || c.defName == "BasicPlantFoodRaw" || c.defName == "LuxuryStuffs") != null
                    && food.thingCategories.Find(cat => cat.defName == "Preserves") == null && !food.ingestible.IsMeal)
                {
                    possibleIngredients.Add(food);
                    return possibleIngredients;
                }
                /*
                else
                {
                    if (food.IsWithinCategory(ThingCategoryDefOf.PlantFoodRaw) || food.IsWithinCategory(ThingCategoryDefOf.MeatRaw)
                        && food.IsIngestible && food.ingestible.foodType != FoodTypeFlags.Meal && !food.HasComp(typeof(CompProperties_Ingredients)))
                    {
                        
                        Log.Message("Adding Random Ingredient");
                        if (food.IsIngestible && food.ingestible.sourceDef != null)
                        {
                            ThingDef temp = ConvertMeatToRandomRace(food);
                            Log.Message("Convert to Race");
                            if (temp != null)
                            {
                                possibleIngredients.Add(temp);
                            }
                            else
                            {
                                possibleIngredients.Add(food);
                            }

                        }
                        else
                        {
                            Log.Message("Add Vegetable/Fruit");
                            possibleIngredients.Add(food);
                        }
                    }
                }*/
                if (possibleIngredients.NullOrEmpty() && food.IsRawFood() && food.GetCompProperties<CompProperties_Ingredients>() == null)
                    possibleIngredients.Add(food);
                return possibleIngredients;
            }
            Log.Message("Total Ingredients to find for recipe: " + matchingRecipe.ingredients.Count);
            foreach (var item in matchingRecipe.ingredients)
            {
                //item.filter.DisplayRootCategory.ChildCategoryNodes.RandomElement().catDef
                Log.Message("checking ThingCount: " + item.Summary);
                if (!item.filter.AllowedThingDefs.EnumerableNullOrEmpty())
                {
                    ThingDef randomIngredient = item.filter.AllowedThingDefs.Where(i => i.IsRawFood()).RandomElementWithFallback();
                    Log.Message("randomIngredient Assigned: " + (randomIngredient != null ? randomIngredient.defName : "Is Null"));
                    if (food.label.ToLower().Contains("prime meat"))
                    {
                        Log.Message("Prime");
                        // Need to seperate code to grab race meat.
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => d.label.ToLower().Contains("prime meat")).RandomElementWithFallback();
                    }
                    else if (item.filter.AllowedThingDefs.Where(d => !d.defName.ToLower().Contains("egg")).EnumerableNullOrEmpty())
                    {
                        Log.Message("Egg");
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => !d.defName.ToLower().Contains("skynet")
                        && !d.defName.ToLower().Contains("droid") && !d.defName.ToLower().Contains("human")
                        && !d.defName.ToLower().Contains("soylentgreen") && !d.defName.ToLower().Contains("salt")).RandomElementWithFallback();
                    }
                    else if (isCannible && item.filter.AllowedThingDefs.Contains(ThingDefOf.Meat_Human))
                    {
                        Log.Message("Human");
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => d.defName.ToLower().Contains("human")
                        || d.defName.ToLower().Contains("soylentgreen")).RandomElementWithFallback();
                    }
                    else
                    {
                        Log.Message("Other");
                        randomIngredient = item.filter.AllowedThingDefs.Where(d => !d.defName.ToLower().Contains("skynet") && !d.defName.ToLower().Contains("droid")
                        && !d.defName.ToLower().Contains("egg") && !d.defName.ToLower().Contains("human")
                        && !d.defName.ToLower().Contains("soylentgreen") && !d.defName.ToLower().Contains("salt")).RandomElementWithFallback();
                    }

                    if (randomIngredient != null)
                    {
                        Log.Message("Ingredient Found");
                        Log.Message("Ingredient is: " + randomIngredient.defName);
                        if (randomIngredient.IsIngestible && randomIngredient.ingestible.sourceDef != null)
                        {

                            possibleIngredients.Add(ConvertMeatToRandomRace(randomIngredient));
                        }
                        else
                        {
                            possibleIngredients.Add(randomIngredient);
                        }
                    }
                    else
                    {
                        Log.Message("randomIngredient is null");
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
            Log.Message("Reached the end. ingredient count is: " + (possibleIngredients == null ? "Null" : possibleIngredients.Count.ToString()));
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

        public static void SetAsMeat(ref ThingDef newIngredient, ThingDef originalRace, ThingDef originalIngredient)
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
            foreach (var meat in DefDatabase<ThingDef>.AllDefs.Where(d => d.IsIngestible && d.ingestible.foodType == FoodTypeFlags.Meat && d.category == ThingCategory.Item))
            {
                if (meat.GetCompProperties<CompProperties_Ingredients>() == null)
                {
                    CompProperties_Ingredients comp = new CompProperties_Ingredients();
                    comp.splitTransferableFoodKind = true;
                    meat.comps.Add(comp);
                }
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

}
