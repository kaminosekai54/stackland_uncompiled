using System.Collections.Generic;
using UnityEngine;

public class PirateBoat : CardData
{
	public float SpawnTime = 20f;

	[ExtraData("spawns_remaining")]
	public int SpawnsRemaining = 3;

	public int Demand = 20;

	public override bool CanBeDragged => false;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		if (!base.MyGameCard.IsDemoCard)
		{
			this.Demand = Mathf.Min(100, 3 + WorldManager.instance.CurrentRunVariables.PirateBoatsBribed * 3);
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyGameCard == null)
		{
			return otherCard.Id == "gold";
		}
		if (WorldManager.instance.BoughtWithGoldChest(otherCard.MyGameCard, this.Demand) || WorldManager.instance.BoughtWithGold(otherCard.MyGameCard, this.Demand))
		{
			return true;
		}
		return false;
	}

	public void Buy()
	{
		base.MyGameCard.DestroyCard(spawnSmoke: true, playSound: false);
		QuestManager.instance.SpecialActionComplete("bribe_pirate_boat");
		WorldManager.instance.CurrentRunVariables.PirateBoatsBribed++;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild)
		{
			GameCard child = base.MyGameCard.Child;
			if (WorldManager.instance.BoughtWithGold(child, this.Demand))
			{
				WorldManager.instance.RemoveCardsFromStackPred(child, this.Demand, (GameCard x) => x.CardData.Id == "gold");
				this.Buy();
			}
			else if (WorldManager.instance.BoughtWithGoldChest(child, this.Demand))
			{
				WorldManager.instance.BuyWithChest(child, this.Demand);
				this.Buy();
			}
		}
		if (!base.MyGameCard.TimerRunning)
		{
			base.MyGameCard.StartTimer(this.SpawnTime, SpawnPirates, SokLoc.Translate("card_pirate_boat_name"), base.GetActionId("SpawnPirates"));
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.descriptionOverride = SokLoc.Translate("card_pirate_boat_status", LocParam.Create("count", this.Demand.ToString()));
	}

	[TimedAction("spawn_pirates")]
	public void SpawnPirates()
	{
		float maxStrength = (float)(1 + WorldManager.instance.CurrentRunVariables.PirateBoatsBribed * (2 + Mathf.Min(2, WorldManager.instance.CurrentRunVariables.PirateBoatsSpawned - 1))) * 30f;
		Combatable item = WorldManager.instance.GetCardPrefab("pirate") as Combatable;
		foreach (CardIdWithEquipment item2 in SpawnHelper.GetEnemiesToSpawn(new List<Combatable> { item }, maxStrength))
		{
			WorldManager.instance.CreateCard(base.transform.position, item2, faceUp: false, checkAddToStack: false).MyGameCard.SendIt();
		}
		base.MyGameCard.DestroyCard(spawnSmoke: true);
	}
}
