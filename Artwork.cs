public class Artwork : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return base.Id == otherCard.Id;
	}
}
