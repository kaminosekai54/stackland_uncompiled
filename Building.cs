public class Building : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == base.Id)
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}
}
