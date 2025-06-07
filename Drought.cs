public class Drought : EventCard
{
	protected override void ExecuteEvent()
	{
		base.EventIsActive = true;
		WorldManager.instance.QueueCutscene(CitiesCutscenes.CitiesDrought(base.MyGameCard));
	}
}
