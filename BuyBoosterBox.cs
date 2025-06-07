using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;

public class BuyBoosterBox : CardTarget
{
	public int Cost;

	public int StoredCostAmount;

	public string BoosterId;

	public Transform SpawnTarget;

	public TextMeshPro BuyText;

	public TextMeshPro NameText;

	public GameObject NewLabel;

	public Rectangle HighlightRectangle;

	public GameObject IdeaIcon;

	public BoardCurrency BoardCurrency;

	private int[] takeOrder = new int[4] { 10, 20, 50, 100 };

	public BoosterpackData Booster => WorldManager.instance.GetBoosterData(this.BoosterId);

	public int GetCost()
	{
		if (this.Booster.BoosterLocation == Location.Cities)
		{
			if (CitiesManager.instance.ActiveEvent == CardEventType.FinancialCrisis)
			{
				return Mathf.RoundToInt((float)this.Cost * 1.5f / 10f) * 10;
			}
			if (CitiesManager.instance.ActiveEvent == CardEventType.PackSale)
			{
				return Mathf.CeilToInt((float)this.Cost * 0.75f / 10f) * 10;
			}
		}
		return this.Cost;
	}

	public override bool CanHaveCard(GameCard card)
	{
		if (!base.MyBoard.IsCurrent)
		{
			return false;
		}
		if (!this.Booster.IsUnlocked)
		{
			return false;
		}
		if (WorldManager.instance.RemovingCards)
		{
			return false;
		}
		if (this.BoardCurrency == BoardCurrency.Gold)
		{
			if (WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "gold") > 0 || WorldManager.instance.GetAmountInChest(card, "gold") > 0)
			{
				return true;
			}
		}
		else if (this.BoardCurrency == BoardCurrency.Shell)
		{
			if (WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "shell") > 0 || WorldManager.instance.GetAmountInChest(card, "shell") > 0)
			{
				return true;
			}
		}
		else if (this.BoardCurrency == BoardCurrency.Dollar && (WorldManager.instance.GetCardCountInStack(card, (CardData x) => x is Dollar) > 0 || WorldManager.instance.GetDollarsInCreditcard(card) > 0))
		{
			return true;
		}
		return false;
	}

	public int GetCurrentCost()
	{
		return this.GetCost() - this.StoredCostAmount;
	}

	public override void CardDropped(GameCard card)
	{
		int currentCost = this.GetCurrentCost();
		if (this.BoardCurrency == BoardCurrency.Gold)
		{
			int cardCountInStack = WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "gold");
			int amountInChest = WorldManager.instance.GetAmountInChest(card, "gold");
			int num = 0;
			if (cardCountInStack > 0)
			{
				num = ((cardCountInStack > currentCost) ? currentCost : cardCountInStack);
				WorldManager.instance.RemoveCardsFromStackPred(card, num, (GameCard x) => x.CardData.Id == "gold");
				this.StoredCostAmount += num;
			}
			else if (amountInChest > 0)
			{
				num = ((amountInChest > currentCost) ? currentCost : amountInChest);
				WorldManager.instance.BuyWithChest(card, num);
				this.StoredCostAmount += num;
			}
		}
		else if (this.BoardCurrency == BoardCurrency.Shell)
		{
			int cardCountInStack2 = WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "shell");
			int amountInChest2 = WorldManager.instance.GetAmountInChest(card, "shell");
			int num2 = 0;
			if (cardCountInStack2 > 0)
			{
				num2 = ((cardCountInStack2 > currentCost) ? currentCost : cardCountInStack2);
				WorldManager.instance.RemoveCardsFromStackPred(card, num2, (GameCard x) => x.CardData.Id == "shell");
				this.StoredCostAmount += num2;
			}
			else if (amountInChest2 > 0)
			{
				num2 = ((amountInChest2 > currentCost) ? currentCost : amountInChest2);
				WorldManager.instance.BuyWithChest(card, num2);
				this.StoredCostAmount += num2;
			}
		}
		else
		{
			List<Dollar> list = (from x in card.GetAllCardsInStack()
				where x.CardData is Dollar
				select x.CardData as Dollar).ToList();
			int num3 = list.Sum((Dollar x) => x.DollarValue);
			int dollarsInCreditcard = WorldManager.instance.GetDollarsInCreditcard(card);
			int num4 = 0;
			if (num3 > 0)
			{
				num4 = Mathf.Min(currentCost, num3);
				int num5 = num4;
				for (int i = 0; i < this.takeOrder.Length; i++)
				{
					int curBillAmount = this.takeOrder[i];
					int b = num5 / curBillAmount;
					b = Mathf.Min(list.Count((Dollar x) => (object)x != null && x.DollarValue == curBillAmount), b);
					num5 -= b * curBillAmount;
					for (int j = 0; j < b; j++)
					{
						Dollar dollar = list.Where((Dollar x) => (object)x != null && x.DollarValue == curBillAmount).FirstOrDefault();
						list.Remove(dollar);
						dollar.MyGameCard.DestroyCard();
					}
					if (num5 <= 0)
					{
						break;
					}
				}
				if (num5 > 0 && list.Count > 0)
				{
					Dollar dollar2 = list.OrderBy((Dollar x) => x.DollarValue).FirstOrDefault();
					int value = dollar2.DollarValue - num5;
					list.Remove(dollar2);
					dollar2.MyGameCard.DestroyCard();
					num4 = currentCost;
					WorldManager.instance.CreateDollarsFromValue(value, base.transform.position);
				}
				WorldManager.instance.Restack(list.Select((Dollar x) => x.MyGameCard).ToList());
				this.StoredCostAmount += num4;
			}
			if (dollarsInCreditcard > 0)
			{
				num4 = ((dollarsInCreditcard > currentCost) ? currentCost : dollarsInCreditcard);
				WorldManager.instance.BuyWithCreditcard(card, num4);
				this.StoredCostAmount += num4;
			}
		}
		WorldManager.instance.CreateSmoke(base.transform.position);
		if (this.StoredCostAmount == this.GetCost())
		{
			this.CreateBoosterPack();
			this.StoredCostAmount = 0;
		}
		base.CardDropped(card);
	}

	private void CreateBoosterPack(GameCard card = null)
	{
		QuestManager.instance.SpecialActionComplete("buy_" + this.BoosterId + "_pack");
		WorldManager.instance.BoughtBoosterIds.Add(this.BoosterId);
		Boosterpack boosterpack = WorldManager.instance.CreateBoosterpack(base.transform.position, this.BoosterId);
		boosterpack.transform.position = (boosterpack.TargetPosition = this.SpawnTarget.position);
		if (card != null)
		{
			Vector3 vector = new Vector3(0.4f, 0f, 0f);
			boosterpack.transform.position = (boosterpack.TargetPosition = this.SpawnTarget.position + vector);
			card.transform.position = (card.TargetPosition = this.SpawnTarget.position - vector);
		}
		this.UpdateUndiscoveredCards();
	}

	protected override void Update()
	{
		if (!base.MyBoard.IsCurrent)
		{
			return;
		}
		if (this.Booster.IsUnlocked)
		{
			base.gameObject.name = this.Booster.Name;
			if (this.BoardCurrency == BoardCurrency.Gold)
			{
				this.BuyText.text = $"{this.GetCost() - this.StoredCostAmount}{Icons.Gold}";
			}
			else if (this.BoardCurrency == BoardCurrency.Shell)
			{
				this.BuyText.text = $"{this.GetCost() - this.StoredCostAmount}{Icons.Shell}";
			}
			else if (this.BoardCurrency == BoardCurrency.Dollar)
			{
				this.BuyText.text = $"{this.GetCost() - this.StoredCostAmount}{Icons.Dollar}";
			}
			this.NameText.text = this.Booster.Name;
			this.NewLabel.gameObject.SetActive(!WorldManager.instance.CurrentSave.FoundBoosterIds.Contains(this.BoosterId));
		}
		else
		{
			base.gameObject.name = "???";
			this.NameText.text = "???";
			this.BuyText.text = "";
			this.NewLabel.gameObject.SetActive(value: false);
		}
		if (WorldManager.instance.CurrentBoard != null)
		{
			this.HighlightRectangle.Color = WorldManager.instance.CurrentBoard.CardHighlightColor;
		}
		this.HighlightRectangle.enabled = WorldManager.instance.DraggingCard != null && this.CanHaveCard(WorldManager.instance.DraggingCard);
		this.HighlightRectangle.DashOffset += Time.deltaTime;
		if (this.HighlightRectangle.DashOffset >= 1f)
		{
			this.HighlightRectangle.DashOffset -= 1f;
		}
		base.Update();
	}

	public void UpdateUndiscoveredCards()
	{
		if (this.Booster.IsUnlocked && this.Booster.UndiscoveredCardCount >= 1 && WorldManager.instance.CurrentSave.FoundBoosterIds.Contains(this.BoosterId))
		{
			this.IdeaIcon.SetActive(value: true);
		}
		else
		{
			this.IdeaIcon.SetActive(value: false);
		}
	}

	public override string GetTooltipText()
	{
		if (this.Booster.IsUnlocked)
		{
			string value = Icons.Gold;
			if (this.BoardCurrency == BoardCurrency.Shell)
			{
				value = Icons.Shell;
			}
			else if (this.BoardCurrency == BoardCurrency.Dollar)
			{
				value = Icons.Dollar;
			}
			return SokLoc.Translate("label_drag_coins_to_buy_pack", LocParam.Create("goldicon", value), LocParam.Create("cost", this.GetCost().ToString())) + "\n\n" + this.Booster.GetSummary();
		}
		string termId = "label_complete_more_quests_for_pack";
		if (this.Booster.BoosterLocation == Location.Island)
		{
			termId = "label_complete_more_island_quests_for_pack";
		}
		return SokLoc.Translate(termId, LocParam.Plural("remaining", this.Booster.RemainingAchievementCountToUnlock));
	}
}
