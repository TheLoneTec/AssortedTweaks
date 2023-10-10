using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AssortedTweaks
{
    public static class FoodUtilities
    {
        public static bool CanBeStacked(CompIngredients ingredients,out string reason)
        {
            ThingWithComps stack = new ThingWithComps();
            //if (DebugSettings.godMode)
               // Log.Message("stack 1");
            stack.AllComps.Add(ingredients);
            //if (DebugSettings.godMode)
                //Log.Message("stack 2");
            return CanBeStacked(stack, out reason);
        }
        public static bool CanBeStacked(CompIngredients ingredients)
        {
            ThingWithComps stack = new ThingWithComps();
            string reason = "";
            //if (DebugSettings.godMode)
                //Log.Message("stack 3");
            stack.AllComps.Add(ingredients);
            //if (DebugSettings.godMode)
                //Log.Message("stack 4");
            return CanBeStacked(stack, out reason);
        }
        public static bool CanBeStacked(Thing otherStack, out string reason)
        {
            FactionIdeosTracker playerFactionTracker = null;
            if (Faction.OfPlayer != null && Faction.OfPlayer.ideos != null)
                playerFactionTracker = Faction.OfPlayer.ideos;

            StringBuilder stringBuilder = new StringBuilder();
            bool nonStackable = false;
            if (ModLister.IdeologyInstalled && playerFactionTracker != null)
            {
                //if (DebugSettings.godMode)
                //    Log.Message("CanBeStacked");
                Ideo ideo = playerFactionTracker.PrimaryIdeo;
                //if (DebugSettings.godMode)
                //    Log.Message("Got ideology");
                bool allowMeat = !ideo.HasVegetarianRequiredPrecept();
                //if (DebugSettings.godMode)
                //    Log.Message("Checked Meat");
                bool allowVeg = !ideo.HasMeatEatingRequiredPrecept();
                //if (DebugSettings.godMode)
                //    Log.Message("Checked Veg");
                bool allowInsect = ideo.PreceptsListForReading.Where(p => p.def.defName == "InsectMeatEating_Loved").EnumerableNullOrEmpty() ? false : true;
                //if (DebugSettings.godMode)
                //    Log.Message("Checked Insect");
                bool allowFungus = ideo.PreceptsListForReading.Where(p => p.def.defName == "FungusEating_Preferred").EnumerableNullOrEmpty() ? false : true;
                //if (DebugSettings.godMode)
                //    Log.Message("Checked Fungus");
                bool allowHumanoidMeat = ideo.HasHumanMeatEatingRequiredPrecept() || ideo.PreceptsListForReading.Where(p => p.def.defName == "Cannibalism_Acceptable").EnumerableNullOrEmpty() ? false : true;
                //if (DebugSettings.godMode)
                    //Log.Message("bools checked");

                CompIngredients compIngredients = otherStack.TryGetComp<CompIngredients>();
                //if (DebugSettings.godMode)
                //Log.Message("compIngredients is: " + (compIngredients == null ? "Null" : "Valid"));
                
                if (compIngredients != null)
                {
                    for (int i = 0; i < compIngredients.ingredients.Count; i++)
                    {
                        //if (DebugSettings.godMode)
                            //Log.Message("Checking: " + compIngredients.ingredients[i].defName);
                        nonStackable = false;
                        if (compIngredients.ingredients[i].race != null)
                        {
                            if (compIngredients.ingredients[i].race.Insect && !allowInsect)
                            {
                                //if (DebugSettings.godMode)
                                //    Log.Message("Is Insect");
                                if (!stringBuilder.ToString().Contains("Insect Meat"))
                                    stringBuilder.Append("Insect Meat");
                                nonStackable = true;
                            }
                            else if (compIngredients.ingredients[i].race.Humanlike && !allowHumanoidMeat)
                            {
                                //if (DebugSettings.godMode)
                                //    Log.Message("Is Humanlike");
                                if (!stringBuilder.ToString().Contains("Humanlike Meat"))
                                    stringBuilder.Append("Humanlike Meat");
                                nonStackable = true;
                            }
                            else
                            {
                                foreach (var animal in ideo.VeneratedAnimals)
                                {
                                    if (animal.race == compIngredients.ingredients[i].race)
                                    {
                                        //if (DebugSettings.godMode)
                                        //    Log.Message("Is Venerated");
                                        if (!stringBuilder.ToString().Contains("Venerated Animal"))
                                            stringBuilder.Append("Venerated Animal");
                                        nonStackable = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (compIngredients.ingredients[i].IsMeat && !allowMeat)
                            {
                                //if (DebugSettings.godMode)
                                //    Log.Message("Is Meat");
                                if (!stringBuilder.ToString().Contains("Meat"))
                                    stringBuilder.Append("Meat");
                                nonStackable = true;
                            }
                            else if (compIngredients.ingredients[i].ingestible != null && compIngredients.ingredients[i].ingestible.foodType == FoodTypeFlags.VegetableOrFruit && !allowVeg)
                            {
                                //if (DebugSettings.godMode)
                                //    Log.Message("Is Veg");
                                if (!stringBuilder.ToString().Contains("Vegetable or Fruit"))
                                    stringBuilder.Append("Vegetable or Fruit");
                                nonStackable = true;
                            }
                            else if (compIngredients.ingredients[i].IsFungus && !allowFungus)
                            {
                                //if (DebugSettings.godMode)
                                //    Log.Message("Is Fungus");
                                if (!stringBuilder.ToString().Contains("Fungus"))
                                    stringBuilder.Append("Fungus");
                                nonStackable = true;
                            }
                        }
                        if (nonStackable && i != 0 && i < compIngredients.ingredients.Count)
                        {
                            stringBuilder.Append(", ");
                        }
                    }
                }

            }
            if (nonStackable && stringBuilder.Length != 0)
            {
                if (stringBuilder.ToString().EndsWith(", "))
                    reason = stringBuilder.ToString().TrimEnd(", ".ToCharArray());
                else
                    reason = stringBuilder.ToString();
                return false;
            }
            reason = "";
            return true;
        }

        public static bool CanIngredientBeStacked(ThingDef ingredient, out string reason)
        {
            //Log.Message("CanIngredientBeStacked Entered");
            FactionIdeosTracker playerFactionTracker = null;
            if (Faction.OfPlayer != null)
                    playerFactionTracker = Faction.OfPlayer.ideos;
            //Log.Message("FactionIdeosTracker passed");
            if (ModLister.IdeologyInstalled && playerFactionTracker != null)
            {
                //if (DebugSettings.godMode)
                   // Log.Message("playerFactionTracker is" + (playerFactionTracker == null ? "Null" : "Valid"));
                Ideo ideo = playerFactionTracker.PrimaryIdeo;
                //if (DebugSettings.godMode)
                   // Log.Message("ideo is" + (ideo == null ? "Null" : "Valid"));
                bool allowMeat = !ideo.HasVegetarianRequiredPrecept();
                bool allowVeg = !ideo.HasMeatEatingRequiredPrecept();
                bool allowInsect = ideo.PreceptsListForReading.Where(p => p.def.defName == "InsectMeatEating_Loved").EnumerableNullOrEmpty() ? false : true;
                bool allowFungus = ideo.PreceptsListForReading.Where(p => p.def.defName == "FungusEating_Preferred").EnumerableNullOrEmpty() ? false : true;
                bool allowHumanoidMeat = ideo.HasHumanMeatEatingRequiredPrecept() || ideo.PreceptsListForReading.Where(p => p.def.defName == "Cannibalism_Acceptable").EnumerableNullOrEmpty() ? false : true;

                    if (ingredient.race != null)
                    {
                        if (ingredient.race.Insect && !allowInsect)
                        {
                            reason = "Insect Meat";
                            return false;
                        }
                        if (ingredient.race.Humanlike && !allowHumanoidMeat)
                        {
                            reason = "Humanlike Meat";
                            return false;
                        }
                        foreach (var animal in ideo.VeneratedAnimals)
                        {
                            if (animal.race == ingredient.race)
                            {
                                reason = "Venerated Animal"; ;
                                return false;
                            }
                        }
                    }
                    if (ingredient.IsMeat && !allowMeat)
                    {
                        reason = "Meat";
                        return false;
                    }
                    if (ingredient.ingestible != null && ingredient.ingestible.foodType == FoodTypeFlags.VegetableOrFruit && !allowVeg)
                    {
                        reason = "Vegetable or Fruit";
                        return false;
                    }

                    if (ingredient.IsFungus && !allowFungus)
                    {
                        reason = "Fungus";
                        return false;
                    }

            }
            reason = "";
            return true;
        }
    }
}
