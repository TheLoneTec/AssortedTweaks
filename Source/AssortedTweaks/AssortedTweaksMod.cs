using Mlie;
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
      listingStandard.CheckboxLabeled("AT.DeliverAsMuchAsYouCan_Label".Translate(), ref Settings.DeliverAsMuchAsYouCan, "AT.DeliverAsMuchAsYouCan_Tooltip".Translate());
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
