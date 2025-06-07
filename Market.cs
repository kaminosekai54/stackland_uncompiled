public class Market : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyGameCard == null)
		{
			return otherCard.Value > 0;
		}
		return WorldManager.instance.CardCanBeSold(otherCard.MyGameCard);
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && WorldManager.instance.CardCanBeSold(base.MyGameCard.Child, checkStatus: false) && (!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation))
		{
			string status = SokLoc.Translate("new_selling_card", LocParam.Create("card", base.MyGameCard.Child.CardData.FullName));
			base.MyGameCard.StartTimer(60f, SellWithMarket, status, base.GetActionId("SellWithMarket"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("SellWithMarket"));
		}
		base.UpdateCard();
	}

	[TimedAction("sell_with_market")]
	public void SellWithMarket()
	{
		GameCard child = base.MyGameCard.Child;
		if (!(child == null))
		{
			GameCard gameCard = null;
			if (child.HasChild && WorldManager.instance.CardCanBeSold(child.Child))
			{
				gameCard = child.Child;
			}
			child.RemoveFromStack();
			if (gameCard != null)
			{
				base.MyGameCard.SetChild(gameCard);
			}
			QuestManager.instance.SpecialActionComplete("sell_at_market", this);
			GameCard gameCard2 = WorldManager.instance.SellCard(base.transform.position, child, 2f, checkAddToStack: false);
			if (gameCard2 != null)
			{
				WorldManager.instance.StackSendCheckTarget(base.MyGameCard, gameCard2.GetRootCard(), base.OutputDir, base.MyGameCard);
			}
		}
	}
}
