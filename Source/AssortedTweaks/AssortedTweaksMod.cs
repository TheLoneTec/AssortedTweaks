using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace AssortedTweaks
{
  [StaticConstructorOnStartup]
  internal class AssortedTweaksMod : Mod
  {
    public static AssortedTweaksMod instance;
    private static string currentVersion;
    private AssortedTweaksSettings settings;

    public AssortedTweaksMod(ModContentPack content)
    : base(content)
    {
      AssortedTweaksMod.instance = this;
      AssortedTweaksMod.currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);

    }

    internal AssortedTweaksSettings Settings
    {
      get
      {
        if (this.settings == null)
          this.settings = this.GetSettings<AssortedTweaksSettings>();
        return this.settings;
      }
      set => this.settings = value;
    }

    public override string SettingsCategory() => "AssortedTweaks";

    public override void DoSettingsWindowContents(Rect rect)
    {
      Listing_Standard listingStandard = new Listing_Standard();
      listingStandard.Begin(rect);
      listingStandard.Gap();
      listingStandard.Label((double) this.Settings.DefaultRadius >= 100.0 ? "AT.DefaultRadiusUnlimited".Translate() : "AT.DefaultRadius".Translate((NamedArgument) this.Settings.DefaultRadius.ToString("F0")), tooltip: ((string)"AT.DefaultRadiusTT".Translate()));
      this.Settings.DefaultRadius = listingStandard.Slider((double) this.Settings.DefaultRadius > 100.0 ? 100f : this.Settings.DefaultRadius, 3f, 100f);
      if ((double) this.Settings.DefaultRadius >= 100.0)
        this.Settings.DefaultRadius = 999f;
      //listingStandard.Label((double)this.Settings.DefaultRadius >= 100.0 ? "AT.DefaultRadiusUnlimited".Translate() : "AT.DefaultRadius".Translate((NamedArgument) this.Settings.DefaultRadius.ToString("F0")), tooltip: ((string)"AT.DefaultRadiusTT".Translate()));
      listingStandard.CheckboxLabeled("AT.MeatIngredients_Label".Translate(), ref Settings.MeatIngredients, "AT.MeatIngredients_Tooltip".Translate());
      listingStandard.CheckboxLabeled("AT.RaceBodyPartsMatter_Label".Translate(), ref Settings.RaceBodyPartsMatter, "AT.RaceBodyPartsMatter_Tooltip".Translate());
      listingStandard.CheckboxLabeled("AT.DeliverAsMuchAsYouCan_Label".Translate(), ref Settings.DeliverAsMuchAsYouCan, "AT.DeliverAsMuchAsYouCan_Tooltip".Translate());
      listingStandard.CheckboxLabeled((string)Translator.Translate("CutPlantsBeforeBuilding"), ref Settings.CutPlantsBeforeBuilding);
      listingStandard.CheckboxLabeled((string)Translator.Translate("CoreSK_ShowTPSInRegularGame"), ref Settings.CoreSK_ShowTPSInRegularGame);
      listingStandard.CheckboxLabeled((string)Translator.Translate("CoreSK_ShowRaidPoints"), ref Settings.CoreSK_ShowRaidPoints);
      int coreSkMaxRaidCount = Settings.CoreSK_MaxRaidCount;
      listingStandard.TextFieldNumericLabeled<int>((string)Translator.Translate("CoreSK_MaxRaidCount"), ref Settings.CoreSK_MaxRaidCount, ref Settings._bufferMaxRaidCount);
      if (!ModActive.ReplaceStuff)
          listingStandard.CheckboxLabeled((string)Translator.Translate("ReplaceStuff_CornerBuildable"), ref Settings.ReplaceStuff_CornerBuildable);
      if (Settings.CoreSK_MaxRaidCount != coreSkMaxRaidCount && Settings.CoreSK_MaxRaidCount > 10)
          CoreSK_Utils.Set_Config_MaxRaidCount(Settings.CoreSK_MaxRaidCount);
      listingStandard.TextFieldNumericLabeled<int>((string)Translator.Translate("CoreSK_Max4Speed"), ref Settings.CoreSK_Max4Speed, ref Settings._bufferMax4Speed);
      if (Prefs.DevMode)
      {
          listingStandard.CheckboxLabeled("DEBUG: Override raid points", ref Settings.OverrideRadePoints);
          listingStandard.TextFieldNumericLabeled<int>("DEBUG: Override raid points value", ref Settings.OverrideRadePointsValue, ref Settings._bufferOverrideRadePointsValue);
          listingStandard.GapLine();
      }
      GUI.color = Color.cyan;
      listingStandard.Label(Translator.Translate("AT_Credits"));
      GUI.color = Color.white;
      if (AssortedTweaksMod.currentVersion != null)
      {
        listingStandard.Gap();
        GUI.contentColor = Color.gray;
        listingStandard.Label("AT.ModVersion".Translate((NamedArgument) AssortedTweaksMod.currentVersion));
        GUI.contentColor = Color.white;
      }
      listingStandard.End();
    }
  }
}
