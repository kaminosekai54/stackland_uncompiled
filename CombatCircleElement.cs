public class CombatCircleElement : Hoverable
{
	public GameCard ParentCard;

	public override string GetTitle()
	{
		if (this.ParentCard.CardData is Combatable combatable)
		{
			return combatable.GetCombatTypeTitle();
		}
		return "";
	}

	public override string GetDescription()
	{
		if (this.ParentCard.CardData is Combatable combatable)
		{
			return "<i>" + combatable.GetCombatTypeLore() + "</i>\n\n" + combatable.GetCombatTypeDescription();
		}
		return "";
	}
}
