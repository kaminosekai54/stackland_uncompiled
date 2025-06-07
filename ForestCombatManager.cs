using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForestCombatManager : MonoBehaviour
{
	public static ForestCombatManager instance;

	public ForestCombatState CombatState = ForestCombatState.Idle;

	public List<AudioClip> WitchSounds;

	public float FirstWaveStrength = 10f;

	public float WaveStrengthIncrement = 10f;

	public int WickedWitchWave = 10;

	public List<string> BlacklistedDropIds;

	private List<SetCardBagType> enemiesAdvanced = new List<SetCardBagType>
	{
		SetCardBagType.Forest_BasicEnemy,
		SetCardBagType.Forest_AdvancedEnemy
	};

	private List<SetCardBagType> enemiesBasic = new List<SetCardBagType> { SetCardBagType.Forest_BasicEnemy };

	private void Awake()
	{
		ForestCombatManager.instance = this;
		this.VerifyBlacklistedDrops();
	}

	private void MinimizeUI()
	{
		GameScreen.instance.SetMinimize(minimized: true);
		GameScreen.instance.UpdateSidePanelPosition();
	}

	public void ResumeForestCombat()
	{
		if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			this.LeaveForest();
		}
		if (this.IsWaveOver())
		{
			this.CombatState = ForestCombatState.Cutscene;
			this.LayoutVillagers(hardSetPosition: true);
			WorldManager.instance.QueueCutscene(Cutscenes.ForestResumeIntro());
		}
		else
		{
			this.CombatState = ForestCombatState.InWave;
		}
		this.MinimizeUI();
	}

	public void InitForestCombat()
	{
		this.CombatState = ForestCombatState.Cutscene;
		this.MinimizeUI();
		this.LayoutVillagers(hardSetPosition: true);
		WorldManager.instance.QueueCutscene(Cutscenes.ForestIntro());
		QuestManager.instance.SpecialActionComplete("find_dark_forest");
	}

	public void PrepareWave()
	{
		WorldManager.instance.CurrentRunVariables.CanDropItem = true;
		int forestWave = WorldManager.instance.CurrentRunVariables.ForestWave;
		Debug.Log($"Start wave {forestWave} with wicked witch at wave {this.WickedWitchWave}");
		GameCamera.instance.Screenshake = 0.3f;
		if (forestWave < this.WickedWitchWave)
		{
			this.SpawnWave(forestWave);
		}
		else if (forestWave == this.WickedWitchWave)
		{
			WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "wicked_witch", faceUp: true, checkAddToStack: false);
			this.SpawnWave(forestWave);
		}
		else
		{
			this.SpawnWave(forestWave);
		}
		ForestCombatManager.StartWaveConflict(forestWave == this.WickedWitchWave);
	}

	private List<SetCardBagType> GetPossibleEnemies(int wave)
	{
		if (wave < 4)
		{
			return this.enemiesBasic;
		}
		return this.enemiesAdvanced;
	}

	private float GetStrengthForWave(int wave)
	{
		return this.WaveStrengthIncrement * (float)wave + this.FirstWaveStrength;
	}

	private void SpawnWave(int wave)
	{
		foreach (CardIdWithEquipment item in SpawnHelper.GetEnemiesToSpawn(strength: (wave > this.WickedWitchWave) ? Random.Range(this.GetStrengthForWave(3), this.GetStrengthForWave(15)) : this.GetStrengthForWave(wave), cardbags: this.GetPossibleEnemies(wave)))
		{
			Combatable obj = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), item, faceUp: true, checkAddToStack: false) as Combatable;
			obj.HealthPoints = obj.ProcessedCombatStats.MaxHealth;
		}
	}

	public void StartWave()
	{
		this.CombatState = ForestCombatState.InWave;
	}

	private static void StartWaveConflict(bool wickedWitchWave)
	{
		List<Combatable> cards = WorldManager.instance.GetCards<Combatable>();
		cards = ((!wickedWitchWave) ? (from x in cards
			orderby x.Team descending
			where x.Id != "wicked_witch"
			select x).ToList() : cards.OrderByDescending((Combatable x) => x.Team).ToList());
		Conflict conflict = Conflict.StartConflict(cards[0]);
		for (int i = 1; i < cards.Count; i++)
		{
			conflict.JoinConflict(cards[i]);
		}
		Vector3 conflictStartPosition = ForestCombatManager.DetermineVillagerPositionAverage(cards);
		conflict.ConflictStartPosition = conflictStartPosition;
		for (int j = 0; j < cards.Count; j++)
		{
			Vector3 positionInConflict = conflict.GetPositionInConflict(cards[j]);
			cards[j].MyGameCard.transform.position = (cards[j].MyGameCard.TargetPosition = positionInConflict);
			if (!(cards[j] is BaseVillager))
			{
				WorldManager.instance.CreateSmoke(positionInConflict);
			}
		}
	}

	private static Vector3 DetermineVillagerPositionAverage(List<Combatable> combatables)
	{
		Vector3 vector = default(Vector3);
		int num = 0;
		for (int i = 0; i < combatables.Count; i++)
		{
			if (combatables[i] is BaseVillager)
			{
				vector += combatables[i].transform.position;
				num++;
			}
		}
		return vector /= (float)num;
	}

	private void FinishWave()
	{
		ForestCombatManager.DeleteAllCorpses();
		QuestManager.instance.SpecialActionComplete("completed_forest_wave");
		WorldManager.instance.CurrentRunVariables.ForestWave++;
		WorldManager.instance.CurrentRunVariables.CanDropItem = true;
		int forestWave = WorldManager.instance.CurrentRunVariables.ForestWave;
		this.CombatState = ForestCombatState.Finished;
		if (forestWave < this.WickedWitchWave)
		{
			this.LayoutVillagers();
			WorldManager.instance.QueueCutscene(Cutscenes.ForestWaveEnd());
		}
		else if (forestWave == this.WickedWitchWave)
		{
			this.LayoutVillagers();
			WorldManager.instance.QueueCutscene(Cutscenes.ForestLastWaveEnd());
		}
		else
		{
			this.LayoutVillagers();
			WorldManager.instance.QueueCutscene(Cutscenes.ForestEndlessWaveEnd());
		}
	}

	private void Update()
	{
		this.CheckResumeCombat();
		if (WorldManager.instance.CurrentGameState != 0 || !(WorldManager.instance.CurrentBoard.Id == "forest"))
		{
			return;
		}
		if (WorldManager.instance.InAnimation)
		{
			this.LayoutVillagers();
		}
		if (this.CombatState == ForestCombatState.InWave)
		{
			if (this.IsWaveOver())
			{
				this.FinishWave();
			}
			else if (this.AllVillagersInForestDied())
			{
				this.CombatState = ForestCombatState.Lost;
				WorldManager.instance.QueueCutscene(Cutscenes.ForestWaveLost());
			}
		}
	}

	private bool IsWaveOver()
	{
		bool result = true;
		foreach (GameCard item in WorldManager.instance.GetAllCardsOnBoard("forest"))
		{
			if (item.CardData is Enemy || item.CardData is Mob { IsAggressive: not false })
			{
				if (item.CardData.Id == "wicked_witch" && WorldManager.instance.CurrentRunVariables.ForestWave != this.WickedWitchWave)
				{
					break;
				}
				result = false;
			}
		}
		return result;
	}

	private bool AllVillagersInForestDied()
	{
		foreach (GameCard item in WorldManager.instance.GetAllCardsOnBoard("forest"))
		{
			if (item.CardData is BaseVillager)
			{
				return false;
			}
		}
		return true;
	}

	public void LeaveForest()
	{
		ForestCombatManager.DeleteAllCorpses();
		List<GameCard> list = (from x in WorldManager.instance.GetAllCardsOnBoard("forest")
			where !x.IsEquipped && x.CardData.MyCardType != CardType.Humans
			select x).ToList();
		list.RemoveAll((GameCard x) => !this.CanDropCard(x.CardData.Id));
		List<GameCard> list2 = (from x in WorldManager.instance.GetAllCardsOnBoard("forest")
			where x.CardData.MyCardType == CardType.Humans
			select x).ToList();
		WorldManager.instance.Restack(list);
		WorldManager.instance.Restack(list2);
		GameBoard boardWithId = WorldManager.instance.GetBoardWithId(WorldManager.instance.CurrentRunVariables.PreviouseBoard);
		if (list.Count > 0)
		{
			WorldManager.instance.SendStackToBoard(list[0], boardWithId, new Vector2(0.4f, 0.5f));
		}
		WorldManager.instance.SendStackToBoard(list2[0], boardWithId, new Vector2(0.5f, 0.5f));
		WorldManager.instance.GoToBoard(boardWithId, delegate
		{
			if (!WorldManager.instance.HasFoundCard("blueprint_stable_portal"))
			{
				WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "blueprint_stable_portal");
			}
		});
	}

	public void ForestWaveLost()
	{
		GameBoard boardWithId = WorldManager.instance.GetBoardWithId(WorldManager.instance.CurrentRunVariables.PreviouseBoard);
		WorldManager.instance.GoToBoard(boardWithId, delegate
		{
			if (!WorldManager.instance.HasFoundCard("blueprint_stable_portal"))
			{
				WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "blueprint_stable_portal");
			}
			ForestCombatManager.DeleteAllCorpses();
			ForestCombatManager.RemoveForestCards();
			this.CombatState = ForestCombatState.Idle;
		});
	}

	private static void RemoveForestCards()
	{
		foreach (GameCard item in WorldManager.instance.GetAllCardsOnBoard("forest"))
		{
			item.DestroyCard();
		}
	}

	private void CheckResumeCombat()
	{
		if (WorldManager.instance.CurrentBoard != null && WorldManager.instance.CurrentBoard.Id == "forest")
		{
			if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				this.LeaveForest();
			}
			if (WorldManager.instance.CurrentRunVariables.VisitedForest && this.CombatState == ForestCombatState.Idle)
			{
				this.ResumeForestCombat();
			}
			else if (!WorldManager.instance.CurrentRunVariables.VisitedForest && this.CombatState == ForestCombatState.Idle)
			{
				this.InitForestCombat();
				WorldManager.instance.CurrentRunVariables.VisitedForest = true;
			}
		}
		else if (this.CombatState != ForestCombatState.Idle)
		{
			this.CombatState = ForestCombatState.Idle;
		}
	}

	public bool CanDropCard(string cardId)
	{
		return !this.BlacklistedDropIds.Contains(cardId);
	}

	private void VerifyBlacklistedDrops()
	{
		foreach (string blacklistedDropId in this.BlacklistedDropIds)
		{
			if (WorldManager.instance.GameDataLoader.GetCardFromId(blacklistedDropId) == null)
			{
				Debug.LogError(blacklistedDropId + " is not a valid card id");
			}
		}
	}

	public static Vector3 GetWitchPosition()
	{
		return ForestCombatManager.GetVillagersPosition() + new Vector3(0f, 0f, GameCard.CardHeight * 1.2f);
	}

	public static void DeleteAllCorpses()
	{
		foreach (GameCard item in (from x in WorldManager.instance.GetAllCardsOnBoard("forest")
			where x.CardData is Corpse
			select x).ToList())
		{
			item.DestroyCard();
		}
	}

	public static Vector3 GetVillagersPosition()
	{
		Vector3 result = WorldManager.instance.GetBoardWithId("forest").MiddleOfBoard();
		float conflictHeight = Conflict.GetConflictHeight();
		result.z += conflictHeight * 0.25f;
		return result;
	}

	private void LayoutVillagers(bool hardSetPosition = false)
	{
		List<BaseVillager> cards = WorldManager.instance.GetCards<BaseVillager>();
		if (cards.Count == 0)
		{
			return;
		}
		Vector3 villagersPosition = ForestCombatManager.GetVillagersPosition();
		for (int i = 0; i < cards.Count; i++)
		{
			float num = (float)i - ((float)cards.Count - 1f) * 0.5f;
			Vector3 vector = new Vector3(num * WorldManager.instance.HorizonalCombatOffset, 0f, 0f);
			cards[i].MyGameCard.RemoveFromStack();
			cards[i].MyGameCard.TargetPosition = villagersPosition + vector;
			if (hardSetPosition)
			{
				cards[i].MyGameCard.transform.position = cards[i].MyGameCard.TargetPosition;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(ForestCombatManager.GetVillagersPosition(), 0.3f);
		}
	}
}
