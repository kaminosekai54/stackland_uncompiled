public class Composter : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Food { FoodValue: <=0 })
		{
			return false;
		}
		return otherCard.MyCardType == CardType.Food;
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData x) => this.CanHaveCard(x)) >= 5)
		{
			base.MyGameCard.StartTimer(60f, Compost, SokLoc.Translate("idea_composting_status"), "compost");
		}
		else
		{
			base.MyGameCard.CancelTimer("compost");
		}
		base.UpdateCard();
	}

	[TimedAction("compost")]
	public void Compost()
	{
		base.MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.MyCardType == CardType.Food, 5);
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "soil", faceUp: false, checkAddToStack: false);
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
	}
}
