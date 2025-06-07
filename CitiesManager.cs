using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CitiesManager : MonoBehaviour
{
	public static CitiesManager instance;

	[HideInInspector]
	private int _wellbeing;

	public int WellbeingStart = 30;

	[HideInInspector]
	public CityState CityState;

	public int TriggerEventFromMonth = 10;

	public CardBag EventList;

	public int MonthsBetweenConflict = 6;

	public int MaxRandomOffset = 1;

	public CardEventType? ActiveEvent;

	[HideInInspector]
	public int NextConflictMonth;

	[HideInInspector]
	public CardConnector DrawingConnector;

	public Material EnergyCableMaterial;

	[HideInInspector]
	public List<HousingConsumer> HousingConsumers = new List<HousingConsumer>();

	[HideInInspector]
	public List<Worker> WorkersOnBoard = new List<Worker>();

	[HideInInspector]
	public List<HousingConsumer> HomelessHousingConsumers = new List<HousingConsumer>();

	[HideInInspector]
	public int PreviousWellbeing;

	private float targetVolume;

	private float targetPitch = 1f;

	private float prevDist;

	private AudioSource stretchSource;

	private int[] takeOrder = new int[4] { 10, 20, 50, 100 };

	public int Wellbeing
	{
		get
		{
			return this._wellbeing;
		}
		set
		{
			this.PreviousWellbeing = this._wellbeing;
			this._wellbeing = Mathf.Clamp(value, 0, 200);
			this.UpdateCityState();
		}
	}

	private void Start()
	{
		CitiesManager.instance = this;
		this.InitStrechSource();
		this.UpdateCityState();
	}

	private void InitStrechSource()
	{
		this.stretchSource = AudioManager.me.GetSource(base.transform, claim: true);
		this.stretchSource.pitch = 0f;
		this.stretchSource.volume = 0f;
		this.stretchSource.clip = AudioManager.me.EnergyStrech;
		this.stretchSource.reverbZoneMix = 0f;
		this.stretchSource.spatialBlend = 0f;
		this.stretchSource.bypassListenerEffects = false;
		this.stretchSource.loop = true;
		this.stretchSource.Play();
	}

	private void Update()
	{
		if (WorldManager.instance.CurrentBoard == null || WorldManager.instance.CurrentBoard?.Id != "cities")
		{
			if (WorldManager.instance.CanUseTransport)
			{
				this.DrawConnectors();
			}
			return;
		}
		WorldManager.instance.GetCardsNonAlloc(this.WorkersOnBoard);
		WorldManager.instance.GetCardsImplementingInterfaceNonAlloc(this.HousingConsumers);
		this.HomelessHousingConsumers.Clear();
		for (int num = this.HousingConsumers.Count - 1; num >= 0; num--)
		{
			if (this.HousingConsumers[num].Housing == null && this.HousingConsumers[num].GetHousingSpaceRequired() > 0)
			{
				this.HomelessHousingConsumers.Add(this.HousingConsumers[num]);
			}
		}
		this.CheckConflict();
		this.CheckCutscenes();
		this.CheckForEvents();
		this.DrawConnectors();
		this.DrawConnectorAudio();
	}

	public void DrawConnectorAudio()
	{
		if (this.DrawingConnector != null)
		{
			AudioClip stretchSoundForType = this.DrawingConnector.GetStretchSoundForType(this.DrawingConnector.ConnectionType);
			this.stretchSource.clip = stretchSoundForType;
			if (!this.stretchSource.isPlaying)
			{
				this.stretchSource.Play();
			}
			Vector3 a = this.DrawingConnector.transform.position + Vector3.down * 0.01f;
			Vector3 mouseWorldPosition = WorldManager.instance.mouseWorldPosition;
			float num = Vector3.Distance(a, mouseWorldPosition);
			float num2 = Mathf.Abs(num - this.prevDist);
			this.prevDist = num;
			if (num2 > 0.001f)
			{
				this.targetVolume = 1f;
			}
			else
			{
				this.targetVolume = 0f;
			}
			if (this.DrawingConnector.ConnectionType == ConnectionType.LV || this.DrawingConnector.ConnectionType == ConnectionType.HV)
			{
				this.targetPitch = Mathf.Lerp(1f, 1.5f, Mathf.InverseLerp(1f, 8f, num));
			}
			else
			{
				this.targetPitch = 1f;
			}
		}
		else
		{
			this.targetVolume = 0f;
		}
		this.stretchSource.volume = Mathf.Lerp(this.stretchSource.volume, this.targetVolume, Time.deltaTime * 15f);
		this.stretchSource.pitch = Mathf.Lerp(this.stretchSource.pitch, this.targetPitch, Time.deltaTime * 5f);
	}

	public void CheckForEvents()
	{
		EventCard card = WorldManager.instance.GetCard<EventCard>();
		if (card != null && card.EventIsActive)
		{
			this.ActiveEvent = card.EventType;
		}
		else
		{
			this.ActiveEvent = null;
		}
	}

	public void CheckCutscenes()
	{
		if (!TransitionScreen.InTransition && this._wellbeing > 0 && !WorldManager.instance.InAnimation)
		{
			if (this._wellbeing >= 40)
			{
				WorldManager.instance.QueueCutsceneIfNotPlayed("cities_wellbeing_30");
			}
			if (this._wellbeing >= 20)
			{
				WorldManager.instance.QueueCutsceneIfNotPlayed("cities_wellbeing_20");
			}
			if (this._wellbeing < 10)
			{
				WorldManager.instance.QueueCutsceneIfNotPlayed("cities_wellbeing_10");
			}
		}
	}

	public void StartDrawCable(CardConnector connector)
	{
		this.DrawingConnector = connector;
	}

	public void StopDrawCable(CardConnector endConnector)
	{
		if (this.DrawingConnector != null && endConnector != null && endConnector.CardDirection != this.DrawingConnector.CardDirection && endConnector.ConnectionType == this.DrawingConnector.ConnectionType && this.DrawingConnector.Parent != endConnector.Parent)
		{
			this.DrawingConnector.SetConnectedNode(endConnector);
			QuestManager.instance.SpecialActionComplete("cities_cable_connected", this.DrawingConnector.Parent.CardData);
			this.DrawingConnector.Parent.CardData.NotifyEnergyConsumers();
		}
		this.DrawingConnector = null;
	}

	private Vector3 DetermineConnectorMiddle(Vector3 start, Vector3 end, ConnectionType conn)
	{
		if (conn == ConnectionType.LV || conn == ConnectionType.HV)
		{
			float value = Mathf.Abs(start.x - end.x);
			float num = 1f - Mathf.InverseLerp(0f, 3f, value);
			float num2 = (end.z - start.z) * 0.3f;
			float num3 = 0.75f;
			return Vector3.Lerp(start, end, 0.5f + num2) + new Vector3(0f, 0f, 0f - num3) * num;
		}
		return Vector3.Lerp(start, end, 0.5f);
	}

	public void DrawConnectors()
	{
		if (this.DrawingConnector != null)
		{
			Vector3 start = this.DrawingConnector.transform.position + Vector3.down * 0.01f;
			Vector3 mouseWorldPosition = WorldManager.instance.mouseWorldPosition;
			Vector3 vector = this.DetermineConnectorMiddle(start, mouseWorldPosition, this.DrawingConnector.ConnectionType);
			if (this.DrawingConnector.ConnectionType == ConnectionType.LV || this.DrawingConnector.ConnectionType == ConnectionType.HV)
			{
				if ((this.DrawingConnector.Middle - vector).sqrMagnitude >= 100f)
				{
					this.DrawingConnector.MiddleVelo = Vector3.zero;
					this.DrawingConnector.Middle = vector;
				}
				this.DrawingConnector.Middle = FRILerp.Spring(this.DrawingConnector.Middle, vector, 25f, 10f, ref this.DrawingConnector.MiddleVelo);
			}
			else if (this.DrawingConnector.ConnectionType == ConnectionType.Sewer)
			{
				this.DrawingConnector.Middle = vector;
			}
			else if (this.DrawingConnector.ConnectionType == ConnectionType.Transport)
			{
				this.DrawingConnector.Middle = vector;
			}
			DrawManager.instance.DrawShape(this.GetShapeForConnectionType(this.DrawingConnector.ConnectionType, start, this.DrawingConnector.Middle, mouseWorldPosition));
		}
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		foreach (GameCard allCard in WorldManager.instance.AllCards)
		{
			if (allCard.CardConnectorChildren.Count == 0 || allCard.MyBoard != currentBoard)
			{
				continue;
			}
			foreach (CardConnector cardConnectorChild in allCard.CardConnectorChildren)
			{
				if (cardConnectorChild.ConnectedNode != null && cardConnectorChild.CardDirection == CardDirection.output)
				{
					Vector3 start2 = cardConnectorChild.transform.position + Vector3.down * 0.01f;
					Vector3 end = cardConnectorChild.ConnectedNode.transform.position + Vector3.down * 0.01f;
					Vector3 vector2 = this.DetermineConnectorMiddle(start2, end, cardConnectorChild.ConnectionType);
					if ((cardConnectorChild.Middle - vector2).sqrMagnitude >= 100f)
					{
						cardConnectorChild.MiddleVelo = Vector3.zero;
						cardConnectorChild.Middle = vector2;
					}
					if (cardConnectorChild.ConnectionType == ConnectionType.LV || cardConnectorChild.ConnectionType == ConnectionType.HV)
					{
						cardConnectorChild.Middle = FRILerp.Spring(cardConnectorChild.Middle, vector2, 25f, 10f, ref cardConnectorChild.MiddleVelo);
					}
					else if (cardConnectorChild.ConnectionType == ConnectionType.Sewer)
					{
						cardConnectorChild.Middle = vector2;
					}
					else if (cardConnectorChild.ConnectionType == ConnectionType.Transport)
					{
						cardConnectorChild.Middle = FRILerp.Spring(cardConnectorChild.Middle, vector2, 30f, 30f, ref cardConnectorChild.MiddleVelo);
					}
					DrawManager.instance.DrawShape(this.GetShapeForConnectionType(cardConnectorChild.ConnectionType, start2, cardConnectorChild.Middle, end));
				}
			}
		}
	}

	private IShape GetShapeForConnectionType(ConnectionType connectionType, Vector3 start, Vector3 middle, Vector3 end)
	{
		switch (connectionType)
		{
		case ConnectionType.LV:
		case ConnectionType.HV:
		{
			EnergyCable energyCable = default(EnergyCable);
			energyCable.Start = start;
			energyCable.Middle = middle;
			energyCable.End = end;
			energyCable.IsLowVoltage = connectionType == ConnectionType.LV;
			return energyCable;
		}
		case ConnectionType.Sewer:
		{
			SewerPipe sewerPipe = default(SewerPipe);
			sewerPipe.Start = start;
			sewerPipe.Middle = middle;
			sewerPipe.End = end;
			return sewerPipe;
		}
		case ConnectionType.Transport:
		{
			if (WorldManager.instance.CurrentBoard.Id == "cities")
			{
				TransportArrow transportArrow = default(TransportArrow);
				transportArrow.Start = start;
				transportArrow.Middle = middle;
				transportArrow.End = end;
				return transportArrow;
			}
			TransportArrowMainland transportArrowMainland = default(TransportArrowMainland);
			transportArrowMainland.Start = start;
			transportArrowMainland.Middle = middle;
			transportArrowMainland.End = end;
			return transportArrowMainland;
		}
		default:
			return null;
		}
	}

	public void CheckConflict()
	{
		if (WorldManager.instance.HasFoundCard("blueprint_barrack") && this.Wellbeing >= 25)
		{
			int currentMonth = WorldManager.instance.CurrentMonth;
			if (this.NextConflictMonth < currentMonth - 1)
			{
				this.NextConflictMonth = this.GetNextConflictMonth(currentMonth);
				Debug.Log("Updated Conflict Month to : " + this.NextConflictMonth);
			}
		}
		else
		{
			this.NextConflictMonth = -1;
		}
	}

	public int GetNextConflictMonth(int currentMonth)
	{
		return currentMonth + Random.Range(this.MonthsBetweenConflict - this.MaxRandomOffset, this.MonthsBetweenConflict + this.MaxRandomOffset);
	}

	public static string GetCityStateTranslated(CityState state)
	{
		return SokLoc.Translate("label_wellbeing_" + state.ToString().ToLower());
	}

	public void CheckCityHealth()
	{
	}

	public void AddWellbeing(int wellbeing)
	{
		this.Wellbeing += wellbeing;
	}

	public bool ShouldTriggerEvent()
	{
		if (WorldManager.instance.CurrentMonth > this.TriggerEventFromMonth)
		{
			return true;
		}
		return false;
	}

	public CardId GetEvent()
	{
		List<string> list = (from x in this.EventList.GetCardsInBag()
			where !WorldManager.instance.CurrentRunVariables.SpawnedEventIds.Contains(x)
			select x).ToList();
		if (list.Count <= 0)
		{
			WorldManager.instance.CurrentRunVariables.SpawnedEventIds = new List<string>();
			list = this.EventList.GetCardsInBag();
		}
		string text = list.Choose();
		WorldManager.instance.CurrentRunVariables.SpawnedEventIds.Add(text);
		return new CardId(text);
	}

	public void UpdateCityState()
	{
		if (this._wellbeing < 0)
		{
			this._wellbeing = 0;
		}
		this.CityState = CitiesManager.GetCityStateForWellbeing(this._wellbeing);
	}

	public static CityState GetCityStateForWellbeing(int amount)
	{
		if (amount <= 10 && amount > 0)
		{
			return CityState.Miserable;
		}
		if (amount <= 20 && amount > 10)
		{
			return CityState.Unhappy;
		}
		if (amount < 40 && amount > 20)
		{
			return CityState.Normal;
		}
		if (amount >= 40 && amount < 50)
		{
			return CityState.Happy;
		}
		if (amount >= 50)
		{
			return CityState.Euphoric;
		}
		return CityState.Gameover;
	}

	private void UpdateEnergyAmount()
	{
	}

	public bool TryConsumeEnergy(int amount, GameCard consumer)
	{
		if (WorldManager.instance.DebugNoEnergyEnabled)
		{
			return true;
		}
		if (amount == 0)
		{
			return true;
		}
		List<IEnergy> cardsImplementingInterface = WorldManager.instance.GetCardsImplementingInterface<IEnergy>();
		if (cardsImplementingInterface.Sum((IEnergy x) => x.EnergyAmount) < amount)
		{
			return false;
		}
		cardsImplementingInterface = cardsImplementingInterface.OrderBy(delegate(IEnergy x)
		{
			CardData cardData = x as CardData;
			return (cardData is Battery) ? (-1000 - cardData.MyGameCard.GetCardIndex()) : (-cardData.MyGameCard.GetCardIndex());
		}).ToList();
		foreach (IEnergy item in cardsImplementingInterface)
		{
			int num = Mathf.Min(item.EnergyAmount, amount);
			item.UseEnergy(num);
			amount -= num;
			if (amount == 0)
			{
				break;
			}
		}
		return true;
	}

	public int TryUseDollars(List<ICurrency> currencyList, int cost, bool onlyTakeIfAmountMet = false, bool spawnSmoke = false, bool keepOnStack = false)
	{
		if (WorldManager.instance.DebugNoFoodEnabled)
		{
			return 0;
		}
		if (cost == 0)
		{
			return cost;
		}
		if (currencyList.Count <= 0)
		{
			return cost;
		}
		if (onlyTakeIfAmountMet && currencyList.Sum((ICurrency x) => x.CurrencyValue) < cost)
		{
			return cost;
		}
		int num = currencyList.Sum((ICurrency x) => x.CurrencyValue);
		int num2 = Mathf.Min(cost, num);
		GameCard rootCard = currencyList[0].Card.MyGameCard.GetRootCard();
		if (num > 0)
		{
			foreach (Creditcard item in (from Creditcard x in currencyList.Where((ICurrency x) => x is Creditcard creditcard && creditcard.DollarCount > 0)
				orderby x.DollarCount
				select x).ToList())
			{
				int num3 = Mathf.Min(item.CurrencyValue, cost);
				item.UseCurrency(num3, spawnSmoke);
				num2 -= num3;
				if (num2 == 0)
				{
					break;
				}
			}
			if (num2 > 0)
			{
				for (int i = 0; i < this.takeOrder.Length; i++)
				{
					int curBillAmount = this.takeOrder[i];
					int b = num2 / curBillAmount;
					b = Mathf.Min(currencyList.Count((ICurrency x) => x is Dollar dollar && dollar.DollarValue == curBillAmount), b);
					num2 -= b * curBillAmount;
					for (int j = 0; j < b; j++)
					{
						Dollar dollar3 = currencyList.Where((ICurrency x) => x is Dollar dollar2 && dollar2.DollarValue == curBillAmount).FirstOrDefault() as Dollar;
						currencyList.Remove(dollar3);
						dollar3.UseCurrency(dollar3.CurrencyValue, spawnSmoke);
					}
					if (num2 <= 0)
					{
						break;
					}
				}
			}
			if (num2 > 0 && currencyList.Count > 0)
			{
				Dollar dollar4 = (from x in currencyList
					where x is Dollar
					orderby x.CurrencyValue
					select x).FirstOrDefault() as Dollar;
				int value = dollar4.DollarValue - num2;
				List<GameCard> source = WorldManager.instance.CreateDollarsFromValue(value, dollar4.Position);
				currencyList.AddRange(source.Select((GameCard x) => x.CardData as ICurrency));
				currencyList.Remove(dollar4);
				dollar4.UseCurrency(dollar4.CurrencyValue, spawnSmoke);
				num2 = 0;
			}
		}
		if (keepOnStack && currencyList.Count > 0 && rootCard != null)
		{
			currencyList[0].Card.MyGameCard.GetRootCard().SetParent(rootCard);
		}
		else if (currencyList.Count > 0)
		{
			currencyList[0].Card.MyGameCard.RemoveFromParent();
			currencyList[0].Card.MyGameCard.SendIt();
		}
		return cost - num2;
	}

	public IEnumerator ConsumeFood(int amount, Vector3 targetPos)
	{
		List<Food> foodToUse = this.GetFoodToUse(amount);
		int num = amount;
		foreach (Food food in foodToUse)
		{
			if (num <= 0)
			{
				break;
			}
			food.IsReserved = true;
			int num2 = Mathf.Min(num, food.FoodValue);
			num -= num2;
			food.FoodValue -= num2;
			food.MyGameCard.PushEnabled = false;
			if (food.FoodValue <= 0)
			{
				if (food.MyGameCard.HasParent && food.MyGameCard.HasChild)
				{
					GameCard parent = food.MyGameCard.Parent;
					GameCard child = food.MyGameCard.Child;
					food.MyGameCard.RemoveFromStack();
					parent.SetChild(child);
				}
				else
				{
					food.MyGameCard.RemoveFromStack();
				}
				_ = food.Position;
				if (!(food is FoodWarehouse))
				{
					food.IsConsumed = true;
					food.MyGameCard.SendToPositionCallback(targetPos, delegate
					{
						food.MyGameCard.DestroyCard();
					});
				}
				else
				{
					WorldManager.instance.CreateSmoke(food.Position);
				}
			}
			food.MyGameCard.PushEnabled = true;
			food.IsReserved = false;
		}
		return null;
	}

	public List<Food> GetFoodToUse(int amount)
	{
		List<Food> source = (from x in WorldManager.instance.GetCards<Food>()
			where !x.IsReserved
			select x).ToList();
		if (source.Sum((Food x) => x.FoodValue) < amount)
		{
			return new List<Food>();
		}
		return source.Where((Food x) => x.FoodValue > 0).OrderByDescending(delegate(Food x)
		{
			bool flag = x.MyGameCard.GetCardWithStatusInStack() != null;
			if (x is FoodWarehouse foodWarehouse && WorldManager.instance.GameDataLoader.GetCardFromId(foodWarehouse.HeldCardId) is Food food)
			{
				return food.FoodValue;
			}
			if (flag)
			{
				return -3;
			}
			return (!x.IsCookedFood) ? (-2) : 0;
		}).ThenBy((Food x) => x.FoodValue)
			.ThenBy((Food x) => -x.MyGameCard.GetCardIndex())
			.ToList();
	}

	public static string GetAmountPrefix(int amount)
	{
		if (amount > 0)
		{
			return "+";
		}
		return "";
	}
}
