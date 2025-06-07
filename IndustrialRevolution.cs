public class IndustrialRevolution : CardData
{
	public override void OnInitialCreate()
	{
		AudioManager.me.PlaySound(AudioManager.me.IndustrialRevolutionCreate, base.transform, 1f, 0.5f);
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && base.MyGameCard.Child.CardData is BaseVillager)
		{
			if (WorldManager.instance.GetCardCount<TimeMachine>() > 0)
			{
				base.MyGameCard.Child.RemoveFromParent();
			}
			if (!base.MyGameCard.TimerRunning)
			{
				base.MyGameCard.StartTimer(10f, ShowModal, SokLoc.Translate("label_go_to_cities"), base.GetActionId("ShowModal"));
			}
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("ShowModal"));
		}
		base.UpdateCard();
	}

	[TimedAction("show_modal")]
	public void ShowModal()
	{
		GameCanvas.instance.GoToCityPrompt(GoToBoard, null);
		base.MyGameCard.RemoveFromStack();
	}

	public void GoToBoard()
	{
		WorldManager.instance.GoToBoard(WorldManager.instance.GetBoardWithId("cities"), delegate
		{
		}, "cities");
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is BaseVillager;
	}
}
