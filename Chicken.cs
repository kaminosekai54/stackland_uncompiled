public class Chicken : Animal
{
	public override bool CanCreate
	{
		get
		{
			if (this.IsBrooding)
			{
				return false;
			}
			return base.CanCreate;
		}
	}

	protected bool IsBrooding
	{
		get
		{
			GameCard cardWithStatusInStack = base.MyGameCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && cardWithStatusInStack.TimerBlueprintId == "blueprint_chicken")
			{
				return true;
			}
			return false;
		}
	}

	public override bool CanMove
	{
		get
		{
			if (this.IsBrooding)
			{
				return false;
			}
			return base.CanMove;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == "egg" && !otherCard.MyGameCard.HasChild)
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}
}
