public class ExportCenter : Factory
{
	public float ExportTime;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (WorldManager.instance.CardCanBeSold(otherCard.MyGameCard))
		{
			return otherCard.AllChildrenMatchPredicate((CardData x) => WorldManager.instance.CardCanBeSold(x.MyGameCard));
		}
		return false;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && WorldManager.instance.CardCanBeSold(base.MyGameCard.GetLeafCard()) && !base.MyGameCard.TimerRunning)
		{
			base.MyGameCard.StartTimer(this.ExportTime, SellCard, SokLoc.Translate("card_export_center_status_1"), base.GetActionId("SellCard"));
		}
		else if (!base.MyGameCard.HasChild)
		{
			base.MyGameCard.CancelTimer(base.GetActionId("SellCard"));
		}
		base.UpdateCard();
	}

	[TimedAction("sell_card")]
	public void SellCard()
	{
		GameCard leafCard = base.MyGameCard.GetLeafCard();
		leafCard.RemoveFromStack();
		GameCard gameCard = WorldManager.instance.SellCard(base.Position, leafCard, 1f, checkAddToStack: false);
		gameCard.RemoveFromParent();
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, gameCard, base.OutputDir);
	}
}
