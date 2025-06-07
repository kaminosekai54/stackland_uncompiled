using UnityEngine;

public class SadEvent : CardData
{
	public float EventTime = 60f;

	public override void UpdateCard()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation && !base.MyGameCard.TimerRunning)
		{
			base.MyGameCard.StartTimer(this.EventTime, StartSadEvent, SokLoc.Translate("new_portal_shaking"), base.GetActionId("StartSadEvent"));
		}
		base.UpdateCard();
	}

	[TimedAction("start_sad_event")]
	public void StartSadEvent()
	{
		int cardCount = WorldManager.instance.GetCardCount<BaseVillager>();
		WorldManager.instance.TryCreateUnhappiness(base.Position, Mathf.FloorToInt(cardCount / 2));
		base.MyGameCard.DestroyCard(spawnSmoke: true);
	}
}
