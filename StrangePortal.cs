using System.Collections.Generic;
using UnityEngine;

public class StrangePortal : Portal
{
	public float SpawnTime = 10f;

	public float TravelTime = 5f;

	public bool IsRarePortal;

	private float SpawnTimer;

	private float TravelTimer;

	[ExtraData("spawns_remaining")]
	public int SpawnsRemaining = 3;

	public override bool CanBeDragged => false;

	public override void UpdateCard()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
			if (!base.MyGameCard.TimerRunning && num == 0)
			{
				base.MyGameCard.StartTimer(this.SpawnTime, SpawnCreature, SokLoc.Translate("new_portal_shaking"), base.GetActionId("SpawnCreature"));
				if (this.SpawnTimer > 0f)
				{
					base.MyGameCard.CurrentTimerTime = this.SpawnTimer;
				}
			}
			if (num > 0)
			{
				if (!WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToForest)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					base.Stay();
					return;
				}
				base.RemoveNonHuman();
				int cardCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
				if (base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager) > base.MaxVillagerCount)
				{
					if (base.MyGameCard.TimerRunning && base.MyGameCard.TimerActionId == base.GetActionId("TakePortal"))
					{
						this.TravelTimer = base.MyGameCard.CurrentTimerTime;
					}
					base.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.MaxVillagerCountPrompt("label_taking_portal_title", base.MaxVillagerCount);
					base.RemoveExcessVillagersInPortal();
				}
				if (num == cardCount)
				{
					if (base.MyGameCard.TimerRunning && base.MyGameCard.TimerActionId == base.GetActionId("TakePortal"))
					{
						this.TravelTimer = base.MyGameCard.CurrentTimerTime;
					}
					base.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_taking_portal_title");
					base.RemoveLastVillagerInPortal();
				}
				else if (base.MyGameCard.TimerRunning && base.MyGameCard.TimerActionId != base.GetActionId("TakePortal"))
				{
					this.SpawnTimer = base.MyGameCard.CurrentTimerTime;
					base.MyGameCard.CancelTimer(base.GetActionId("SpawnCreature"));
					base.MyGameCard.StartTimer(this.TravelTime, base.TakePortal, SokLoc.Translate("card_stable_portal_status"), base.GetActionId("TakePortal"));
					if (this.TravelTimer > 0f)
					{
						base.MyGameCard.CurrentTimerTime = this.TravelTimer;
					}
				}
				else if (!base.MyGameCard.TimerRunning)
				{
					base.MyGameCard.StartTimer(this.TravelTime, base.TakePortal, SokLoc.Translate("card_stable_portal_status"), base.GetActionId("TakePortal"));
					if (this.TravelTimer > 0f)
					{
						base.MyGameCard.CurrentTimerTime = this.TravelTimer;
					}
				}
			}
			else
			{
				if (base.MyGameCard.TimerRunning && base.MyGameCard.TimerActionId == base.GetActionId("TakePortal"))
				{
					this.TravelTimer = base.MyGameCard.CurrentTimerTime;
				}
				base.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
			}
		}
		base.UpdateCard();
	}

	[TimedAction("spawn_creature")]
	public void SpawnCreature()
	{
		List<EnemySetCardBag> list = new List<EnemySetCardBag>();
		if (WorldManager.instance.CurrentMonth >= 24)
		{
			list.Add(EnemySetCardBag.BasicEnemy);
			list.Add(EnemySetCardBag.AdvancedEnemy);
			list.Add(EnemySetCardBag.Forest_BasicEnemy);
		}
		else if (WorldManager.instance.CurrentMonth >= 16)
		{
			list.Add(EnemySetCardBag.BasicEnemy);
			list.Add(EnemySetCardBag.AdvancedEnemy);
		}
		else
		{
			list.Add(EnemySetCardBag.BasicEnemy);
		}
		int value = Mathf.RoundToInt((float)Mathf.Max(12, WorldManager.instance.CurrentMonth) * 1.5f);
		value = Mathf.Clamp(value, 0, 70);
		if (this.IsRarePortal)
		{
			value = Mathf.RoundToInt((float)value * 1.5f);
		}
		foreach (CardIdWithEquipment item in SpawnHelper.GetEnemiesToSpawn(WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBagList(list), value))
		{
			Combatable obj = WorldManager.instance.CreateCard(base.transform.position, item, faceUp: false, checkAddToStack: false) as Combatable;
			obj.HealthPoints = obj.ProcessedCombatStats.MaxHealth;
			obj.MyGameCard.SendIt();
		}
		base.MyGameCard.DestroyCard(spawnSmoke: true);
	}
}
