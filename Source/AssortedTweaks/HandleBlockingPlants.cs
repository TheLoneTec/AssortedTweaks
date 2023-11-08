// Decompiled with JetBrains decompiler
// Type: AnotherTweaks.HandleBlockingPlants
// Assembly: AnotherTweaks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5472CAC4-0D69-4D05-9482-DABB8FC5A27B
// Assembly location: E:\AlternativeSteamVersions\RimWorldHSK_1.4\Mods\AnotherTweaks\Assemblies\AnotherTweaks.dll

using HarmonyLib;
using RimWorld;
using SK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AssortedTweaks
{
  public static class HandleBlockingPlants
  {
    public static IEnumerable<CodeInstruction> HandleBlockingThingJob(
      MethodBase __originalMethod,
      IEnumerable<CodeInstruction> instructions,
      ILGenerator ilGen)
    {
      List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
      int index1 = -1;
      for (int index2 = 0; index2 < list.Count; ++index2)
      {
        if (list[index2].opcode == OpCodes.Ldc_I4_4 && list[index2 + 1].opcode == OpCodes.Bne_Un_S)
          index1 = index2 + 2;
      }
      if (index1 == -1)
      {
        Log.Error("[HandleBlockingPlants] Can't find insertion place");
        return (IEnumerable<CodeInstruction>) list;
      }
      bool flag = __originalMethod.DeclaringType == typeof (RoofUtility);
      Label operand = ilGen.DefineLabel();
      list[index1].labels.Add(operand);
      list.InsertRange(index1, (IEnumerable<CodeInstruction>) new CodeInstruction[6]
      {
        new CodeInstruction(OpCodes.Ldarg_1),
        flag ? new CodeInstruction(OpCodes.Ldarg_0) : new CodeInstruction(OpCodes.Ldloc_0),
        new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (HandleBlockingPlants), "CanCutBlockingPlant")),
        new CodeInstruction(OpCodes.Brtrue_S, (object) operand),
        new CodeInstruction(OpCodes.Ldnull),
        new CodeInstruction(OpCodes.Ret)
      });
      return (IEnumerable<CodeInstruction>) list;
    }

    private static bool CanCutBlockingPlant(Pawn worker, Thing t)
    {
      if (!AssortedTweaksMod.instance.Settings.CutPlantsBeforeBuilding || worker.workSettings.WorkIsActive(WorkTypeDefOf.PlantCutting))
        return true;
      if (t is Plant plant)
      {
        DesignationManager designationManager = t.Map.designationManager;
        if (!designationManager.AllDesignationsOn((Thing) plant).Any<Designation>((Func<Designation, bool>) (x => x.def == DesignationDefOf.CutPlant || x.def == DesignationDefOf.HarvestPlant)))
        {
          designationManager.RemoveAllDesignationsOn((Thing) plant);
          if (plant.HarvestableNow)
            designationManager.AddDesignation(new Designation((LocalTargetInfo) (Thing) plant, DesignationDefOf.HarvestPlant));
          else
            designationManager.AddDesignation(new Designation((LocalTargetInfo) (Thing) plant, DesignationDefOf.CutPlant));
        }
        // Survival Tools check
        if (AssortedTweaksMod.ST_FellTree != null && (t as Plant).def.ingestible != null && (t as Plant).def.ingestible.foodType == FoodTypeFlags.Tree && worker.workSettings.GetPriority(AssortedTweaksMod.ST_FellTree) > 0)
            return true;
      }
      return false;
    }
  }
}
