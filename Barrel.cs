using System.Collections.Generic;

public class Barrel : CardData
{
	public List<string> CanBottleIds;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.CanBottleIds.Contains(otherCard.Id);
	}
}
