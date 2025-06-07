using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CardBag
{
	public CardBagType CardBagType;

	public int CardsInPack = 3;

	public float ExpectedValue;

	public List<CardChance> Chances;

	[Card]
	public List<string> SetPackCards;

	public SetCardBagType SetCardBag;

	public bool UseFallbackBag;

	public EnemySetCardBag EnemyCardBag;

	public float StrengthLevel;

	public SetCardBagType FallbackBag;

	public string EnemiesIncluded;

	private void RecalculateEnemiesIncluded()
	{
		List<string> cardsInBag = this.GetCardsInBag(new GameDataLoader());
		this.EnemiesIncluded = string.Join(", ", cardsInBag);
	}

	public void RecalculateOdds()
	{
		if (this.Chances == null || this.CardBagType != 0)
		{
			return;
		}
		float num = 0f;
		foreach (CardChance chance in this.Chances)
		{
			num += (float)chance.Chance;
		}
		foreach (CardChance chance2 in this.Chances)
		{
			chance2.PercentageChance = (chance2.PercentageChance = (float)chance2.Chance / num);
		}
	}

	private float CalculatedExpectedValue(GameDataLoader loader, List<CardChance> chances)
	{
		float num = 0f;
		foreach (CardChance chance in chances)
		{
			num += (float)chance.Chance;
		}
		foreach (CardChance chance2 in chances)
		{
			chance2.PercentageChance = (float)chance2.Chance / num;
		}
		float num2 = 0f;
		foreach (CardChance chance3 in chances)
		{
			CardData cardFromId = loader.GetCardFromId(chance3.Id);
			float num3 = (chance3.IsEnemy ? 0f : this.GetCardValue(loader, cardFromId));
			num2 += chance3.PercentageChance * num3;
		}
		return num2;
	}

	private float GetCardValue(GameDataLoader loader, CardData card)
	{
		float expectedValue;
		if (card is Harvestable { MyCardBag: var myCardBag } harvestable)
		{
			myCardBag.CalculateExpectedValueForBag(loader);
			expectedValue = Mathf.Max(card.GetValue(), myCardBag.ExpectedValue * (float)harvestable.Amount);
		}
		else
		{
			expectedValue = Mathf.Max(0f, card.GetValue());
		}
		card.ExpectedValue = expectedValue;
		return card.ExpectedValue;
	}

	public void CalculateExpectedValueForBag(GameDataLoader loader)
	{
		if (this.CardBagType == CardBagType.SetCardBag)
		{
			this.ExpectedValue = this.CalculatedExpectedValue(loader, CardBag.GetChancesForSetCardBag(loader, this.SetCardBag, null));
		}
		else if (this.CardBagType == CardBagType.SetPack)
		{
			this.ExpectedValue = this.CalculatedExpectedValue(loader, this.SetPackCards.Select((string x) => new CardChance(x, 1)).ToList());
		}
		else if (this.CardBagType == CardBagType.Chances)
		{
			this.ExpectedValue = this.CalculatedExpectedValue(loader, this.Chances);
		}
		else if (this.CardBagType == CardBagType.Enemies)
		{
			this.ExpectedValue = 0f;
		}
	}

	public ICardId GetCard(bool removeCard = true)
	{
		ICardId cardId;
		if (this.CardBagType == CardBagType.SetPack)
		{
			cardId = new CardId(this.SetPackCards[this.SetPackCards.Count - this.CardsInPack]);
		}
		else if (this.CardBagType == CardBagType.Chances)
		{
			cardId = WorldManager.instance.GetRandomCard(this.Chances, removeCard);
			if (cardId == null)
			{
				cardId = WorldManager.instance.GetRandomCard(CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, this.FallbackBag, null), removeCard);
			}
		}
		else if (this.CardBagType == CardBagType.SetCardBag)
		{
			List<CardChance> chances = ((!this.UseFallbackBag) ? CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, this.SetCardBag, null) : CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, this.SetCardBag, this.FallbackBag));
			cardId = WorldManager.instance.GetRandomCard(chances, removeCard);
		}
		else if (this.CardBagType == CardBagType.Enemies)
		{
			if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, CardBag.GetCurrentFallbackBag(), null);
				cardId = WorldManager.instance.GetRandomCard(chancesForSetCardBag, removeCard);
			}
			else
			{
				cardId = CardBag.GetCardIdForEnemyBag(this.EnemyCardBag, this.StrengthLevel);
			}
		}
		else
		{
			cardId = null;
		}
		if (removeCard)
		{
			this.CardsInPack--;
		}
		return cardId;
	}

	public ICardId GetCardFiltered(Predicate<string> filter, bool removeCard = true)
	{
		if (this.CardBagType != 0)
		{
			throw new Exception("Can't get a filtered card for bag that is not using CardBagType.Chances");
		}
		if (removeCard)
		{
			this.CardsInPack--;
		}
		return WorldManager.instance.GetRandomCard(this.Chances.Where((CardChance x) => filter(x.Id)).ToList(), removeCard);
	}

	public static ICardId GetCardIdForEnemyBag(EnemySetCardBag enemyBag, float strengthLevel)
	{
		return SpawnHelper.GetEnemyToSpawn(WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBag(enemyBag).AsList(), strengthLevel);
	}

	public List<string> GetCardsInBag(GameDataLoader loader)
	{
		if (this.CardBagType == CardBagType.SetPack)
		{
			return this.SetPackCards;
		}
		if (this.CardBagType == CardBagType.Chances)
		{
			return this.Chances.SelectMany((CardChance x) => CardBag.CardChanceToIds(x, loader)).ToList();
		}
		if (this.CardBagType == CardBagType.SetCardBag)
		{
			return CardBag.GetChancesForSetCardBag(loader, this.SetCardBag, null).SelectMany((CardChance x) => CardBag.CardChanceToIds(x, loader)).ToList();
		}
		if (this.CardBagType == CardBagType.Enemies)
		{
			SetCardBagType setCardBagForEnemyCardBag = loader.GetSetCardBagForEnemyCardBag(this.EnemyCardBag);
			List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(loader, setCardBagForEnemyCardBag, null);
			chancesForSetCardBag.RemoveAll(delegate(CardChance x)
			{
				Combatable combatable = loader.GetCardFromId(x.Id) as Combatable;
				return (combatable != null && combatable.ProcessedCombatStats.CombatLevel > this.StrengthLevel) ? true : false;
			});
			return chancesForSetCardBag.SelectMany((CardChance x) => CardBag.CardChanceToIds(x, loader)).ToList();
		}
		throw new Exception();
	}

	private static List<string> CardChanceToIds(CardChance c, GameDataLoader loader)
	{
		if (c.IsEnemy)
		{
			SetCardBagType setCardBagForEnemyCardBag = loader.GetSetCardBagForEnemyCardBag(c.EnemyBag);
			List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(loader, setCardBagForEnemyCardBag, null);
			chancesForSetCardBag.RemoveAll((CardChance x) => ((loader.GetCardFromId(x.Id) as Combatable).ProcessedCombatStats.CombatLevel > x.Strength) ? true : false);
			return chancesForSetCardBag.Select((CardChance x) => x.Id).ToList();
		}
		return c.Id.AsList();
	}

	public List<string> GetCardsInBag()
	{
		return this.GetCardsInBag(WorldManager.instance.GameDataLoader);
	}

	private static List<CardChance> GetRawCardChanges(GameDataLoader loader, SetCardBagType bag)
	{
		return CardBag.ToCardChances(loader.SetCardBags.Where((SetCardBagData x) => x.SetCardBagType == bag && x.IsActive()).SelectMany((SetCardBagData x) => x.Chances).ToList());
	}

	private static List<CardChance> GetChancesForBagNoFallback(GameDataLoader loader, SetCardBagType bag)
	{
		List<CardChance> rawCardChanges = CardBag.GetRawCardChanges(loader, bag);
		for (int num = rawCardChanges.Count - 1; num >= 0; num--)
		{
			if (string.IsNullOrWhiteSpace(rawCardChanges[num].Id))
			{
				Debug.LogError($"Error while processing {bag}");
			}
			else
			{
				CardData cardFromId = loader.GetCardFromId(rawCardChanges[num].Id);
				if (WorldManager.instance != null)
				{
					if ((cardFromId.MyCardType == CardType.Ideas || cardFromId.MyCardType == CardType.Rumors) && WorldManager.instance.CurrentSave.FoundCardIds.Contains(rawCardChanges[num].Id))
					{
						rawCardChanges.RemoveAt(num);
					}
					if (cardFromId is Enemy && WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
					{
						rawCardChanges.RemoveAt(num);
					}
				}
			}
		}
		return rawCardChanges;
	}

	public static List<CardChance> GetChancesForSetCardBag(GameDataLoader loader, SetCardBagType bag, SetCardBagType? fallbackBag = null)
	{
		List<CardChance> list = CardBag.GetChancesForBagNoFallback(loader, bag);
		if (list.Count == 0)
		{
			if (WorldManager.instance != null && WorldManager.instance.CurrentBoard != null)
			{
				SetCardBagType bag2 = CardBag.GetCurrentFallbackBag();
				if (fallbackBag.HasValue)
				{
					bag2 = fallbackBag.Value;
				}
				list = CardBag.GetRawCardChanges(loader, bag2);
			}
			else
			{
				list = CardBag.GetRawCardChanges(loader, SetCardBagType.BasicHarvestable);
			}
		}
		return list;
	}

	private static SetCardBagType GetCurrentFallbackBag()
	{
		return WorldManager.instance.CurrentBoard.BoardOptions.FallbackBag;
	}

	private static List<CardChance> ToCardChances(List<SimpleCardChance> sc)
	{
		List<CardChance> list = new List<CardChance>();
		foreach (SimpleCardChance item in sc)
		{
			list.Add(new CardChance(item.CardId, item.Chance));
		}
		return list;
	}

	public void SelectSetCardBag()
	{
	}
}
