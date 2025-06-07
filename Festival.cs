public class Festival : EventCard
{
	protected override void ExecuteEvent()
	{
		base.MyGameCard.StartTimer(5f, StopEvent, SokLoc.Translate(base.EventText), base.GetActionId("StopEvent"));
		WorldManager.instance.QueueCutscene("cities_festival");
		CardData cardData = WorldManager.instance.CreateCard(base.Position, "merch", faceUp: true, checkAddToStack: false);
		WorldManager.instance.CreateWellbeingPlus(base.Position);
		cardData.MyGameCard.SendIt();
		base.EventIsActive = true;
	}

	[TimedAction("stop_event")]
	public void StopEvent()
	{
		base.EndEvent();
	}

	protected override void EndEvent()
	{
		base.EndEvent();
	}

	public override void UpdateCardText()
	{
		if (base.MyGameCard != null && base.MyGameCard.TimerRunning && base.MyGameCard.TimerActionId == base.GetActionId("StopEvent"))
		{
			base.descriptionOverride = SokLoc.Translate(base.EventText);
		}
		base.UpdateCardText();
	}
}
