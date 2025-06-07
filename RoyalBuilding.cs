public class RoyalBuilding : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == base.Id)
		{
			return true;
		}
		if (otherCard is Royal)
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}
}
