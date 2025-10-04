using Verse;

namespace AssortedTweaks
{
  internal class AssortedTweaksSettings : ModSettings
  {
    public float DefaultRadius = 30f;
    public bool MeatIngredients = true;
    public bool RaceBodyPartsMatter = false;
    public bool DeliverAsMuchAsYouCan = true;
    public bool CoreSK_ShowTPSInRegularGame;
    public bool CoreSK_ShowRaidPoints = true;
    public int CoreSK_Max4Speed = 900;
    public int CoreSK_MaxRaidCount = 130;
    public bool OverrideRadePoints;
    public int OverrideRadePointsValue = 10000;
    public bool CutPlantsBeforeBuilding = true;
    public bool ShowDebugMessages = false;
    public string _bufferMaxRaidCount;
    public string _bufferMax4Speed;
    public string _bufferOverrideRadePointsValue;
    public override void ExposeData()
    {
      base.ExposeData();
        Scribe_Values.Look<float>(ref this.DefaultRadius, "DefaultRadius", 30f);
        Scribe_Values.Look<bool>(ref this.MeatIngredients, "MeatIngredients", true);
        Scribe_Values.Look<bool>(ref this.RaceBodyPartsMatter, "RaceBodyPartsMatter", false);
        Scribe_Values.Look<bool>(ref this.DeliverAsMuchAsYouCan, "DeliverAsMuchAsYouCan", true);
        Scribe_Values.Look<bool>(ref this.CoreSK_ShowTPSInRegularGame, "CoreSK_ShowTPSInRegularGame");
        Scribe_Values.Look<bool>(ref this.CoreSK_ShowRaidPoints, "CoreSK_ShowRaidPoints", true);
        Scribe_Values.Look<int>(ref this.CoreSK_MaxRaidCount, "CoreSK_MaxRaidCount");
        Scribe_Values.Look<int>(ref this.CoreSK_Max4Speed, "CoreSK_Max4Speed", 900);
        Scribe_Values.Look<bool>(ref this.CutPlantsBeforeBuilding, "CutPlantsBeforeBuilding", true);
        Scribe_Values.Look<bool>(ref this.ShowDebugMessages, "CutPlantsBeforeBuilding", false);
    }
  }
}
