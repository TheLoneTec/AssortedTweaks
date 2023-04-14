using Verse;

namespace AssortedTweaks
{
  internal class AssortedTweaksSettings : ModSettings
  {
    public float DefaultRadius = 30f;
    public bool MeatIngredients = true;
    public bool DeliverAsMuchAsYouCan = true;

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look<float>(ref this.DefaultRadius, "DefaultRadius", 30f);
      Scribe_Values.Look<bool>(ref this.MeatIngredients, "MeatIngredients", true);
      Scribe_Values.Look<bool>(ref this.DeliverAsMuchAsYouCan, "DeliverAsMuchAsYouCan", true);
    }
  }
}
