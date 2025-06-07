public class SlaughterHouse : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		int num = base.GetChildCount() + (1 + otherCard.GetChildCount());
		if (otherCard is Animal)
		{
			return num <= 5;
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && base.MyGameCard.Child.CardData is Animal)
		{
			base.MyGameCard.StartTimer(60f, SlaughterAnimal, SokLoc.Translate("action_slaughtering_status"), base.GetActionId("SlaughterAnimal"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("SlaughterAnimal"));
		}
		base.UpdateCard();
	}

	[TimedAction("slaughter_animal")]
	public void SlaughterAnimal()
	{
		if (base.MyGameCard.HasChild && base.MyGameCard.Child.CardData is Animal)
		{
			GameCard child = base.MyGameCard.Child;
			base.RemoveFirstChildFromStack();
			child.DestroyCard();
			CardData cardData = ((child.CardData.MyCardType == CardType.Fish) ? WorldManager.instance.CreateCard(base.transform.position, "raw_fish") : ((!(child.CardData.Id == "crab")) ? WorldManager.instance.CreateCard(base.transform.position, "raw_meat") : WorldManager.instance.CreateCard(base.transform.position, "raw_crab_meat")));
			WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
			WorldManager.instance.CreateSmoke(base.transform.position);
		}
	}
}
