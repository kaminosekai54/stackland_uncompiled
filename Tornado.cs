public class Tornado : EventCard
{
	protected override void ExecuteEvent()
	{
		WorldManager.instance.QueueCutscene(CitiesCutscenes.CitiesTornado());
		base.EventIsActive = true;
		base.ExecuteEvent();
	}
}
