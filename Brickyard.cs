public class Brickyard : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard.Id == "stone"))
		{
			return otherCard.Id == "sandstone";
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "stone" || c.Id == "sandstone") >= 2)
		{
			base.MyGameCard.StartTimer(10f, CompleteMaking, SokLoc.Translate("card_brickyard_status"), base.GetActionId("CompleteMaking"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("CompleteMaking"));
		}
		base.UpdateCard();
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	[TimedAction("complete_making")]
	public void CompleteMaking()
	{
		base.MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c.Id == "stone" || c.Id == "sandstone", 2);
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "brick", faceUp: false, checkAddToStack: false);
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir, base.MyGameCard);
	}
}
