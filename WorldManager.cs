using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WorldManager : MonoBehaviour
{
	public enum GameState
	{
		Playing,
		Paused,
		GameOver,
		InMenu
	}

	public static WorldManager instance;

	public float CardOverlayOffset = 0.1f;

	public float CollapsedCardOverlayOffset = 0.02f;

	public float CombatOffset = 1f;

	public float HorizonalCombatOffset = 0.2f;

	public float CombatMissOffset = 0.2f;

	public float CardOverlayHeightOffset = 0.001f;

	public float ConflictWidthIncrease;

	public float ConflictHeightIncrease;

	public float ConflictArrowLengthDecrease = 0.2f;

	private List<ParticleSystem> smokeBuffer = new List<ParticleSystem>();

	private List<ParticleSystem> energyMinusBuffer = new List<ParticleSystem>();

	private List<ParticleSystem> wellbeingPlusBuffer = new List<ParticleSystem>();

	private List<FloatingStatus> floatingTextBuffer = new List<FloatingStatus>();

	public AnimationCurve CombatYPosition;

	public AnimationCurve CombatFlatPositionCurve;

	public AnimationCurve CombatKnockbackCurve;

	public float CombatSpeed = 5f;

	public List<Draggable> AllDraggables = new List<Draggable>();

	public List<Draggable> PhysicsDraggables = new List<Draggable>();

	public GetComponentCacher<Draggable> DraggableLookup = new GetComponentCacher<Draggable>();

	public GetComponentCacher<Interactable> InteractableLookup = new GetComponentCacher<Interactable>();

	public GetComponentCacher<Hoverable> HoverableLookup = new GetComponentCacher<Hoverable>();

	public Draggable HoveredDraggable;

	public Draggable DraggingDraggable;

	public Interactable HoveredInteractable;

	public Hoverable CurrentHoverable;

	[HideInInspector]
	public List<GameCard> AllCards = new List<GameCard>();

	[HideInInspector]
	public Dictionary<string, GameCard> UniqueIdToCard = new Dictionary<string, GameCard>();

	[HideInInspector]
	public List<CardTarget> CardTargets = new List<CardTarget>();

	public List<Boosterpack> AllBoosters = new List<Boosterpack>();

	public List<string> BoughtBoosterIds = new List<string>();

	public List<BuyBoosterBox> AllBoosterBoxes = new List<BuyBoosterBox>();

	public Material HitMaterial;

	public ViewType CurrentView = ViewType.Default;

	public List<GameBoard> Boards;

	public float MonthTimer;

	public float AnimationTime;

	public BoardMonths BoardMonths;

	public int OldCurrentMonth;

	public bool DebugScreenOpened;

	public float CardTargetSnapDistance = 0.2f;

	public GameState CurrentGameState;

	public bool CanUseTransport;

	[HideInInspector]
	public Vector3 mouseWorldPosition;

	private RaycastHit[] hits = new RaycastHit[40];

	[HideInInspector]
	public Vector3 grabOffset;

	public GameDataLoader GameDataLoader;

	private GameDataValidator validator;

	[HideInInspector]
	public bool DebugEndlessMoonEnabled;

	[HideInInspector]
	public bool DebugNoFoodEnabled;

	[HideInInspector]
	public bool ForestMoonEnabled;

	[HideInInspector]
	public bool DebugDontNeedVillagers;

	[HideInInspector]
	public bool DebugNoEnergyEnabled;

	public List<SerializedKeyValuePair> RoundExtraKeyValues = new List<SerializedKeyValuePair>();

	private bool IsLoadingSaveRound;

	private bool clickStartedGrabbing;

	[HideInInspector]
	public bool CutsceneBoardView;

	private TMP_InputField currentSelectedInput;

	public List<Curse> ActiveCurses = new List<Curse>();

	public List<ActionTimeBase> actionTimeBases = new List<ActionTimeBase>();

	public List<ActionTimeModifier> actionTimeModifiers = new List<ActionTimeModifier>();

	public CardTarget NearbyCardTarget;

	public float SpeedUp = 1f;

	public QueuedAnimation currentAnimation;

	private List<QueuedAnimation> queuedAnimations = new List<QueuedAnimation>();

	private float physicsTimer;

	private float preAutoPauseSpeed;

	private bool isAutoPaused;

	public bool IsShiftDragging;

	public float GridWidth = 0.75f;

	public float GridHeight = 0.85f;

	public float gridAlpha;

	private HashSet<string> doesntCountTowardsCount = new HashSet<string> { "gold", "shell", "happiness", "unhappiness", "pollution" };

	private List<CityHall> townhalls = new List<CityHall>();

	private List<Dollar> dollars = new List<Dollar>();

	private List<Creditcard> creditcards = new List<Creditcard>();

	public bool ShowContinueButton;

	public string ContinueButtonText = "";

	public string CutsceneText = "";

	public string CutsceneTitle = "";

	public bool RemovingCards;

	public bool ConnectConnectors;

	[HideInInspector]
	public bool ContinueClicked;

	[HideInInspector]
	public int ContinueButtonIndex;

	public bool InEatingAnimation;

	public float EndOfMonthSpeedup;

	public bool VillagersStarvedAtEndOfMoon;

	public bool VillagersAngryAtEndOfMoon;

	public Coroutine currentAnimationRoutine;

	private WeightedRandomBag<CardChance> chanceBag = new WeightedRandomBag<CardChance>();

	public int QuestsCompleted;

	public int NewCardsFound;

	public RunOptions CurrentRunOptions;

	public RunVariables CurrentRunVariables;

	public List<string> GivenCards = new List<string>();

	public GameCard DraggingCard => this.DraggingDraggable as GameCard;

	public GameCard HoveredCard => this.HoveredDraggable as GameCard;

	public List<CardData> CardDataPrefabs => this.GameDataLoader.CardDataPrefabs;

	public List<Blueprint> BlueprintPrefabs => this.GameDataLoader.BlueprintPrefabs;

	public List<BoosterpackData> BoosterPackDatas => this.GameDataLoader.BoosterpackDatas;

	public SaveGame CurrentSave => SaveManager.instance.CurrentSave;

	public int CurrentMonth
	{
		get
		{
			if (this.BoardMonths != null)
			{
				return this.BoardMonths.GetCurrentMonth();
			}
			return 0;
		}
	}

	public float MonthTime
	{
		get
		{
			if (this.CurrentRunOptions != null)
			{
				if (this.CurrentRunOptions.MoonLength == MoonLength.Short)
				{
					return 90f;
				}
				if (this.CurrentRunOptions.MoonLength == MoonLength.Normal)
				{
					return 120f;
				}
				if (this.CurrentRunOptions.MoonLength == MoonLength.Long)
				{
					return 200f;
				}
			}
			return 120f;
		}
	}

	public bool IsPlaying => this.CurrentGameState == GameState.Playing;

	public bool InAnimation
	{
		get
		{
			if (this.currentAnimationRoutine == null)
			{
				return this.currentAnimation != null;
			}
			return true;
		}
	}

	public bool CanInteract
	{
		get
		{
			if (this.IsPlaying && ((this.currentAnimationRoutine == null && this.currentAnimation == null) || this.RemovingCards) && !GameScreen.instance.ControllerIsInUI)
			{
				return !GameCanvas.instance.ModalIsOpen;
			}
			return false;
		}
	}

	public List<SerializedKeyValuePair> SaveExtraKeyValues => this.CurrentSave.ExtraKeyValues;

	public GameBoard CurrentBoard { get; private set; }

	public Boosterpack IntroPack => this.AllBoosters.FirstOrDefault((Boosterpack x) => x.IsIntroPack && x.MyBoard.IsCurrent);

	public float TimeScale
	{
		get
		{
			if (!this.IsPlaying)
			{
				return 0f;
			}
			if (this.currentAnimationRoutine != null || this.currentAnimation != null)
			{
				return 0f;
			}
			if (GameCanvas.instance.ModalIsOpen)
			{
				return 0f;
			}
			if (TransitionScreen.InTransition)
			{
				return 0f;
			}
			return this.SpeedUp;
		}
	}

	public float PhysicsTimeScale
	{
		get
		{
			if (!this.IsPlaying)
			{
				return 0f;
			}
			if (this.SpeedUp == 0f)
			{
				return 1f;
			}
			return this.SpeedUp;
		}
	}

	private void Awake()
	{
		WorldManager.instance = this;
		this.AllCards = new List<GameCard>();
		this.Boards = UnityEngine.Object.FindObjectsOfType<GameBoard>().ToList();
		this.CurrentGameState = GameState.InMenu;
		this.GameDataLoader = new GameDataLoader(this.SpiritDLCInstalled(), this.CitiesDLCInstalled());
		Subprint.UpdateAnyVillagerCardIds();
		Subprint.UpdateAnyWorkerCardIds();
		if (Application.isEditor)
		{
			this.DebugEndlessMoonEnabled = DebugOptions.Default.EndlessMoonEnabled;
			this.DebugNoFoodEnabled = DebugOptions.Default.NoFoodEnabled;
			this.DebugDontNeedVillagers = DebugOptions.Default.DontNeedVillagers;
			this.DebugNoEnergyEnabled = DebugOptions.Default.NoEnergyEnabled;
			this.CurrentSave.FinishedDeath = DebugOptions.Default.CursedFinished;
			this.CurrentSave.FinishedGreed = DebugOptions.Default.CursedFinished;
			this.CurrentSave.FinishedHappiness = DebugOptions.Default.CursedFinished;
			base.gameObject.AddComponent<Screenshotter>();
		}
		Shader.SetGlobalFloat("_WorldSizeIncrease", 0f);
		Shader.SetGlobalFloat("_WorldSizeIncreaseNormalized", 0f);
		SokLoc.instance.LanguageChanged += OnLanguageChange;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		this.InitializeBaseVillagerSpeedRules();
	}

	private void Start()
	{
		OptionsScreen.LoadSettings();
		QuestManager.instance.CheckSteamAchievements();
		WorldManager.CheckForceReloadSave();
	}

	private void InitActionTimeBases()
	{
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "dog" || p.villager.Id == "cat", 2f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "fisher" && p.actionId == "complete_harvest" && p.baseCard.Id == "fishing_spot", 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "explorer", 1.25f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "explorer" && p.actionId == "complete_harvest" && p.baseCard.MyCardType == CardType.Locations, 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "lumberjack" && p.actionId == "complete_harvest" && (p.baseCard.Id == "lumbercamp" || p.baseCard.Id == "apple_tree" || p.baseCard.Id == "tree" || p.baseCard.Id == "olive_tree"), 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "builder" && p.actionId == "finish_blueprint", 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "miner" && p.actionId == "complete_harvest" && (p.baseCard.Id == "gold_mine" || p.baseCard.Id == "mine" || p.baseCard.Id == "rock" || p.baseCard.Id == "quarry" || p.baseCard.Id == "iron_deposit"), 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.HasEquipableWithId("scythe") && p.actionId == "complete_harvest" && (p.baseCard.Id == "berrybush" || p.baseCard.Id == "olive_tree" || p.baseCard.Id == "apple_tree" || p.baseCard.Id == "grape_vine" || p.baseCard.Id == "tomato_plant"), 0.5f));
	}

	private void InitActionTimeModifiers()
	{
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.HasStatusEffectOfType<StatusEffect_Drunk>(), 2f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.HasStatusEffectOfType<StatusEffect_Anxious>(), 2.5f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.HasStatusEffectOfType<StatusEffect_WellFed>(), 0.5f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.MyLifeStage == LifeStage.Teenager, 0.75f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.MyLifeStage == LifeStage.Elderly, 1.25f));
	}

	public void InitializeBaseVillagerSpeedRules()
	{
		this.InitActionTimeBases();
		this.InitActionTimeModifiers();
	}

	public HashSet<string> FindMissingCardsInSave()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (SavedCard savedCard in this.CurrentSave.LastPlayedRound.SavedCards)
		{
			if (this.GameDataLoader.GetCardFromId(savedCard.CardPrefabId) == null)
			{
				hashSet.Add(savedCard.CardPrefabId);
			}
		}
		return hashSet;
	}

	public void LoadPreviousRound()
	{
		this.LoadSaveRound(this.CurrentSave.LastPlayedRound);
		foreach (GameBoard board in this.Boards)
		{
			board.WorldSizeIncrease = this.DetermineTargetWorldSize(board);
		}
		if (QuestManager.instance.QuestIsComplete(AllQuests.KillDemon) && !this.CurrentSave.GotIslandIntroPack)
		{
			this.QueueCutscene(Cutscenes.IslandIntroPack());
		}
	}

	public SaveRound GetSaveRound()
	{
		SaveRound saveRound = new SaveRound();
		saveRound.SaveVersion = 3;
		saveRound.SavedCards = new List<SavedCard>();
		saveRound.SavedBoosters = new List<SavedBooster>();
		saveRound.SavedBoosterBoxes = new List<SavedBoosterBox>();
		saveRound.SavedConflicts = new List<SavedConflict>();
		saveRound.RunVariables = this.CurrentRunVariables;
		saveRound.RunOptions = this.CurrentRunOptions;
		saveRound.BoughtBoosterIds = this.BoughtBoosterIds;
		saveRound.CurrentBoardId = this.CurrentBoard.Id;
		saveRound.ExtraKeyValues = this.RoundExtraKeyValues;
		foreach (GameCard allCard in this.AllCards)
		{
			saveRound.SavedCards.Add(allCard.ToSavedCard());
		}
		foreach (Boosterpack allBooster in this.AllBoosters)
		{
			saveRound.SavedBoosters.Add(new SavedBooster
			{
				BoosterId = allBooster.BoosterId,
				TimesOpened = allBooster.TimesOpened,
				BoardId = allBooster.MyBoard.Id,
				Position = allBooster.TargetPosition
			});
		}
		foreach (BuyBoosterBox allBoosterBox in this.AllBoosterBoxes)
		{
			saveRound.SavedBoosterBoxes.Add(new SavedBoosterBox
			{
				BoosterId = allBoosterBox.BoosterId,
				StoredCostAmount = allBoosterBox.StoredCostAmount
			});
		}
		saveRound.MonthTimer = this.MonthTimer;
		saveRound.CurrentMonth = this.CurrentMonth;
		saveRound.OldCurrentMonth = this.OldCurrentMonth;
		saveRound.BoardMonths = this.BoardMonths.ToSavedMonth();
		saveRound.NewCardsFound = this.NewCardsFound;
		saveRound.QuestsCompleted = this.QuestsCompleted;
		saveRound.CitiesWellbeing = CitiesManager.instance.Wellbeing;
		saveRound.CitiesConflictMonth = CitiesManager.instance.NextConflictMonth;
		saveRound.CitiesDisaster = CitiesManager.instance.ActiveEvent;
		foreach (Conflict allConflict in this.GetAllConflicts())
		{
			saveRound.SavedConflicts.Add(new SavedConflict
			{
				Id = allConflict.Id,
				InitiatorCardId = allConflict.Initiator.UniqueId,
				InvolvedCards = allConflict.Participants.Select((Combatable x) => x.UniqueId).ToList(),
				StartPosition = allConflict.ConflictStartPosition
			});
		}
		return saveRound;
	}

	private void OnApplicationQuit()
	{
		Shader.SetGlobalFloat("_WorldSizeIncrease", 0f);
		Shader.SetGlobalFloat("_WorldSizeIncreaseNormalized", 0f);
		if (this.CurrentGameState == GameState.Playing && this.currentAnimationRoutine == null && this.currentAnimation == null)
		{
			SaveManager.instance.Save(saveRound: true);
		}
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= OnLanguageChange;
		}
	}

	public void SaveAndGoBackToMenu()
	{
		SaveManager.instance.Save(saveRound: true);
		WorldManager.RestartGame();
	}

	public void UpdateCardTargets()
	{
		foreach (CardTarget cardTarget in this.CardTargets)
		{
			if (cardTarget is BuyBoosterBox buyBoosterBox)
			{
				buyBoosterBox.UpdateUndiscoveredCards();
			}
		}
	}

	public void Play()
	{
		GameCanvas.instance.SetScreen<GameScreen>();
		this.CurrentGameState = GameState.Playing;
		GameCamera.instance.CenterOnBoard(this.CurrentBoard);
		QuestManager.instance.CheckPacksUnlocked();
		GameScreen.instance.OnBoardChange();
		this.UpdateCardTargets();
	}

	public Vector3 MiddleOfBoard()
	{
		return this.CurrentBoard.MiddleOfBoard();
	}

	public void SetViewType(ViewType type)
	{
		this.CurrentView = type;
		foreach (GameCard allCard in this.AllCards)
		{
			allCard.UpdateCardPalette();
		}
		CitiesManager.instance.StopDrawCable(null);
	}

	public void SavePreset(SavedPreset preset)
	{
		string content = JsonUtility.ToJson(preset);
		FileHelper.SavePresetFile(preset.SaveId, content);
	}

	public void StartNewRound()
	{
		this.ClearRound();
		this.CurrentBoard = this.GetBoardWithId("main");
		this.CreateBoosterpack(this.MiddleOfBoard(), "starter");
	}

	public void ClearSaveAndRestart()
	{
		SaveGame saveGame = new SaveGame();
		saveGame.LastSavedUtc = DateTime.UtcNow;
		saveGame.SaveId = this.CurrentSave.SaveId;
		string content = JsonUtility.ToJson(saveGame);
		FileHelper.SaveFile(this.CurrentSave.SaveId, content);
		SaveManager.instance.Save(saveGame);
		WorldManager.RestartGame();
	}

	public void QuestCompleted(Quest quest)
	{
		UnityEngine.Debug.Log("Completed quest " + quest.Id);
		if (GameScreen.instance != null && quest.QuestLocation == this.CurrentBoard.Location)
		{
			GameScreen.instance.AddNotification(SokLoc.Translate("label_quest_completed"), quest.Description, delegate
			{
				GameScreen.instance.ScrollToQuest(quest);
			});
		}
		AudioManager.me.PlaySound2D(AudioManager.me.QuestComplete, 1f, 0.1f);
		BoosterpackData boosterpackData = QuestManager.instance.JustUnlockedPack();
		if (boosterpackData != null)
		{
			bool flag = boosterpackData.BoosterLocation == quest.QuestLocation;
			if (boosterpackData.BoosterLocation != this.CurrentBoard.Location)
			{
				flag = false;
			}
			if (TransitionScreen.InTransition)
			{
				flag = false;
			}
			if (flag)
			{
				this.QueueCutscene(Cutscenes.JustUnlockedPack(boosterpackData));
			}
		}
	}

	public void OnLanguageChange()
	{
		foreach (GameCard allCard in this.AllCards)
		{
			allCard.CardData.OnLanguageChange();
		}
		foreach (CardData cardDataPrefab in this.GameDataLoader.CardDataPrefabs)
		{
			cardDataPrefab.OnLanguageChange();
		}
	}

	private void QueueAnimation(QueuedAnimation anim)
	{
		this.queuedAnimations.Add(anim);
	}

	private void CheckQueuedAnimations()
	{
		if (!this.InAnimation && !GameCanvas.instance.ModalIsOpen && this.IsPlaying && this.currentAnimation == null && this.queuedAnimations.Count > 0)
		{
			this.CloseOpenInventories();
			this.SetViewType(ViewType.Default);
			this.currentAnimation = this.queuedAnimations[0];
			this.currentAnimation.OnActivate();
			this.queuedAnimations.RemoveAt(0);
		}
	}

	public void QueueCutscene(IEnumerator coroutine)
	{
		this.QueueAnimation(new QueuedAnimation(delegate
		{
			base.StartCoroutine(coroutine);
		}));
	}

	public void QueueCutsceneIfNotQueued(IEnumerator coroutine, string id)
	{
		if (!this.queuedAnimations.Any((QueuedAnimation x) => x.Id == id))
		{
			this.QueueAnimation(new QueuedAnimation(delegate
			{
				base.StartCoroutine(coroutine);
			}, id));
		}
	}

	public void QueueCutsceneIfNotPlayed(string cutsceneId)
	{
		if (!this.CurrentRunVariables.PlayedCutsceneIds.Contains(cutsceneId))
		{
			this.CurrentRunVariables.PlayedCutsceneIds.Add(cutsceneId);
			this.QueueCutscene(cutsceneId);
		}
	}

	public void QueueCutscene(string cutsceneId)
	{
		ScriptableCutscene cutsceneWithId = this.GameDataLoader.GetCutsceneWithId(cutsceneId);
		this.QueueCutscene(cutsceneWithId);
	}

	public void QueueCutscene(ScriptableCutscene cutscene)
	{
		this.QueueAnimation(new QueuedAnimation(delegate
		{
			if (!this.CurrentRunVariables.PlayedCutsceneIds.Contains(cutscene.CutsceneId))
			{
				this.CurrentRunVariables.PlayedCutsceneIds.Add(cutscene.CutsceneId);
			}
			base.StartCoroutine(Cutscenes.RunScriptableCutscene(cutscene));
		}));
	}

	public void ModalAbandonCity()
	{
		GameCanvas.instance.AbandonCityPrompt(AbandonCity, null);
	}

	public void AbandonCity()
	{
		GameBoard citiesBoard = this.GetCurrentBoardSafe();
		this.GoToBoard(this.GetBoardWithId("main"), delegate
		{
			this.RemoveAllCardsFromBoard(citiesBoard.Id);
			this.ResetBoughtBoostersOnLocation(citiesBoard.Location);
			this.ResetCityVariables();
			if (this.CurrentGameState == GameState.Paused)
			{
				this.TogglePause();
			}
		}, "cities");
	}

	private void ResetCityVariables()
	{
		this.CurrentRunVariables.HasCitiesBoard = false;
		this.CurrentRunVariables.BuiltLandmarks = new List<string>();
		this.CurrentRunVariables.OpenedFirstTrash = false;
		this.BoardMonths.CitiesMonth = 1;
		CitiesManager.instance.Wellbeing = 15;
	}

	public void TogglePause()
	{
		if (this.currentAnimationRoutine == null && this.currentAnimation == null && (this.CurrentGameState == GameState.Paused || this.CurrentGameState == GameState.Playing) && !GameCanvas.instance.ModalIsOpen)
		{
			if (this.CurrentGameState == GameState.Playing)
			{
				this.CurrentGameState = GameState.Paused;
			}
			else if (this.CurrentGameState == GameState.Paused)
			{
				this.CurrentGameState = GameState.Playing;
			}
			bool num = this.CurrentGameState == GameState.Paused;
			this.SetViewType(ViewType.Default);
			if (num)
			{
				GameCanvas.instance.SetScreen<PauseScreen>();
			}
			else
			{
				GameCanvas.instance.SetScreen<GameScreen>();
			}
		}
	}

	private void CheckDisableInput()
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TMP_InputField component = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			if (component != null)
			{
				this.currentSelectedInput = component;
				InputController.instance.DisableAllInput = true;
				return;
			}
		}
		InputController.instance.DisableAllInput = false;
		this.currentSelectedInput = null;
	}

	public float DetermineTargetWorldSize(GameBoard board)
	{
		float a = Mathf.Max(0.15f, board.PackLineWidth - 8.86f);
		float num = 4.5f;
		a = Mathf.Max(a, board.BoardOptions.BaseBoardSize);
		if (board.Id == "cities")
		{
			num = 10f;
		}
		return Mathf.Clamp(Mathf.Clamp(a + (float)this.CardCapIncrease(board) * 0.05f, a, num) + (float)this.BoardSizeIncrease(board) * 0.05f, a, num + 3f);
	}

	public Vector3 ScreenPosToWorldPos(Vector3 screenPos, out Ray ray)
	{
		ray = Camera.main.ScreenPointToRay(screenPos);
		new Plane(Vector3.up, Vector3.zero).Raycast(ray, out var enter);
		return ray.origin + ray.direction * enter;
	}

	public void IncrementMonth()
	{
		this.BoardMonths.IncrementMonth();
	}

	private void Update()
	{
		if (Application.isEditor)
		{
			this.CheckDebugInput();
		}
		if (this.CurrentBoard != null && this.CurrentBoard.Id == "forest")
		{
			this.ForestMoonEnabled = true;
		}
		else
		{
			this.ForestMoonEnabled = false;
		}
		Shader.SetGlobalFloat("_GridWidth", this.GridWidth);
		Shader.SetGlobalFloat("_GridHeight", this.GridHeight);
		Shader.SetGlobalFloat("_AnimationTime", this.AnimationTime);
		if (this.CurrentBoard != null)
		{
			Shader.SetGlobalColor("_BoardBackgroundA", this.CurrentBoard.BoardOptions.CardBackgroundPallete.Color.linear);
			Shader.SetGlobalColor("_BoardBackgroundB", this.CurrentBoard.BoardOptions.CardBackgroundPallete.Color2.linear);
		}
		this.gridAlpha = Mathf.Lerp(this.gridAlpha, 0f, Time.deltaTime * 3f);
		Shader.SetGlobalFloat("_GridAlpha", this.gridAlpha);
		this.UpdatePhysics();
		this.CheckQueuedAnimations();
		this.CheckResetCanDropItem();
		this.clickStartedGrabbing = false;
		bool flag = true;
		if (this.IntroPack != null && !this.IntroPack.WasClicked)
		{
			flag = false;
		}
		if (this.currentAnimationRoutine != null || this.currentAnimation != null)
		{
			flag = false;
		}
		if (this.ForestMoonEnabled)
		{
			flag = false;
		}
		if (flag && !this.DebugEndlessMoonEnabled)
		{
			this.MonthTimer += Time.deltaTime * this.TimeScale;
		}
		this.AnimationTime += Time.deltaTime * this.TimeScale;
		if (!this.DebugEndlessMoonEnabled && this.MonthTimer >= this.MonthTime && this.currentAnimationRoutine == null)
		{
			this.MonthTimer -= this.MonthTime;
			this.IncrementMonth();
			this.EndOfMonth();
		}
		Ray ray;
		if (InputController.instance.CurrentSchemeIsController)
		{
			this.mouseWorldPosition = this.ScreenPosToWorldPos(new Vector2(Screen.width, Screen.height) * 0.5f, out ray);
		}
		else if (InputController.instance.CurrentSchemeIsTouch)
		{
			this.mouseWorldPosition = this.ScreenPosToWorldPos(InputController.instance.GetSafeTouchPosition(0), out ray);
		}
		else
		{
			this.mouseWorldPosition = this.ScreenPosToWorldPos(InputController.instance.ClampedMousePosition(), out ray);
		}
		this.CheckDisableInput();
		this.HoveredDraggable = null;
		this.HoveredInteractable = null;
		this.CurrentHoverable = null;
		this.CanUseTransport = this.GetCardCount<RoadBuilder>() > 0;
		if (InputController.instance.ToggleViewTriggered() && !this.InAnimation && !TransitionScreen.InTransition && (this.CurrentGameState == GameState.Playing || this.CurrentGameState == GameState.Paused))
		{
			if (this.CurrentBoard.Id == "cities")
			{
				int viewType = (int)((this.CurrentView == ViewType.Calamity) ? ViewType.Default : (this.CurrentView + 1));
				this.SetViewType((ViewType)viewType);
			}
			else if (this.CanUseTransport)
			{
				if (this.CurrentView == ViewType.Transport)
				{
					this.SetViewType(ViewType.Default);
				}
				else
				{
					this.SetViewType(ViewType.Transport);
				}
			}
		}
		if (this.GetCurrentBoardSafe().Id != "cities" && !this.CanUseTransport)
		{
			this.SetViewType(ViewType.Default);
		}
		if (InputController.instance.PauseTriggered())
		{
			this.TogglePause();
		}
		if (this.IsPlaying && InputController.instance.SnapCardsTriggered())
		{
			this.SnapCardsToGrid();
		}
		bool flag2 = InputController.instance.GetInput(0) && GameCanvas.instance.PositionIsOverUI(InputController.instance.GetInputPosition(0));
		bool flag3 = AccessibilityScreen.ClickToDragEnabled;
		if (InputController.instance.CurrentSchemeIsController || InputController.instance.CurrentSchemeIsTouch)
		{
			flag3 = false;
		}
		if (InputController.instance.CurrentScheme == ControlScheme.KeyboardMouse)
		{
			flag2 = GameCanvas.instance.MousePositionIsOverUI();
		}
		if (InputController.instance.CurrentSchemeIsController)
		{
			flag2 = false;
		}
		if (!flag2 && (InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsController))
		{
			int num = Physics.RaycastNonAlloc(ray, this.hits);
			float num2 = float.MaxValue;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = this.hits[i];
				if (!raycastHit.collider.gameObject.activeInHierarchy)
				{
					continue;
				}
				Draggable component = this.DraggableLookup.GetComponent(raycastHit.collider.gameObject);
				if (component != null)
				{
					float distance = raycastHit.distance;
					if (distance < num2)
					{
						num2 = distance;
						if (this.DraggingDraggable == null)
						{
							this.HoveredDraggable = component;
						}
					}
				}
				Interactable component2 = this.InteractableLookup.GetComponent(raycastHit.collider.gameObject);
				if (component2 != null)
				{
					this.HoveredInteractable = component2;
				}
				Hoverable component3 = this.HoverableLookup.GetComponent(raycastHit.collider.gameObject);
				if (component3 != null)
				{
					this.CurrentHoverable = component3;
				}
			}
		}
		if (InputController.instance.CurrentSchemeIsTouch)
		{
			int num3 = Physics.RaycastNonAlloc(ray, this.hits);
			float num4 = float.MaxValue;
			for (int j = 0; j < num3; j++)
			{
				RaycastHit raycastHit2 = this.hits[j];
				if (!raycastHit2.collider.gameObject.activeInHierarchy)
				{
					continue;
				}
				Draggable component4 = this.DraggableLookup.GetComponent(raycastHit2.collider.gameObject);
				if (component4 != null)
				{
					float distance2 = raycastHit2.distance;
					if (distance2 < num4)
					{
						num4 = distance2;
						if (this.DraggingDraggable == null)
						{
							this.HoveredDraggable = component4;
						}
					}
				}
				Interactable component5 = this.InteractableLookup.GetComponent(raycastHit2.collider.gameObject);
				if (component5 != null)
				{
					this.HoveredInteractable = component5;
				}
				Hoverable component6 = this.HoverableLookup.GetComponent(raycastHit2.collider.gameObject);
				if (component6 != null)
				{
					this.CurrentHoverable = component6;
				}
			}
		}
		if (this.HoveredInteractable != null)
		{
			GameScreen.InfoBoxTitle = this.HoveredInteractable.name;
			GameScreen.InfoBoxText = this.HoveredInteractable.GetTooltipText();
		}
		if (this.CurrentHoverable != null)
		{
			GameScreen.InfoBoxTitle = this.CurrentHoverable.GetTitle();
			GameScreen.InfoBoxText = this.CurrentHoverable.GetDescription();
		}
		if (this.CanInteract || this.ConnectConnectors)
		{
			if (this.ConnectConnectors && this.HoveredDraggable != null && !(this.HoveredDraggable is CardConnector))
			{
				return;
			}
			bool flag4 = InputController.instance.StartedGrabbing();
			if ((InputController.instance.GetInputBegan(0) || flag4) && !flag2)
			{
				if (this.HoveredDraggable != null)
				{
					if (this.HoveredDraggable.CanBeDragged())
					{
						Draggable draggable = this.HoveredDraggable;
						draggable.DragTag = null;
						draggable.ClickedObject = null;
						if (this.HoveredDraggable is GameCard gameCard && InputController.instance.GetKey(Key.LeftShift))
						{
							this.IsShiftDragging = true;
							draggable = gameCard.GetRootCard();
							draggable.ClickedObject = gameCard;
						}
						else
						{
							this.IsShiftDragging = false;
						}
						this.DraggingDraggable = draggable;
						this.DraggingDraggable.DragStartPosition = this.DraggingDraggable.transform.position;
						this.grabOffset = this.mouseWorldPosition - this.DraggingDraggable.transform.position;
						this.DraggingDraggable.StartDragging();
						if (flag3)
						{
							this.clickStartedGrabbing = true;
						}
					}
					else
					{
						this.HoveredDraggable.Clicked();
						GameCard gameCard2 = this.HoveredDraggable as GameCard;
						if (gameCard2 != null)
						{
							gameCard2.RotWobble(1f);
						}
					}
				}
				else if (this.HoveredInteractable != null)
				{
					this.HoveredInteractable.Click();
				}
				else if ((InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsTouch) && !InputController.instance.GetRightMouseBegan())
				{
					GameCamera.instance.StartDragging();
				}
				else
				{
					GameCamera.instance.Clicked();
				}
			}
			if (InputController.instance.ToggleInventoryTriggered() && !flag2 && this.HoveredCard != null && this.HoveredCard.CardData.HasInventory)
			{
				this.HoveredCard.ShowInventory = !this.HoveredCard.ShowInventory;
			}
			if (InputController.instance.SellTriggered() && !flag2 && this.HoveredCard != null && this.CardCanBeSold(this.HoveredCard))
			{
				this.SellCard(this.HoveredCard.transform.position, this.HoveredCard.GetRootCard());
			}
		}
		else if (this.CutsceneBoardView)
		{
			bool flag5 = InputController.instance.StartedGrabbing();
			if ((InputController.instance.GetInputBegan(0) || flag5) && !flag2 && this.HoveredDraggable == null)
			{
				if ((InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsTouch) && !InputController.instance.GetRightMouseBegan())
				{
					GameCamera.instance.StartDragging();
				}
				else
				{
					GameCamera.instance.Clicked();
				}
			}
		}
		else if ((InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsTouch) && InputController.instance.GetInputBegan(0) && !flag2 && !this.InAnimation)
		{
			GameCamera.instance.StartDragging();
		}
		bool flag6 = false;
		if (InputController.instance.CurrentSchemeIsController && AccessibilityScreen.AutoPauseWhenUsingController)
		{
			flag6 = true;
		}
		if (InputController.instance.CurrentScheme == ControlScheme.KeyboardMouse && AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse)
		{
			flag6 = true;
		}
		if (flag6)
		{
			bool flag7 = this.DraggingDraggable != null;
			if (flag7 && !this.isAutoPaused)
			{
				this.isAutoPaused = true;
				this.preAutoPauseSpeed = this.SpeedUp;
				this.SpeedUp = 0f;
			}
			if (this.isAutoPaused && !flag7)
			{
				this.isAutoPaused = false;
				this.SpeedUp = this.preAutoPauseSpeed;
			}
		}
		this.NearbyCardTarget = null;
		float num5 = float.MaxValue;
		if (this.DraggingDraggable != null)
		{
			this.DraggingDraggable.TargetPosition = this.mouseWorldPosition - this.grabOffset;
			if (this.DraggingCard != null)
			{
				this.DraggingCard.Clampieee();
			}
		}
		if (this.DraggingCard != null)
		{
			foreach (CardTarget cardTarget in this.CardTargets)
			{
				if (cardTarget.CanHaveCard(this.DraggingCard))
				{
					float b = Vector3.Distance(this.DraggingCard.TargetPosition + this.grabOffset, cardTarget.transform.position);
					b = Mathf.Min(Vector3.Distance(this.DraggingCard.TargetPosition, cardTarget.transform.position), b);
					if (b < this.CardTargetSnapDistance && b < num5)
					{
						this.NearbyCardTarget = cardTarget;
						this.DraggingCard.TargetPosition = cardTarget.transform.position;
					}
				}
			}
		}
		if (!this.CanInteract && this.DraggingDraggable != null)
		{
			this.DraggingDraggable.StopDragging();
			this.SetDraggingDraggableToNull();
		}
		bool flag8 = InputController.instance.StoppedGrabbing();
		if (InputController.instance.GetInputEnded(0) || flag8 || !this.CanInteract)
		{
			if (flag3)
			{
				if (InputController.instance.GetLeftMouseEnded())
				{
					this.DropCard();
				}
			}
			else
			{
				this.DropCard();
			}
		}
		if (flag3 && InputController.instance.GetRightMouseBegan() && !this.clickStartedGrabbing)
		{
			this.DropCard();
		}
		if (this.CurrentGameState == GameState.Playing && this.currentAnimationRoutine == null && this.currentAnimation == null && this.IntroPack == null && !TransitionScreen.InTransition)
		{
			if (this.CheckAllVillagersDead())
			{
				if (this.CurrentBoard.Id == "main")
				{
					this.CurrentGameState = GameState.GameOver;
					GameCanvas.instance.SetScreen<GameOverScreen>();
				}
				else if (this.CurrentBoard.Id == "island")
				{
					this.QueueCutscene(Cutscenes.EveryoneOnIslandDead());
				}
				else if (this.CurrentBoard.BoardOptions.IsSpiritWorld)
				{
					this.QueueCutscene(Cutscenes.EveryoneInSpiritWorldDead(this.CurrentBoard.Id));
				}
				else if (!(this.CurrentBoard.Id == "forest") && !(this.CurrentBoard.Id == "cities"))
				{
					this.QueueCutscene(Cutscenes.EveryoneOnIslandDead());
				}
			}
			CitiesManager.instance.CheckCityHealth();
		}
		if (this.TimeScale > 0f)
		{
			this.CheckSpiritCutscenes();
		}
		this.DebugUpdate();
	}

	public void SetDraggingDraggableToNull()
	{
		this.DraggingDraggable = null;
		foreach (Draggable allDraggable in this.AllDraggables)
		{
			allDraggable.BeingDragged = false;
		}
	}

	public Sprite GetCurrencyIcon(BoardCurrency? currency)
	{
		if (currency.HasValue)
		{
			if (currency == BoardCurrency.Gold)
			{
				return SpriteManager.instance.CoinIcon;
			}
			if (currency == BoardCurrency.Shell)
			{
				return SpriteManager.instance.ShellIcon;
			}
			if (currency == BoardCurrency.Dollar)
			{
				return SpriteManager.instance.DollarIcon;
			}
		}
		return SpriteManager.instance.CoinIcon;
	}

	private void CheckResetCanDropItem()
	{
		if (!(this.CurrentBoard == null) && this.CurrentBoard.BoardOptions.ResetItemDrops && !this.CurrentRunVariables.CanDropItem && this.GetCardCount((CardData x) => x is Enemy) == 0)
		{
			this.CurrentRunVariables.CanDropItem = true;
		}
	}

	private void DropCard()
	{
		if (this.DraggingCard != null)
		{
			if (this.NearbyCardTarget != null)
			{
				this.NearbyCardTarget.CardDropped(this.DraggingCard);
			}
			else
			{
				this.CheckIfCanAddOnStack(this.DraggingCard);
			}
		}
		if (this.DraggingDraggable != null)
		{
			this.DraggingDraggable.StopDragging();
			this.SetDraggingDraggableToNull();
		}
	}

	public void OnBoosterOpened(string boosterId)
	{
		if (this.CurrentBoard.Id == "greed" && boosterId == "greed_intro")
		{
			this.QueueCutscene(Cutscenes.GreedIntro());
		}
		else if (this.CurrentBoard.Id == "happiness" && boosterId == "happiness_intro")
		{
			this.QueueCutscene(Cutscenes.HappinessIntro());
		}
		else if (this.CurrentBoard.Id == "death" && boosterId == "death_intro")
		{
			this.QueueCutscene(Cutscenes.DeathIntro());
		}
		else if (this.CurrentBoard.Id == "cities" && boosterId == "cities_intro")
		{
			this.QueueCutscene("cities_start");
		}
	}

	public void CloseOpenInventories()
	{
		foreach (GameCard allCard in this.AllCards)
		{
			allCard.ShowInventory = false;
		}
	}

	private void UpdatePhysics()
	{
		this.physicsTimer += Time.deltaTime * this.PhysicsTimeScale;
		while (this.physicsTimer >= 0.02f)
		{
			this.physicsTimer -= 0.02f;
			foreach (Draggable physicsDraggable in this.PhysicsDraggables)
			{
				physicsDraggable.UpdatePhysics(0.02f);
			}
		}
	}

	public void SnapCardsToGrid()
	{
		this.gridAlpha = 1f;
		foreach (GameCard allCard in this.AllCards)
		{
			if (!allCard.HasParent && !(allCard.CardData is Mob))
			{
				Vector3 position = allCard.transform.position;
				position.x = (float)Mathf.RoundToInt(position.x / this.GridWidth) * this.GridWidth;
				position.z = (float)Mathf.RoundToInt(position.z / this.GridHeight) * this.GridHeight;
				allCard.TargetPosition = position;
			}
		}
	}

	public void DissolveStack(GameCard card)
	{
		GameCard gameCard = card.GetLeafCard();
		while (gameCard != null)
		{
			GameCard parent = gameCard.Parent;
			gameCard.RemoveFromStack();
			gameCard = parent;
		}
	}

	public bool CardCanBeSold(GameCard card, bool checkStatus = true, bool checkSpeedup = false)
	{
		if (checkSpeedup && this.SpeedUp == 0f)
		{
			return false;
		}
		if (card.IsEquipped || card.IsWorking)
		{
			return false;
		}
		if (card.MyBoard.Id == "forest")
		{
			return false;
		}
		if (card.WorkerChildren.Count > 0)
		{
			return false;
		}
		if (card.InConflict)
		{
			return false;
		}
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		if (allCardsInStack.Any((GameCard x) => x.CardData is ResourceChest resourceChest && resourceChest.ResourceCount > 0))
		{
			return false;
		}
		if (allCardsInStack.Any((GameCard x) => x.CardData is Chest chest && chest.CoinCount > 0))
		{
			return false;
		}
		if (allCardsInStack.Any((GameCard x) => x.CardData is Creditcard creditcard && creditcard.DollarCount > 0))
		{
			return false;
		}
		if (allCardsInStack.Any((GameCard x) => x.CardData is FoodWarehouse foodWarehouse && foodWarehouse.FoodValue > 0))
		{
			return false;
		}
		if (checkStatus)
		{
			GameCard cardWithStatusInStack = card.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && !(cardWithStatusInStack.CardData is EnergyGenerator))
			{
				return false;
			}
		}
		return !allCardsInStack.Any((GameCard x) => x.CardData.GetValue() == -1);
	}

	public void ClearRoundAndRestart()
	{
		this.CurrentSave.LastPlayedRound = null;
		SaveManager.instance.Save(saveRound: false);
		WorldManager.RestartGame();
	}

	public static void RestartGame()
	{
		SaveManager.instance.Load();
		WorldManager.instance.CurrentGameState = GameState.InMenu;
		WorldManager.instance.SpeedUp = 1f;
		GameCamera.instance.PauseVolume.enabled = false;
		GameCamera.instance.PauseVolume.gameObject.SetActive(value: false);
		GameCanvas.instance.SetScreen<MainMenu>();
		GameCamera.instance.OnRestartGame();
		CardopediaScreen.instance.RefreshCardopedia();
		QuestManager.instance.UpdateCurrentQuests();
		foreach (BuyBoosterBox allBoosterBox in WorldManager.instance.AllBoosterBoxes)
		{
			allBoosterBox.UpdateUndiscoveredCards();
		}
		WorldManager.instance.ClearRound();
		Shader.SetGlobalFloat("_WorldSizeIncrease", 0f);
		Shader.SetGlobalFloat("_WorldSizeIncreaseNormalized", 0f);
		WorldManager.CheckForceReloadSave();
	}

	private static void CheckForceReloadSave()
	{
		if (SaveManager.IsForceReload)
		{
			SaveManager.IsForceReload = false;
			WorldManager.instance.LoadPreviousRound();
			WorldManager.instance.Play();
		}
	}

	public static void RebootGame()
	{
		UnityEngine.Debug.Log("attempting to reboot game..");
		if (!PlatformHelper.UseSteam)
		{
			Application.quitting += RelaunchProcess;
			Application.Quit();
		}
		else if (SteamManager.Initialized)
		{
			Application.quitting += RelaunchSteam;
			Application.Quit();
		}
		else
		{
			UnityEngine.Debug.Log("cant figure out how to reboot game");
		}
	}

	private static void RelaunchProcess()
	{
		string fileName = ((Application.platform != RuntimePlatform.OSXPlayer) ? Path.Combine(Application.dataPath, "..", "Stacklands.exe") : Path.Combine(Application.dataPath, "MacOS/Stacklands"));
		Process.Start(fileName);
	}

	private static void RelaunchSteam()
	{
		Application.OpenURL("steam://rungameid/1948280");
	}

	public GameCard CreateCardStack(Vector3 pos, int amount, string cardId, bool checkAddToStack = true)
	{
		if (amount == 0)
		{
			return null;
		}
		GameCard gameCard = null;
		while (amount > 0)
		{
			int num = Mathf.Min(amount, 30);
			gameCard = null;
			for (int i = 0; i < num; i++)
			{
				GameCard myGameCard = this.CreateCard(pos, cardId, faceUp: true, checkAddToStack).MyGameCard;
				if (gameCard != null)
				{
					myGameCard.SetParent(gameCard);
				}
				gameCard = myGameCard;
			}
			amount -= num;
		}
		return gameCard;
	}

	public void StartCursePlaythrough(CurseType curse, Action onArrive)
	{
		GameBoard newBoard = null;
		switch (curse)
		{
		case CurseType.Happiness:
			newBoard = this.GetBoardWithId("happiness");
			break;
		case CurseType.Death:
			newBoard = this.GetBoardWithId("death");
			break;
		case CurseType.Greed:
			newBoard = this.GetBoardWithId("greed");
			break;
		}
		GameCanvas.instance.SetScreen<EmptyScreen>();
		this.GoToBoard(newBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			onArrive();
		}, "spirit");
	}

	public List<GameCard> CreateDollarsFromValue(int value, Vector3 pos, bool checkAddToStack = true)
	{
		if (value <= 0)
		{
			return new List<GameCard>();
		}
		int result = 0;
		int amount = Math.DivRem(value, 100, out result);
		GameCard gameCard = this.CreateCardStack(pos, amount, "hundred_dollar", checkAddToStack: false);
		amount = Math.DivRem(result, 50, out result);
		GameCard gameCard2 = this.CreateCardStack(pos, amount, "fifty_dollar", checkAddToStack: false);
		amount = Math.DivRem(result, 20, out result);
		GameCard gameCard3 = this.CreateCardStack(pos, amount, "twenty_dollar", checkAddToStack: false);
		amount = Math.DivRem(result, 10, out result);
		GameCard gameCard4 = this.CreateCardStack(pos, amount, "ten_dollar", checkAddToStack: false);
		List<GameCard> list = new List<GameCard>();
		if (gameCard != null)
		{
			list.AddRange(gameCard.GetAllCardsInStack());
		}
		if (gameCard2 != null)
		{
			list.AddRange(gameCard2.GetAllCardsInStack());
		}
		if (gameCard3 != null)
		{
			list.AddRange(gameCard3.GetAllCardsInStack());
		}
		if (gameCard4 != null)
		{
			list.AddRange(gameCard4.GetAllCardsInStack());
		}
		this.Restack(list);
		if (checkAddToStack)
		{
			this.CheckIfCanAddOnStack(list.First());
		}
		else
		{
			list.First().SendIt();
		}
		return list;
	}

	public GameCard SellCard(Vector3 pos, GameCard card, float multiplier = 1f, bool checkAddToStack = true)
	{
		if (card.CardData.Id == "coin_chest" || card.CardData.Id == "shell_chest")
		{
			multiplier = 1f;
		}
		CardValue stackValue = this.GetStackValue(card);
		bool flag = false;
		foreach (GameCard item in card.GetAllCardsInStack())
		{
			if (item.CardData != null)
			{
				item.CardData.OnSellCard();
			}
			if (item.CardData is Worker)
			{
				flag = true;
			}
			this.CreateSmoke(item.transform.position);
		}
		this.DestroyStack(card);
		GameCard gameCard = null;
		if (flag)
		{
			QuestManager.instance.SpecialActionComplete("worker_removed");
		}
		if (this.CurrentBoard.Id == "cities")
		{
			if (stackValue.TotalValue >= 10)
			{
				List<GameCard> list = this.CreateDollarsFromValue(stackValue.TotalValue, pos);
				gameCard = list?.FirstOrDefault();
				if (list != null)
				{
					Dollar dollar = null;
					for (int i = 0; i < list.Count; i++)
					{
						Dollar dollar2 = list[i].CardData as Dollar;
						if (dollar == null || dollar2.DollarValue > dollar.DollarValue)
						{
							dollar = dollar2;
						}
					}
					if (dollar != null)
					{
						AudioManager.me.PlaySound2D(dollar.PickupSound, UnityEngine.Random.Range(0.8f, 1.2f), 0.8f);
					}
				}
			}
		}
		else
		{
			string cardId = (this.CurrentBoard.BoardOptions.UsesShells ? "shell" : "gold");
			gameCard = this.CreateCardStack(pos, Mathf.CeilToInt(multiplier * (float)stackValue.TotalValue), cardId, checkAddToStack);
			if (gameCard != null)
			{
				QuestManager.instance.SpecialActionComplete("sell_card", gameCard.CardData);
				AudioManager.me.PlaySound2D(AudioManager.me.Coin, UnityEngine.Random.Range(0.8f, 1.2f), 0.8f);
			}
		}
		return gameCard;
	}

	public void DestroyStack(GameCard card)
	{
		GameCard gameCard = card;
		while (gameCard != null)
		{
			gameCard.Destroyed = true;
			this.AllCards.Remove(card);
			this.UniqueIdToCard.Remove(card.CardData.UniqueId);
			UnityEngine.Object.Destroy(gameCard.gameObject);
			gameCard = gameCard.Child;
		}
	}

	public Vector3 GetRandomSpawnPosition()
	{
		Bounds worldBounds = this.CurrentBoard.WorldBounds;
		float x = Mathf.Lerp(worldBounds.min.x, worldBounds.max.x, UnityEngine.Random.Range(0.1f, 0.9f));
		float z = Mathf.Lerp(worldBounds.min.z, worldBounds.max.z, UnityEngine.Random.Range(0.1f, 0.9f));
		return new Vector3(x, 0f, z);
	}

	private void DebugUpdate()
	{
		bool flag = false;
		if (Application.isEditor && InputController.instance.GetKeyDown(Key.F1))
		{
			flag = true;
		}
		if (!Application.isEditor && InputController.instance.GetKeyDown(Key.F1) && InputController.instance.GetKey(Key.K) && InputController.instance.GetKey(Key.O))
		{
			flag = true;
		}
		if (flag)
		{
			GameScreen.instance?.DebugScreen?.gameObject.SetActive(!GameScreen.instance.DebugScreen.gameObject.activeInHierarchy);
			this.DebugScreenOpened = true;
		}
		if (this.DebugScreenOpened && InputController.instance.GetKeyDown(Key.F5))
		{
			GameCanvas.instance.gameObject.SetActive(!GameCanvas.instance.gameObject.activeInHierarchy);
		}
	}

	public void CheckStackOrders()
	{
		if (this.validator == null)
		{
			this.validator = new GameDataValidator(this.GameDataLoader);
		}
		this.validator.CheckStackOrders();
	}

	public void SpawnAndDestroyEveryCard()
	{
		base.StartCoroutine(this.SpawnAndDestroyCards());
	}

	private IEnumerator SpawnAndDestroyCards()
	{
		foreach (CardData cardDataPrefab in this.CardDataPrefabs)
		{
			CardData card = this.CreateCard(this.GetRandomSpawnPosition(), cardDataPrefab, faceUp: true);
			yield return null;
			card.MyGameCard.DestroyCard();
		}
	}

	public void GoToBoard(GameBoard newBoard, Action onComplete = null, string transitionId = "default")
	{
		if (newBoard.BoardOptions.IsSpiritWorld || this.CurrentBoard.BoardOptions.IsSpiritWorld)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.SpiritTransitionEnter, 1f, 0.5f);
		}
		else if (newBoard.Id == "cities" || this.CurrentBoard.Id == "cities")
		{
			AudioManager.me.PlaySound2D(AudioManager.me.CitiesTransitionEnter, 1f, 0.5f);
		}
		TransitionScreen.instance.StartTransition(delegate
		{
			if (this.DraggingDraggable != null)
			{
				this.DraggingDraggable.StopDragging();
				this.SetDraggingDraggableToNull();
			}
			GameBoard currentBoard = this.CurrentBoard;
			this.CurrentRunVariables.PreviouseBoard = currentBoard.Id;
			this.CurrentBoard = newBoard;
			QuestManager.instance.SpecialActionComplete("board_" + this.CurrentBoard.Id);
			if (this.CurrentBoard.Id == "island")
			{
				if (!this.CurrentRunVariables.VisitedIsland)
				{
					this.QueueCutscene(Cutscenes.IslandIntro());
				}
				this.CurrentRunVariables.VisitedIsland = true;
				this.CheckSpawnIslandBooster();
			}
			if (this.CurrentBoard.Id == "cities" && !this.CurrentRunVariables.HasCitiesBoard)
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "cities_intro");
				this.CurrentRunVariables.HasCitiesBoard = true;
				CitiesManager.instance.Wellbeing = CitiesManager.instance.WellbeingStart;
			}
			if (this.CurrentBoard.Id == "greed")
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "greed_intro");
			}
			if (this.CurrentBoard.Id == "happiness")
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "happiness_intro");
			}
			if (this.CurrentBoard.Id == "death")
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "death_intro");
			}
			onComplete?.Invoke();
			if (newBoard.BoardOptions.IsSpiritWorld || this.CurrentBoard.BoardOptions.IsSpiritWorld)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.SpiritTransitionExit, 1f, 0.5f);
			}
			else if (newBoard.Id == "cities" || this.CurrentBoard.Id == "cities")
			{
				AudioManager.me.PlaySound2D(AudioManager.me.CitiesTransitionExit, 1f, 0.5f);
			}
			GameScreen.instance.OnBoardChange();
			GameScreen.instance.UpdateQuestLog();
			GameScreen.instance.UpdateIdeasLog();
			GameScreen.instance.SetQuestTab();
			this.CurrentRunVariables.LastGoToBoardMonth = this.CurrentMonth;
			if (currentBoard.Id != "forest" && this.CurrentBoard.Id != "forest")
			{
				this.MonthTimer = 0f;
			}
			this.SpeedUp = 1f;
			if (currentBoard.Id == "cities" && !this.HasFoundCard("blueprint_road_builder"))
			{
				this.CreateCard(this.CurrentBoard.MiddleOfBoard(), "blueprint_road_builder");
			}
			GameCamera.instance.CenterOnBoard(this.CurrentBoard);
			QuestManager.instance.CheckPacksUnlocked();
			this.SetViewType(ViewType.Default);
			SaveManager.instance.Save(saveRound: true);
		}, transitionId, 2f);
	}

	private void CreateBoosterIfNotExists(Vector3 pos, string boosterId)
	{
		if (this.AllBoosters.Count((Boosterpack x) => x.BoosterId == boosterId && x.MyBoard.IsCurrent) <= 0)
		{
			this.CreateBoosterpack(pos, boosterId);
		}
	}

	private void CheckSpawnIslandBooster()
	{
		if (!(this.CurrentBoard.Id != "island") && this.GetCardCount() == 0 && this.AllBoosters.Count((Boosterpack x) => x.MyBoard.Id == "island") == 0)
		{
			this.CreateBoosterpack(this.CurrentBoard.NormalizedPosToWorldPos(new Vector2(0.6f, 0.5f)), "island1");
		}
	}

	public void SendStackToBoard(GameCard rootCard, GameBoard newBoard, Vector2 normalizedPos)
	{
		rootCard = rootCard.GetRootCard();
		this.SendToBoard(rootCard, newBoard, normalizedPos);
	}

	public void SendToBoard(GameCard rootCard, GameBoard newBoard, Vector2 normalizedPos)
	{
		rootCard.MyBoard = newBoard;
		foreach (GameCard childCard in rootCard.GetChildCards())
		{
			childCard.MyBoard = newBoard;
		}
		foreach (GameCard item in rootCard.GetAllCardsInStack())
		{
			foreach (GameCard equipmentChild in item.EquipmentChildren)
			{
				equipmentChild.MyBoard = newBoard;
			}
		}
		rootCard.transform.position = (rootCard.TargetPosition = newBoard.NormalizedPosToWorldPos(normalizedPos));
		rootCard.UpdateChildPositions(hardSetPos: true);
	}

	public void Restack(List<GameCard> cards)
	{
		foreach (GameCard card in cards)
		{
			card.RemoveFromStack();
		}
		for (int i = 0; i < cards.Count; i++)
		{
			if (i > 0)
			{
				cards[i].SetParent(cards[i - 1]);
			}
		}
	}

	public bool CheckIfCanAddOnStack(GameCard topCard)
	{
		List<GameCard> overlappingCards = topCard.GetOverlappingCards();
		float num = float.MaxValue;
		GameCard gameCard = null;
		foreach (GameCard item in overlappingCards)
		{
			if (item == topCard || item.IsChildOf(topCard))
			{
				continue;
			}
			bool num2 = topCard == item.removedChild;
			GameCard leafCard = item.GetLeafCard();
			if (!num2)
			{
				GameCard cardWithStatusInStack = leafCard.GetCardWithStatusInStack();
				if (cardWithStatusInStack != null && !cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus())
				{
					continue;
				}
			}
			if (leafCard.CardData.CanHaveCardOnTop(topCard.CardData))
			{
				Vector3 vector = topCard.transform.position - item.transform.position;
				vector.y = 0f;
				if (vector.magnitude < num)
				{
					gameCard = leafCard;
					num = vector.magnitude;
				}
			}
		}
		if (gameCard != null)
		{
			topCard.SetParent(gameCard);
			return true;
		}
		return false;
	}

	public GameBoard GetCurrentBoardSafe()
	{
		if (this.CurrentBoard != null)
		{
			return this.CurrentBoard;
		}
		if (this.IsCitiesDlcActive())
		{
			return this.GetBoardWithId("cities");
		}
		if (this.IsSpiritDlcActive())
		{
			return this.GetBoardWithId("death");
		}
		return this.GetBoardWithId("main");
	}

	public int GetCardCount(string id)
	{
		return this.GetCardCount(id, this.CurrentBoard);
	}

	public int GetCardCount(string id, GameBoard board)
	{
		int num = 0;
		for (int num2 = this.AllCards.Count - 1; num2 >= 0; num2--)
		{
			GameCard gameCard = this.AllCards[num2];
			if (!(gameCard.MyBoard != board) && gameCard.CardData.Id == id)
			{
				num++;
			}
		}
		return num;
	}

	public int GetCardCountWithChest(string id)
	{
		return this.GetCardCountWithChest(id, this.CurrentBoard);
	}

	public int GetCardCountWithChest(string id, GameBoard board)
	{
		int num = 0;
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard != board))
			{
				if (allCard.CardData.Id == id)
				{
					num++;
				}
				if (allCard.CardData is ResourceChest resourceChest && resourceChest.HeldCardId == id)
				{
					num += resourceChest.ResourceCount;
				}
			}
		}
		return num;
	}

	public int GetCardCount(Predicate<CardData> pred)
	{
		int num = 0;
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard != this.CurrentBoard) && pred(allCard.CardData))
			{
				num++;
			}
		}
		return num;
	}

	public int GetCardCountInStack(GameCard card, Predicate<CardData> pred)
	{
		int num = 0;
		foreach (GameCard item in card.GetAllCardsInStack())
		{
			if (pred(item.CardData))
			{
				num++;
			}
		}
		return num;
	}

	public CardData GetCardPrefab(string id, bool showError = true)
	{
		return this.GameDataLoader.GetCardFromId(id, showError);
	}

	public T GetCardPrefab<T>(string id, bool showError = true) where T : CardData
	{
		CardData cardFromId = this.GameDataLoader.GetCardFromId(id, showError);
		if (!(cardFromId is T))
		{
			UnityEngine.Debug.LogError($"Card {id} is not of type {typeof(T)}");
			return null;
		}
		return cardFromId as T;
	}

	public BoosterpackData GetBoosterData(string boosterId)
	{
		return this.GameDataLoader.GetBoosterData(boosterId);
	}

	public Boosterpack CreateBoosterpack(Vector3 position, string boosterId)
	{
		Boosterpack boosterpack = UnityEngine.Object.Instantiate(PrefabManager.instance.BoosterpackPrefab);
		BoosterpackData boosterpackData = (boosterpack.PackData = UnityEngine.Object.Instantiate(this.GetBoosterData(boosterId)));
		boosterpack.transform.position = position;
		boosterpack.MyBoard = this.CurrentBoard;
		if (!this.CurrentSave.FoundBoosterIds.Contains(boosterId))
		{
			this.CurrentSave.FoundBoosterIds.Add(boosterId);
		}
		foreach (BoosterAddition boosterAddition in boosterpackData.BoosterAdditions)
		{
			if (boosterAddition.Filter.IsMet())
			{
				boosterpackData.CardBags.AddRange(boosterAddition.CardBags);
			}
		}
		boosterpack.TotalCardsInPack = boosterpackData.CardBags.Sum((CardBag x) => x.CardsInPack);
		return boosterpack;
	}

	public void StackSend(GameCard myCard, Vector3 outputDirection, GameCard initialParent = null, bool sendToChest = true)
	{
		if (this.TrySendToMagnet(myCard) || (sendToChest && this.TrySendToChest(myCard)) || myCard.BounceTarget != null)
		{
			return;
		}
		GameCard gameCard = null;
		float num = float.MaxValue;
		Vector3 value = Vector3.zero;
		foreach (GameCard allCard in this.AllCards)
		{
			if (!allCard.MyBoard.IsCurrent || allCard == myCard)
			{
				continue;
			}
			GameCard cardWithStatusInStack = allCard.GetCardWithStatusInStack();
			if ((!(cardWithStatusInStack != null) || cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus()) && !(allCard.GetCardInCombatInStack() != null) && !allCard.BeingDragged && !allCard.IsChildOf(myCard) && !allCard.IsParentOf(myCard) && (!(initialParent != null) || (!allCard.IsChildOf(initialParent) && !(allCard == initialParent))) && !allCard.HasChild && allCard.CardData.CanHaveCardOnTop(myCard.CardData) && allCard.CardData.Id == myCard.CardData.Id)
			{
				Vector3 vector = allCard.transform.position - myCard.transform.position;
				vector.y = 0f;
				if (vector.magnitude <= 2f && vector.magnitude <= num)
				{
					gameCard = allCard;
					num = vector.magnitude;
					value = new Vector3(vector.x * 4f, 7f, vector.z * 4f);
				}
			}
		}
		if (gameCard != null)
		{
			myCard.BounceTarget = gameCard;
			myCard.Velocity = value;
		}
		else
		{
			myCard.SendIt();
		}
	}

	public void StackSendCheckTarget(GameCard origin, GameCard myCard, Vector3 outputDirection, GameCard initialParent = null, bool sendToChest = true, int outputIndex = -1)
	{
		if (!this.TrySendWithPipe(myCard, origin, outputIndex) && !this.TrySendToMagnet(myCard) && !(myCard.BounceTarget != null))
		{
			this.StackSend(myCard, outputDirection, initialParent, sendToChest);
		}
	}

	public GameCard GetTargetCard(GameCard origin, CardData inputCardDataPrefab, Vector3 direction, bool allowDraggedCards, GameCard inputCard = null)
	{
		return this.GetBestCardInDirection(origin, direction, allowDraggedCards, delegate(GameCard gameCard)
		{
			if (inputCard != null && gameCard == inputCard)
			{
				return false;
			}
			if (gameCard == inputCardDataPrefab)
			{
				return false;
			}
			if (!this.OutputCardAllowed(gameCard, inputCardDataPrefab))
			{
				return false;
			}
			if (inputCardDataPrefab.MyGameCard != null && inputCardDataPrefab.MyGameCard == gameCard)
			{
				return false;
			}
			return !gameCard.IsPartOfSameStack(origin);
		});
	}

	public GameCard GetBestCardInDirection(GameCard origin, Vector3 direction, bool allowDraggedCards, Func<GameCard, bool> pred)
	{
		Vector3 position = origin.transform.position;
		float num = float.MinValue;
		GameCard result = null;
		float num2 = float.MaxValue;
		foreach (GameCard item in this.AllCards.Where((GameCard x) => x.MyBoard == this.CurrentBoard))
		{
			if (item == origin || (!allowDraggedCards && item.BeingDragged) || !pred(item))
			{
				continue;
			}
			Vector3 rhs = item.transform.position - position;
			float num3 = Vector3.Dot(direction, rhs);
			if (num3 <= 0f)
			{
				continue;
			}
			float num4 = num3 / rhs.sqrMagnitude;
			if (num4 > 0.5f && num4 > num)
			{
				num = num4;
				Vector3 vector = item.transform.position - position;
				vector.y = 0f;
				if (vector.magnitude <= 2f && vector.magnitude <= num2)
				{
					result = item;
					num2 = vector.magnitude;
				}
			}
		}
		return result;
	}

	private bool OutputCardAllowed(GameCard gameCard, CardData inputCardDataPrefab)
	{
		if (gameCard.CardData.Id == "heavy_foundation")
		{
			return false;
		}
		if (gameCard.Velocity.HasValue || gameCard.BounceTarget != null)
		{
			return false;
		}
		if (gameCard.HasChild)
		{
			return false;
		}
		if (!gameCard.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (gameCard.MyBoard == null)
		{
			UnityEngine.Debug.Log(gameCard?.ToString() + " does not have a board");
			return false;
		}
		if (!gameCard.MyBoard.IsCurrent)
		{
			return false;
		}
		try
		{
			if (!gameCard.CardData.CanHaveCardOnTop(inputCardDataPrefab, isPrefab: true))
			{
				return false;
			}
		}
		catch (Exception message)
		{
			if (Application.isEditor)
			{
				UnityEngine.Debug.LogError(message);
			}
			return false;
		}
		return true;
	}

	public void StackSendTo(GameCard myCard, GameCard target)
	{
		Vector3 vector = target.transform.position - myCard.transform.position;
		vector.y = 0f;
		Vector3 value = new Vector3(vector.x * 4f, 7f, vector.z * 4f);
		if (target.GetChildCount() > 0)
		{
			target = target.GetChildCards().Last();
		}
		if (target != null && target.CardData.CanHaveCardOnTop(myCard.CardData))
		{
			myCard.BounceTarget = target.GetRootCard();
			myCard.Velocity = value;
		}
		else
		{
			myCard.SendIt();
		}
	}

	public CardData CreateCard(Vector3 position, ICardId cardId, bool faceUp = true, bool checkAddToStack = true, bool playSound = true)
	{
		if (cardId is CardIdWithEquipment cardIdWithEquipment)
		{
			Combatable combatable = (Combatable)this.CreateCard(position, cardId.Id, faceUp, checkAddToStack, playSound);
			{
				foreach (string item in cardIdWithEquipment.Equipment)
				{
					combatable.CreateAndEquipCard(item, markAsFound: false);
				}
				return combatable;
			}
		}
		return this.CreateCard(position, cardId.Id, faceUp, checkAddToStack, playSound);
	}

	public CardData CreateCard(Vector3 position, string cardId, bool faceUp = true, bool checkAddToStack = true, bool playSound = true)
	{
		CardData cardPrefab = this.GetCardPrefab(cardId);
		Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.001f;
		position += new Vector3(vector.x, 0f, vector.y);
		return this.CreateCard(position, cardPrefab, faceUp, checkAddToStack, playSound);
	}

	private string GetUniqueId()
	{
		return Guid.NewGuid().ToString().Substring(0, 12);
	}

	public bool HasFoundCard(string cardId)
	{
		return this.CurrentSave.FoundCardIds.Contains(cardId);
	}

	public void FoundCard(CardData card)
	{
		if (!this.CurrentSave.FoundCardIds.Contains(card.Id))
		{
			if (card.MyCardType == CardType.Ideas || card.MyCardType == CardType.Rumors)
			{
				this.CurrentSave.NewKnowledgeIds.Add(card.Id);
			}
			this.CurrentSave.NewCardopediaIds.Add(card.Id);
			this.CurrentSave.FoundCardIds.Add(card.Id);
			this.NewCardsFound++;
			this.UpdateCardTargets();
			card.MyGameCard.IsNew = true;
		}
	}

	public void DebugUnlockIdeas(bool justBasegame)
	{
		foreach (CardData item in this.CardDataPrefabs.Where((CardData x) => x.MyCardType == CardType.Ideas && !x.HideFromCardopedia).ToList())
		{
			if ((!justBasegame || item.CardUpdateType != CardUpdateType.Spirit) && !this.HasFoundCard(item.Id))
			{
				if (item.MyCardType == CardType.Ideas || item.MyCardType == CardType.Rumors)
				{
					this.CurrentSave.NewKnowledgeIds.Add(item.Id);
				}
				this.CurrentSave.NewCardopediaIds.Add(item.Id);
				this.CurrentSave.FoundCardIds.Add(item.Id);
				this.NewCardsFound++;
			}
		}
	}

	public CardData ChangeToCard(GameCard card, string cardId)
	{
		CardData cardPrefab = this.GetCardPrefab(cardId);
		CardData cardData = card.CardData;
		CardData cardData2 = UnityEngine.Object.Instantiate(cardPrefab);
		cardData2.StatusEffects = cardData.StatusEffects;
		foreach (StatusEffect statusEffect in cardData2.StatusEffects)
		{
			statusEffect.ParentCard = cardData2;
		}
		card.StatusEffectsChanged();
		cardData2.SetExtraCardData(card.CardData.GetExtraCardData());
		cardData2.UniqueId = card.CardData.UniqueId;
		cardData2.transform.SetParent(card.transform);
		cardData2.transform.localPosition = Vector3.zero;
		cardData2.MyGameCard = card;
		card.CardData = cardData2;
		if (cardData.IsFoil)
		{
			cardData2.SetFoil();
		}
		card.gameObject.name = cardPrefab.gameObject.name;
		if (!this.IsLoadingSaveRound)
		{
			QuestManager.instance.CardCreated(cardData2);
		}
		cardData2.MyGameCard.IsNew = false;
		this.FoundCard(cardData2);
		if (GameScreen.instance != null && (cardData2.MyCardType == CardType.Ideas || cardData2.MyCardType == CardType.Rumors))
		{
			GameScreen.instance.UpdateIdeasLog();
		}
		if (cardData is Combatable combatable && cardData2 is Combatable combatable2)
		{
			if (combatable.InConflict)
			{
				combatable.MyConflict.SwapParticipant(combatable, combatable2);
			}
			combatable2.HealthPoints = Mathf.Min(combatable2.HealthPoints, combatable2.ProcessedCombatStats.MaxHealth);
		}
		UnityEngine.Object.Destroy(cardData.gameObject);
		card.UpdateIcon();
		card.UpdateCardPalette();
		this.CreateSmoke(card.transform.position + Vector3.up * 0.05f);
		return cardData2;
	}

	public CardData CreateCard(Vector3 position, CardData cardDataPrefab, bool faceUp, bool checkAddToStack = true, bool playSound = true, bool markAsFound = true)
	{
		if (cardDataPrefab == null)
		{
			return null;
		}
		if (playSound)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.CardCreate, 1f, 0.1f);
		}
		GameCard gameCard = UnityEngine.Object.Instantiate(PrefabManager.instance.GameCardPrefab);
		gameCard.transform.position = position;
		gameCard.MyBoard = this.CurrentBoard;
		CardData cardData = UnityEngine.Object.Instantiate(cardDataPrefab);
		cardData.transform.SetParent(gameCard.transform);
		cardData.transform.localPosition = Vector3.zero;
		cardData.CreationMonth = this.CurrentMonth;
		cardData.UniqueId = this.GetUniqueId();
		gameCard.CardData = cardData;
		cardData.MyGameCard = gameCard;
		gameCard.gameObject.name = cardDataPrefab.gameObject.name;
		gameCard.SetFaceUp(faceUp);
		if (checkAddToStack)
		{
			this.CheckIfCanAddOnStack(gameCard);
		}
		gameCard.transform.position = position;
		this.AllCards.Add(gameCard);
		if (!this.IsLoadingSaveRound)
		{
			this.UniqueIdToCard[cardData.UniqueId] = gameCard;
		}
		if (gameCard.CardData is Curse item)
		{
			this.ActiveCurses.Add(item);
		}
		if (!this.IsLoadingSaveRound)
		{
			QuestManager.instance.CardCreated(cardData);
		}
		if (markAsFound)
		{
			this.FoundCard(cardData);
		}
		if (GameScreen.instance != null && (cardData.MyCardType == CardType.Ideas || cardData.MyCardType == CardType.Rumors))
		{
			GameScreen.instance.UpdateIdeasLog();
		}
		if (cardData.WorkerAmount > 0)
		{
			gameCard.WorkerTransformHolder.UpdateWorkerAmount(cardData.WorkerAmount);
		}
		if (cardData.EnergyConnectors.Count > 0)
		{
			gameCard.CreateCardConnectors();
		}
		if (!this.IsLoadingSaveRound)
		{
			this.TrySendToMagnet(gameCard);
			cardData.OnInitialCreate();
		}
		cardData.gameObject.SetActive(value: true);
		return cardData;
	}

	public CardData GetNearestCardMatchingPred(GameCard card, Predicate<GameCard> pred)
	{
		CardData result = null;
		float num = float.MaxValue;
		foreach (GameCard item in from x in this.AllCards.FindAll(pred)
			where WorldManager.instance.CurrentBoard.Id == x.MyBoard.Id
			select x)
		{
			Vector3 vector = item.transform.position - card.transform.position;
			vector.y = 0f;
			if (vector.sqrMagnitude < num)
			{
				num = vector.sqrMagnitude;
				result = item.CardData;
			}
		}
		return result;
	}

	public bool TrySendWithPipe(GameCard card, GameCard origin, int outputIndex = -1)
	{
		if (origin.CardConnectorChildren.Any((CardConnector x) => x.ConnectionType == ConnectionType.Transport && x.CardDirection == CardDirection.output && x.ConnectedNode != null))
		{
			List<CardConnector> list = origin.CardConnectorChildren.FindAll((CardConnector x) => x.ConnectionType == ConnectionType.Transport && x.CardDirection == CardDirection.output && x.ConnectedNode != null);
			CardConnector cardConnector = null;
			if (outputIndex >= 0 && outputIndex < list.Count)
			{
				cardConnector = list[outputIndex];
			}
			if (cardConnector == null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int index = (origin.ConnectorOutputIndex + i) % list.Count;
					cardConnector = list[index];
					if (cardConnector != null && cardConnector.ConnectedNode != null)
					{
						break;
					}
				}
			}
			if (cardConnector != null)
			{
				origin.ConnectorOutputIndex = list.IndexOf(cardConnector) + 1;
				GameCard parent = cardConnector.ConnectedNode.Parent;
				this.StackSendTo(card, parent.GetLeafCard());
				return true;
			}
		}
		return false;
	}

	public bool TrySendToMagnet(GameCard card)
	{
		CardData nearestCardMatchingPred = this.GetNearestCardMatchingPred(card, (GameCard x) => x.CardData is ResourceMagnet resourceMagnet && resourceMagnet.PullCardId == card.CardData.Id && x.MyBoard == card.MyBoard && resourceMagnet.MyGameCard.GetStackCount() < 30);
		if (nearestCardMatchingPred == null)
		{
			return false;
		}
		this.StackSendTo(card, nearestCardMatchingPred.MyGameCard.GetLeafCard());
		QuestManager.instance.SpecialActionComplete("use_magnet");
		return true;
	}

	public bool TrySendToChest(GameCard card)
	{
		GameCard gameCard = null;
		float num = float.MaxValue;
		List<GameCard> list = new List<GameCard>();
		list = ((!(card.CardData.Id == "gold") && !(card.CardData.Id == "shell")) ? this.AllCards.FindAll((GameCard x) => x.CardData is ResourceChest resourceChest && resourceChest.HeldCardId == card.CardData.Id && resourceChest.ResourceCount < resourceChest.MaxResourceCount) : this.AllCards.FindAll((GameCard x) => x.CardData is Chest chest && chest.HeldCardId == card.CardData.Id && chest.CoinCount < chest.MaxCoinCount));
		foreach (GameCard item in list)
		{
			Vector3 vector = item.transform.position - card.transform.position;
			vector.y = 0f;
			if (vector.magnitude <= 2f && vector.magnitude <= num)
			{
				gameCard = item;
				num = vector.magnitude;
			}
		}
		if (gameCard != null)
		{
			this.StackSendTo(card, gameCard.GetLeafCard());
			return true;
		}
		return false;
	}

	public CardValue GetStackValue(GameCard card)
	{
		CardValue cardValue = new CardValue(card.CardData.GetValue());
		if (card.IsPartOfStack())
		{
			foreach (GameCard item in card.GetAllCardsInStack())
			{
				if (item != card)
				{
					cardValue.BaseValue += item.CardData.GetValue();
				}
			}
		}
		cardValue.BaseValue = Mathf.Max(0, cardValue.BaseValue);
		return cardValue;
	}

	public bool StackAllSame(GameCard card)
	{
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		return this.AllCardsSame(allCardsInStack);
	}

	public bool AllCardsSame(List<GameCard> cards)
	{
		return cards.Select((GameCard x) => x.CardData.Id).Distinct().Count() == 1;
	}

	public bool AllCardsInStackMatchPred(GameCard card, Predicate<GameCard> pred)
	{
		return card.GetAllCardsInStack().All((GameCard x) => pred(x));
	}

	private bool CountsTowardCardCount(GameCard card)
	{
		CardData cardData = card.CardData;
		if (cardData is Poop && this.CurseIsActive(CurseType.Death))
		{
			return false;
		}
		if (this.doesntCountTowardsCount.Contains(cardData.Id))
		{
			return false;
		}
		if (!(cardData is Dollar) && !(cardData is Energy) && cardData.MyCardType != CardType.Weather)
		{
			return !(cardData is Worker);
		}
		return false;
	}

	public int GetCardCount()
	{
		int num = 0;
		bool canTravelToIsland = this.CurrentBoard.BoardOptions.CanTravelToIsland;
		for (int i = 0; i < this.AllCards.Count; i++)
		{
			GameCard gameCard = this.AllCards[i];
			if (gameCard.MyBoard != this.CurrentBoard || gameCard.IsEquipped || !this.CountsTowardCardCount(gameCard) || gameCard.CardData is Food { IsConsumed: not false })
			{
				continue;
			}
			GameCard rootCard = gameCard.GetRootCard();
			if ((!canTravelToIsland || !rootCard.CardData.AnyChildMatchesPredicate((CardData x) => x is Boat boat && boat.InSailOff)) && !(rootCard.CardData is Boat { InSailOff: not false }))
			{
				if (gameCard.CardData is ResourceChest resourceChest)
				{
					num += ((this.CurrentBoard.Id == "cities") ? 1 : resourceChest.ResourceCount);
				}
				num++;
			}
		}
		return num;
	}

	public int GetMaxCardCount()
	{
		return this.GetMaxCardCount(this.CurrentBoard);
	}

	public int GetMaxCardCount(GameBoard board)
	{
		return board.BoardOptions.BaseCardCount + this.CardCapIncrease(board);
	}

	public int CardCapIncrease(GameBoard board)
	{
		if (board.Id == "cities")
		{
			return this.GetTownHallIncrease(board);
		}
		int num = 0;
		for (int num2 = this.AllCards.Count - 1; num2 >= 0; num2--)
		{
			GameCard gameCard = this.AllCards[num2];
			if (!(gameCard.MyBoard != board))
			{
				if (gameCard.CardData.Id == "shed")
				{
					num += 4;
				}
				else if (gameCard.CardData.Id == "warehouse")
				{
					num += 14;
				}
				else if (gameCard.CardData.Id == "lighthouse")
				{
					num += 14;
				}
			}
		}
		return num;
	}

	public int GetTownHallIncrease(GameBoard board)
	{
		this.GetCardsNonAlloc(board, this.townhalls);
		if (this.townhalls.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		foreach (CityHall townhall in this.townhalls)
		{
			num += townhall.DollarAmount;
		}
		return Mathf.FloorToInt(num / CityHall.DollarPerCardcap);
	}

	public int BoardSizeIncrease(GameBoard board)
	{
		return this.GetCardCount("lighthouse", board) * 10;
	}

	public int GetFoodCount(bool allowDebug = true)
	{
		if (this.DebugNoFoodEnabled && allowDebug)
		{
			return 99;
		}
		int num = 0;
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.MyBoard.IsCurrent && allCard.CardData is Food food)
			{
				num += food.FoodValue;
			}
		}
		return num;
	}

	public int GetHappinessCount(bool allowDebug = true, bool includeInChest = true)
	{
		if (this.DebugNoFoodEnabled && allowDebug)
		{
			return 99;
		}
		int num = 0;
		if (includeInChest)
		{
			num = this.GetCountInChests("happiness");
		}
		return this.GetCardCount("happiness") + num;
	}

	private int GetCountInChests(string cardId)
	{
		int num = 0;
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.MyBoard.IsCurrent)
			{
				if (allCard.CardData is ResourceChest resourceChest && resourceChest.HeldCardId == cardId)
				{
					num += resourceChest.ResourceCount;
				}
				if (allCard.CardData is Chest chest && chest.HeldCardId == cardId)
				{
					num += chest.CoinCount;
				}
			}
		}
		return num;
	}

	public int GetShellCount(bool includeInChest)
	{
		int num = 0;
		if (includeInChest)
		{
			num = this.GetCountInChests("shell");
		}
		return this.GetCardCount<Shell>() + num;
	}

	public int GetGoldCount(bool includeInChest)
	{
		int num = 0;
		if (includeInChest)
		{
			num = this.GetCountInChests("gold");
		}
		return this.GetCardCount<Gold>() + num;
	}

	public int GetDollarCount(bool includeInChest)
	{
		int num = 0;
		if (includeInChest)
		{
			num = this.GetDollarInBank();
		}
		this.GetCardsNonAlloc(this.dollars);
		int num2 = 0;
		for (int i = 0; i < this.dollars.Count; i++)
		{
			num2 += this.dollars[i].DollarValue;
		}
		return num2 + num;
	}

	public int GetDollarInBank()
	{
		this.GetCardsNonAlloc(this.creditcards);
		int num = 0;
		for (int i = 0; i < this.creditcards.Count; i++)
		{
			num += this.creditcards[i].DollarCount;
		}
		return num;
	}

	public int GetIdeaCount()
	{
		int num = 0;
		foreach (string foundCardId in this.CurrentSave.FoundCardIds)
		{
			if (foundCardId.StartsWith("blueprint"))
			{
				num++;
			}
		}
		return num;
	}

	public int GetCardCount<T>(Predicate<T> pred) where T : CardData
	{
		int num = 0;
		GameBoard currentBoard = this.CurrentBoard;
		for (int num2 = this.AllCards.Count - 1; num2 >= 0; num2--)
		{
			GameCard gameCard = this.AllCards[num2];
			if (!(gameCard.MyBoard != currentBoard) && gameCard.CardData is T obj && (pred == null || pred(obj)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetCardCount<T>() where T : CardData
	{
		return this.GetCardCount<T>(null);
	}

	public T GetCard<T>() where T : CardData
	{
		for (int i = 0; i < this.AllCards.Count; i++)
		{
			GameCard gameCard = this.AllCards[i];
			if (gameCard.MyBoard.IsCurrent && gameCard.CardData is T)
			{
				return (T)gameCard.CardData;
			}
		}
		return null;
	}

	public T GetCard<T>(GameBoard board) where T : CardData
	{
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard.Id != board.Id) && allCard.CardData is T)
			{
				return (T)allCard.CardData;
			}
		}
		return null;
	}

	public CardData GetCard(string cardId)
	{
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.MyBoard.IsCurrent && allCard.CardData.Id == cardId)
			{
				return allCard.CardData;
			}
		}
		return null;
	}

	public List<GameCard> GetAllCardsOnBoard(string board)
	{
		return this.AllCards.Where((GameCard card) => card.MyBoard.Id == board).ToList();
	}

	public List<CardData> GetCards(string cardId)
	{
		List<CardData> list = new List<CardData>();
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.MyBoard.IsCurrent && allCard.CardData.Id == cardId)
			{
				list.Add(allCard.CardData);
			}
		}
		return list;
	}

	public List<T> GetCardsImplementingInterface<T>()
	{
		if (!typeof(T).IsInterface)
		{
			throw new ArgumentException();
		}
		List<T> list = new List<T>();
		GameBoard currentBoard = this.CurrentBoard;
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard != currentBoard) && allCard.CardData is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<T> GetCardsImplementingInterfaceNonAlloc<T>(List<T> list)
	{
		if (!typeof(T).IsInterface)
		{
			throw new ArgumentException();
		}
		list.Clear();
		GameBoard currentBoard = this.CurrentBoard;
		for (int num = this.AllCards.Count - 1; num >= 0; num--)
		{
			GameCard gameCard = this.AllCards[num];
			if (!(gameCard.MyBoard != currentBoard) && gameCard.CardData is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<T> GetCards<T>() where T : CardData
	{
		List<T> list = new List<T>();
		for (int i = 0; i < this.AllCards.Count; i++)
		{
			GameCard gameCard = this.AllCards[i];
			if (gameCard.MyBoard.IsCurrent && gameCard.CardData is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void GetCardsNonAlloc<T>(List<T> list) where T : CardData
	{
		list.Clear();
		GameBoard currentBoard = this.CurrentBoard;
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard != currentBoard) && allCard.CardData is T item)
			{
				list.Add(item);
			}
		}
	}

	public List<T> GetCards<T>(GameBoard board) where T : CardData
	{
		List<T> list = new List<T>();
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard.Id != board.Id) && allCard.CardData is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void GetCardsNonAlloc<T>(GameBoard board, List<T> list) where T : CardData
	{
		list.Clear();
		foreach (GameCard allCard in this.AllCards)
		{
			if (!(allCard.MyBoard.Id != board.Id) && allCard.CardData is T item)
			{
				list.Add(item);
			}
		}
	}

	public List<Boosterpack> GetAllBoostersOnBoard(string board)
	{
		return this.AllBoosters.Where((Boosterpack booster) => booster.MyBoard.Id == board).ToList();
	}

	public int GetRequiredFoodCount()
	{
		if (this.DebugNoFoodEnabled)
		{
			return 0;
		}
		int num = 0;
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.MyBoard.IsCurrent)
			{
				num += this.GetCardRequiredFoodCount(allCard);
			}
		}
		return num;
	}

	public int GetCardRequiredFoodCount(GameCard c)
	{
		if (c.CardData is BaseVillager baseVillager)
		{
			return baseVillager.GetRequiredFoodCount();
		}
		if (c.CardData is Kid)
		{
			if (!(this.CurrentBoard.Id == "cities"))
			{
				return 1;
			}
			return 0;
		}
		if (c.CardData is Apartment apartment)
		{
			return apartment.RequirementHolders.Sum((RequirementHolder x) => x.CardRequirements.Sum((CardRequirement x) => (x is CardRequirement_TakeFood cardRequirement_TakeFood) ? cardRequirement_TakeFood.Amount : 0));
		}
		return 0;
	}

	public int GetRequiredHappinessCount()
	{
		if (this.DebugNoFoodEnabled)
		{
			return 0;
		}
		int num = 0;
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.MyBoard.IsCurrent)
			{
				num += this.GetCardRequiredHappinessCount(allCard);
			}
		}
		return num;
	}

	public int GetCardRequiredHappinessCount(GameCard c)
	{
		if (c.CardData is BaseVillager)
		{
			return 1;
		}
		if (c.CardData is Kid)
		{
			return 1;
		}
		if (c.CardData is Unhappiness)
		{
			return 1;
		}
		if (c.CardData is ResourceChest { HeldCardId: "unhappiness" } resourceChest)
		{
			return resourceChest.ResourceCount;
		}
		return 0;
	}

	public Blueprint GetBlueprintWithId(string id)
	{
		foreach (Blueprint blueprintPrefab in this.BlueprintPrefabs)
		{
			if (blueprintPrefab.Id == id)
			{
				return blueprintPrefab;
			}
		}
		return null;
	}

	public string GetStackSummary(List<GameCard> cards)
	{
		List<string> list = cards.Select((GameCard x) => x.CardData.FullName).Distinct().ToList();
		string text = "";
		for (int i = 0; i < list.Count; i++)
		{
			string card = list[i];
			int num = cards.Count((GameCard x) => x.CardData.FullName == card);
			text += $"{num}x {card}";
			if (i < list.Count - 1)
			{
				text += ", ";
			}
		}
		return text;
	}

	private void EndOfMonth(EndOfMonthParameters param = null)
	{
		if (param == null)
		{
			param = new EndOfMonthParameters();
		}
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		this.CloseOpenInventories();
		if (this.CurrentBoard.Id == "cities")
		{
			this.currentAnimationRoutine = base.StartCoroutine(this.EndOfMonthCitiesRoutine(param));
		}
		else
		{
			this.currentAnimationRoutine = base.StartCoroutine(this.EndOfMonthRoutine(param));
		}
		if (GameScreen.instance.ControllerIsInUI)
		{
			GameScreen.instance.SetControllerInUI(inUI: false);
		}
		this.SpeedUp = 1f;
		QuestManager.instance.SpecialActionComplete("month_end");
	}

	public IEnumerator FinishDemand(Demand demand, DemandEvent demandEvent)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		this.CloseOpenInventories();
		yield return this.FinishDemandRoutine(demand, demandEvent);
		if (GameScreen.instance.ControllerIsInUI)
		{
			GameScreen.instance.SetControllerInUI(inUI: false);
		}
		this.SpeedUp = 1f;
		QuestManager.instance.SpecialActionComplete("first_demand");
	}

	public void ForceEndOfMoon(EndOfMonthParameters param)
	{
		this.MonthTimer = 0f;
		this.IncrementMonth();
		this.EndOfMonth(param);
	}

	private IEnumerator WaitForContinueClicked(string text)
	{
		this.ContinueClicked = false;
		this.ContinueButtonText = text;
		this.ShowContinueButton = true;
		while (!this.ContinueClicked)
		{
			yield return null;
		}
		this.ShowContinueButton = false;
	}

	private IEnumerator EndOfMonthRoutine(EndOfMonthParameters param)
	{
		if (this.CurrentView != ViewType.Default)
		{
			this.SetViewType(ViewType.Default);
		}
		foreach (TravellingCart card in this.GetCards<TravellingCart>())
		{
			card.MyGameCard.DestroyCard(spawnSmoke: true);
		}
		foreach (CardData card2 in this.GetCards("trained_monkey"))
		{
			this.ChangeToCard(card2.MyGameCard, "monkey").UpdateCardText();
		}
		AudioManager.me.PlaySound2D(AudioManager.me.EndOfMoon, 0.8f, 0.2f);
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_end_of_moon", LocParam.Create("moon", (this.CurrentMonth - 1).ToString()));
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		if (!this.DebugNoFoodEnabled)
		{
			this.VillagersStarvedAtEndOfMoon = false;
			yield return EndOfMonthCutscenes.FeedVillagers();
			if (this.VillagersStarvedAtEndOfMoon)
			{
				yield break;
			}
			if (this.CurseIsActive(CurseType.Happiness))
			{
				this.VillagersAngryAtEndOfMoon = false;
				yield return EndOfMonthCutscenes.UseHappiness();
				if (this.VillagersAngryAtEndOfMoon)
				{
					yield break;
				}
			}
			else
			{
				this.CurrentRunVariables.VillagersUnhappyMonthCount = 0;
			}
		}
		if (this.CurseIsActive(CurseType.Death))
		{
			CardData fountain = this.GetCard("fountain_of_youth");
			if (fountain != null)
			{
				this.CutsceneTitle = SokLoc.Translate("label_fountain_title");
				this.CutsceneText = SokLoc.Translate("label_fountain_text");
				yield return new WaitForSeconds(1f);
				GameCamera.instance.TargetPositionOverride = fountain.MyGameCard.transform.position;
				yield return new WaitForSeconds(2f);
			}
			else
			{
				List<BaseVillager> villagersToAge = EndOfMonthCutscenes.GetVillagersToAge();
				if (villagersToAge.Any((BaseVillager x) => x.DetermineLifeStageFromAge(x.Age) == LifeStage.Elderly))
				{
					QuestManager.instance.SpecialActionComplete("villager_old");
				}
				if (EndOfMonthCutscenes.AnyVillagerWillChangeLifeStage(villagersToAge))
				{
					yield return EndOfMonthCutscenes.AgeVillagers(villagersToAge);
				}
				else
				{
					foreach (BaseVillager item in villagersToAge)
					{
						item.Age++;
					}
				}
				if (!this.VillagersStarvedAtEndOfMoon)
				{
					bool flag = false;
					if (this.CurrentBoard.Id == "death" && this.BoardMonths.DeathMonth >= 6)
					{
						flag = true;
					}
					else if ((this.CurrentBoard.Id == "main" || this.CurrentBoard.Id == "island") && this.CurrentMonth > 6)
					{
						flag = true;
					}
					if (flag)
					{
						yield return EndOfMonthCutscenes.CheckMakeSick();
					}
					if (this.BoardMonths.DeathMonth == 4 && this.CurrentBoard.Id == "death")
					{
						yield return EndOfMonthCutscenes.NewVillagerSpawnsInDeath();
					}
				}
			}
			if (!this.VillagersStarvedAtEndOfMoon)
			{
				List<Animal> cards = this.GetCards<Animal>();
				foreach (Animal item2 in cards)
				{
					if (this.CurrentMonth - item2.CreationMonth >= 3)
					{
						item2.IsOld = true;
					}
					else
					{
						item2.IsOld = false;
					}
				}
				if (EndOfMonthCutscenes.AnyAnimalWillDie(cards))
				{
					yield return EndOfMonthCutscenes.KillAnimals((from x in this.GetCards<Animal>()
						where this.CurrentMonth - x.CreationMonth >= 5
						select x).ToList());
				}
			}
		}
		yield return new WaitForSeconds(1.5f);
		yield return EndOfMonthCutscenes.MaxCardCount();
		if (this.CurseIsActive(CurseType.Greed))
		{
			yield return DemandManager.instance.CheckDemands(this.CurrentMonth);
		}
		if (this.IsCitiesDlcActive() && this.CurrentBoard.Id == "main" && this.GetCards<Food>().Sum((Food x) => x.FoodValue) >= 25 && this.GetCard("event_industrial_revolution") == null)
		{
			yield return EndOfMonthCutscenes.IndustrialRevolutionEvent();
		}
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_start_of_moon", LocParam.Create("moon", this.CurrentMonth.ToString()));
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		if (!param.SkipSpecialEvents)
		{
			yield return EndOfMonthCutscenes.SpecialEvents();
		}
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_start_of_moon", LocParam.Create("moon", this.CurrentMonth.ToString()));
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		this.CutsceneText = "";
		if (!param.SkipEndConfirmation)
		{
			yield return this.WaitForContinueClicked(SokLoc.Translate("label_start_new_moon"));
		}
		GameCanvas.instance.SetScreen<GameScreen>();
		if (param.OnDone != null)
		{
			param.OnDone?.Invoke();
		}
		GameCamera.instance.TargetPositionOverride = null;
		this.currentAnimationRoutine = null;
		SaveManager.instance.Save(saveRound: true);
		if (DebugScreen.instance != null)
		{
			DebugScreen.instance.AutoSave();
		}
	}

	public IEnumerator EndOfMonthCitiesRoutine(EndOfMonthParameters param)
	{
		AudioManager.me.PlaySound2D(AudioManager.me.EndOfMoon, 0.8f, 0.2f);
		if (this.CurrentView != ViewType.Default)
		{
			this.SetViewType(ViewType.Default);
		}
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_end_of_moon", LocParam.Create("moon", (this.CurrentMonth - 1).ToString()));
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		CutsceneScreen.instance.EnableWellbeingBar(CitiesManager.instance.Wellbeing);
		yield return new WaitForSeconds(1f);
		List<CardData> requirementsCards = (from x in this.GetCards<CardData>()
			where x.RequirementHolders != null && x.RequirementHolders.Count > 0 && (!(x.MyGameCard.GetCardWithStatusInStack() != null) || !(x.MyGameCard.GetCardWithStatusInStack().TimerActionId == "finish_blueprint"))
			select x).ToList();
		int previousWellbeing = CitiesManager.instance.Wellbeing;
		(from x in this.GetCards<Enemy>()
			where x.InConflict
			select x).ToList();
		this.GetCardCount<CitiesCombatable>();
		foreach (CardData requirementCard in requirementsCards)
		{
			foreach (RequirementHolder requirementHolder in requirementCard.RequirementHolders)
			{
				bool flag = true;
				foreach (CardRequirement cardRequirement in requirementHolder.CardRequirements)
				{
					flag = cardRequirement.Satisfied(requirementCard.MyGameCard);
					if (!flag)
					{
						break;
					}
				}
				if (flag)
				{
					foreach (CardRequirementResult positiveResult in requirementHolder.PositiveResults)
					{
						if (requirementCard.MyGameCard != null)
						{
							yield return positiveResult.Perform(requirementCard.MyGameCard);
						}
					}
					continue;
				}
				foreach (CardRequirementResult negativeResult in requirementHolder.NegativeResults)
				{
					if (requirementCard.MyGameCard != null)
					{
						yield return negativeResult.Perform(requirementCard.MyGameCard);
					}
				}
			}
		}
		List<CardData> rootWithResults = (from x in this.GetCards<CardData>()
			where x.MonthlyRequirementResult != null
			select x).ToList();
		CutsceneScreen.instance.WellbeingAmount = CitiesManager.instance.Wellbeing;
		if (CitiesManager.instance.Wellbeing - previousWellbeing > 0)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.AddWellbeing, 1f, 0.5f);
		}
		else if (CitiesManager.instance.Wellbeing - previousWellbeing < 0)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.LostWellbeing, 1f, 0.5f);
		}
		if (CitiesManager.instance.Wellbeing - previousWellbeing >= 5)
		{
			QuestManager.instance.SpecialActionComplete("cities_wellbeing_gained_5");
		}
		else if (CitiesManager.instance.Wellbeing - previousWellbeing <= -5)
		{
			QuestManager.instance.SpecialActionComplete("cities_wellbeing_lost_5");
		}
		bool lostGame = false;
		if (CitiesManager.instance.Wellbeing > 0)
		{
			foreach (CardData item in rootWithResults)
			{
				int num = 0;
				foreach (KeyValuePair<string, MonthlyResult> result in item.MonthlyRequirementResult.results)
				{
					if (result.Value.Amount != 0)
					{
						this.CreateFloatingText(item.MyGameCard, result.Value.Amount > 0, result.Value.Amount, result.Value.Card.CardData.GetRequirementDescription(result.Value.Card, result.Value.CardAmount), this.GetIconStringFromRequirementType(result.Value.Type), result.Value.Amount > 0, num, 0f);
					}
					num++;
				}
			}
			this.CutsceneBoardView = true;
			yield return EndOfMonthCutscenes.MaxCardCount();
			this.CutsceneBoardView = false;
			if (CitiesManager.instance.Wellbeing > 25 && !this.CurrentRunOptions.IsPeacefulMode && this.CurrentMonth == CitiesManager.instance.NextConflictMonth && CitiesManager.instance.NextConflictMonth != -1)
			{
				Vector3 randomSpawnPosition = this.GetRandomSpawnPosition();
				GameCamera.instance.TargetPositionOverride = randomSpawnPosition;
				this.CutsceneTitle = SokLoc.Translate("label_goblin_conflict_title");
				this.CutsceneText = SokLoc.Translate("label_goblin_conflict_text");
				this.CreateCard(randomSpawnPosition, "event_goblin_attack");
				yield return this.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
				GameCamera.instance.TargetPositionOverride = null;
			}
			this.CutsceneBoardView = true;
			CutsceneScreen.instance.CanMoveScreen = true;
			int num2 = CitiesManager.instance.Wellbeing - previousWellbeing;
			this.CutsceneTitle = SokLoc.Translate("label_end_of_moon_cities_wellbeing");
			string value = Mathf.Abs(num2).ToString();
			if (num2 > 0)
			{
				this.CutsceneText = SokLoc.Translate("label_end_of_moon_cities_gained", LocParam.Create("amount", value), LocParam.Create("icon", Icons.Wellbeing));
			}
			else if (num2 == 0)
			{
				this.CutsceneText = SokLoc.Translate("label_end_of_moon_cities_same", LocParam.Create("icon", Icons.Wellbeing));
			}
			else
			{
				this.CutsceneText = SokLoc.Translate("label_end_of_moon_cities_lost", LocParam.Create("amount", value), LocParam.Create("icon", Icons.Wellbeing));
			}
			if (num2 != 0)
			{
				this.CutsceneText = this.CutsceneText + ", " + SokLoc.Translate("label_hover_status_wellbeing");
			}
			yield return this.WaitForContinueClicked(SokLoc.Translate((num2 >= 0) ? "label_nice" : "label_uh_oh"));
			if (param.CutsceneTitle == null)
			{
				this.CutsceneTitle = SokLoc.Translate("label_start_of_moon", LocParam.Create("moon", this.CurrentMonth.ToString()));
			}
			else
			{
				this.CutsceneTitle = param.CutsceneTitle;
			}
			this.CutsceneText = "";
			if (!param.SkipEndConfirmation)
			{
				yield return this.WaitForContinueClicked(SokLoc.Translate("label_start_new_moon"));
			}
			GameCamera.instance.TargetPositionOverride = null;
		}
		else
		{
			lostGame = true;
			this.CutsceneTitle = SokLoc.Translate("label_cities_game_over_title");
			this.CutsceneText = SokLoc.Translate("label_cities_game_over_text");
			yield return this.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
			this.CutsceneText = SokLoc.Translate("label_cities_game_over_text_1");
			yield return this.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			GameCamera.instance.TargetPositionOverride = null;
		}
		CutsceneScreen.instance.CanMoveScreen = false;
		if (!lostGame)
		{
			this.CloseAllFloatingTextObjects();
			foreach (CardData requirementCard in requirementsCards.Where((CardData x) => x != null))
			{
				foreach (RequirementHolder requirementHolder2 in requirementCard.RequirementHolders)
				{
					bool flag2 = true;
					foreach (CardRequirement cardRequirement2 in requirementHolder2.CardRequirements)
					{
						flag2 = cardRequirement2.Satisfied(requirementCard.MyGameCard);
						if (!flag2)
						{
							break;
						}
					}
					if (flag2)
					{
						foreach (CardRequirementResult positiveResult2 in requirementHolder2.PositiveResults)
						{
							yield return positiveResult2.EndOfCutscenePerform(requirementCard.MyGameCard);
						}
						continue;
					}
					foreach (CardRequirementResult negativeResult2 in requirementHolder2.NegativeResults)
					{
						yield return negativeResult2.EndOfCutscenePerform(requirementCard.MyGameCard);
					}
				}
			}
			foreach (CardData item2 in rootWithResults)
			{
				item2.MonthlyRequirementResult = null;
			}
			if (this.CurrentMonth % 4 == 0)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.EndOfMoon, 0.5f, 0.5f);
				this.CutsceneTitle = SokLoc.Translate("label_weather_report_title");
				this.CutsceneText = SokLoc.Translate("label_weather_report_text");
				GameCamera.instance.TargetPositionOverride = this.MiddleOfBoard();
				yield return new WaitForSeconds(0.5f);
				Boosterpack pack = this.CreateBoosterpack(this.MiddleOfBoard(), "cities_weather");
				AudioManager.me.PlaySound2D(AudioManager.me.CardCreate, 1f, 0.5f);
				yield return new WaitForSeconds(0.5f);
				for (int i = 0; i < pack.TotalCardsInPack; i++)
				{
					pack.Clicked();
					yield return new WaitForSeconds(0.2f);
				}
				yield return this.WaitForContinueClicked(SokLoc.Translate("label_nice"));
				GameCamera.instance.TargetPositionOverride = null;
			}
		}
		QuestManager.instance.SpecialActionComplete("cities_wellbeing_changed");
		GameCanvas.instance.SetScreen<GameScreen>();
		if (param.OnDone != null)
		{
			param.OnDone?.Invoke();
		}
		this.currentAnimationRoutine = null;
		this.CutsceneBoardView = false;
		GameCamera.instance.TargetPositionOverride = null;
		this.CutsceneTitle = "";
		this.CutsceneText = "";
		if (lostGame)
		{
			GameBoard citiesBoard = this.GetCurrentBoardSafe();
			this.GoToBoard(this.GetBoardWithId("main"), delegate
			{
				this.RemoveAllCardsFromBoard(citiesBoard.Id);
				this.ResetBoughtBoostersOnLocation(citiesBoard.Location);
				this.ResetCityVariables();
			}, "cities");
		}
		else
		{
			SaveManager.instance.Save(saveRound: true);
			if (DebugScreen.instance != null)
			{
				DebugScreen.instance.AutoSave();
			}
			this.QueueCutsceneIfNotPlayed("cities_first_moon");
		}
	}

	public string GetIconStringFromRequirementType(RequirementType type)
	{
		return type switch
		{
			RequirementType.Food => Icons.Food, 
			RequirementType.WellBeing => Icons.Wellbeing, 
			RequirementType.Energy => Icons.Energy, 
			RequirementType.Dollar => Icons.Dollar, 
			RequirementType.Pollution => Icons.Pollution, 
			RequirementType.Health => Icons.Health, 
			_ => throw new NotImplementedException("Icon is not implemented"), 
		};
	}

	private IEnumerator FinishDemandRoutine(Demand demand, DemandEvent demandEvent)
	{
		if (demand.Amount == demandEvent.AmountGiven)
		{
			yield return GreedCutscenes.FinishDemandSuccessPreMoon(demand);
			demandEvent.Successful = true;
			this.CurrentRunVariables.LastDemandMonth = this.CurrentMonth + 1;
		}
		else if (this.GetCardCount((CardData x) => x.Id == demand.CardToGet) >= demand.Amount - demandEvent.AmountGiven)
		{
			yield return GreedCutscenes.FinishDemandSuccess(demandEvent);
			demandEvent.Successful = true;
			this.CurrentRunVariables.LastDemandMonth = this.CurrentMonth;
		}
		else
		{
			yield return GreedCutscenes.FinishDemandFailed(demand);
			demandEvent.Successful = false;
			this.CurrentRunVariables.LastDemandMonth = this.CurrentMonth;
		}
		this.CurrentRunVariables.PreviousDemandEvents.Add(demandEvent);
		this.CurrentRunVariables.ActiveDemand = null;
		if (demandEvent.Successful)
		{
			QuestManager.instance.SpecialActionComplete("demand_success");
		}
		else
		{
			QuestManager.instance.SpecialActionComplete("demand_failed");
		}
	}

	private void LateUpdate()
	{
		if (!this.ShowContinueButton)
		{
			this.ContinueClicked = false;
		}
	}

	public void KillVillager(Combatable combatable, Action onComplete = null, Action onCreateCorpse = null)
	{
		this.currentAnimationRoutine = base.StartCoroutine(this.KillVillagerCoroutine(combatable, delegate
		{
			this.currentAnimationRoutine = null;
			onComplete?.Invoke();
		}, onCreateCorpse));
	}

	public IEnumerator KillVillagerCoroutine(Combatable combatable, Action onComplete, Action onCreateCorpse, bool resetTargetOnDeath = true)
	{
		GameCamera.instance.TargetPositionOverride = combatable.MyGameCard.transform.position;
		yield return new WaitForSeconds(1f);
		List<Equipable> allEquipables = combatable.GetAllEquipables();
		List<ExtraCardData> extraCardData = combatable.GetExtraCardData();
		if (combatable.MyGameCard.HasParent && combatable.MyGameCard.HasChild)
		{
			GameCard parent = combatable.MyGameCard.Parent;
			GameCard child = combatable.MyGameCard.Child;
			combatable.MyGameCard.RemoveFromStack();
			child.SetParent(parent);
		}
		combatable.MyGameCard.DestroyCard(spawnSmoke: true);
		if (combatable is Animal)
		{
			combatable.Die();
		}
		else if (combatable.Id != "trained_monkey")
		{
			this.CreateCard(combatable.MyGameCard.transform.position, "corpse", faceUp: true, checkAddToStack: false).SetExtraCardData(extraCardData);
			onCreateCorpse?.Invoke();
			this.TryCreateUnhappiness(combatable.MyGameCard.transform.position, 2);
		}
		foreach (Equipable item in allEquipables)
		{
			if (item != null && !string.IsNullOrEmpty(item.Id))
			{
				this.CreateCard(combatable.transform.position, item.Id, faceUp: true, checkAddToStack: false, playSound: false).MyGameCard.SendIt();
			}
		}
		yield return new WaitForSeconds(1f);
		if (resetTargetOnDeath)
		{
			GameCamera.instance.TargetPositionOverride = null;
		}
		onComplete?.Invoke();
	}

	public bool CheckAllVillagersDead()
	{
		if (this.DebugDontNeedVillagers)
		{
			return false;
		}
		return this.GetCardCount<BaseVillager>() <= 0;
	}

	public void CreateSmoke(Vector3 pos)
	{
		ParticleSystem particleSystem = null;
		foreach (ParticleSystem item in this.smokeBuffer)
		{
			if (!item.isPlaying)
			{
				particleSystem = item;
			}
		}
		if (particleSystem == null)
		{
			particleSystem = UnityEngine.Object.Instantiate(PrefabManager.instance.SmokeParticlePrefab).GetComponentInChildren<ParticleSystem>();
			this.smokeBuffer.Add(particleSystem);
		}
		particleSystem.Play();
		particleSystem.transform.position = pos + Vector3.up * 0.05f;
	}

	public void CreateMinusElectricity(Vector3 pos)
	{
		ParticleSystem particleSystem = null;
		foreach (ParticleSystem item in this.energyMinusBuffer)
		{
			if (!item.isPlaying)
			{
				particleSystem = item;
			}
		}
		if (particleSystem == null)
		{
			particleSystem = UnityEngine.Object.Instantiate(PrefabManager.instance.ElectricityMinusParticlePrefab).GetComponentInChildren<ParticleSystem>();
			this.energyMinusBuffer.Add(particleSystem);
		}
		particleSystem.Play();
		particleSystem.transform.position = pos + Vector3.up * 0.05f;
	}

	public void CreateWellbeingPlus(Vector3 pos)
	{
		ParticleSystem particleSystem = null;
		foreach (ParticleSystem item in this.wellbeingPlusBuffer)
		{
			if (!item.isPlaying)
			{
				particleSystem = item;
			}
		}
		if (particleSystem == null)
		{
			particleSystem = UnityEngine.Object.Instantiate(PrefabManager.instance.WellbeingPlusParticlePrefab).GetComponentInChildren<ParticleSystem>();
			this.wellbeingPlusBuffer.Add(particleSystem);
		}
		particleSystem.Play();
		particleSystem.transform.position = pos + Vector3.up * 0.05f;
	}

	public void CloseAllFloatingTextObjects()
	{
		foreach (FloatingStatus item in this.floatingTextBuffer)
		{
			if (item != null && (bool)item.ParentCard)
			{
				item.StopAnimation();
			}
		}
	}

	public void CreateFloatingText(GameCard parent, bool isPositive, int amount, string descriptionText, string iconTag, bool desiredBehaviour, int index = 1, float timer = 1f, bool closeOnHover = false)
	{
		FloatingStatus floatingStatus = null;
		foreach (FloatingStatus item in this.floatingTextBuffer)
		{
			if (!item.InAnimation)
			{
				floatingStatus = item;
			}
		}
		if (floatingStatus == null)
		{
			floatingStatus = UnityEngine.Object.Instantiate(PrefabManager.instance.FloatingTextPrefab).GetComponentInChildren<FloatingStatus>();
			this.floatingTextBuffer.Add(floatingStatus);
		}
		floatingStatus.StartAnimation(parent, isPositive, amount, descriptionText, iconTag, desiredBehaviour, index, timer, closeOnHover);
	}

	public void TryCreateHappiness(Vector3 position, int amount)
	{
		if (this.CurseIsActive(CurseType.Happiness))
		{
			for (int i = 0; i < amount; i++)
			{
				CardData cardData = this.CreateCard(position, "happiness", faceUp: true, checkAddToStack: false);
				this.CreateSmoke(position);
				this.StackSend(cardData.MyGameCard, Vector3.zero);
			}
		}
	}

	public void TryCreateUnhappiness(Vector3 position, int amount)
	{
		if (this.CurseIsActive(CurseType.Happiness))
		{
			for (int i = 0; i < amount; i++)
			{
				CardData cardData = this.CreateCard(position, "unhappiness", faceUp: true, checkAddToStack: false);
				this.CreateSmoke(position);
				this.StackSend(cardData.MyGameCard, Vector3.zero);
			}
		}
	}

	public ICardId GetRandomCard(List<CardChance> chances, bool removeCard)
	{
		this.chanceBag.Clear();
		foreach (CardChance chance in chances)
		{
			if ((chance.HasMaxCount && this.GetCurrentCardCount(chance.Id) >= chance.MaxCountToGive) || (chance.HasPrerequisiteCard && !this.GivenCards.Contains(chance.PrerequisiteCardId)))
			{
				continue;
			}
			if (chance.IsEnemy)
			{
				if (chance.Chance != 0)
				{
					this.chanceBag.AddEntry(chance, chance.Chance);
				}
				continue;
			}
			CardData cardPrefab = this.GetCardPrefab(chance.Id);
			if ((!this.CurrentRunOptions.IsPeacefulMode || !(cardPrefab is Enemy)) && (!this.CurrentRunOptions.IsPeacefulMode || !(cardPrefab.Id == "catacombs")) && ((cardPrefab.MyCardType != CardType.Ideas && cardPrefab.MyCardType != CardType.Rumors) || !this.CurrentSave.FoundCardIds.Contains(chance.Id)) && chance.Chance != 0)
			{
				this.chanceBag.AddEntry(chance, chance.Chance);
			}
		}
		CardChance cardChance = this.chanceBag.Choose();
		if (cardChance != null)
		{
			if (cardChance.IsEnemy && !this.CurrentRunOptions.IsPeacefulMode)
			{
				return CardBag.GetCardIdForEnemyBag(cardChance.EnemyBag, cardChance.Strength);
			}
			return (CardId)cardChance.Id;
		}
		return null;
	}

	private int GetCurrentCardCount(string cardId)
	{
		return this.AllCards.Count((GameCard c) => c.CardData.Id == cardId && c.MyBoard.IsCurrent);
	}

	public GameCard GetCardWithUniqueId(string uniqueId)
	{
		if (!this.UniqueIdToCard.TryGetValue(uniqueId, out var value))
		{
			return null;
		}
		return value;
	}

	public int GetTimesAnyBoosterWasBought()
	{
		return this.BoughtBoosterIds.Count;
	}

	public int GetTimesBoosterWasBoughtOnLocation(Location location)
	{
		int num = 0;
		foreach (string boughtBoosterId in this.BoughtBoosterIds)
		{
			BoosterpackData boosterData = this.GetBoosterData(boughtBoosterId);
			if (boosterData != null && boosterData.BoosterLocation == location)
			{
				num++;
			}
		}
		return num;
	}

	public void ResetBoughtBoostersOnLocation(Location location)
	{
		this.BoughtBoosterIds.RemoveAll(delegate(string boosterId)
		{
			BoosterpackData boosterData = this.GetBoosterData(boosterId);
			return (boosterData != null && boosterData.BoosterLocation == location) ? true : false;
		});
	}

	public List<Conflict> GetAllConflicts()
	{
		List<Conflict> list = new List<Conflict>();
		foreach (GameCard allCard in this.AllCards)
		{
			if (allCard.InConflict && !list.Contains(allCard.Combatable.MyConflict))
			{
				list.Add(allCard.Combatable.MyConflict);
			}
		}
		return list;
	}

	public void LoadSaveRound(SaveRound saveRound)
	{
		this.IsLoadingSaveRound = true;
		this.ClearRound();
		this.AllCards.Clear();
		this.UniqueIdToCard.Clear();
		if (Application.isEditor)
		{
			UnityEngine.Debug.Log($"Loading Run with {saveRound.RunOptions.MoonLength} moon length and peaceful mode: {saveRound.RunOptions.IsPeacefulMode}");
		}
		this.MonthTimer = saveRound.MonthTimer;
		this.OldCurrentMonth = saveRound.CurrentMonth;
		this.BoardMonths = new BoardMonths(saveRound.BoardMonths);
		this.GivenCards = saveRound.GivenCards;
		this.BoughtBoosterIds = saveRound.BoughtBoosterIds;
		this.CurrentBoard = this.GetBoardWithId(saveRound.CurrentBoardId);
		this.CurrentRunOptions = saveRound.RunOptions;
		this.CurrentRunVariables = saveRound.RunVariables;
		this.RoundExtraKeyValues = saveRound.ExtraKeyValues;
		if (CitiesManager.instance == null)
		{
			new Exception("CitiesManager should be active before loading the saveRound");
		}
		CitiesManager.instance.Wellbeing = saveRound.CitiesWellbeing;
		CitiesManager.instance.NextConflictMonth = saveRound.CitiesConflictMonth;
		CitiesManager.instance.ActiveEvent = saveRound.CitiesDisaster;
		if (this.CurrentRunVariables.ActiveDemand != null && string.IsNullOrEmpty(this.CurrentRunVariables.ActiveDemand.DemandId))
		{
			this.CurrentRunVariables.ActiveDemand = null;
		}
		foreach (SavedCard savedCard in saveRound.SavedCards)
		{
			CardData cardData = this.CreateCard(savedCard.CardPosition, savedCard.CardPrefabId, savedCard.FaceUp, checkAddToStack: false, playSound: false);
			if (cardData == null)
			{
				continue;
			}
			cardData.MyGameCard.MyBoard = this.GetBoardWithId(savedCard.BoardId);
			cardData.UniqueId = savedCard.UniqueId;
			this.UniqueIdToCard[cardData.UniqueId] = cardData.MyGameCard;
			cardData.ParentUniqueId = savedCard.ParentUniqueId;
			cardData.EquipmentHolderUniqueId = savedCard.EquipmentHolderUniqueId;
			cardData.WorkerHolderUniqueId = savedCard.WorkerHolderUniqueId;
			cardData.WorkerIndex = savedCard.WorkerIndex;
			cardData.SetExtraCardData(savedCard.ExtraCardData);
			if (savedCard.IsFoil)
			{
				cardData.SetFoil();
			}
			cardData.IsDamaged = savedCard.IsDamaged;
			cardData.DamageType = savedCard.DamageType;
			if (savedCard.StatusEffects != null && savedCard.StatusEffects.Count > 0)
			{
				List<StatusEffect> list = savedCard.StatusEffects.Select((SavedStatusEffect x) => StatusEffect.FromSavedStatusEffect(x)).ToList();
				list.RemoveAll((StatusEffect x) => x == null);
				foreach (StatusEffect item in list)
				{
					item.ParentCard = cardData;
				}
				cardData.StatusEffects = list;
				cardData.MyGameCard.StatusEffectsChanged();
			}
			else
			{
				cardData.StatusEffects = new List<StatusEffect>();
			}
			if (savedCard.CardConnectors != null && savedCard.CardConnectors.Count > 0)
			{
				List<SavedCardConnector> cardConnectors = savedCard.CardConnectors;
				cardConnectors.RemoveAll((SavedCardConnector x) => x == null || string.IsNullOrEmpty(x.ConnectedNodeUniqueId));
				for (int i = 0; i < cardData.MyGameCard.CardConnectorChildren.Count; i++)
				{
					CardConnector cardConnector = cardData.MyGameCard.CardConnectorChildren[i];
					string myUniqueId = cardConnector.GetConnectorUniqueId();
					SavedCardConnector savedCardConnector = cardConnectors.Find((SavedCardConnector x) => x.UniqueId == myUniqueId);
					if (savedCardConnector != null)
					{
						cardConnector.UniqueId = savedCardConnector.UniqueId;
						cardConnector.ConnectedNodeUniqueId = savedCardConnector.ConnectedNodeUniqueId;
					}
				}
			}
			if (savedCard.TimerRunning)
			{
				TimerAction delegateForActionId = cardData.GetDelegateForActionId(savedCard.TimerActionId);
				if (delegateForActionId != null)
				{
					cardData.MyGameCard.StartTimer(savedCard.TargetTimerTime, delegateForActionId, savedCard.Status, savedCard.TimerActionId, savedCard.WithStatusBar, skipWorkerEnergyCheck: true);
					cardData.MyGameCard.CurrentTimerTime = savedCard.CurrentTimerTime;
					cardData.MyGameCard.TimerBlueprintId = savedCard.TimerBlueprintId;
					cardData.MyGameCard.TimerSubprintIndex = savedCard.SubprintIndex;
					cardData.MyGameCard.SkipCitiesChecks = savedCard.SkipCitiesChecks;
				}
			}
		}
		foreach (GameCard allCard in this.AllCards)
		{
			foreach (CardConnector connector in allCard.CardConnectorChildren)
			{
				if (!string.IsNullOrEmpty(connector.ConnectedNodeUniqueId))
				{
					CardConnector cardConnector2 = (from x in this.AllCards.SelectMany((GameCard x) => x.CardConnectorChildren)
						where x.UniqueId == connector.ConnectedNodeUniqueId
						select x).FirstOrDefault();
					if (cardConnector2 != null)
					{
						connector.ConnectedNode = cardConnector2;
					}
				}
			}
		}
		foreach (GameCard allCard2 in this.AllCards)
		{
			if (!string.IsNullOrEmpty(allCard2.CardData.ParentUniqueId))
			{
				GameCard cardWithUniqueId = this.GetCardWithUniqueId(allCard2.CardData.ParentUniqueId);
				if (cardWithUniqueId != null)
				{
					allCard2.SetParent(cardWithUniqueId);
				}
			}
		}
		foreach (GameCard allCard3 in this.AllCards)
		{
			if (!string.IsNullOrEmpty(allCard3.CardData.EquipmentHolderUniqueId))
			{
				GameCard cardWithUniqueId2 = this.GetCardWithUniqueId(allCard3.CardData.EquipmentHolderUniqueId);
				if (cardWithUniqueId2 != null)
				{
					cardWithUniqueId2.EquipmentChildren.Add(allCard3);
					allCard3.EquipmentHolder = cardWithUniqueId2;
					allCard3.IsEquipped = true;
				}
			}
		}
		foreach (GameCard allCard4 in this.AllCards)
		{
			if (allCard4.CardData.WorkerAmount > 0)
			{
				allCard4.WorkerTransformHolder.UpdateWorkerAmount(allCard4.CardData.WorkerAmount);
			}
			if (string.IsNullOrEmpty(allCard4.CardData.WorkerHolderUniqueId))
			{
				continue;
			}
			GameCard cardWithUniqueId3 = this.GetCardWithUniqueId(allCard4.CardData.WorkerHolderUniqueId);
			if (cardWithUniqueId3 != null)
			{
				if (cardWithUniqueId3.WorkerChildren.Count < cardWithUniqueId3.CardData.WorkerAmount)
				{
					cardWithUniqueId3.WorkerChildren.Add(allCard4);
					allCard4.WorkerHolder = cardWithUniqueId3;
					allCard4.IsWorking = true;
				}
				else
				{
					allCard4.CardData.WorkerHolderUniqueId = null;
					allCard4.IsWorking = false;
				}
			}
		}
		foreach (GameCard allCard5 in this.AllCards)
		{
			allCard5.StatusEffectsChanged();
		}
		foreach (SavedConflict savedConflict in saveRound.SavedConflicts)
		{
			Conflict.CreateFromSavedConflict(savedConflict);
		}
		foreach (SavedBooster savedBooster2 in saveRound.SavedBoosters)
		{
			Boosterpack boosterpack = this.CreateBoosterpack(savedBooster2.Position, savedBooster2.BoosterId);
			if (!(boosterpack != null))
			{
				continue;
			}
			boosterpack.MyBoard = this.GetBoardWithId(savedBooster2.BoardId);
			int num = savedBooster2.TimesOpened;
			boosterpack.TimesOpened = savedBooster2.TimesOpened;
			for (int j = 0; j < boosterpack.CardBags.Count; j++)
			{
				CardBag cardBag = boosterpack.CardBags[j];
				int num2 = Mathf.Min(num, cardBag.CardsInPack);
				cardBag.CardsInPack -= num2;
				num -= num2;
				if (num <= 0)
				{
					break;
				}
			}
		}
		foreach (SavedBoosterBox savedBooster in saveRound.SavedBoosterBoxes)
		{
			BuyBoosterBox buyBoosterBox = this.AllBoosterBoxes.Find((BuyBoosterBox x) => x.BoosterId == savedBooster.BoosterId);
			if (buyBoosterBox != null)
			{
				buyBoosterBox.StoredCostAmount = savedBooster.StoredCostAmount;
			}
		}
		if (saveRound.SaveVersion != 3)
		{
			this.PerformSaveRoundMigration(saveRound.SaveVersion, 3);
		}
		this.IsLoadingSaveRound = false;
	}

	public void PerformSaveRoundMigration(int oldVersion, int newVersion)
	{
		if (oldVersion == 0 && newVersion == 1)
		{
			UnityEngine.Debug.Log($"Performing save round migration from v{oldVersion} to v{newVersion}");
			foreach (GameCard allCard in this.AllCards)
			{
				if (allCard.CardData is BaseVillager baseVillager)
				{
					baseVillager.HealthPoints = Mathf.Min(baseVillager.ProcessedCombatStats.MaxHealth, baseVillager.HealthPoints * 3);
				}
			}
			for (int num = this.AllCards.Count - 1; num >= 0; num--)
			{
				if (this.AllCards[num].CardData is Combatable combatable)
				{
					if (combatable.Id == "swordsman")
					{
						combatable.CreateAndEquipCard("sword", markAsFound: true);
					}
					if (combatable.Id == "explorer")
					{
						combatable.CreateAndEquipCard("map", markAsFound: true);
					}
					if (combatable.Id == "militia")
					{
						combatable.CreateAndEquipCard("spear", markAsFound: true);
					}
					if (combatable.Id == "fisher")
					{
						combatable.CreateAndEquipCard("fishing_rod", markAsFound: true);
					}
				}
			}
		}
		if (oldVersion == 1 && newVersion == 2 && this.BoardMonths.IsEmpty && this.MonthTimer > 0f)
		{
			this.BoardMonths = new BoardMonths();
			this.BoardMonths.MainMonth = this.OldCurrentMonth - this.CurrentRunVariables.IslandMonths;
			this.BoardMonths.IslandMonth = this.CurrentRunVariables.IslandMonths;
			this.BoardMonths.DeathMonth = Mathf.Max(1, this.CurrentRunVariables.DeathMonths);
		}
		if (oldVersion == 2 && newVersion == 3)
		{
			List<GameCard> list = this.AllCards.Where((GameCard x) => x.CardData.Id == "strange_portal").ToList();
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].DestroyCard();
			}
		}
	}

	public GameBoard GetBoardWithId(string id)
	{
		foreach (GameBoard board in this.Boards)
		{
			if (board.Id == id)
			{
				return board;
			}
		}
		return null;
	}

	public GameBoard GetBoardWithLocation(Location loc)
	{
		foreach (GameBoard board in this.Boards)
		{
			if (board.Location == loc)
			{
				return board;
			}
		}
		return null;
	}

	public bool BoughtWithGoldChest(GameCard card, int count)
	{
		return this.BoughtWithChest(card, count, "gold");
	}

	public bool BoughtWithShellChest(GameCard card, int count)
	{
		return this.BoughtWithChest(card, count, "shell");
	}

	private bool BoughtWithChest(GameCard card, int count, string heldCardId)
	{
		return card.GetAllCardsInStack().Sum((GameCard x) => (x.CardData is Chest chest && chest.HeldCardId == heldCardId) ? chest.CoinCount : 0) >= count;
	}

	public int GetAmountInChest(GameCard card, string heldCardId)
	{
		return card.GetAllCardsInStack().Sum((GameCard x) => (x.CardData is Chest chest && chest.HeldCardId == heldCardId) ? chest.CoinCount : 0);
	}

	public void BuyWithChest(GameCard childCard, int toUse)
	{
		List<Chest> list = (from x in childCard.GetAllCardsInStack()
			where x.CardData is Chest
			select x.CardData as Chest).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Chest chest = list[i];
			int num = Mathf.Min(toUse, chest.CoinCount);
			chest.CoinCount -= num;
			toUse -= num;
			if (toUse <= 0)
			{
				break;
			}
		}
		if (childCard.HasParent)
		{
			childCard.RemoveFromStack();
			childCard.SendIt();
		}
	}

	public bool BoughtWithGold(GameCard card, int count, bool checkStackAllSame = false)
	{
		return this.GetCardCountInStack(card, (CardData x) => x.Id == "gold") >= count;
	}

	public bool BoughtWithShells(GameCard card, int count, bool checkStackAllSame = false)
	{
		return this.GetCardCountInStack(card, (CardData x) => x.Id == "shell") >= count;
	}

	public int GetDollarsInCreditcard(GameCard card)
	{
		return card.GetAllCardsInStack().Sum((GameCard x) => (x.CardData is Creditcard creditcard) ? creditcard.DollarCount : 0);
	}

	public void BuyWithCreditcard(GameCard childCard, int toUse)
	{
		List<Creditcard> list = (from x in childCard.GetAllCardsInStack()
			where x.CardData is Creditcard
			select x.CardData as Creditcard).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Creditcard creditcard = list[i];
			int num = Mathf.Min(toUse, creditcard.DollarCount);
			creditcard.DollarCount -= num;
			toUse -= num;
			if (toUse <= 0)
			{
				break;
			}
		}
		if (childCard.HasParent)
		{
			childCard.RemoveFromStack();
			childCard.SendIt();
		}
	}

	public void RemoveCardsFromStack(GameCard childCard, int toRemove)
	{
		for (int i = 0; i < toRemove; i++)
		{
			childCard.GetLeafCard().DestroyCard(spawnSmoke: true);
		}
		if (childCard != null && childCard.HasParent)
		{
			childCard.RemoveFromParent();
		}
	}

	public void RemoveCardsFromStackPred(GameCard card, int toRemove, Predicate<GameCard> pred)
	{
		List<GameCard> list = card.GetAllCardsInStack().FindAll(pred);
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		int num = 0;
		foreach (GameCard item in list)
		{
			if (num == toRemove)
			{
				break;
			}
			allCardsInStack.Remove(item);
			item.RemoveFromStack();
			item.DestroyCard(spawnSmoke: true);
			num++;
		}
		this.Restack(allCardsInStack);
	}

	private void ClearRound()
	{
		this.QuestsCompleted = 0;
		this.NewCardsFound = 0;
		this.MonthTimer = 0f;
		this.BoardMonths = new BoardMonths();
		if (CitiesManager.instance != null)
		{
			CitiesManager.instance.Wellbeing = CitiesManager.instance.WellbeingStart;
			CitiesManager.instance.NextConflictMonth = 0;
		}
		this.GivenCards.Clear();
		this.BoughtBoosterIds.Clear();
		for (int num = this.AllCards.Count - 1; num >= 0; num--)
		{
			if (num <= 0 || num < this.AllCards.Count)
			{
				this.AllCards[num].DestroyCard(spawnSmoke: false, playSound: false);
			}
		}
		for (int num2 = this.AllBoosters.Count - 1; num2 >= 0; num2--)
		{
			UnityEngine.Object.Destroy(this.AllBoosters[num2].gameObject);
		}
	}

	public HitText CreateHitText(Vector3 pos, string text, HitText prefab)
	{
		HitText hitText = UnityEngine.Object.Instantiate(prefab);
		hitText.transform.position = pos;
		hitText.TextMesh.text = text;
		return hitText;
	}

	public void CheckDebugInput()
	{
		if (InputController.instance.GetKeyDown(Key.Period))
		{
			CitiesManager.instance.AddWellbeing(5);
		}
		if (InputController.instance.GetKeyDown(Key.Comma))
		{
			CitiesManager.instance.AddWellbeing(-5);
		}
		if (this.HoveredCard != null && InputController.instance.GetKeyDown(Key.Z))
		{
			GameCard cardWithStatusInStack = this.HoveredCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null)
			{
				cardWithStatusInStack.CurrentTimerTime = cardWithStatusInStack.TargetTimerTime;
			}
		}
		if (InputController.instance.GetKeyDown(Key.M))
		{
			AudioManager.me.SkipSong();
		}
		if (InputController.instance.GetKeyDown(Key.U) && this.HoveredCard != null)
		{
			this.HoveredCard.CardData.StatusEffects.Clear();
			this.HoveredCard.CardData.IsDamaged = false;
			this.HoveredCard.CardData.DamageType = CardDamageType.None;
			this.HoveredCard.StatusEffectsChanged();
		}
		if (InputController.instance.GetKeyDown(Key.L) && this.HoveredCard != null)
		{
			BaseVillager card = this.GetCard<BaseVillager>();
			this.HoveredCard.CardAnimations.Add(new CardAnimation_FakeMeleeAttack(this.HoveredCard, card.MyGameCard));
		}
		if (InputController.instance.GetKeyDown(Key.H))
		{
			Combatable combatable = this.HoveredCard?.CardData as Combatable;
			if (combatable != null)
			{
				combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			}
		}
		if (InputController.instance.GetKeyDown(Key.R) && (this.HoveredCard != null || this.HoveredDraggable != null))
		{
			if (this.HoveredCard != null)
			{
				if (this.HoveredCard.CardData is Combatable combatable2)
				{
					combatable2.Damage(100);
				}
				else
				{
					this.HoveredCard.DestroyCard();
				}
			}
			else if (this.HoveredDraggable != null && this.HoveredDraggable is Boosterpack)
			{
				UnityEngine.Object.Destroy(this.HoveredDraggable.gameObject);
			}
		}
		if (InputController.instance.GetKeyDown(Key.G) && this.HoveredCard != null)
		{
			if (!this.HoveredCard.CardData.IsFoil)
			{
				this.HoveredCard.CardData.SetFoil();
			}
			else
			{
				this.HoveredCard.CardData.IsFoil = false;
				if (this.HoveredCard.CardData.Value != -1)
				{
					this.HoveredCard.CardData.Value /= 5;
				}
				if (this.HoveredCard.CardData.CitiesValue != -1)
				{
					this.HoveredCard.CardData.CitiesValue /= 5;
				}
			}
		}
		if (InputController.instance.GetKeyDown(Key.F) && this.HoveredCard != null)
		{
			this.CreateFloatingText(this.HoveredCard, isPositive: true, 5, "Test hovered", Icons.Wellbeing, desiredBehaviour: true, 0, 0f, closeOnHover: true);
		}
		if (InputController.instance.GetKeyDown(Key.C) && this.HoveredCard != null)
		{
			CardData cardData = this.CreateCard(this.HoveredCard.transform.position, this.HoveredCard.CardData, faceUp: true, checkAddToStack: false);
			if (cardData is Worker worker)
			{
				worker.Housing = null;
			}
			this.StackSendTo(cardData.MyGameCard, this.HoveredCard);
		}
		if (InputController.instance.GetKeyDown(Key.O) && this.HoveredCard != null && this.HoveredCard.CardData is BaseVillager baseVillager)
		{
			baseVillager.Age++;
		}
	}

	private bool SpiritDLCInstalled()
	{
		if (Application.isEditor)
		{
			return DebugOptions.Default.SpiritDlcEnabled;
		}
		if (SteamManager.Initialized && SteamApps.BIsDlcInstalled(new AppId_t(2446110u)))
		{
			UnityEngine.Debug.Log("Load Spirit DLC");
			return true;
		}
		return false;
	}

	private bool CitiesDLCInstalled()
	{
		if (Application.isEditor)
		{
			return DebugOptions.Default.CitiesDlcEnabled;
		}
		if (SteamManager.Initialized && SteamApps.BIsDlcInstalled(new AppId_t(2867570u)))
		{
			UnityEngine.Debug.Log("Load Cities DLC");
			return true;
		}
		return false;
	}

	public bool IsSpiritDlcActive()
	{
		return this.GameDataLoader.SpiritDlcLoaded;
	}

	public bool IsCitiesDlcActive()
	{
		return this.GameDataLoader.CitiesDlcLoaded;
	}

	public bool CurseIsActive(CurseType curseType)
	{
		foreach (Curse activeCurse in this.ActiveCurses)
		{
			if (activeCurse.MyGameCard.MyBoard.IsCurrent && activeCurse.CurseType == curseType)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAllCardsFromBoard(string boardId, bool removeBoosters = true)
	{
		foreach (GameCard item in this.GetAllCardsOnBoard(boardId))
		{
			item.DestroyCard();
		}
		if (removeBoosters)
		{
			this.RemoveAllBoostersFromBoard(boardId);
		}
	}

	public void RemoveAllBoostersFromBoard(string boardId)
	{
		foreach (Boosterpack item in this.GetAllBoostersOnBoard(boardId))
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public void CheckSpiritCutscenes()
	{
		if (this.CurseIsActive(CurseType.Happiness) && this.CurrentBoard.Id == "happiness")
		{
			int happinessCount = this.GetHappinessCount();
			if (happinessCount >= 5)
			{
				this.QueueCutsceneIfNotPlayed("happiness_middle");
			}
			if (happinessCount >= 10)
			{
				this.QueueCutsceneIfNotPlayed("happiness_end");
			}
		}
		if (this.CurseIsActive(CurseType.Death) && this.CurrentBoard.Id == "death" && this.AllBoosterBoxes.Any((BuyBoosterBox x) => x.BoosterId == "death_locations" && x.Booster.IsUnlocked))
		{
			this.QueueCutsceneIfNotPlayed("death_end");
		}
	}
}
