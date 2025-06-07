public class BreedingPen : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		switch (base.MyGameCard.GetChildCount())
		{
		case 0:
			if (otherCard is Animal animal)
			{
				return animal.IsBreedable;
			}
			return false;
		case 1:
			return base.MyGameCard.Child.CardData.Id == otherCard.Id;
		default:
			return false;
		}
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.GetChildCount() == 2)
		{
			base.MyGameCard.StartTimer(120f, BreedAnimals, SokLoc.Translate("action_breeding_status"), base.GetActionId("BreedAnimals"));
		}
		else if (base.MyGameCard.GetChildCount() > 2)
		{
			GameCard gameCard = base.MyGameCard.TryGetNthChild(3);
			if (gameCard != null)
			{
				gameCard.RemoveFromParent();
			}
			base.MyGameCard.CancelTimer(base.GetActionId("BreedAnimals"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("BreedAnimals"));
		}
		base.UpdateCard();
	}

	[TimedAction("breed_animals")]
	public void BreedAnimals()
	{
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, base.MyGameCard.Child.CardData.Id);
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
		GameCard child = base.MyGameCard.Child;
		if (child.Child != null)
		{
			GameCard child2 = child.Child;
			child2.RemoveFromStack();
			WorldManager.instance.StackSend(child2, base.OutputDir);
		}
		QuestManager.instance.SpecialActionComplete("breed_" + cardData.Id);
		child.RemoveFromStack();
		WorldManager.instance.StackSend(child, base.OutputDir);
	}
}
