public class PackSale : EventCard
{
	protected override void ExecuteEvent()
	{
		base.MyGameCard.StartTimer(WorldManager.instance.MonthTime / 2f, StopEvent, SokLoc.Translate("label_nice"), base.GetActionId("StopEvent"));
		WorldManager.instance.QueueCutscene("cities_pack_sale");
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
