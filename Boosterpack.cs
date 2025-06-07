using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Boosterpack : Draggable
{
	public BoosterpackData PackData;

	public MeshRenderer PackRenderer;

	private MaterialPropertyBlock propBlock;

	public TextMeshPro BoosterText;

	public TextMeshPro CardCountText;

	protected List<MaterialChanger> materialChangers = new List<MaterialChanger>();

	private Vector3 startScale;

	public int TotalCardsInPack;

	private CardId spawnedEvent;

	[HideInInspector]
	public int TimesOpened;

	[HideInInspector]
	public bool WasClicked;

	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(this.PackData.nameOverride))
			{
				return this.PackData.nameOverride;
			}
			return SokLoc.Translate(this.PackData.NameTerm);
		}
	}

	protected override bool HasPhysics => true;

	public string BoosterId => this.PackData.BoosterId;

	public bool IsIntroPack => this.PackData.IsIntroPack;

	public int Cost => this.PackData.Cost;

	public Location BoosterLocation => this.PackData.BoosterLocation;

	public List<CardBag> CardBags => this.PackData.CardBags;

	protected override float Mass => 10f;

	public override bool CanBeDragged()
	{
		if (this.IsIntroPack)
		{
			return false;
		}
		return base.CanBeDragged();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
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
		this.startScale = base.transform.localScale;
		this.propBlock = new MaterialPropertyBlock();
		if (!WorldManager.instance.AllBoosters.Contains(this))
		{
			WorldManager.instance.AllBoosters.Add(this);
		}
		base.Start();
	}

	protected override void OnDestroy()
	{
		WorldManager.instance.AllBoosters.Remove(this);
		base.OnDestroy();
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		if (draggable is GameCard)
		{
			return false;
		}
		return base.CanBePushedBy(draggable);
	}

	protected override void Update()
	{
		this.PackRenderer.GetPropertyBlock(this.propBlock, 1);
		if (this.PackData.Icon != null)
		{
			this.propBlock.SetTexture("_IconTex", this.PackData.Icon.texture);
		}
		else
		{
			this.propBlock.SetTexture("_IconTex", SpriteManager.instance.EmptyTexture.texture);
		}
		this.PackRenderer.SetPropertyBlock(this.propBlock, 1);
		this.BoosterText.text = this.Name;
		int num = this.PackData.CardBags.Sum((CardBag x) => x.CardsInPack);
		this.CardCountText.text = num.ToString();
		if (num == 0)
		{
			this.CardCountText.transform.parent.gameObject.SetActive(value: false);
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.startScale, Time.deltaTime * 16f);
		base.Update();
	}

	private Vector3 GetCardVelocity()
	{
		if (this.TotalCardsInPack == 1)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * 4f;
			base.Velocity = new Vector3(vector.x, 6f, vector.y);
		}
		int num = this.TotalCardsInPack - this.PackData.CardBags.Sum((CardBag x) => x.CardsInPack);
		int totalCardsInPack = this.TotalCardsInPack;
		float f = (float)num / (float)totalCardsInPack * 360f * (MathF.PI / 180f);
		return new Vector3(Mathf.Cos(f) * 4.5f, 6f, Mathf.Sin(f) * 4.5f);
	}

	public override void Clicked()
	{
		CardBag currentCardBag = this.PackData.CardBags.FirstOrDefault((CardBag x) => x.CardsInPack > 0);
		if (currentCardBag == null)
		{
			return;
		}
		this.WasClicked = true;
		ICardId cardId = currentCardBag.GetCard();
		int timesBoosterWasBoughtOnLocation = WorldManager.instance.GetTimesBoosterWasBoughtOnLocation(this.BoosterLocation);
		GameCamera.instance.Screenshake = 0.3f;
		if (this.BoosterLocation == Location.Mainland && timesBoosterWasBoughtOnLocation == 1)
		{
			if (currentCardBag.CardsInPack == 1)
			{
				cardId = (CardId)"berrybush";
			}
			else if (currentCardBag.CardsInPack == 0)
			{
				cardId = (CardId)"tree";
			}
		}
		if (this.BoosterId == "cities_weather" && this.TimesOpened == this.TotalCardsInPack - 1)
		{
			if (CitiesManager.instance.ShouldTriggerEvent())
			{
				cardId = (this.spawnedEvent = CitiesManager.instance.GetEvent());
			}
		}
		else
		{
			int cardCount = WorldManager.instance.GetCardCount<Worker>();
			if (this.BoosterLocation == Location.Cities && cardCount <= 1 && ((2 - cardCount >= 0) ? (2 - cardCount) : 0) - this.TimesOpened >= 0)
			{
				cardId = (CardId)"worker";
			}
		}
		if (WorldManager.instance.GetBoardWithLocation(this.BoosterLocation).BoardOptions.NewVillagerSpawnsFromPack)
		{
			int cardCount2 = WorldManager.instance.GetCardCount((BaseVillager x) => x.CanBreed);
			cardCount2 += WorldManager.instance.GetCardCount<TeenageVillager>();
			if (!this.IsIntroPack && (timesBoosterWasBoughtOnLocation == 7 || (timesBoosterWasBoughtOnLocation > 7 && timesBoosterWasBoughtOnLocation % 5 == 0)) && currentCardBag.CardsInPack == 0 && cardCount2 <= 1)
			{
				cardId = (CardId)"villager";
			}
			if (base.MyBoard.BoardOptions.CanSpawnCombatIntro && timesBoosterWasBoughtOnLocation >= 10 && !WorldManager.instance.CurrentSave.FoundBoosterIds.Contains("combat_intro") && currentCardBag.CardsInPack == 0)
			{
				WorldManager.instance.CreateBoosterpack(base.transform.position, "combat_intro").SendIt();
			}
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardId, faceUp: false, checkAddToStack: false);
		if (cardData == null)
		{
			Debug.LogError($"CardData is null after creating card with id '{cardId}'");
		}
		cardData.MyGameCard.RotWobble(1f);
		cardData.MyGameCard.Velocity = this.GetCardVelocity();
		AudioManager.me.PlaySound2D(AudioManager.me.OpenBooster, UnityEngine.Random.Range(0.9f, 1.1f), 0.3f);
		WorldManager.instance.GivenCards.Add(cardData.Id);
		if (currentCardBag.CardBagType != CardBagType.SetPack && UnityEngine.Random.value <= 0.01f)
		{
			cardData.SetFoil();
		}
		this.TimesOpened++;
		this.SetHitEffect(delegate
		{
			if (currentCardBag.CardsInPack <= 0 && currentCardBag == this.PackData.CardBags[this.PackData.CardBags.Count - 1])
			{
				WorldManager.instance.CreateSmoke(base.transform.position);
				UnityEngine.Object.Destroy(base.gameObject);
				QuestManager.instance.SpecialActionComplete(this.PackData.BoosterId + "_opened");
			}
		});
		if (this.TimesOpened == this.TotalCardsInPack)
		{
			WorldManager.instance.OnBoosterOpened(this.PackData.BoosterId);
			if (this.spawnedEvent != null)
			{
				CardData cardFromId = WorldManager.instance.GameDataLoader.GetCardFromId(this.spawnedEvent.Id);
				if (cardFromId.MyCardType == CardType.Disaster)
				{
					WorldManager.instance.QueueCutsceneIfNotPlayed("cities_disaster");
				}
				else if (cardFromId.Id == "ufo_event")
				{
					WorldManager.instance.QueueCutscene("cities_event_ufo");
				}
				this.spawnedEvent = null;
			}
		}
		base.transform.localScale *= 1.2f;
		base.Clicked();
	}

	protected void SetHitEffect(Action after)
	{
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
		base.StartCoroutine(this.WaitFor(0.1f, delegate
		{
			after?.Invoke();
		}));
	}

	private IEnumerator WaitFor(float time, Action a)
	{
		yield return new WaitForSeconds(time);
		a?.Invoke();
	}
}
