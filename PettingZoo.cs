public class PettingZoo : CardData
{
	public float GenerationTime;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == "soil")
		{
			return true;
		}
		int num = base.GetChildCount() + (1 + otherCard.GetChildCount());
		if (otherCard is Animal && otherCard.MyCardType != CardType.Fish)
		{
			return num <= 5;
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData x) => x is Animal) > 0)
		{
			base.MyGameCard.StartTimer(this.GenerationTime, CompletePetting, SokLoc.Translate("card_petting_zoo_status_active"), "complete_petting");
		}
		else
		{
			base.MyGameCard.CancelTimer("complete_petting");
		}
		base.UpdateCard();
	}

	[TimedAction("complete_petting")]
	public void CompletePetting()
	{
		int amount = base.ChildrenMatchingPredicateCount((CardData x) => x is Animal);
		WorldManager.instance.TryCreateHappiness(base.MyGameCard.transform.position, amount);
	}
}
