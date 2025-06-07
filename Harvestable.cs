using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Harvestable : CardData
{
	[Header("Harvestable")]
	[Term]
	public string StatusTerm = "";

	[ExtraData("amount")]
	public int Amount = 3;

	public bool IsUnlimited;

	public float HarvestTime = 10f;

	public CardBag MyCardBag;

	[Header("Multiple villager options")]
	public int RequiredVillagerCount = 1;

	private List<CardData> villagers = new List<CardData>();

	[Card]
	public List<string> CanHaveCardIds = new List<string>();

	public string StatusText => SokLoc.Translate(this.StatusTerm);

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is BaseVillager) && !(otherCard is Worker) && !(otherCard.Id == base.Id) && (base.MyCardType != CardType.Weather || otherCard.MyCardType != CardType.Weather))
		{
			return this.CanHaveCardIds.Contains(otherCard.Id);
		}
		return true;
	}

	public override void SetFoil()
	{
		base.SetFoil();
	}

	protected override void Awake()
	{
		base.Awake();
		SokLoc.instance.LanguageChanged += UpdateDescription;
		this.UpdateDescription();
	}

	private void OnDestroy()
	{
		SokLoc.instance.LanguageChanged -= UpdateDescription;
	}

	private void UpdateDescription()
	{
		if (base.MyCardType == CardType.Locations)
		{
			base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm, LocParam.Create("required_count", this.RequiredVillagerCount.ToString())) + "\n\n" + BoosterpackData.GetSummaryFromAllCards(this.MyCardBag.GetCardsInBag());
		}
	}

	public override void UpdateCard()
	{
		if (!(this is EnergyHarvestable))
		{
			base.GetChildrenMatchingPredicate((CardData x) => x is BaseVillager || x is Worker, this.villagers);
			bool flag = true;
			GameCard cardWithStatusInStack = base.MyGameCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && cardWithStatusInStack.TimerRunning && cardWithStatusInStack.TimerActionId == "finish_blueprint")
			{
				flag = false;
			}
			if (this.villagers.Count >= this.RequiredVillagerCount && (base.HasCardOnTop<BaseVillager>() || base.HasCardOnTop<Worker>()) && flag)
			{
				string actionId = base.GetActionId("CompleteHarvest");
				float num = 1f;
				List<CardData> list = this.villagers.FindAll((CardData x) => x is BaseVillager).ToList();
				if (list.Count > 0)
				{
					num = list.Max((CardData v) => ((BaseVillager)v).GetActionTimeModifier(actionId, this));
				}
				base.MyGameCard.StartTimer(num * this.HarvestTime, CompleteHarvest, this.StatusText, actionId);
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
			}
		}
		base.UpdateCard();
	}

	public virtual void SendCard(GameCard card)
	{
		WorldManager.instance.StackSend(card, base.OutputDir);
	}

	public virtual ICardId GetCardToGive()
	{
		ICardId result = this.MyCardBag.GetCard();
		if (base.Id == "catacombs" && this.Amount == 1)
		{
			result = (CardId)"goblet";
		}
		if (base.Id == "cave" && this.Amount == 1)
		{
			result = (CardId)"treasure_map";
		}
		if (base.Id == "ruins" && this.Amount == 1)
		{
			result = (CardId)"blueprint_fountain_of_youth";
		}
		if (base.Id == "old_tome")
		{
			List<CardData> list = WorldManager.instance.CardDataPrefabs.Where((CardData x) => x.MyCardType == CardType.Ideas && !WorldManager.instance.HasFoundCard(x.Id) && !x.HideFromCardopedia).ToList();
			list.RemoveAll((CardData x) => x.CardUpdateType == CardUpdateType.Spirit);
			if (!WorldManager.instance.CurrentRunVariables.VisitedIsland)
			{
				list.RemoveAll((CardData x) => x.CardUpdateType == CardUpdateType.Island);
			}
			result = ((list.Count <= 0) ? ((CardId)"map") : ((CardId)list.Choose().Id));
		}
		return result;
	}

	[TimedAction("complete_harvest")]
	public void CompleteHarvest()
	{
		if (!this.IsUnlimited)
		{
			this.Amount--;
		}
		GameCard gameCard = null;
		if (base.HasCardOnTop<BaseVillager>() || base.HasCardOnTop<Worker>())
		{
			gameCard = base.MyGameCard.Child;
			gameCard.RotWobble(0.5f);
		}
		ICardId cardToGive = this.GetCardToGive();
		if (cardToGive != null && !string.IsNullOrEmpty(cardToGive.Id))
		{
			CardData cardData = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, cardToGive, faceUp: false, checkAddToStack: false);
			WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
			if (cardData is Combatable combatable)
			{
				combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			}
			if (cardData is Creditcard creditcard)
			{
				creditcard.DollarCount = Random.Range(1, 3) * 10;
			}
		}
		if (!this.IsUnlimited && this.Amount <= 0)
		{
			if (gameCard != null && base.MyGameCard.HasParent && base.MyGameCard.Parent.CardData.Id == base.Id)
			{
				GameCard parent = base.MyGameCard.Parent;
				base.MyGameCard.RemoveFromStack();
				gameCard.SetParent(parent);
			}
			this.Emptied();
			base.MyGameCard.DestroyCard(spawnSmoke: true);
		}
		this.OnHarvestComplete();
		this.UpdateDescription();
	}

	public virtual void OnHarvestComplete()
	{
	}

	protected virtual void Emptied()
	{
	}
}
