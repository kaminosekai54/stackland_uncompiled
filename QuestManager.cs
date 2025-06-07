using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
	public static QuestManager instance;

	public List<Quest> AllQuests = new List<Quest>();

	public List<Quest> CurrentQuests = new List<Quest>();

	private Dictionary<string, Quest> idToQuest = new Dictionary<string, Quest>();

	private List<Quest> sortedQuests;

	private List<BoosterpackData> activeBoosterpackDatas = new List<BoosterpackData>();

	private void Awake()
	{
		QuestManager.instance = this;
		this.AllQuests = QuestManager.GetAllQuests();
		foreach (Quest allQuest in this.AllQuests)
		{
			this.idToQuest[allQuest.Id] = allQuest;
		}
		this.sortedQuests = this.AllQuests.OrderBy((Quest x) => x.QuestGroup).ToList();
		Debug.Log($"{this.AllQuests.Count} Quests Found!");
	}

	private void Start()
	{
		this.UpdateCurrentQuests();
	}

	public void CheckSteamAchievements()
	{
		if (Application.isEditor)
		{
			return;
		}
		foreach (Quest allQuest in this.AllQuests)
		{
			if (allQuest.IsSteamAchievement && this.QuestIsComplete(allQuest))
			{
				AchievementHelper.UnlockAchievement(allQuest.Id);
			}
		}
	}

	private Quest GetQuestWithId(string id)
	{
		this.idToQuest.TryGetValue(id, out var value);
		return value;
	}

	public void UpdateCurrentQuests()
	{
		this.CurrentQuests.Clear();
		List<Quest> list = this.AllQuests.OrderBy((Quest x) => x.QuestGroup).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Quest quest = list[i];
			if (!this.QuestIsComplete(quest))
			{
				this.CurrentQuests.Add(quest);
			}
		}
		if (GameScreen.instance != null)
		{
			GameScreen.instance.UpdateQuestLog();
		}
	}

	public void CheckPacksUnlocked()
	{
		if (this.BoosterIsUnlocked(WorldManager.instance.GetBoosterData("structures"), allowDebug: false))
		{
			this.SpecialActionComplete("unlocked_all_packs");
		}
		if (this.BoosterIsUnlocked(WorldManager.instance.GetBoosterData("island_locations"), allowDebug: false))
		{
			this.SpecialActionComplete("unlocked_all_island_packs");
		}
		BoosterpackData boosterData = WorldManager.instance.GetBoosterData("cities_ideas_2");
		if (boosterData != null && this.BoosterIsUnlocked(boosterData, allowDebug: false))
		{
			this.SpecialActionComplete("unlocked_all_cities_packs");
		}
		foreach (BuyBoosterBox allBoosterBox in WorldManager.instance.AllBoosterBoxes)
		{
			if (allBoosterBox.Booster.IsUnlocked)
			{
				QuestManager.instance.SpecialActionComplete("unlocked_" + allBoosterBox.Booster.BoosterId + "_pack");
			}
		}
	}

	public bool QuestIsVisible(Quest quest)
	{
		int num = this.sortedQuests.IndexOf(quest);
		if (num == 0 || this.sortedQuests[num - 1].QuestGroup != quest.QuestGroup || quest.DefaultVisible)
		{
			return true;
		}
		if (quest.QuestGroup == QuestGroup.Island_Misc)
		{
			return true;
		}
		if (quest.QuestGroup == QuestGroup.Death_Misc)
		{
			return true;
		}
		if (quest.QuestGroup == QuestGroup.Equipment)
		{
			return true;
		}
		if (quest.QuestGroup == QuestGroup.Discover_Spirits && WorldManager.instance.CurrentRunVariables.FinishedDemon && this.QuestIsComplete(this.sortedQuests.Find((Quest x) => x.QuestGroup == quest.QuestGroup)))
		{
			return true;
		}
		if (this.QuestIsComplete(this.sortedQuests[num - 1]) || this.QuestIsComplete(this.sortedQuests[num]))
		{
			return true;
		}
		return false;
	}

	public bool BoosterIsUnlocked(BoosterpackData p, bool allowDebug = true)
	{
		if (allowDebug && Application.isEditor && DebugOptions.Default.DebugUnlockBoosters)
		{
			return true;
		}
		return this.RemainingQuestCountToComplete(p) <= 0;
	}

	public int RemainingQuestCountToComplete(BoosterpackData p)
	{
		return Mathf.Min(this.AllQuests.Count, p.MinAchievementCount) - this.GetCompletedQuestCount(p.BoosterLocation);
	}

	private List<BoosterpackData> CurrentlyActiveBoosterpacks()
	{
		List<BoosterpackData> boosterPackDatas = WorldManager.instance.BoosterPackDatas;
		this.activeBoosterpackDatas.Clear();
		foreach (BoosterpackData item in boosterPackDatas)
		{
			if (item.BoosterLocation == WorldManager.instance.CurrentBoard.Location && item.MinAchievementCount > 0)
			{
				this.activeBoosterpackDatas.Add(item);
			}
		}
		return this.activeBoosterpackDatas;
	}

	public BoosterpackData NextPackUnlock()
	{
		List<BoosterpackData> list = this.CurrentlyActiveBoosterpacks();
		int completedQuestCount = this.GetCompletedQuestCount(WorldManager.instance.CurrentBoard.Location);
		for (int i = 0; i < list.Count; i++)
		{
			if (completedQuestCount < list[i].MinAchievementCount)
			{
				return list[i];
			}
		}
		return null;
	}

	public BoosterpackData JustUnlockedPack()
	{
		List<BoosterpackData> list = this.CurrentlyActiveBoosterpacks();
		int completedQuestCount = this.GetCompletedQuestCount(WorldManager.instance.CurrentBoard.Location);
		for (int i = 0; i < list.Count; i++)
		{
			if (completedQuestCount == list[i].MinAchievementCount)
			{
				return list[i];
			}
		}
		return null;
	}

	public int GetCompletedQuestCount(Location loc)
	{
		return this.GetCompletedQuestCountOnLocation(loc);
	}

	private int GetCompletedQuestCountOnLocation(Location loc)
	{
		List<string> completedAchievementIds = WorldManager.instance.CurrentSave.CompletedAchievementIds;
		int num = 0;
		foreach (string item in completedAchievementIds)
		{
			Quest questWithId = this.GetQuestWithId(item);
			if (questWithId != null && questWithId.QuestLocation == loc)
			{
				num++;
			}
		}
		return num;
	}

	public bool QuestIsComplete(Quest quest)
	{
		return this.QuestIsComplete(quest.Id);
	}

	public bool QuestIsComplete(string id)
	{
		return WorldManager.instance.CurrentSave.CompletedAchievementIds.Contains(id);
	}

	public bool IsInactiveSpiritQuest(Quest quest, Location currentLocation)
	{
		Location questLocation = quest.QuestLocation;
		if (questLocation == Location.Death || questLocation == Location.Greed || questLocation == Location.Happiness)
		{
			if (WorldManager.instance.CurrentBoard == null)
			{
				return false;
			}
			return quest.QuestLocation != currentLocation;
		}
		return false;
	}

	public void ActionComplete(CardData card, string action, CardData focusCard = null)
	{
		bool flag = false;
		Location location = WorldManager.instance.CurrentBoard.Location;
		foreach (Quest currentQuest in this.CurrentQuests)
		{
			if (!this.IsInactiveSpiritQuest(currentQuest, location) && currentQuest.OnActionComplete != null && currentQuest.OnActionComplete(card, action))
			{
				if (focusCard == null)
				{
					this.MarkQuestComplete(currentQuest, card);
				}
				else
				{
					this.MarkQuestComplete(currentQuest, focusCard);
				}
				flag = true;
			}
		}
		if (flag)
		{
			this.UpdateCurrentQuests();
		}
	}

	public void SpecialActionComplete(string action, CardData card = null)
	{
		bool flag = false;
		Location location = WorldManager.instance.CurrentBoard.Location;
		foreach (Quest currentQuest in this.CurrentQuests)
		{
			if (!this.IsInactiveSpiritQuest(currentQuest, location) && currentQuest.OnSpecialAction != null && currentQuest.OnSpecialAction(action))
			{
				this.MarkQuestComplete(currentQuest, card);
				flag = true;
			}
		}
		if (flag)
		{
			this.UpdateCurrentQuests();
		}
	}

	public void CardCreated(CardData card)
	{
		bool flag = false;
		Location location = WorldManager.instance.CurrentBoard.Location;
		foreach (Quest currentQuest in this.CurrentQuests)
		{
			if (!this.IsInactiveSpiritQuest(currentQuest, location) && currentQuest.OnCardCreate != null && currentQuest.OnCardCreate(card))
			{
				this.MarkQuestComplete(currentQuest, card);
				flag = true;
			}
		}
		if (flag)
		{
			this.UpdateCurrentQuests();
		}
	}

	public void DebugUnlockAllQuests()
	{
		foreach (Quest allQuest in QuestManager.GetAllQuests())
		{
			this.MarkQuestComplete(allQuest);
		}
	}

	public bool AnyIslandQuestComplete()
	{
		return this.AllQuests.Any((Quest x) => x.QuestLocation == Location.Island && this.QuestIsComplete(x));
	}

	private void MarkQuestComplete(Quest quest, CardData card = null)
	{
		SaveGame currentSave = WorldManager.instance.CurrentSave;
		if (!currentSave.CompletedAchievementIds.Contains(quest.Id))
		{
			currentSave.CompletedAchievementIds.Add(quest.Id);
			WorldManager.instance.QuestsCompleted++;
			WorldManager.instance.QuestCompleted(quest);
			if (quest.IsSteamAchievement)
			{
				AchievementHelper.UnlockAchievement(quest.Id);
			}
		}
	}

	public static List<Quest> GetAllQuests()
	{
		List<Quest> list = new List<Quest>();
		FieldInfo[] fields = typeof(AllQuests).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeof(Quest))
			{
				Quest item = (Quest)fieldInfo.GetValue(null);
				list.Add(item);
			}
		}
		return list;
	}
}
