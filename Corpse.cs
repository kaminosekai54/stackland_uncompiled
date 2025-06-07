public class Corpse : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Corpse;
	}

	public override void UpdateCardText()
	{
		string text = SokLoc.Translate(base.NameTerm);
		if (!string.IsNullOrEmpty(base.CustomName))
		{
			text = SokLoc.Translate("card_corpse_name_long", LocParam.Create("name", base.CustomName));
		}
		base.nameOverride = text;
	}
}
