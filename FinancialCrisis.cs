public class FinancialCrisis : EventCard
{
	protected override void ExecuteEvent()
	{
		base.MyGameCard.StartTimer(WorldManager.instance.MonthTime, StopEvent, SokLoc.Translate("label_uh_oh"), base.GetActionId("StopEvent"));
		base.EventIsActive = true;
		WorldManager.instance.QueueCutscene(CitiesCutscenes.CitiesFinancialCrisis());
	}

	[TimedAction("stop_event")]
	public void StopEvent()
	{
		base.EndEvent();
	}

	protected override void EndEvent()
	{
		WorldManager.instance.QueueCutscene(CitiesCutscenes.CitiesStopDisaster());
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
