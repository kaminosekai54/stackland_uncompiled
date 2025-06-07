public class TravellingCart : CardData
{
	public CardBag MyCardBag;

	public int GoldToUse = 3;

	[ExtraData("items_bought")]
	public int ItemsBought;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyGameCard == null)
		{
			return otherCard.Id == "gold";
		}
		if (WorldManager.instance.BoughtWithGold(otherCard.MyGameCard, this.GoldToUse, checkStackAllSame: true) || WorldManager.instance.BoughtWithGoldChest(otherCard.MyGameCard, this.GoldToUse))
		{
			return true;
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild)
		{
			GameCard child = base.MyGameCard.Child;
			if (WorldManager.instance.BoughtWithGold(child, this.GoldToUse))
			{
				WorldManager.instance.RemoveCardsFromStackPred(child, this.GoldToUse, (GameCard x) => x.CardData.Id == "gold");
				this.Buy();
			}
			else if (WorldManager.instance.BoughtWithGoldChest(child, this.GoldToUse))
			{
				WorldManager.instance.BuyWithChest(child, this.GoldToUse);
				this.Buy();
			}
		}
		base.UpdateCard();
	}

	private void Buy()
	{
		ICardId cardId = this.MyCardBag.GetCard(removeCard: false);
		if (this.ItemsBought == 5 && WorldManager.instance.GetCardCount("goblet") == 0)
		{
			cardId = (CardId)"goblet";
		}
		QuestManager.instance.SpecialActionComplete("travelling_cart_buy", this);
		WorldManager.instance.CreateCard(base.transform.position, cardId, faceUp: true, checkAddToStack: false).MyGameCard.SendIt();
		this.ItemsBought++;
	}
}
