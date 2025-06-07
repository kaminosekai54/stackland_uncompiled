using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameCard : Draggable, IGameCardOrCardData
{
	private enum PositionType
	{
		InConflict,
		InAttack,
		InStack,
		IsRoot,
		IsEquipped,
		InAnimation,
		None,
		IsWorking
	}

	public TextMeshPro CardNameText;

	public SpriteRenderer IconRenderer;

	public CardData CardData;

	public GameCard Parent;

	public GameCard Child;

	public GameCard LastParent;

	public Transform HitTextPosition;

	public Transform Visuals;

	public int ConnectorOutputIndex;

	public bool IsEquipped;

	public bool IsWorking;

	public bool ShowInventory;

	public GameCard EquipmentHolder;

	public List<GameCard> EquipmentChildren;

	public GameCard WorkerHolder;

	public List<GameCard> WorkerChildren = new List<GameCard>();

	public GameObject EnergyConnectorPrefab;

	public Transform EnergyConnectorTransform;

	private Vector3 startScale;

	public Renderer CardRenderer;

	public Rectangle HighlightRectangle;

	public SpriteRenderer CoinIcon;

	public TextMeshPro CoinText;

	public TextMeshPro SpecialText;

	public SpriteRenderer SpecialIcon;

	public SpriteRenderer CombatStatusCircle;

	public SpriteRenderer DropShadowRenderer;

	public Transform EquipmentRectangle;

	public Transform WorkerRectangle;

	public WorkerTransformHolder WorkerTransformHolder;

	public InventoryInteractable InventoryInteractable;

	public InventoryInteractable WorkerInventoryInteractable;

	public OnOffInteractable OnOffInteractable;

	private Vector3 onOffBasePosition;

	private Vector3 onOffTargetPosition;

	public SpriteRenderer HeadInventoryIcon;

	public SpriteRenderer TorsoInventoryIcon;

	public SpriteRenderer HandInventoryIcon;

	public SpriteRenderer WorkerInventoryIcon;

	public GameObject HeadEquipmentPosition;

	public GameObject TorsoEquipmentPosition;

	public GameObject HandEquipmentPosition;

	public Rectangle EquipmentButton;

	public Rectangle WorkerButton;

	public int? SpecialValue;

	public bool HighlightActive;

	private Vector3 lastPosition;

	public Vector3 SpawnRotation;

	private bool snappedToParent;

	private MaterialPropertyBlock propBlock;

	private MaterialPropertyBlock combatCirclePropBlock;

	public bool FaceUp;

	public SpriteRenderer NewCircle;

	private Vector3 newCircleStartSize;

	public ParticleSystem FoilParticles;

	protected List<MaterialChanger> materialChangers = new List<MaterialChanger>();

	[HideInInspector]
	public bool IsDemoCard;

	public GameCard BounceTarget;

	[HideInInspector]
	public bool PushEnabled = true;

	[HideInInspector]
	public bool SetY = true;

	[Header("Status")]
	public float DistanceBetweenStatusses = 0.01f;

	[HideInInspector]
	public List<StatusEffectElement> StatusEffectElements = new List<StatusEffectElement>();

	public Vector3 equipmentRectangleStartOffset;

	[HideInInspector]
	public bool ShowSpecialIcon;

	[HideInInspector]
	public bool StackUpdate;

	private CardPalette myCardPalette;

	public List<CardAnimation> CardAnimations = new List<CardAnimation>();

	[HideInInspector]
	private Action closeToTargetPositionCallback;

	[HideInInspector]
	public List<CardConnector> CardConnectorChildren = new List<CardConnector>();

	public Color CombatCircleColor;

	private int propColor = Shader.PropertyToID("_Color");

	private int propColor2 = Shader.PropertyToID("_Color2");

	private int propIconColor = Shader.PropertyToID("_IconColor");

	private int propHasSecondaryIcon = Shader.PropertyToID("_HasSecondaryIcon");

	private int propHasOutputDir = Shader.PropertyToID("_HasOutputDir");

	private int propSecondaryTex = Shader.PropertyToID("_SecondaryTex");

	private int propBigShineStrength = Shader.PropertyToID("_BigShineStrength");

	private int propShineStrength = Shader.PropertyToID("_ShineStrength");

	private int propFoil = Shader.PropertyToID("_Foil");

	private int propDamaged = Shader.PropertyToID("_Damaged");

	private int propIconTex = Shader.PropertyToID("_IconTex");

	[HideInInspector]
	public bool Destroyed;

	private List<GameCard> cardsInvolved = new List<GameCard>();

	public bool WasClicked;

	public bool IsNew;

	public float ZRotOffset;

	private Vector3 onOffVelocity;

	private Vector3 onOffTargetPos;

	private Color colOff = new Color(0f, 0f, 0f, 0.5f);

	private Color colOn = new Color(0f, 0f, 0f, 1f);

	private float ConnectorAmountOffset = 0.077f;

	private float CardTextOffset = 0.01f;

	public Rectangle StatusEffectBackground;

	private Transform statusEffectBackgroundTransform;

	private float statusEffectBackgroundWidth;

	private float flipTimer;

	public float RotWobbleAmp = 1f;

	public float RotWobbleSpeed = 1f;

	public float RotWobbleSpringiness = 1f;

	private float wobbleRotVelo;

	public bool AutoRotWobble;

	public float AutoRotWobbleTimer;

	public float AutoRotWobbleAmount = 0.1f;

	private float timer;

	private float rotWobbleTimer;

	private float curZ = 270f;

	public bool TimerRunning;

	public string Status;

	public float CurrentTimerTime;

	public float TargetTimerTime;

	public TimerAction TimerAction;

	public string TimerBlueprintId;

	public int TimerSubprintIndex;

	public bool SkipCitiesChecks;

	public string TimerActionId;

	public Statusbar CurrentStatusbar;

	[HideInInspector]
	public GameCard removedChild;

	public Transform StatusEffectElementParent;

	private float curHeight;

	[HideInInspector]
	public bool IsHit;

	protected override bool HasPhysics => true;

	public Vector3 Position => base.transform.position;

	public override bool IsHovered => base.IsHovered;

	public static float CardHeight => PrefabManager.instance.GameCardPrefab.GetHeight();

	public bool BeingHovered
	{
		get
		{
			if (WorldManager.instance.HoveredCard == this)
			{
				return true;
			}
			if (this.IsParentOf(WorldManager.instance.HoveredCard) || this.IsChildOf(WorldManager.instance.HoveredCard))
			{
				return true;
			}
			return false;
		}
	}

	public override Vector3 AutoMoveSnapPosition
	{
		get
		{
			if (WorldManager.instance != null && WorldManager.instance.DraggingCard != null)
			{
				return this.CardNameText.transform.position + new Vector3(0f, WorldManager.instance.CardOverlayHeightOffset, 0f - WorldManager.instance.CardOverlayOffset);
			}
			if (this.Child == null && this.Parent == null)
			{
				return base.transform.position;
			}
			return this.CardNameText.transform.position;
		}
	}

	public override bool CanBeAutoMovedTo
	{
		get
		{
			if (WorldManager.instance.DraggingCard != null && this.Child != null)
			{
				return false;
			}
			if (this.IsEquipped && (WorldManager.instance.DraggingCard == this.EquipmentHolder || !this.EquipmentHolder.ShowInventory))
			{
				return false;
			}
			if (this.IsWorking && (WorldManager.instance.DraggingCard == this.WorkerHolder || !this.WorkerHolder.ShowInventory))
			{
				return false;
			}
			return !base.BeingDragged;
		}
	}

	public bool InventoryVisible => this.ShowInventory;

	public bool IsWorkerInventory
	{
		get
		{
			CardData cardData = this.CardData;
			if ((object)cardData == null)
			{
				return false;
			}
			return cardData.WorkerAmount > 0;
		}
	}

	public bool TimerRunningInStack => this.GetAllCardsInStack().Any((GameCard x) => x.TimerRunning);

	public bool HasParent => this.Parent != null;

	public bool HasChild => this.Child != null;

	protected override float Mass
	{
		get
		{
			float num = 1f;
			if (this.CardData is Mob)
			{
				num += 50f;
			}
			if (this.CardData.MyCardType == CardType.Structures && this.CardData.IsBuilding)
			{
				num += 8f;
			}
			if (this.CardData is HeavyFoundation)
			{
				num += 1000f;
			}
			if (this.Child != null)
			{
				num += this.Child.Mass;
			}
			return num;
		}
	}

	public bool IsCollapsed
	{
		get
		{
			if (!base.BeingDragged)
			{
				return false;
			}
			if (WorldManager.instance.NearbyCardTarget != null)
			{
				return true;
			}
			if (this.GetRootCard().GetChildCount() >= 10 && !WorldManager.instance.IsShiftDragging)
			{
				return true;
			}
			return false;
		}
	}

	public Combatable Combatable => this.CardData as Combatable;

	public bool InConflict
	{
		get
		{
			if (this.Combatable != null)
			{
				return this.Combatable.InConflict;
			}
			return false;
		}
	}

	public bool InAttack
	{
		get
		{
			if (this.Combatable != null)
			{
				return this.Combatable.InAttack;
			}
			return false;
		}
	}

	public GameCard TryGetNthChild(int n)
	{
		GameCard gameCard = this;
		for (int i = 0; i < n; i++)
		{
			if (gameCard.Child != null)
			{
				gameCard = gameCard.Child;
				continue;
			}
			return null;
		}
		return gameCard;
	}

	protected override void Awake()
	{
		base.Awake();
		base.transform.rotation = Quaternion.Euler(270f, 90f, 90f);
		this.propBlock = new MaterialPropertyBlock();
		this.combatCirclePropBlock = new MaterialPropertyBlock();
		base.GetComponentsInChildren(includeInactive: true, this.materialChangers);
		MaterialChanger component = base.GetComponent<MaterialChanger>();
		if (component != null)
		{
			this.materialChangers.Add(component);
		}
		foreach (MaterialChanger materialChanger in this.materialChangers)
		{
			materialChanger.Init();
		}
		this.CombatStatusCircle.gameObject.SetActiveFast(active: false);
		this.DropShadowRenderer.enabled = false;
		this.newCircleStartSize = this.NewCircle.transform.localScale;
		this.NewCircle.gameObject.SetActiveFast(active: true);
		this.NewCircle.transform.localScale = Vector3.zero;
		this.CombatCircleColor = this.CombatStatusCircle.color;
		this.StatusEffectBackground.transform.localScale = Vector3.zero;
		ParticleSystem.EmissionModule emission = this.FoilParticles.emission;
		emission.enabled = false;
		this.EquipmentRectangle.gameObject.SetActiveFast(active: true);
		this.WorkerRectangle.gameObject.SetActiveFast(active: true);
		this.SpecialText.gameObject.SetActiveFast(active: false);
		this.CoinText.gameObject.SetActiveFast(active: false);
		this.CoinIcon.gameObject.SetActiveFast(active: false);
		this.statusEffectBackgroundTransform = this.StatusEffectBackground.transform;
	}

	protected override void Start()
	{
		this.startScale = base.transform.localScale;
		if (this.IsDemoCard)
		{
			this.startScale *= 0.2f;
			base.transform.localScale = this.startScale;
		}
		this.UpdateIcon();
		this.lastPosition = (base.TargetPosition = base.transform.position);
		this.UpdateCardPalette();
		this.SetColors();
		this.HighlightRectangle.enabled = false;
		if (!WorldManager.instance.AllCards.Contains(this) && !this.IsDemoCard)
		{
			WorldManager.instance.AllCards.Add(this);
		}
		if (!WorldManager.instance.UniqueIdToCard.ContainsKey(this.CardData.UniqueId))
		{
			WorldManager.instance.UniqueIdToCard[this.CardData.UniqueId] = this;
		}
		this.onOffBasePosition = this.OnOffInteractable.transform.localPosition;
		this.onOffTargetPosition = this.onOffBasePosition + new Vector3(0.09f, 0f, 0f);
		this.onOffTargetPos = this.onOffBasePosition;
		this.OnOffInteractable.gameObject.SetActive(value: false);
		if (!this.CardData.HasInventory)
		{
			UnityEngine.Object.Destroy(this.HeadEquipmentPosition);
			UnityEngine.Object.Destroy(this.HandEquipmentPosition);
			UnityEngine.Object.Destroy(this.TorsoEquipmentPosition);
		}
	}

	public void UpdateIcon()
	{
		if (this.CardData.MyCardType == CardType.Ideas)
		{
			if (this.CardData.CardUpdateType == CardUpdateType.Main)
			{
				this.IconRenderer.sprite = SpriteManager.instance.IdeaIcon;
			}
			else if (this.CardData.CardUpdateType == CardUpdateType.Island)
			{
				this.IconRenderer.sprite = SpriteManager.instance.IslandIdeaIcon;
			}
			else if (this.CardData.CardUpdateType == CardUpdateType.Spirit)
			{
				this.IconRenderer.sprite = SpriteManager.instance.SpiritIdeaIcon;
			}
			else if (this.CardData.CardUpdateType == CardUpdateType.Cities)
			{
				this.IconRenderer.sprite = SpriteManager.instance.CitiesIdeaIcon;
			}
			else
			{
				this.IconRenderer.sprite = SpriteManager.instance.IdeaIcon;
			}
		}
		if (this.CardData.Icon != null)
		{
			this.IconRenderer.sprite = this.CardData.Icon;
		}
	}

	public void UpdateCardPalette()
	{
		this.myCardPalette = ColorManager.instance.GetPaletteForCard(this.CardData);
	}

	public void ToggleDirection()
	{
		if (this.CardData.OutputDir == Vector3.zero)
		{
			this.CardData.OutputDir = Vector3.right;
		}
		else if (this.CardData.OutputDir == Vector3.right)
		{
			this.CardData.OutputDir = Vector3.back;
		}
		else if (this.CardData.OutputDir == Vector3.back)
		{
			this.CardData.OutputDir = Vector3.left;
		}
		else if (this.CardData.OutputDir == Vector3.left)
		{
			this.CardData.OutputDir = Vector3.forward;
		}
		else if (this.CardData.OutputDir == Vector3.forward)
		{
			this.CardData.OutputDir = Vector3.zero;
		}
		QuestManager.instance.SpecialActionComplete("output_direction_changed", this.CardData);
	}

	private void SetColors()
	{
		this.CombatStatusCircle.color = this.CombatCircleColor;
		this.CombatStatusCircle.color = Color.red;
		if (this.myCardPalette == null)
		{
			Debug.LogError("Could not find card color pallet");
			return;
		}
		Color color = this.myCardPalette.Color;
		Color value = this.myCardPalette.Color2;
		Color color2 = this.myCardPalette.Icon;
		if (this.IsHit)
		{
			this.CombatStatusCircle.color = Color.white;
			color = (value = (color2 = Color.white));
		}
		this.CardRenderer.shadowCastingMode = ((!this.IsEquipped && !this.IsWorking) ? ShadowCastingMode.On : ShadowCastingMode.Off);
		this.CardRenderer.GetPropertyBlock(this.propBlock, 2);
		this.propBlock.SetColor(this.propColor, color);
		this.propBlock.SetColor(this.propColor2, value);
		this.propBlock.SetColor(this.propIconColor, color2);
		Texture2D texture2D = null;
		bool flag = false;
		if (this.CardData is ResourceChest || this.CardData is FoodWarehouse)
		{
			texture2D = SpriteManager.instance.ChestIconSecondary.texture;
		}
		else if (this.CardData is ResourceMagnet)
		{
			texture2D = SpriteManager.instance.MagnetIconSecondary.texture;
		}
		bool flag2 = texture2D != null;
		this.propBlock.SetFloat(this.propHasSecondaryIcon, flag2 ? 1f : 0f);
		this.propBlock.SetFloat(this.propHasOutputDir, flag ? 1f : 0f);
		if (texture2D != null)
		{
			this.propBlock.SetTexture(this.propSecondaryTex, texture2D);
		}
		float value2 = ((this.CardData is Equipable) ? 0.3f : 1f);
		this.propBlock.SetFloat(this.propBigShineStrength, (this.CardData is Equipable) ? 0f : 1f);
		this.propBlock.SetFloat(this.propShineStrength, value2);
		this.propBlock.SetFloat(this.propFoil, (this.CardData.IsFoil || this.CardData.IsShiny || this.CardData is Equipable) ? 1f : 0f);
		this.propBlock.SetFloat(this.propDamaged, this.CardData.IsDamaged ? 1f : 0f);
		if (this.IconRenderer.sprite != null)
		{
			this.propBlock.SetTexture(this.propIconTex, this.IconRenderer.sprite.texture);
		}
		else
		{
			this.propBlock.SetTexture(this.propIconTex, SpriteManager.instance.EmptyTexture.texture);
		}
		this.CardRenderer.SetPropertyBlock(this.propBlock, 2);
		if (this.SpecialText.color != color)
		{
			this.SpecialText.color = color;
		}
		this.SpecialIcon.color = color2;
		this.IconRenderer.color = color2;
		if (this.CoinText.color != color)
		{
			this.CoinText.color = color;
		}
		this.CoinIcon.color = color2;
		if (this.EquipmentButton.Color != color)
		{
			this.EquipmentButton.Color = color;
		}
		if (this.WorkerButton.Color != color)
		{
			this.WorkerButton.Color = color;
		}
		Color color3 = color2;
		color3.a = 0.5f;
		this.WorkerInventoryIcon.color = (this.HasAnyWorkers() ? color2 : color3);
		if (this.CardNameText.color != color2)
		{
			this.CardNameText.color = color2;
		}
	}

	private static Sprite GetSpriteForAttackType(AttackType type)
	{
		return type switch
		{
			AttackType.Magic => SpriteManager.instance.MagicFightIcon, 
			AttackType.Melee => SpriteManager.instance.MeleeFightIcon, 
			AttackType.Ranged => SpriteManager.instance.RangedFightIcon, 
			AttackType.Foot => SpriteManager.instance.FootFightIcon, 
			AttackType.Armour => SpriteManager.instance.ArmourFightIcon, 
			AttackType.Air => SpriteManager.instance.AirFightIcon, 
			_ => null, 
		};
	}

	protected override void OnDestroy()
	{
		if (WorldManager.instance != null)
		{
			WorldManager.instance.AllCards.Remove(this);
			if (WorldManager.instance.UniqueIdToCard.ContainsKey(this.CardData.UniqueId) && WorldManager.instance.UniqueIdToCard[this.CardData.UniqueId] == this)
			{
				WorldManager.instance.UniqueIdToCard.Remove(this.CardData.UniqueId);
			}
		}
		base.OnDestroy();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(base.debugBounds.center, base.debugBounds.size);
	}

	public virtual void DestroyCard(bool spawnSmoke = false, bool playSound = true)
	{
		this.RemoveFromStack();
		WorldManager.instance.AllCards.Remove(this);
		WorldManager.instance.UniqueIdToCard.Remove(this.CardData.UniqueId);
		this.Destroyed = true;
		this.CardData.OnDestroyCard();
		if (playSound)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.CardDestroy, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);
		}
		if (spawnSmoke)
		{
			WorldManager.instance.CreateSmoke(base.transform.position);
		}
		if (this.CardData is Curse item)
		{
			WorldManager.instance.ActiveCurses.Remove(item);
		}
		if (this.CardData.HasInventory)
		{
			foreach (GameCard equipmentChild in this.EquipmentChildren)
			{
				equipmentChild.EquipmentHolder = null;
				equipmentChild.IsEquipped = false;
				equipmentChild.DestroyCard(spawnSmoke: false, playSound: false);
			}
		}
		if (this.CardData.WorkerAmount > 0)
		{
			foreach (GameCard workerChild in this.WorkerChildren)
			{
				workerChild.WorkerHolder = null;
				workerChild.IsWorking = false;
				workerChild.DestroyCard(spawnSmoke: false, playSound: false);
			}
		}
		if (this.Combatable != null && this.Combatable.InConflict)
		{
			this.Combatable.MyConflict.LeaveConflict(this.Combatable);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SetChild(GameCard card)
	{
		this.cardsInvolved.Clear();
		this.cardsInvolved.Add(this);
		if (card == this)
		{
			Debug.LogError("Child is same as Parent");
		}
		else if (card == null)
		{
			if (this.Child != null)
			{
				this.cardsInvolved.Add(this.Child);
				this.Child.Parent = null;
			}
			this.Child = null;
			this.NotifyStackUpdate(this.cardsInvolved);
		}
		else
		{
			this.Child = card;
			card.Parent = this;
			this.cardsInvolved.Add(card);
			this.NotifyStackUpdate(this.cardsInvolved);
		}
	}

	public void SetParent(GameCard card)
	{
		this.cardsInvolved.Clear();
		this.cardsInvolved.Add(this);
		if (card == this)
		{
			Debug.LogError("Child is same as Parent");
		}
		else if (card == null)
		{
			if (this.Parent != null)
			{
				this.cardsInvolved.Add(this.Parent);
				this.Parent.Child = null;
			}
			this.Parent = null;
			this.NotifyStackUpdate(this.cardsInvolved);
		}
		else
		{
			this.Parent = card;
			card.Child = this;
			this.cardsInvolved.Add(card);
			this.NotifyStackUpdate(this.cardsInvolved);
		}
	}

	public void RemoveFromStack()
	{
		this.SetParent(null);
		this.SetChild(null);
	}

	private void NotifyStackUpdate(List<GameCard> cardsInvolved)
	{
		foreach (GameCard item in cardsInvolved)
		{
			item.GetRootCard().StackUpdate = true;
			item.StackUpdate = true;
		}
	}

	public void RemoveFromParent()
	{
		if (this.Parent != null)
		{
			this.Parent.SetChild(null);
		}
		this.SetParent(null);
	}

	public override bool CanBePushed()
	{
		if (this.CardData is Food && WorldManager.instance.InEatingAnimation)
		{
			return false;
		}
		if (this.CardData is Spirit || this.CardData is CityAdvisor)
		{
			return false;
		}
		if (this.IsWorking || this.IsEquipped)
		{
			return false;
		}
		if (!base.BeingDragged)
		{
			return this.PushEnabled;
		}
		return false;
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		if (this.IsEquipped || this.IsWorking)
		{
			return false;
		}
		if (draggable is Boosterpack && WorldManager.instance.CurrentBoard.Id == "cities" && this.GetRootCard().CardData.MyCardType == CardType.Structures)
		{
			return false;
		}
		if (draggable is GameCard gameCard)
		{
			if (gameCard.IsChildOf(this) || gameCard.IsParentOf(this))
			{
				return false;
			}
			if (gameCard.BounceTarget != null)
			{
				return false;
			}
			if (gameCard.Destroyed)
			{
				return false;
			}
			if (!gameCard.PushEnabled)
			{
				return false;
			}
			if (gameCard.CardData is Food && WorldManager.instance.InEatingAnimation)
			{
				return false;
			}
			if (WorldManager.instance.CurrentBoard.Id == "cities" && this.GetRootCard().CardData.MyCardType == CardType.Structures && (gameCard.CardData is Resource || gameCard.CardData is Food))
			{
				return false;
			}
			if (gameCard.CardData is Spirit || gameCard.CardData is CityAdvisor)
			{
				return false;
			}
			if (gameCard.CardData is Energy)
			{
				return false;
			}
			if (gameCard.IsEquipped || gameCard.IsWorking)
			{
				return false;
			}
			if (!this.CardData.CanBePushedBy(gameCard.CardData))
			{
				return false;
			}
		}
		return base.CanBePushedBy(draggable);
	}

	public override bool CanBeDragged()
	{
		if (this.CardData is Combatable { BeingAttacked: not false })
		{
			return false;
		}
		if (WorldManager.instance.RemovingCards && this.GetRootCard().CardData is Boat { InSailOff: not false })
		{
			return false;
		}
		if (!base.BeingDragged && this.CardData.CanBeDragged)
		{
			return this.FaceUp;
		}
		return false;
	}

	public override void Clicked()
	{
		if (!this.FaceUp)
		{
			this.FaceUp = true;
		}
		if (base.DragTag == "inventory")
		{
			this.InventoryInteractable.Clicked();
			this.WorkerInventoryInteractable.Clicked();
		}
		else
		{
			this.CardData.Clicked();
		}
		this.WasClicked = true;
		base.Clicked();
	}

	public void ForceUpdate()
	{
		this.Update();
	}

	public void Equip(Equipable equipable)
	{
		GameCard myGameCard = equipable.MyGameCard;
		this.EquipmentChildren.Add(myGameCard);
		myGameCard.EquipmentHolder = this;
		myGameCard.IsEquipped = true;
		myGameCard.RemoveFromStack();
		this.CardData.OnEquipItem(equipable);
	}

	public void Unequip(Equipable equipable)
	{
		GameCard myGameCard = equipable.MyGameCard;
		this.EquipmentChildren.Remove(myGameCard);
		myGameCard.EquipmentHolder = null;
		myGameCard.IsEquipped = false;
		this.CardData.OnUnequipItem(equipable);
		if (this.Combatable != null && this.Combatable.HealthPoints > this.Combatable.ProcessedCombatStats.MaxHealth)
		{
			this.Combatable.HealthPoints = this.Combatable.ProcessedCombatStats.MaxHealth;
		}
	}

	public void EquipWorker(Worker worker, int index)
	{
		GameCard myGameCard = worker.MyGameCard;
		worker.WorkerIndex = index;
		this.WorkerChildren.Add(myGameCard);
		myGameCard.WorkerHolder = this;
		myGameCard.IsWorking = true;
		myGameCard.RemoveFromStack();
		this.CardData.OnEquipItem(null);
	}

	public void UnequipWorker(GameCard worker)
	{
		this.WorkerChildren.Remove(worker);
		worker.CardData.WorkerIndex = -1;
		worker.WorkerHolder = null;
		worker.IsWorking = false;
		this.GetRootCard().StackUpdate = true;
		this.CardData?.OnUnequipItem(null);
	}

	protected override void Bounce()
	{
		if (this.HasParent)
		{
			this.BounceTarget = null;
		}
		if (this.BounceTarget != null)
		{
			GameCard gameCard = this.BounceTarget;
			if (gameCard.Child != null)
			{
				gameCard = gameCard.GetLeafCard();
			}
			this.BounceTarget = null;
			if (gameCard == this || gameCard.BounceTarget != null || gameCard.GetCardInCombatInStack() != null || gameCard.BeingDragged)
			{
				return;
			}
			GameCard cardWithStatusInStack = gameCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && !cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus())
			{
				return;
			}
			if (gameCard.CardData.CanHaveCardOnTop(this.CardData))
			{
				this.SetParent(gameCard);
				base.Velocity = null;
				AudioManager.me.PlaySound2D(AudioManager.me.DropOnStack, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);
			}
		}
		base.Bounce();
	}

	protected override void Update()
	{
		if (!this.IsDemoCard && !base.MyBoard.IsCurrent)
		{
			return;
		}
		if (this.HasChild && this.CardData.IsDamaged)
		{
			if (this.CardData.DamageType == CardDamageType.Fire && this.Child.CardData.Id == "water")
			{
				this.Child.DestroyCard();
				this.CardData.SetCardUndamaged();
				WorldManager.instance.CreateSmoke(this.Position);
				AudioManager.me.PlaySound2D(AudioManager.me.ExtinguishCardSound, UnityEngine.Random.Range(0.9f, 1.1f), 0.3f);
			}
			else if (this.CardData.DamageType == CardDamageType.Drought && this.CardData.ChildrenMatchingPredicate((CardData x) => x.Id == "water").Count >= 3)
			{
				this.CardData.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == "water", 3);
				this.CardData.SetCardUndamaged();
				WorldManager.instance.CreateSmoke(this.Position);
				AudioManager.me.PlaySound2D(AudioManager.me.DroughtSolved, UnityEngine.Random.Range(0.9f, 1.1f), 0.3f);
			}
			else if (this.CardData.DamageType == CardDamageType.Damaged && this.Child.CardData is ICurrency && this.CardData.GetDollarCountInStack(includeInChest: true) >= this.CardData.GetRepairCost())
			{
				List<ICurrency> currencyList = this.CardData.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList();
				CitiesManager.instance.TryUseDollars(currencyList, this.CardData.GetRepairCost(), onlyTakeIfAmountMet: true);
				this.CardData.SetCardUndamaged();
				AudioManager.me.PlaySound2D(AudioManager.me.RepairCardSound, UnityEngine.Random.Range(0.9f, 1.1f), 0.3f);
			}
		}
		this.CardData.UpdateCard();
		this.SetColors();
		string text = this.CardData.Name;
		if (this.CardNameText.text != text)
		{
			this.CardNameText.text = this.CardData.Name;
		}
		Vector3 b = (this.IsNew ? this.newCircleStartSize : Vector3.zero);
		this.NewCircle.transform.localScale = Vector3.Lerp(this.NewCircle.transform.localScale, b, Time.deltaTime * 20f);
		bool flag = WorldManager.instance.DraggingCard != null && WorldManager.instance.DraggingCard.CardData.Id == this.CardData.Id;
		if (base.BeingDragged || this.WasClicked || this.Child != null || this.Parent != null || this.InConflict || this.GetCardWithStatusInStack() != null || flag || this.CardData is Spirit || this.CardData is CityAdvisor)
		{
			this.IsNew = false;
		}
		if (this.Child != null && !(this.Child.CardData is Equipable))
		{
			this.ShowInventory = false;
		}
		if (base.BeingDragged)
		{
			this.ShowInventory = false;
		}
		if (this.Combatable != null && this.Combatable.InAttack)
		{
			this.ShowInventory = false;
		}
		ParticleSystem.EmissionModule emission = this.FoilParticles.emission;
		emission.enabled = !this.IsDemoCard && (this.CardData.IsFoil || this.CardData.Id == "goblet");
		PerformanceHelper.SetActive(this.CombatStatusCircle.gameObject, this.InConflict || (this.Combatable != null && this.Combatable is Enemy && !this.IsDemoCard));
		if (this.Combatable != null)
		{
			this.CombatStatusCircle.sprite = GameCard.GetSpriteForAttackType(this.Combatable.ProcessedAttackType);
			this.CombatStatusCircle.GetPropertyBlock(this.combatCirclePropBlock);
			float value = (this.Combatable.InConflict ? this.Combatable.TimeToAttackNormalized : 1f);
			this.combatCirclePropBlock.SetFloat("_FillAmount", value);
			this.CombatStatusCircle.SetPropertyBlock(this.combatCirclePropBlock);
		}
		PerformanceHelper.SetActive(this.SpecialText.gameObject, this.SpecialValue.HasValue);
		if (this.SpecialValue.HasValue)
		{
			this.SpecialText.text = this.SpecialValue.Value.ToStringCached();
		}
		PerformanceHelper.SetActive(this.SpecialIcon.gameObject, this.SpecialValue.HasValue || this.ShowSpecialIcon);
		int value2 = this.CardData.GetValue();
		if (value2 != -1)
		{
			this.CoinText.text = value2.ToStringCached();
			PerformanceHelper.SetActive(this.CoinIcon.gameObject, active: true);
			PerformanceHelper.SetActive(this.CoinText.gameObject, active: true);
		}
		else
		{
			PerformanceHelper.SetActive(this.CoinIcon.gameObject, active: false);
			PerformanceHelper.SetActive(this.CoinText.gameObject, active: false);
		}
		this.UpdateShowInventory();
		this.UpdateShowWorkerInventory();
		if (this.CardData.HasInventory)
		{
			this.HandInventoryIcon.color = (this.CardData.HasEquipableOfEquipableType(EquipableType.Weapon) ? this.colOn : this.colOff);
			this.TorsoInventoryIcon.color = (this.CardData.HasEquipableOfEquipableType(EquipableType.Torso) ? this.colOn : this.colOff);
			this.HeadInventoryIcon.color = (this.CardData.HasEquipableOfEquipableType(EquipableType.Head) ? this.colOn : this.colOff);
		}
		this.DropShadowRenderer.enabled = this.IsEquipped && this.EquipmentHolder.ShowInventory;
		this.DropShadowRenderer.enabled = this.IsWorking && this.WorkerHolder.ShowInventory;
		this.OnOffInteractable.gameObject.SetActiveFast(active: false);
		Vector3 b2 = this.startScale;
		if ((this.IsEquipped || this.IsWorking) && !base.BeingDragged)
		{
			b2 = this.startScale * 0.8f;
		}
		if (!this.IsDemoCard)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, b2, Time.deltaTime * 12f);
		}
		if (!this.IsDemoCard)
		{
			this.UpdatePosition();
		}
		Vector3 position = base.transform.position;
		position.y = (0f - position.z) * 0.001f;
		this.EquipmentRectangle.position = position + this.equipmentRectangleStartOffset;
		this.WorkerRectangle.position = position + this.equipmentRectangleStartOffset;
		if (!this.IsDemoCard && !this.FaceUp)
		{
			this.flipTimer += Time.deltaTime * WorldManager.instance.PhysicsTimeScale;
			if (this.flipTimer >= 0.1f)
			{
				this.FaceUp = true;
			}
		}
		this.wobbleRotVelo -= Time.deltaTime * this.RotWobbleSpringiness;
		if (this.wobbleRotVelo <= 0f)
		{
			this.wobbleRotVelo = 0f;
		}
		float num = this.RotWobbleAmp * Mathf.Sin(this.wobbleRotVelo * this.RotWobbleSpeed) * this.wobbleRotVelo;
		if (this.AutoRotWobble)
		{
			this.rotWobbleTimer += Time.deltaTime;
			if (this.rotWobbleTimer > this.AutoRotWobbleTimer)
			{
				this.rotWobbleTimer -= this.AutoRotWobbleTimer;
				this.RotWobble(this.AutoRotWobbleAmount);
			}
		}
		bool active = true;
		if (!this.IsDemoCard)
		{
			if (this.IsEquipped)
			{
				if (this.EquipmentHolder.ShowInventory && !base.BeingDragged)
				{
					Transform myEquipmentStackPosition = this.GetMyEquipmentStackPosition();
					base.transform.localRotation = Camera.main.transform.localRotation;
					base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, base.transform.localEulerAngles.y, myEquipmentStackPosition.localEulerAngles.z);
				}
				else if (!base.BeingDragged)
				{
					active = false;
				}
			}
			else if (this.IsWorking)
			{
				if (this.WorkerHolder.ShowInventory && !base.BeingDragged)
				{
					Transform transformAtIndex = this.WorkerHolder.WorkerTransformHolder.GetTransformAtIndex(this.CardData.WorkerIndex);
					base.transform.localRotation = Camera.main.transform.localRotation;
					base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, base.transform.localEulerAngles.y, transformAtIndex.localEulerAngles.z);
				}
				else if (!base.BeingDragged)
				{
					active = false;
				}
			}
			else
			{
				float b3 = (this.FaceUp ? 90f : 270f);
				this.curZ = Mathf.Lerp(this.curZ, b3, Time.deltaTime * 14f * WorldManager.instance.PhysicsTimeScale);
				if (this.Parent != null)
				{
					this.curZ = b3;
				}
				base.transform.localRotation = Quaternion.Euler(this.curZ, 0f + num + this.ZRotOffset, 0f);
			}
		}
		else
		{
			this.SetDemoCardRotation();
		}
		PerformanceHelper.SetActive(this.Visuals.gameObject, active);
		if (this.Parent == null)
		{
			this.snappedToParent = false;
		}
		if (WorldManager.instance.CurrentBoard != null && this.HighlightActive)
		{
			this.HighlightRectangle.Color = WorldManager.instance.CurrentBoard.CardHighlightColor;
		}
		this.HighlightRectangle.enabled = this.HighlightActive;
		if (this.HighlightActive)
		{
			this.HighlightRectangle.DashOffset += Time.deltaTime;
			if (this.HighlightRectangle.DashOffset >= 1f)
			{
				this.HighlightRectangle.DashOffset -= 1f;
			}
		}
		this.lastPosition = base.transform.position;
		this.UpdateTimer();
		if (this.removedChild != null && !this.removedChild.BeingDragged)
		{
			this.removedChild = null;
			this.StackUpdate = true;
		}
		this.UpdateStatusEffectElements();
		this.UpdateCardAnimations();
		if (this.CardData.IsDamaged)
		{
			if (this.CardData.DamageType == CardDamageType.Damaged)
			{
				this.CardData.AddStatusEffect(new StatusEffect_Damaged());
			}
			if (this.CardData.DamageType == CardDamageType.Fire)
			{
				this.CardData.AddStatusEffect(new StatusEffect_OnFire());
			}
			if (this.CardData.DamageType == CardDamageType.Drought)
			{
				this.CardData.AddStatusEffect(new StatusEffect_Drought());
			}
		}
		else
		{
			this.CardData.RemoveStatusEffect<StatusEffect_Damaged>();
			this.CardData.RemoveStatusEffect<StatusEffect_OnFire>();
			this.CardData.RemoveStatusEffect<StatusEffect_Drought>();
		}
		if (this.IsHovered && this.CardData.IsDamaged)
		{
			if (this.CardData.DamageType == CardDamageType.Damaged)
			{
				Tooltip.Text = "<b>" + SokLoc.Translate("label_damaged") + "</b>\n" + SokLoc.Translate("label_damaged_card_cost", LocParam.Create("amount", this.CardData.GetRepairCost().ToStringCached()), LocParam.Create("icon", Icons.Dollar));
			}
			if (this.CardData.DamageType == CardDamageType.Fire)
			{
				Tooltip.Text = "<b>" + SokLoc.Translate("label_on_fire") + "</b>\n" + SokLoc.Translate("label_fire_card_cost");
			}
		}
	}

	private bool HasAnyWorkers()
	{
		List<GameCard> workerChildren = this.CardData.MyGameCard.WorkerChildren;
		for (int i = 0; i < workerChildren.Count; i++)
		{
			if (workerChildren[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	private void animateOnOffInteractable()
	{
		bool flag = false;
		if (!this.CardData.WorkerAmountMet())
		{
			flag = false;
		}
		if (WorldManager.instance.CurrentView != ViewType.Default)
		{
			flag = true;
		}
		if (this.CardData.CanToggleCardOnOff())
		{
			if (!this.OnOffInteractable.Velocity.HasValue && this.onOffBasePosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude < 0.001f && this.onOffBasePosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude > -0.001f)
			{
				if (flag)
				{
					this.OnOffInteractable.gameObject.SetActive(value: true);
					this.onOffTargetPos = this.onOffTargetPosition;
				}
				else
				{
					this.OnOffInteractable.gameObject.SetActive(value: false);
				}
			}
			else if (!flag && !this.OnOffInteractable.Velocity.HasValue && this.onOffTargetPosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude < 0.001f && this.onOffTargetPosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude > -0.001f)
			{
				this.onOffTargetPos = this.onOffBasePosition;
			}
		}
		else
		{
			this.OnOffInteractable.gameObject.SetActive(value: false);
		}
		this.OnOffInteractable.transform.localPosition = FRILerp.Spring(this.OnOffInteractable.transform.localPosition, this.onOffTargetPos, 20f, 30f, ref this.onOffVelocity);
	}

	public void UpdateCardAnimations()
	{
		for (int i = 0; i < this.CardAnimations.Count; i++)
		{
			CardAnimation cardAnimation = this.CardAnimations[i];
			if (!cardAnimation.HasStarted)
			{
				cardAnimation.Start();
			}
			cardAnimation.Update();
			if (cardAnimation.IsDone)
			{
				this.CardAnimations.RemoveAt(i);
				i--;
			}
			else if (cardAnimation.IsBlocking)
			{
				break;
			}
		}
	}

	public void CreateCardConnectors()
	{
		this.CardData.EnergyConnectors.OrderBy((CardConnectorData x) => x.EnergyConnectionStrength);
		foreach (CardConnectorData energyConnector in this.CardData.EnergyConnectors)
		{
			int energyConnectionAmount = energyConnector.EnergyConnectionAmount;
			float x2 = ((energyConnector.EnergyConnectionType == CardDirection.input) ? (-0.19f) : 0.19f);
			for (int i = 0; i < energyConnectionAmount; i++)
			{
				Vector3 localPosition = new Vector3(x2, (float)i * this.ConnectorAmountOffset - (float)(energyConnectionAmount / 2) * this.ConnectorAmountOffset + this.ConnectorAmountOffset / 2f * ((energyConnectionAmount % 2 == 0) ? 1f : 0f) - this.CardTextOffset, -0.03f);
				GameObject obj = UnityEngine.Object.Instantiate(this.EnergyConnectorPrefab, Vector3.zero, base.transform.rotation, this.EnergyConnectorTransform);
				obj.transform.localPosition = localPosition;
				CardConnector component = obj.GetComponent<CardConnector>();
				component.InitializeEnergyNode(energyConnector, this);
				this.CardConnectorChildren.Add(component);
			}
		}
	}

	private void UpdateConnectors()
	{
		foreach (CardConnector cardConnectorChild in this.CardConnectorChildren)
		{
			if (WorldManager.instance.CurrentBoard.Id != "cities")
			{
				cardConnectorChild.gameObject.SetActive(value: false);
				break;
			}
			if (WorldManager.instance.CurrentView == ViewType.Default)
			{
				cardConnectorChild.gameObject.SetActive(value: true);
			}
			else if (WorldManager.instance.CurrentView == ViewType.Energy)
			{
				cardConnectorChild.gameObject.SetActive(cardConnectorChild.ConnectionType == ConnectionType.LV || cardConnectorChild.ConnectionType == ConnectionType.HV);
			}
			else if (WorldManager.instance.CurrentView == ViewType.Sewer)
			{
				cardConnectorChild.gameObject.SetActive(cardConnectorChild.ConnectionType == ConnectionType.Sewer);
			}
			else if (WorldManager.instance.CurrentView == ViewType.Transport)
			{
				cardConnectorChild.gameObject.SetActive(cardConnectorChild.ConnectionType == ConnectionType.Transport);
			}
		}
	}

	private void UpdateShowInventory()
	{
		bool flag = this.CardData.WorkerAmount > 0 && this.Child == null && !this.CardData.HasInventory;
		bool flag2 = this.CardData.HasInventory && this.Child == null && this.EquipmentChildren.Count > 0;
		PerformanceHelper.SetActive(this.EquipmentButton.gameObject, flag2);
		PerformanceHelper.SetActive(this.InventoryInteractable.gameObject, flag2);
		if (this.ShowInventory && !flag2 && !flag)
		{
			this.ShowInventory = false;
		}
	}

	private void UpdateShowWorkerInventory()
	{
		bool active = this.CardData.WorkerAmount > 0 && this.Child == null && !this.CardData.HasInventory && !this.IsDemoCard;
		PerformanceHelper.SetActive(this.WorkerButton.gameObject, active);
		PerformanceHelper.SetActive(this.WorkerInventoryInteractable.gameObject, active);
	}

	private PositionType DeterminePositionType()
	{
		if (this.CardAnimations.Count > 0)
		{
			return PositionType.InAnimation;
		}
		if (this.IsEquipped)
		{
			if (base.BeingDragged)
			{
				return PositionType.None;
			}
			return PositionType.IsEquipped;
		}
		if (this.IsWorking)
		{
			if (base.BeingDragged)
			{
				return PositionType.None;
			}
			return PositionType.IsWorking;
		}
		if (this.InConflict)
		{
			if (base.BeingDragged)
			{
				return PositionType.None;
			}
			if (this.InAttack)
			{
				return PositionType.InAttack;
			}
			return PositionType.InConflict;
		}
		if (this.Parent != null)
		{
			return PositionType.InStack;
		}
		if (this.Parent == null)
		{
			return PositionType.IsRoot;
		}
		return PositionType.None;
	}

	private void UpdatePosition()
	{
		switch (this.DeterminePositionType())
		{
		case PositionType.InConflict:
			base.TargetPosition = this.Combatable.MyConflict.GetPositionInConflict(this.Combatable);
			base.transform.position = Vector3.Lerp(base.transform.position, base.TargetPosition, Time.deltaTime * 20f);
			break;
		case PositionType.InAttack:
		{
			AttackAnimation currentAttackAnimation = this.Combatable.CurrentAttackAnimation;
			base.transform.position = currentAttackAnimation.Position;
			base.TargetPosition = currentAttackAnimation.TargetPosition;
			break;
		}
		case PositionType.InAnimation:
		{
			CardAnimation cardAnimation = this.CardAnimations[0];
			base.transform.position = cardAnimation.Position;
			base.TargetPosition = cardAnimation.TargetPosition;
			break;
		}
		case PositionType.IsEquipped:
			if (this.EquipmentHolder.InventoryVisible)
			{
				base.TargetPosition = this.GetMyEquipmentStackPosition().position;
				if (this.IsHovered)
				{
					base.TargetPosition -= base.transform.forward * 0.1f;
				}
				base.transform.position = base.TargetPosition;
			}
			else
			{
				base.TargetPosition = this.EquipmentHolder.transform.position + new Vector3(0f, -0.1f, 0f);
				base.transform.position = base.TargetPosition;
			}
			break;
		case PositionType.IsWorking:
			if (this.WorkerHolder.InventoryVisible)
			{
				base.TargetPosition = this.WorkerHolder.WorkerTransformHolder.GetTransformAtIndex(this.CardData.WorkerIndex).position;
				if (this.IsHovered)
				{
					base.TargetPosition -= base.transform.forward * 0.1f;
				}
				base.transform.position = base.TargetPosition;
			}
			else
			{
				base.TargetPosition = this.WorkerHolder.transform.position + new Vector3(0f, -0.1f, 0f);
				base.transform.position = base.TargetPosition;
			}
			break;
		case PositionType.InStack:
			this.SetToParentPosition();
			base.TargetPosition = base.transform.position;
			break;
		case PositionType.IsRoot:
		case PositionType.None:
			if (!base.Velocity.HasValue)
			{
				Vector3 targetPosition = base.TargetPosition;
				float num = 20f;
				if (this.SetY)
				{
					targetPosition.y = (0f - targetPosition.z) * 0.001f;
					targetPosition.y += (base.BeingDragged ? 0.1f : 0f);
					if (this.IsHovered && this.CanBeDragged() && WorldManager.instance.CanInteract)
					{
						targetPosition.y += 0.06f;
					}
					if (this.CardData is Spirit || this.CardData is CityAdvisor)
					{
						targetPosition.y += 0.25f;
					}
				}
				else
				{
					num = 10f + WorldManager.instance.EndOfMonthSpeedup * 3f;
				}
				base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, Time.deltaTime * num);
			}
			this.UpdateChildPositions();
			break;
		}
		if (this.closeToTargetPositionCallback != null && Vector3.Distance(base.transform.position, base.TargetPosition) < 0.1f)
		{
			this.closeToTargetPositionCallback();
		}
	}

	public Transform GetEquipmentStackPosition(EquipableType equipableType)
	{
		return equipableType switch
		{
			EquipableType.Head => this.HeadEquipmentPosition.transform, 
			EquipableType.Torso => this.TorsoEquipmentPosition.transform, 
			EquipableType.Weapon => this.HandEquipmentPosition.transform, 
			_ => throw new ArgumentException($"EquipableType does not have a stack position set for {equipableType}"), 
		};
	}

	public void ToggleInventory()
	{
		this.OpenInventory(!this.ShowInventory);
	}

	public void ToggleCardOnOff()
	{
		this.CardData.ToggleCardOnOff();
	}

	public void OpenInventory(bool showInventory)
	{
		if (showInventory == this.ShowInventory)
		{
			return;
		}
		this.ShowInventory = showInventory;
		if (!this.ShowInventory)
		{
			return;
		}
		foreach (GameCard allCard in WorldManager.instance.AllCards)
		{
			if (allCard != this && allCard.ShowInventory)
			{
				allCard.ShowInventory = false;
			}
		}
	}

	public void StatusEffectsChanged()
	{
		foreach (StatusEffect statusEffect in this.CardData.StatusEffects)
		{
			if (!this.ElementExistsForStatusEffect(statusEffect))
			{
				StatusEffectElement item = this.CreateElementForStatusEffect(statusEffect);
				this.StatusEffectElements.Add(item);
			}
		}
		for (int i = 0; i < this.StatusEffectElements.Count; i++)
		{
			if (!this.CardData.StatusEffects.Contains(this.StatusEffectElements[i].MyStatusEffect))
			{
				this.StatusEffectElements[i].DestroyMe = true;
			}
		}
		List<StatusEffectElement> list = this.StatusEffectElements.Where((StatusEffectElement x) => !x.DestroyMe).ToList();
		for (int j = 0; j < list.Count; j++)
		{
			float x2 = (float)j * this.DistanceBetweenStatusses - (float)(this.CardData.StatusEffects.Count - 1) * this.DistanceBetweenStatusses * 0.5f;
			list[j].TargetLocalPosition = new Vector3(x2, 0f, -0.001f);
		}
	}

	private void UpdateStatusEffectElements()
	{
		Vector3 b = ((this.StatusEffectElements.Count == 0) ? Vector3.zero : Vector3.one);
		this.statusEffectBackgroundTransform.localScale = Vector3.Lerp(this.statusEffectBackgroundTransform.localScale, b, Time.deltaTime * 12f);
		PerformanceHelper.SetActive(this.StatusEffectBackground.gameObject, this.statusEffectBackgroundTransform.localScale.sqrMagnitude > 0.001f);
		float b2 = 0.1125f + (float)(this.StatusEffectElements.Count - 1) * this.DistanceBetweenStatusses;
		this.statusEffectBackgroundWidth = Mathf.Lerp(this.statusEffectBackgroundWidth, b2, Time.deltaTime * 12f);
		if (Mathf.Abs(this.statusEffectBackgroundWidth - this.StatusEffectBackground.Width) > 0.01f)
		{
			this.StatusEffectBackground.Width = this.statusEffectBackgroundWidth;
		}
	}

	public void SetDemoCardRotation()
	{
		if (this.FaceUp)
		{
			base.transform.rotation = Camera.main.transform.rotation;
		}
		else
		{
			base.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
		}
	}

	private Transform GetMyEquipmentStackPosition()
	{
		if (!this.IsEquipped)
		{
			throw new Exception("Not equipped!");
		}
		return this.EquipmentHolder.GetEquipmentStackPosition(((Equipable)this.CardData).EquipableType);
	}

	protected override void LateUpdate()
	{
		if (!(base.MyBoard != null) || base.MyBoard.IsCurrent)
		{
			this.PushAwayFromOthers();
			if (this.Parent == null && !this.IsEquipped && !this.IsWorking)
			{
				this.ClampPos();
			}
			if (this.Parent != null)
			{
				this.LastParent = this.Parent;
			}
		}
	}

	public void SetFaceUp(bool faceUp)
	{
		this.FaceUp = faceUp;
		this.curZ = (this.FaceUp ? 90f : 270f);
		base.transform.localRotation = Quaternion.Euler(this.curZ, 0f, 0f);
	}

	public override void SendIt()
	{
		if (base.MyBoard.Id == "cities" && this.HasParent)
		{
			base.Velocity = this.GetRootCard().CardData.OutputDir * 7f;
		}
		else
		{
			base.SendIt();
		}
		this.RotWobble(1f);
	}

	public GameCard FindNextGameCardInDirection(Vector3 direction, CardType? type = null)
	{
		float num = float.MinValue;
		GameCard result = null;
		foreach (GameCard allCard in WorldManager.instance.AllCards)
		{
			if (!allCard.gameObject.activeInHierarchy || allCard == WorldManager.instance.DraggingDraggable)
			{
				continue;
			}
			if (allCard.MyBoard == null)
			{
				Debug.Log(allCard?.ToString() + " does not have a board");
			}
			else
			{
				if (!allCard.MyBoard.IsCurrent || !allCard.CanBeAutoMovedTo || (type.HasValue && allCard.CardData.MyCardType != type))
				{
					continue;
				}
				Vector3 rhs = allCard.AutoMoveSnapPosition - base.transform.position;
				float num2 = Vector3.Dot(direction, rhs);
				if (!((double)num2 <= 0.3))
				{
					float num3 = num2 / rhs.sqrMagnitude;
					if (num3 > num && rhs.sqrMagnitude < 1f)
					{
						num = num3;
						result = allCard;
					}
				}
			}
		}
		return result;
	}

	public override void SendDirection(Vector3 direction)
	{
		this.RotWobble(1f);
		base.SendDirection(direction);
	}

	public override void SendToPosition(Vector3 position)
	{
		this.RotWobble(1f);
		base.SendToPosition(position);
	}

	public void SendToPositionCallback(Vector3 position, Action callback)
	{
		this.RotWobble(1f);
		base.TargetPosition = position;
		this.closeToTargetPositionCallback = callback;
	}

	public void RotWobble(float amount)
	{
		this.wobbleRotVelo = amount;
	}

	private void SetToParentPosition(bool hardSetPos = false)
	{
		Vector3 vector = ((!this.IsCollapsed) ? (this.Parent.transform.position + new Vector3(0f, WorldManager.instance.CardOverlayHeightOffset, 0f - WorldManager.instance.CardOverlayOffset)) : (this.Parent.transform.position + new Vector3(0f, WorldManager.instance.CardOverlayHeightOffset, 0f - WorldManager.instance.CollapsedCardOverlayOffset)));
		if (!this.snappedToParent)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, vector, Time.deltaTime * 20f);
			if (Vector3.Distance(base.transform.position, vector) < 0.001f)
			{
				this.snappedToParent = true;
			}
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, vector, Time.deltaTime * 20f);
			Vector3 position = base.transform.position;
			position.y = vector.y;
			base.transform.position = position;
		}
		if (hardSetPos)
		{
			base.transform.position = (base.TargetPosition = vector);
		}
	}

	public void UpdateChildPositions(bool hardSetPos = false)
	{
		if (!(this.Child == null))
		{
			this.Child.SetToParentPosition(hardSetPos);
			this.Child.UpdateChildPositions(hardSetPos);
		}
	}

	public Conflict GetOverlappingConflict()
	{
		foreach (Conflict allConflict in WorldManager.instance.GetAllConflicts())
		{
			if (allConflict.GetBounds().Intersects(base.DraggableBounds))
			{
				return allConflict;
			}
		}
		return null;
	}

	public List<GameCard> GetOverlappingCardsInBox(Vector3 center, Vector3 size)
	{
		List<GameCard> list = new List<GameCard>();
		int num = Physics.OverlapBoxNonAlloc(center, size * 0.5f, base.hits, Quaternion.identity, -5, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			GameCard component = base.hits[i].gameObject.GetComponent<GameCard>();
			if (component != null && component != this)
			{
				list.Add(component);
			}
		}
		return list;
	}

	public List<GameCard> GetOverlappingCards()
	{
		List<GameCard> list = new List<GameCard>();
		int num = PhysicsExtensions.OverlapBoxNonAlloc(base.boxCollider, base.hits, -5, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			GameCard component = base.hits[i].gameObject.GetComponent<GameCard>();
			if (component != null && component != this)
			{
				list.Add(component);
			}
		}
		return list;
	}

	public void StartBlueprintTimer(float time, TimerAction a, string status, string actionId, string blueprintId, int subprintIndex, CardData consumer, bool skipWorkerEnergyCheck = false)
	{
		if (this.IsDemoCard || base.BeingDragged)
		{
			return;
		}
		GameCard gameCard = this.GetRootCard();
		if (gameCard.CardData is HeavyFoundation && gameCard.HasChild)
		{
			gameCard = gameCard.Child;
		}
		if ((!this.HasTransportCard() || !(actionId != "sail_off") || !(actionId != "leave_spirit") || !(actionId != "take_portal")) && (!(this.removedChild != null) || !this.removedChild.BeingDragged))
		{
			if (this.TimerActionId == actionId && this.TimerBlueprintId == blueprintId && this.TimerSubprintIndex == subprintIndex)
			{
				this.TargetTimerTime = time;
			}
			else if (this.CardData.IsOn && (skipWorkerEnergyCheck || gameCard.CardData.ShouldStartTimerWorkers(actionId)) && (skipWorkerEnergyCheck || gameCard.CardData.ShouldStartTimerEnergy(consumer, actionId)) && !gameCard.CardData.IsDamaged)
			{
				this.TimerBlueprintId = blueprintId;
				this.TimerSubprintIndex = subprintIndex;
				this.SkipCitiesChecks = skipWorkerEnergyCheck;
				this.InitTimer(time, a, status, actionId);
			}
		}
	}

	public void StartTimer(float time, TimerAction a, string status, string actionId, bool withStatusBar = true, bool skipWorkerEnergyCheck = false, bool skipDamageOnOffCheck = false)
	{
		if (!this.IsDemoCard && !base.BeingDragged)
		{
			if (this.TimerActionId == actionId)
			{
				this.TargetTimerTime = time;
			}
			else if ((this.CardData.IsOn || skipDamageOnOffCheck) && (skipWorkerEnergyCheck || this.CardData.ShouldStartTimerWorkers(actionId)) && (skipWorkerEnergyCheck || this.CardData.HasEnergyInput()) && (skipWorkerEnergyCheck || this.CardData.HasSewerConnected()) && (!this.CardData.IsDamaged || skipDamageOnOffCheck))
			{
				this.InitTimer(time, a, status, actionId, withStatusBar);
			}
		}
	}

	private void InitTimer(float time, TimerAction a, string status, string actionId, bool withStatusBar = true)
	{
		if (withStatusBar)
		{
			Statusbar statusbar = UnityEngine.Object.Instantiate(PrefabManager.instance.StatusBarPrefab);
			statusbar.StatusTime = time;
			statusbar.ParentCard = this;
			this.CurrentStatusbar = statusbar;
		}
		this.Status = status;
		this.TimerRunning = true;
		this.TimerAction = a;
		this.TimerActionId = actionId;
		this.CurrentTimerTime = 0f;
		this.TargetTimerTime = time;
	}

	public void CancelTimer(string actionId)
	{
		if ((!(this.removedChild != null) || !this.removedChild.BeingDragged) && this.TimerRunning && !(this.TimerActionId != actionId))
		{
			this.StopTimer();
		}
	}

	private void StopTimer()
	{
		this.TimerRunning = false;
		this.TimerActionId = "";
		this.Status = "";
		this.TimerBlueprintId = "";
		this.TimerSubprintIndex = 0;
		this.CurrentTimerTime = 0f;
		this.SkipCitiesChecks = false;
		if (this.CurrentStatusbar != null)
		{
			this.CurrentStatusbar.DestroyMe = true;
			this.CurrentStatusbar = null;
		}
	}

	public void CancelAnyTimer()
	{
		if (this.TimerRunning)
		{
			this.StopTimer();
		}
	}

	public void UpdateTimer()
	{
		if (!this.TimerRunning)
		{
			return;
		}
		if (this.removedChild == null || !this.removedChild.BeingDragged)
		{
			this.CurrentTimerTime += Time.deltaTime * WorldManager.instance.TimeScale;
		}
		if (this.CurrentStatusbar != null)
		{
			this.CurrentStatusbar.Paused = this.removedChild != null && this.removedChild.BeingDragged;
		}
		if (!(this.CurrentTimerTime >= this.TargetTimerTime))
		{
			return;
		}
		this.TimerRunning = false;
		if (!this.ShouldCompleteTimer(this.TimerActionId))
		{
			this.TimerActionId = "";
			this.Status = "";
			this.TimerBlueprintId = "";
			this.TimerSubprintIndex = 0;
			this.CurrentTimerTime = 0f;
			this.CurrentStatusbar.DestroyMe = true;
			this.CurrentStatusbar = null;
			return;
		}
		try
		{
			this.TimerAction();
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		if (this.TimerActionId == "finish_blueprint")
		{
			QuestManager.instance.ActionComplete(WorldManager.instance.GetBlueprintWithId(this.TimerBlueprintId), this.TimerActionId, this.CardData);
		}
		else
		{
			QuestManager.instance.ActionComplete(this.CardData, this.TimerActionId);
		}
		this.TimerActionId = "";
		this.Status = "";
		this.TimerBlueprintId = "";
		this.TimerSubprintIndex = 0;
		this.CurrentTimerTime = 0f;
		if (this.CurrentStatusbar != null)
		{
			this.CurrentStatusbar.DestroyMe = true;
		}
		this.CurrentStatusbar = null;
	}

	public virtual bool ShouldCompleteTimer(string timerActionId)
	{
		return this.CardData.ShouldCompleteTimer(timerActionId);
	}

	public bool HasTransportCard()
	{
		GameCard gameCard = this.GetRootCard();
		if (gameCard.CardData is HeavyFoundation && gameCard.HasChild)
		{
			gameCard = gameCard.Child;
		}
		if (gameCard.CardData is Boat || gameCard.CardData is Spirit || gameCard.CardData is Portal)
		{
			return true;
		}
		return false;
	}

	public bool ElementExistsForStatusEffect(StatusEffect effect)
	{
		foreach (StatusEffectElement statusEffectElement in this.StatusEffectElements)
		{
			if (statusEffectElement.MyStatusEffect == effect)
			{
				return true;
			}
		}
		return false;
	}

	public StatusEffectElement CreateElementForStatusEffect(StatusEffect effect)
	{
		StatusEffectElement statusEffectElement = UnityEngine.Object.Instantiate(PrefabManager.instance.StatusEffectElementPrefab);
		statusEffectElement.SetStatusEffect(this, effect);
		statusEffectElement.transform.SetParent(this.StatusEffectElementParent);
		statusEffectElement.transform.localRotation = Quaternion.identity;
		statusEffectElement.transform.localScale = Vector3.zero;
		float x = (float)this.StatusEffectElements.Count * this.DistanceBetweenStatusses - (float)(this.StatusEffectElements.Count - 1) * this.DistanceBetweenStatusses * 0.5f;
		statusEffectElement.transform.localPosition = new Vector3(x, 0f, -0.001f);
		return statusEffectElement;
	}

	public bool IsPartOfStack()
	{
		if (!(this.Parent != null))
		{
			return this.Child != null;
		}
		return true;
	}

	public GameCard GetCardWithStatusInStack()
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard.TimerRunning)
			{
				return gameCard;
			}
			gameCard = gameCard.Child;
		}
		return null;
	}

	public int GetCardIndex()
	{
		GameCard gameCard = this.GetRootCard();
		int num = 0;
		while (gameCard != null)
		{
			if (gameCard == this)
			{
				return num;
			}
			gameCard = gameCard.Child;
			num++;
		}
		return -1;
	}

	public GameCard GetCardInCombatInStack()
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard.Combatable != null && gameCard.Combatable.InConflict)
			{
				return gameCard;
			}
			gameCard = gameCard.Child;
		}
		return null;
	}

	public List<GameCard> GetAllCardsInStack()
	{
		GameCard rootCard = this.GetRootCard();
		List<GameCard> childCards = rootCard.GetChildCards();
		childCards.Insert(0, rootCard);
		return childCards;
	}

	public CardData HasCardInStack(Predicate<CardData> pred)
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				return gameCard.CardData;
			}
			gameCard = gameCard.Child;
		}
		return null;
	}

	public bool IsPartOfSameStack(GameCard otherCard)
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard == otherCard)
			{
				return true;
			}
			gameCard = gameCard.Child;
		}
		return false;
	}

	public string GetStackSummary()
	{
		return WorldManager.instance.GetStackSummary(this.GetAllCardsInStack());
	}

	public bool IsChildOf(GameCard card)
	{
		if (card == null)
		{
			return false;
		}
		GameCard parent = this.Parent;
		while ((object)parent != null)
		{
			if (parent == card)
			{
				return true;
			}
			parent = parent.Parent;
		}
		return false;
	}

	public bool IsParentOf(GameCard card)
	{
		if (card == null)
		{
			return false;
		}
		GameCard child = this.Child;
		while ((object)child != null)
		{
			if (child == card)
			{
				return true;
			}
			child = child.Child;
		}
		return false;
	}

	public void SetCollidersInStack(bool enabled)
	{
		GameCard gameCard = this;
		while ((object)gameCard != null)
		{
			gameCard.boxCollider.enabled = enabled;
			gameCard = gameCard.Child;
		}
	}

	public List<GameCard> GetChildCards()
	{
		List<GameCard> list = new List<GameCard>();
		GameCard child = this.Child;
		while (child != null)
		{
			list.Add(child);
			child = child.Child;
		}
		return list;
	}

	public GameCard GetRootCard()
	{
		GameCard gameCard = this;
		while (gameCard.Parent != null)
		{
			gameCard = gameCard.Parent;
		}
		return gameCard;
	}

	public GameCard GetLeafCard()
	{
		GameCard gameCard = this;
		while (gameCard.Child != null)
		{
			gameCard = gameCard.Child;
		}
		return gameCard;
	}

	public int GetChildCount()
	{
		GameCard gameCard = this;
		int num = 0;
		while (gameCard.Child != null)
		{
			num++;
			gameCard = gameCard.Child;
		}
		return num;
	}

	public int GetStackCount()
	{
		GameCard gameCard = this.GetRootCard();
		int num = 1;
		while (gameCard.Child != null)
		{
			num++;
			gameCard = gameCard.Child;
		}
		return num;
	}

	private void NotifyChildDrag(GameCard card)
	{
		this.removedChild = card;
	}

	public override void StopDragging()
	{
		if (this.Parent != null)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.DropOnStack, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);
		}
		else if (this.CardData.PickupSound != null && this.CardData.PickupSoundGroup == PickupSoundGroup.Custom)
		{
			AudioManager.me.PlaySound2D(this.CardData.PickupSound, UnityEngine.Random.Range(0.8f, 1f), 0.5f);
		}
		else
		{
			List<AudioClip> soundForPickupSoundGroup = AudioManager.me.GetSoundForPickupSoundGroup(this.CardData.PickupSoundGroup);
			AudioManager.me.PlaySound2D(soundForPickupSoundGroup, UnityEngine.Random.Range(0.8f, 1f), 0.5f);
		}
		GameCard child = this.Child;
		while (child != null)
		{
			child.BeingDragged = false;
			child = child.Child;
		}
		this.CardData.StoppedDragging();
		this.StackUpdate = true;
		base.StopDragging();
	}

	public override void StartDragging()
	{
		if (this.CardData.PickupSound != null && this.CardData.PickupSoundGroup == PickupSoundGroup.Custom)
		{
			AudioManager.me.PlaySound2D(this.CardData.PickupSound, UnityEngine.Random.Range(1f, 1.2f), 0.5f);
		}
		else
		{
			List<AudioClip> soundForPickupSoundGroup = AudioManager.me.GetSoundForPickupSoundGroup(this.CardData.PickupSoundGroup);
			AudioManager.me.PlaySound2D(soundForPickupSoundGroup, UnityEngine.Random.Range(1f, 1.2f), 0.5f);
		}
		GameCard parent = this.Parent;
		while (parent != null)
		{
			parent.NotifyChildDrag(this);
			parent = parent.Parent;
		}
		if (this.Parent != null)
		{
			this.SetParent(null);
		}
		parent = this.Child;
		while (parent != null)
		{
			parent.BeingDragged = true;
			parent = parent.Child;
		}
		this.BounceTarget = null;
		base.StartDragging();
	}

	public void Clampieee()
	{
		this.ClampPos();
	}

	protected override void ClampPos()
	{
		if (!this.IsDemoCard && this.SetY)
		{
			int childCount = this.GetChildCount();
			float b = (float)childCount * WorldManager.instance.CardOverlayOffset;
			if (this.IsCollapsed)
			{
				b = (float)childCount * WorldManager.instance.CollapsedCardOverlayOffset;
			}
			this.curHeight = Mathf.Lerp(this.curHeight, b, Time.deltaTime * 12f);
			base.transform.position = this.ClampPos2(base.transform.position);
			base.TargetPosition = this.ClampPos2(base.TargetPosition);
		}
	}

	public float GetHeight()
	{
		PrefabManager.instance.GameCardPrefab.boxCollider.ToWorldSpaceBox(out var _, out var halfExtents, out var _);
		return halfExtents.y * 2f;
	}

	public float GetWidth()
	{
		PrefabManager.instance.GameCardPrefab.boxCollider.ToWorldSpaceBox(out var _, out var halfExtents, out var _);
		return halfExtents.x * 2f;
	}

	public Bounds GetBounds()
	{
		return new Bounds(base.transform.position, new Vector3(this.GetWidth(), 0.01f, this.GetHeight()));
	}

	private Vector3 ClampPos2(Vector3 p)
	{
		Bounds bounds = (base.BeingDragged ? base.MyBoard.WorldBounds : base.MyBoard.TightWorldBounds);
		base.boxCollider.ToWorldSpaceBox2(out var halfExtents);
		float num = 0.1f;
		p.x = Mathf.Clamp(p.x, bounds.min.x + halfExtents.x + num, bounds.max.x - halfExtents.x - num);
		p.z = Mathf.Clamp(p.z, bounds.min.z + halfExtents.y + num + this.curHeight, bounds.max.z - halfExtents.y - num);
		return p;
	}

	public SavedCard ToSavedCard()
	{
		SavedCard savedCard = new SavedCard();
		savedCard.CardPosition = base.transform.position;
		savedCard.CardPrefabId = this.CardData.Id;
		savedCard.UniqueId = this.CardData.UniqueId;
		savedCard.IsFoil = this.CardData.IsFoil;
		savedCard.FaceUp = this.FaceUp;
		savedCard.IsDamaged = this.CardData.IsDamaged;
		savedCard.DamageType = this.CardData.DamageType;
		if (this.Parent != null)
		{
			savedCard.ParentUniqueId = this.Parent.CardData.UniqueId;
		}
		if (this.EquipmentHolder != null)
		{
			savedCard.EquipmentHolderUniqueId = this.EquipmentHolder.CardData.UniqueId;
		}
		if (this.WorkerHolder != null)
		{
			savedCard.WorkerHolderUniqueId = this.WorkerHolder.CardData.UniqueId;
			savedCard.WorkerIndex = this.CardData.WorkerIndex;
		}
		savedCard.ExtraCardData = this.CardData.GetExtraCardData();
		savedCard.TimerRunning = this.TimerRunning;
		savedCard.WithStatusBar = this.CurrentStatusbar != null;
		savedCard.TimerActionId = this.TimerActionId;
		savedCard.Status = this.Status;
		savedCard.CurrentTimerTime = this.CurrentTimerTime;
		savedCard.TargetTimerTime = this.TargetTimerTime;
		savedCard.TimerBlueprintId = this.TimerBlueprintId;
		savedCard.SkipCitiesChecks = this.SkipCitiesChecks;
		savedCard.SubprintIndex = this.TimerSubprintIndex;
		savedCard.BoardId = base.MyBoard.Id;
		savedCard.StatusEffects = this.CardData.StatusEffects.Select((StatusEffect x) => x.ToSavedStatusEffect()).ToList();
		savedCard.CardConnectors = (from x in this.CardConnectorChildren
			select x.ToSavedEnergyConnector() into x
			where x != null
			select x).ToList();
		return savedCard;
	}

	public void SetHitEffect(Action after = null)
	{
		this.IsHit = true;
		foreach (MaterialChanger mc in this.materialChangers)
		{
			if (mc != null)
			{
				mc.SetMaterial(WorldManager.instance.HitMaterial);
				base.StartCoroutine(this.WaitFor(0.1f, delegate
				{
					mc.ResetMaterials();
				}));
			}
		}
		base.StartCoroutine(this.WaitFor(0.11f, delegate
		{
			this.IsHit = false;
			after?.Invoke();
		}));
	}

	public bool HasConnectorOfType(ConnectionType connectionType)
	{
		for (int i = 0; i < this.CardData.EnergyConnectors.Count; i++)
		{
			if (this.CardData.EnergyConnectors[i].EnergyConnectionStrength == connectionType)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator WaitFor(float time, Action a)
	{
		yield return new WaitForSeconds(time);
		a?.Invoke();
	}
}
