using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Subprint
{
	[HideInInspector]
	public int SubprintIndex;

	[HideInInspector]
	public Blueprint ParentBlueprint;

	[Card]
	public string[] RequiredCards;

	[Card]
	public string[] CardsToRemove;

	public int ResultPolution;

	public int ResultWellbeing;

	[Card]
	public string ResultCard;

	public string ResultAction;

	[Card]
	public string[] ExtraResultCards;

	public float Time = 10f;

	[Term]
	public string StatusTerm;

	[HideInInspector]
	public string statusOverride;

	private List<string> missingCards = new List<string>();

	private static Dictionary<string, string> specialCardIds = new Dictionary<string, string>
	{
		{ "any_villager", "" },
		{ "any_villager_young", "" },
		{ "any_villager_old", "" },
		{ "breedable_villager", "" },
		{ "stone", "stone|sandstone" },
		{ "cotton", "cotton" },
		{ "fish", "cod|eel|mackerel|tuna|shark" },
		{ "any_worker", "" },
		{ "any_educated_worker", "" }
	};

	public string StatusName
	{
		get
		{
			if (!string.IsNullOrEmpty(this.statusOverride))
			{
				return this.statusOverride;
			}
			return SokLoc.Translate(this.StatusTerm);
		}
	}

	public static List<string> GetSpecialCardIds()
	{
		Subprint.UpdateAnyVillagerCardIds();
		Subprint.UpdateAnyWorkerCardIds();
		return Subprint.specialCardIds.Keys.ToList();
	}

	public static void UpdateAnyVillagerCardIds()
	{
		Dictionary<string, string> dictionary = Subprint.specialCardIds;
		Dictionary<string, string> dictionary2 = Subprint.specialCardIds;
		string text2 = (Subprint.specialCardIds["any_villager"] = string.Join("|", Subprint.GetVillagerCardIds()));
		string value = (dictionary2["any_villager_young"] = text2);
		dictionary["any_villager_old"] = value;
		Subprint.specialCardIds["breedable_villager"] = string.Join("|", Subprint.GetVillagerCardIds((BaseVillager x) => x.CanBreed));
	}

	public static void UpdateAnyWorkerCardIds()
	{
		Subprint.specialCardIds["any_worker"] = string.Join("|", Subprint.GetWorkerCardIds());
		Subprint.specialCardIds["any_educated_worker"] = string.Join("|", Subprint.GetWorkerCardIds((Worker x) => x.WorkerType == WorkerType.Educated || x.WorkerType == WorkerType.Robot));
	}

	private static List<string> GetVillagerCardIds(Predicate<BaseVillager> pred = null)
	{
		List<string> list = new List<string>();
		GameDataLoader gameDataLoader = WorldManager.instance?.GameDataLoader;
		if (gameDataLoader == null)
		{
			gameDataLoader = GameDataLoader.instance;
		}
		foreach (CardData cardDataPrefab in gameDataLoader.CardDataPrefabs)
		{
			if (cardDataPrefab is BaseVillager obj && (pred == null || pred(obj)))
			{
				list.Add(cardDataPrefab.Id);
			}
		}
		return list;
	}

	private static List<string> GetWorkerCardIds(Predicate<Worker> pred = null)
	{
		List<string> list = new List<string>();
		GameDataLoader gameDataLoader = WorldManager.instance?.GameDataLoader;
		if (gameDataLoader == null)
		{
			gameDataLoader = GameDataLoader.instance;
		}
		foreach (CardData cardDataPrefab in gameDataLoader.CardDataPrefabs)
		{
			if (cardDataPrefab is Worker obj && (pred == null || pred(obj)))
			{
				list.Add(cardDataPrefab.Id);
			}
		}
		return list;
	}

	public bool StackMatchesSubprint(GameCard rootCard, out SubprintMatchInfo matchInfo)
	{
		matchInfo = default(SubprintMatchInfo);
		int num = rootCard.GetChildCount() + 1;
		if ((bool)rootCard.HasCardInStack((CardData x) => x.Id == "heavy_foundation"))
		{
			num--;
		}
		if (num < this.RequiredCards.Length)
		{
			return false;
		}
		if (this.ParentBlueprint.NeedsExactMatch && num != this.RequiredCards.Length)
		{
			return false;
		}
		this.missingCards.Clear();
		this.missingCards.AddRange(this.RequiredCards);
		GameCard gameCard = rootCard;
		int num2 = 0;
		while (gameCard != null)
		{
			for (int num3 = this.missingCards.Count - 1; num3 >= 0; num3--)
			{
				string s = this.ParseCardId(this.missingCards[num3]);
				if (CardStringSplitter.me.Split(s).Contains(gameCard.CardData.Id))
				{
					this.missingCards.RemoveAt(num3);
					break;
				}
			}
			if (this.missingCards.Count == 0)
			{
				matchInfo = new SubprintMatchInfo(num2, this.RequiredCards.Length);
				return true;
			}
			gameCard = gameCard.Child;
			num2++;
		}
		return false;
	}

	public List<string> GetAllCardsToRemove()
	{
		List<string> list = new List<string>();
		if (this.CardsToRemove != null && this.CardsToRemove.Length != 0)
		{
			string[] cardsToRemove = this.CardsToRemove;
			foreach (string cardId in cardsToRemove)
			{
				string item = this.ParseCardId(cardId);
				list.Add(item);
			}
		}
		else
		{
			string[] cardsToRemove = this.RequiredCards;
			foreach (string cardId2 in cardsToRemove)
			{
				string text = this.ParseCardId(cardId2);
				string id = CardStringSplitter.me.Split(text)[0];
				CardData cardPrefab = WorldManager.instance.GetCardPrefab(id);
				if (cardPrefab.MyCardType != CardType.Humans && cardPrefab.MyCardType != 0 && cardPrefab.Id != "worker")
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	private string ParseCardId(string cardId)
	{
		if (Subprint.specialCardIds.ContainsKey(cardId))
		{
			return Subprint.specialCardIds[cardId];
		}
		return cardId;
	}

	public string DefaultText()
	{
		List<string> list = this.RequiredCards.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			string[] array = CardStringSplitter.me.Split(list[i]);
			CardData cardPrefab = WorldManager.instance.GetCardPrefab(array[0]);
			cardPrefab.UpdateCardText();
			list[i] = cardPrefab.Name;
		}
		List<string> list2 = list.Distinct().ToList();
		string text = "";
		for (int j = 0; j < list2.Count; j++)
		{
			string card = list2[j];
			int num = list.Count((string x) => x == card);
			text += $"{num}x {card}";
			if (j < list2.Count - 1)
			{
				text += "\n";
			}
		}
		return text;
	}
}
