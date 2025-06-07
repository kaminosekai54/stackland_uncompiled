public class WildFire : EventCard
{
	protected override void ExecuteEvent()
	{
		base.EventIsActive = true;
		WorldManager.instance.QueueCutscene(CitiesCutscenes.CitiesWildFire(base.MyGameCard));
	}
}
