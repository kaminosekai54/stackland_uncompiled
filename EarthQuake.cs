public class EarthQuake : EventCard
{
	protected override void ExecuteEvent()
	{
		base.EventIsActive = true;
		WorldManager.instance.QueueCutscene(CitiesCutscenes.CitiesEarthQuake(base.MyGameCard));
	}
}
