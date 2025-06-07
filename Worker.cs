using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Worker : CardData, HousingConsumer
{
	public int HousingSpaceRequired = 1;

	public WorkerType WorkerType;

	[HideInInspector]
	[ExtraData("housingUniqueId")]
	public string HousingUniqueId;

	[HideInInspector]
	public Apartment Housing
	{
		get
		{
			if (this.HousingUniqueId != null)
			{
				GameCard cardWithUniqueId = WorldManager.instance.GetCardWithUniqueId(this.HousingUniqueId);
				if (cardWithUniqueId != null)
				{
					return cardWithUniqueId.CardData as Apartment;
				}
			}
			return null;
		}
		set
		{
			this.HousingUniqueId = ((value != null) ? value.UniqueId : "");
		}
	}

	public string HousingId => this.HousingUniqueId;

	public override void OnInitialCreate()
	{
		this.Housing = null;
		QuestManager.instance.SpecialActionComplete("worker_created", this);
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		if ((object)currentBoard == null || currentBoard.Location != Location.Cities)
		{
			base.RemoveStatusEffect<StatusEffect_Homeless>();
			base.UpdateCard();
			return;
		}
		Apartment housing = this.Housing;
		bool flag = housing != null && !housing.IsDamaged && housing.HasEnergyInput();
		if (this.GetHousingSpaceRequired() == 0)
		{
			flag = true;
		}
		if (!flag && !base.HasStatusEffectOfType<StatusEffect_Homeless>())
		{
			base.AddStatusEffect(new StatusEffect_Homeless());
		}
		if (flag && base.HasStatusEffectOfType<StatusEffect_Homeless>())
		{
			base.RemoveStatusEffect<StatusEffect_Homeless>();
		}
		if (CitiesManager.instance.WorkersOnBoard.Count <= 1)
		{
			base.CitiesValue = -1;
		}
		else if (this.WorkerType == WorkerType.Educated)
		{
			base.CitiesValue = 30;
		}
		else if (this.WorkerType == WorkerType.Normal)
		{
			base.CitiesValue = 20;
		}
		else if (this.WorkerType == WorkerType.Robot)
		{
			base.CitiesValue = 40;
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(base.CustomName))
		{
			base.nameOverride = base.CustomName;
		}
	}

	public float GetActionTimeModifier()
	{
		if (base.Id == "educated_worker")
		{
			return 0.75f;
		}
		if (base.Id == "genius" || base.Id == "robot_genius")
		{
			return 0.5f;
		}
		return 1f;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Worker;
	}

	public override void StoppedDragging()
	{
		List<CardData> list = base.CardsInStackMatchingPredicate((CardData x) => x is Worker);
		List<GameCard> list2 = (from x in base.MyGameCard.GetAllCardsInStack()
			where x.CardData.WorkerAmount > 0
			select x).ToList();
		if (list2.Count > 0)
		{
			foreach (GameCard item in list2)
			{
				foreach (Worker item2 in list)
				{
					if (item2 != null)
					{
						item2.TryEquipOnCard(item);
					}
				}
			}
		}
		else
		{
			foreach (Worker item3 in list)
			{
				item3.TryUnequipSelf();
			}
		}
		base.StoppedDragging();
	}

	private void TryUnequipSelf()
	{
		base.MyGameCard.WorkerHolder?.UnequipWorker(base.MyGameCard);
	}

	private void TryEquipOnCard(GameCard card)
	{
		if ((object)card == null || !(card.CardData?.WorkerAmount <= 0))
		{
			if (base.MyGameCard.WorkerHolder != null)
			{
				base.MyGameCard.WorkerHolder.UnequipWorker(base.MyGameCard);
			}
			if (card != null && card.CardData.WorkerAmount > 0)
			{
				card.OpenInventory(showInventory: true);
				card.CardData.EquipWorker(this);
			}
		}
	}

	public GameCard GetGameCard()
	{
		return base.MyGameCard;
	}

	public int GetHousingSpaceRequired()
	{
		return this.HousingSpaceRequired;
	}

	public WorkerType GetWorkerType()
	{
		return this.WorkerType;
	}

	public override void OnSellCard()
	{
		if (this.Housing != null)
		{
			this.Housing.UsedSpace -= this.GetHousingSpaceRequired();
			this.Housing = null;
		}
		QuestManager.instance.SpecialActionComplete("worker_removed", this);
		base.OnSellCard();
	}

	public override void OnDestroyCard()
	{
		if (this.Housing != null)
		{
			this.Housing.UsedSpace -= this.GetHousingSpaceRequired();
			this.Housing = null;
		}
		QuestManager.instance.SpecialActionComplete("worker_removed", this);
		base.OnDestroyCard();
	}
}
