using System.Collections.Generic;

public class EnergyGenerator : CardData
{
	public List<string> AcceptedCards;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.AcceptedCards.Contains(otherCard.Id);
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}
}
