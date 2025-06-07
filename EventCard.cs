using UnityEngine;

public class EventCard : CardData
{
	public bool IsPositiveEvent;

	public float PreEventTime;

	[Term]
	public string PreEventText;

	[Term]
	public string EventText;

	[HideInInspector]
	public bool ShouldStartEvent;

	public CardEventType EventType;

	[ExtraData("event_is_active")]
	public bool EventIsActive;

	public AudioClip EventStartOverride;

	public override void OnInitialCreate()
	{
		if (!base.MyGameCard.TimerRunning)
		{
			base.MyGameCard.StartTimer(this.PreEventTime, StartEvent, SokLoc.Translate(this.PreEventText), base.GetActionId("StartEvent"));
		}
		if (this.IsPositiveEvent)
		{
			AudioManager.me.PlaySound((this.EventStartOverride != null) ? this.EventStartOverride : AudioManager.me.PositiveEventSpawn, base.transform, Random.Range(0.9f, 1.1f), 0.5f);
		}
		else
		{
			AudioManager.me.PlaySound((this.EventStartOverride != null) ? this.EventStartOverride : AudioManager.me.NegativeEventSpawn, base.transform, Random.Range(0.9f, 1.1f), 0.5f);
		}
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		if (this.ShouldStartEvent && !base.MyGameCard.TimerRunning)
		{
			this.ExecuteEvent();
		}
		this.ShouldStartEvent = false;
		base.UpdateCard();
	}

	[TimedAction("start_disaster")]
	public void StartEvent()
	{
		this.ShouldStartEvent = true;
		QuestManager.instance.SpecialActionComplete("event_disaster", this);
	}

	protected virtual void ExecuteEvent()
	{
	}

	protected virtual void EndEvent()
	{
		base.MyGameCard.DestroyCard(spawnSmoke: true);
	}
}
