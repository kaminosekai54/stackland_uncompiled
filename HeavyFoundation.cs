public class HeavyFoundation : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			if (!base.MyGameCard.HasChild)
			{
				return base.DetermineCanHaveCardsWhenIsRoot;
			}
			return base.MyGameCard.Child.CardData.DetermineCanHaveCardsWhenIsRoot;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		if (base.MyGameCard.HasChild)
		{
			return base.MyGameCard.Child.CardData.CanHaveCardsWhileHasStatus();
		}
		return base.CanHaveCardsWhileHasStatus();
	}

	public override bool CanBePushedBy(CardData otherCard)
	{
		return otherCard.Id == base.Id;
	}
}
