public class RadarStation : CardData
{
	public override void UpdateCard()
	{
		int currentMonth = WorldManager.instance.CurrentMonth;
		int nextConflictMonth = CitiesManager.instance.NextConflictMonth;
		if (currentMonth >= nextConflictMonth - 3 && currentMonth < nextConflictMonth)
		{
			base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm) + ". " + SokLoc.Translate("statuseffect_radar_description", LocParam.Create("amount", (CitiesManager.instance.NextConflictMonth - 1).ToString()));
			base.AddStatusEffect(new StatusEffect_Radar());
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_Radar>();
		}
		base.UpdateCard();
	}
}
