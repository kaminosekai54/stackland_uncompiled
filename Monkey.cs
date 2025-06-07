public class Monkey : Animal
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == "banana")
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && base.MyGameCard.Child.CardData.Id == "banana")
		{
			base.MyGameCard.StartTimer(1f, TrainMonkey, SokLoc.Translate("idea_training_monkey_status"), base.GetActionId("TrainMonkey"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("TrainMonkey"));
		}
		base.UpdateCard();
	}

	[TimedAction("train_monkey")]
	public void TrainMonkey()
	{
		base.MyGameCard.Child.DestroyCard();
		WorldManager.instance.ChangeToCard(base.MyGameCard, "trained_monkey");
	}
}
