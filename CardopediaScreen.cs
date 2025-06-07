using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardopediaScreen : SokScreen
{
	public RectTransform EntriesParent;

	public CustomButton BackButton;

	public ScrollRect ScrollRect;

	public TMP_InputField SearchField;

	public ExpandableLabelCardopedia LabelPrefab;

	public CardopediaEntryElement CardopediaEntryPrefab;

	public CardopediaEntryElement HoveredEntry;

	public TextMeshProUGUI CardFoundAmount;

	public TextMeshProUGUI CardDescription;

	public Transform TargetCardPos;

	public Transform CardopediaBackground;

	public CustomButton All;

	public CustomButton Main;

	public CustomButton Island;

	public CustomButton Forest;

	public CustomButton Order;

	public CustomButton Spirit;

	public CustomButton Cities;

	public CustomButton Modded;

	private List<CustomButton> tabButtons;

	private CustomButton activeTab;

	private CardUpdateType? activeCardUpdateType;

	private List<CardopediaEntryElement> entries = new List<CardopediaEntryElement>();

	private List<ExpandableLabelCardopedia> labels = new List<ExpandableLabelCardopedia>();

	private CardopediaEntryElement lastHoveredEntry;

	private GameCard demoCard;

	private List<object> listChildren = new List<object>();

	public static CardopediaScreen instance;

	private bool SearchDisabled;

	private int totalFoundCount;

	private int currentTotalCardCount;

	public bool IsSearching => !string.IsNullOrEmpty(this.SearchField.text);

	private void Awake()
	{
		CardopediaScreen.instance = this;
		this.tabButtons = new List<CustomButton> { this.All, this.Main, this.Island, this.Forest, this.Order, this.Spirit, this.Cities, this.Modded };
		this.BackButton.Clicked += delegate
		{
			this.CardopediaBackground.gameObject.SetActive(value: false);
			this.ClearScreen();
			if (WorldManager.instance.CurrentGameState == WorldManager.GameState.InMenu)
			{
				GameCanvas.instance.SetScreen<MainMenu>();
			}
			else
			{
				GameCanvas.instance.SetScreen<PauseScreen>();
			}
		};
		this.SearchField.onValueChanged.AddListener(delegate(string value)
		{
			this.FilterEntries();
			foreach (ExpandableLabelCardopedia label in this.labels)
			{
				if (this.GetActiveLabelChildrenCount(label) > 0 && !string.IsNullOrEmpty(value))
				{
					label.SetExpanded(expanded: true);
					label.ShowChildrenCardopedia();
				}
			}
		});
		SokLoc.instance.LanguageChanged += Instance_LanguageChanged;
		this.AddTabListeners();
		this.CardopediaBackground = GameCamera.instance.transform.Find("CardopediaBackground");
		this.TargetCardPos = GameCamera.instance.transform.Find("TargetCardPos");
		this.CardopediaBackground.gameObject.SetActive(value: false);
		this.CreateEntries();
		if (!PlatformHelper.HasModdingSupport)
		{
			this.Modded.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= Instance_LanguageChanged;
		}
	}

	private void Instance_LanguageChanged()
	{
		foreach (CardopediaEntryElement entry in this.entries)
		{
			entry.UpdateText();
		}
		if (this.demoCard != null)
		{
			this.demoCard.CardData.OnLanguageChange();
		}
		this.UpdateLabels();
	}

	public void RefreshCardopedia()
	{
		foreach (CardopediaEntryElement entry in this.entries)
		{
			entry.SetCardData(entry.MyCardData);
			entry.UpdateText();
		}
		this.UpdateLabels();
	}

	private void OnEnable()
	{
		this.RefreshCardopedia();
		this.CardDescription.transform.parent.gameObject.SetActive(value: false);
		this.CardopediaBackground.gameObject.SetActive(value: true);
		this.totalFoundCount = this.DetermineFoundCount(null);
		this.SwitchActiveTab(this.All);
		this.ScrollRect.verticalNormalizedPosition = 1f;
	}

	private int DetermineFoundCount(CardUpdateType? updateType = null)
	{
		List<string> foundCardIds = WorldManager.instance.CurrentSave.FoundCardIds;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in foundCardIds)
		{
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
			}
		}
		int num = 0;
		List<CardData> list = WorldManager.instance.CardDataPrefabs;
		if (updateType.HasValue)
		{
			list = list.Where((CardData x) => x.CardUpdateType == updateType).ToList();
		}
		foreach (CardData item2 in list)
		{
			if (!item2.HideFromCardopedia && hashSet.Contains(item2.Id))
			{
				num++;
			}
		}
		return num;
	}

	private void AddTabListeners()
	{
		if (this.activeTab == null)
		{
			this.SwitchActiveTab(this.All);
		}
		foreach (CustomButton tab in this.tabButtons)
		{
			tab.Clicked += delegate
			{
				this.SwitchActiveTab(tab);
			};
			tab.ExplicitNavigationChanged += delegate(CustomButton but, Navigation nav)
			{
				nav.selectOnUp = null;
				nav.selectOnDown = this.GetFirstSelectableInList();
				return nav;
			};
		}
	}

	private Selectable GetFirstSelectableInList()
	{
		return this.labels.FirstOrDefault((ExpandableLabelCardopedia x) => x.gameObject.activeInHierarchy).MyButton;
	}

	private void SwitchActiveTab(CustomButton tab)
	{
		this.activeTab = tab;
		this.ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		this.SearchDisabled = false;
		this.CardFoundAmount.gameObject.SetActive(value: true);
		if (this.activeTab == this.All)
		{
			this.activeCardUpdateType = null;
			this.FilterEntriesCardUpdateType(null);
		}
		else if (this.activeTab == this.Main)
		{
			this.activeCardUpdateType = CardUpdateType.Main;
			this.FilterEntriesCardUpdateType(CardUpdateType.Main);
		}
		else if (this.activeTab == this.Island)
		{
			this.activeCardUpdateType = CardUpdateType.Island;
			this.FilterEntriesCardUpdateType(CardUpdateType.Island);
		}
		else if (this.activeTab == this.Forest)
		{
			this.activeCardUpdateType = CardUpdateType.Forest;
			this.FilterEntriesCardUpdateType(CardUpdateType.Forest);
		}
		else if (this.activeTab == this.Order)
		{
			this.activeCardUpdateType = CardUpdateType.Order;
			this.FilterEntriesCardUpdateType(CardUpdateType.Order);
		}
		else if (this.activeTab == this.Spirit)
		{
			this.activeCardUpdateType = CardUpdateType.Spirit;
			this.FilterEntriesCardUpdateType(CardUpdateType.Spirit);
			if (!WorldManager.instance.IsSpiritDlcActive())
			{
				this.SetTempDemoCard(WorldManager.instance.CardDataPrefabs.Find((CardData x) => x.Id == "card_display_spirit_dlc"));
				this.ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
				this.SearchDisabled = true;
				this.CardFoundAmount.gameObject.SetActive(value: false);
			}
		}
		else if (this.activeTab == this.Cities)
		{
			this.activeCardUpdateType = CardUpdateType.Cities;
			this.FilterEntriesCardUpdateType(CardUpdateType.Cities);
			if (!WorldManager.instance.IsCitiesDlcActive())
			{
				this.SetTempDemoCard(WorldManager.instance.CardDataPrefabs.Find((CardData x) => x.Id == "display_2000_dlc"));
				this.ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
				this.SearchDisabled = true;
				this.CardFoundAmount.gameObject.SetActive(value: false);
			}
		}
		else if (this.activeTab == this.Modded)
		{
			this.activeCardUpdateType = CardUpdateType.Mod;
			this.FilterEntriesCardUpdateType(CardUpdateType.Mod);
		}
		this.currentTotalCardCount = WorldManager.instance.CardDataPrefabs.Count((CardData x) => !x.HideFromCardopedia);
		if (this.activeTab != this.All)
		{
			this.currentTotalCardCount = WorldManager.instance.CardDataPrefabs.Count((CardData x) => !x.HideFromCardopedia && x.CardUpdateType == this.activeCardUpdateType);
			this.SearchField.text = "";
		}
	}

	private void FilterEntriesCardUpdateType(CardUpdateType? cardUpdateType)
	{
		foreach (CardopediaEntryElement entry in this.entries)
		{
			if (!cardUpdateType.HasValue || entry.MyCardData.CardUpdateType == cardUpdateType)
			{
				entry.IsFilteredUpdate = true;
			}
			else
			{
				entry.IsFilteredUpdate = false;
			}
		}
		this.UpdateLabels();
		this.UpdateEntries();
	}

	private void FilterEntries()
	{
		string text = this.SearchField.text;
		if (!string.IsNullOrEmpty(text))
		{
			if (this.activeTab != this.All)
			{
				this.SwitchActiveTab(this.All);
			}
			text = text.ToLower().Replace(" ", "");
			foreach (CardopediaEntryElement entry in this.entries)
			{
				if (entry.MyCardData.Name.ToLower().Replace(" ", "").Contains(text))
				{
					entry.IsFiltered = true;
				}
				else
				{
					entry.IsFiltered = false;
				}
			}
		}
		else
		{
			foreach (CardopediaEntryElement entry2 in this.entries)
			{
				entry2.IsFiltered = true;
			}
		}
		this.UpdateLabels();
	}

	public void UpdateLabels()
	{
		foreach (ExpandableLabelCardopedia label in this.labels)
		{
			label.ShowChildrenCardopedia();
			if (this.IsSearching)
			{
				if (this.GetActiveLabelChildrenCountSearch(label) > 0)
				{
					CardType type = label.Children[0].MyCardData.MyCardType;
					int num = WorldManager.instance.CardDataPrefabs.Count((CardData x) => x.MyCardType == type && !x.HideFromCardopedia);
					label.SetText(this.CardTypeToText(type) + $" ({this.GetActiveLabelChildrenCountSearch(label)}/{num})");
					label.gameObject.SetActive(value: true);
					continue;
				}
			}
			else if (label.Children.Count((CardopediaEntryElement x) => x.IsFilteredUpdate) > 0)
			{
				CardType type2 = label.Children[0].MyCardData.MyCardType;
				int num2 = (this.activeCardUpdateType.HasValue ? WorldManager.instance.CardDataPrefabs.Count((CardData x) => x.MyCardType == type2 && !x.HideFromCardopedia && x.CardUpdateType == this.activeCardUpdateType) : WorldManager.instance.CardDataPrefabs.Count((CardData x) => x.MyCardType == type2 && !x.HideFromCardopedia));
				label.SetText(this.CardTypeToText(type2) + $" ({this.GetActiveLabelChildrenCount(label)}/{num2})");
				label.gameObject.SetActive(value: true);
				continue;
			}
			label.gameObject.SetActive(value: false);
		}
		this.totalFoundCount = this.DetermineFoundCount(this.activeCardUpdateType);
	}

	private int GetActiveLabelChildrenCountSearch(ExpandableLabelCardopedia label)
	{
		return label.Children.Count((CardopediaEntryElement x) => x.IsFiltered && x.wasFound);
	}

	private int GetActiveLabelChildrenCount(ExpandableLabelCardopedia label)
	{
		return label.Children.Count((CardopediaEntryElement x) => x.IsFilteredUpdate && x.wasFound);
	}

	public void UpdateEntries()
	{
		float verticalNormalizedPosition = this.ScrollRect.verticalNormalizedPosition;
		this.FilterEntries();
		this.UpdatePositions();
		this.ScrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
		this.UpdatePositions();
	}

	private void CreateEntries()
	{
		List<CardData> cardDataPrefabs = WorldManager.instance.CardDataPrefabs;
		cardDataPrefabs = (from x in cardDataPrefabs
			orderby x.MyCardType, x.FullName
			select x).ToList();
		cardDataPrefabs.RemoveAll((CardData x) => x.HideFromCardopedia);
		new List<Transform>();
		foreach (Transform item in this.EntriesParent)
		{
			Object.Destroy(item.gameObject);
		}
		ExpandableLabelCardopedia expandableLabelCardopedia = null;
		this.labels = new List<ExpandableLabelCardopedia>();
		this.entries.Clear();
		this.listChildren.Clear();
		for (int i = 0; i < cardDataPrefabs.Count; i++)
		{
			CardData c = cardDataPrefabs[i];
			if (i == 0 || cardDataPrefabs[i - 1].MyCardType != cardDataPrefabs[i].MyCardType)
			{
				ExpandableLabelCardopedia label = Object.Instantiate(this.LabelPrefab);
				label.transform.SetParentClean(this.EntriesParent);
				int num = cardDataPrefabs.Count((CardData x) => x.MyCardType == c.MyCardType);
				int num2 = cardDataPrefabs.Count((CardData x) => x.MyCardType == c.MyCardType && WorldManager.instance.CurrentSave.FoundCardIds.Contains(x.Id));
				label.SetText(this.CardTypeToText(cardDataPrefabs[i].MyCardType) + $" ({num2}/{num})");
				label.Tag = cardDataPrefabs[i].MyCardType;
				label.SetCallback(delegate
				{
					float num3 = 0f - label.transform.localPosition.y - this.EntriesParent.localPosition.y;
					this.UpdateEntries();
					Vector3 localPosition = this.EntriesParent.transform.localPosition;
					localPosition.y = 0f - label.transform.localPosition.y - num3;
					this.EntriesParent.transform.localPosition = localPosition;
				});
				label.SetExpanded(expanded: false);
				label.MyButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
				{
					if (cb == this.GetFirstSelectableInList())
					{
						nav.selectOnUp = this.activeTab;
					}
					Selectable selectOnLeft = (nav.selectOnRight = null);
					nav.selectOnLeft = selectOnLeft;
					return nav;
				};
				this.listChildren.Add(label);
				this.labels.Add(label);
				expandableLabelCardopedia = label;
			}
			CardopediaEntryElement cardopediaEntryElement = Object.Instantiate(this.CardopediaEntryPrefab);
			cardopediaEntryElement.transform.SetParentClean(this.EntriesParent);
			cardopediaEntryElement.SetCardData(c);
			cardopediaEntryElement.IsEnabled = false;
			cardopediaEntryElement.IsFiltered = false;
			cardopediaEntryElement.IsFilteredUpdate = true;
			cardopediaEntryElement.Button.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
			{
				Selectable selectOnLeft2 = (nav.selectOnRight = null);
				nav.selectOnLeft = selectOnLeft2;
				return nav;
			};
			expandableLabelCardopedia.Children.Add(cardopediaEntryElement);
			this.entries.Add(cardopediaEntryElement);
			this.listChildren.Add(cardopediaEntryElement);
		}
		foreach (ExpandableLabelCardopedia l in this.labels)
		{
			if (this.entries.Any((CardopediaEntryElement e) => e.IsNew && e.MyCardData.MyCardType == (CardType)l.Tag))
			{
				l.SetExpanded(expanded: true);
			}
		}
	}

	private string CardTypeToText(CardType type)
	{
		return type.TranslateEnum();
	}

	private void OnDisable()
	{
		this.SearchField.text = string.Empty;
		this.ClearScreen();
	}

	private void ClearScreen()
	{
		if (this.demoCard != null)
		{
			Object.Destroy(this.demoCard.gameObject);
		}
		this.CardDescription.transform.parent.gameObject.SetActive(value: false);
		this.lastHoveredEntry = null;
		if (this.CardopediaBackground != null)
		{
			this.CardopediaBackground.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		this.HoveredEntry = null;
		if (GameCanvas.instance.ScreenIsInteractable<CardopediaScreen>())
		{
			foreach (CardopediaEntryElement entry in this.entries)
			{
				if (entry.Button.IsHovered || entry.Button.IsSelected)
				{
					this.HoveredEntry = entry;
				}
			}
		}
		if (this.lastHoveredEntry != null)
		{
			this.lastHoveredEntry.Button.Image.color = ColorManager.instance.ButtonColor;
		}
		if (this.HoveredEntry != null)
		{
			this.HoveredEntry.Button.Image.color = ColorManager.instance.HoverButtonColor;
		}
		this.UpdatePositions();
		if (this.lastHoveredEntry != this.HoveredEntry && this.HoveredEntry != null)
		{
			if (this.demoCard != null)
			{
				Object.Destroy(this.demoCard.gameObject);
			}
			this.demoCard = Object.Instantiate(PrefabManager.instance.GameCardPrefab);
			CardData cardData = Object.Instantiate(this.HoveredEntry.MyCardData);
			cardData.transform.SetParent(this.demoCard.transform);
			this.demoCard.CardData = cardData;
			cardData.MyGameCard = this.demoCard;
			this.demoCard.FaceUp = this.HoveredEntry.wasFound;
			this.demoCard.IsDemoCard = true;
			this.demoCard.SetDemoCardRotation();
			this.demoCard.CardData.UpdateCardText();
			this.demoCard.UpdateCardPalette();
			cardData.UpdateCard();
			this.demoCard.ForceUpdate();
		}
		if (this.demoCard != null)
		{
			Vector3 position = this.TargetCardPos.position;
			this.demoCard.transform.position = (this.demoCard.TargetPosition = position);
		}
		if (this.HoveredEntry != null)
		{
			this.CardDescription.transform.parent.gameObject.SetActive(value: true);
			if (this.HoveredEntry.wasFound)
			{
				this.demoCard.CardData.UpdateCardText();
				string dropSummaryFromCard = this.GetDropSummaryFromCard(this.HoveredEntry.MyCardData);
				string description = this.demoCard.CardData.Description;
				description = description.Replace("\\d", "\n\n");
				if (!string.IsNullOrEmpty(dropSummaryFromCard) && this.HoveredEntry.MyCardData.MyCardType != CardType.Locations)
				{
					description = description + "\n\n" + dropSummaryFromCard;
				}
				if (this.HoveredEntry.MyCardData is Blueprint blueprint)
				{
					description = blueprint.GetText();
				}
				this.CardDescription.text = description;
			}
			else
			{
				this.CardDescription.text = SokLoc.Translate("label_card_not_found");
			}
		}
		this.SearchField.gameObject.SetActive(!InputController.instance.CurrentSchemeIsController && !this.SearchDisabled);
		this.CardFoundAmount.text = SokLoc.Translate("label_cards_found", LocParam.Create("found", this.totalFoundCount.ToString()), LocParam.Create("total", this.currentTotalCardCount.ToString()));
		this.lastHoveredEntry = this.HoveredEntry;
		this.UpdateTabs();
	}

	private void UpdateTabs()
	{
		foreach (CustomButton tabButton in this.tabButtons)
		{
			if (tabButton.gameObject.activeInHierarchy)
			{
				bool flag = tabButton == this.activeTab;
				Color color = (tabButton.IsSelected ? ColorManager.instance.BackgroundColor2 : ((!flag) ? ColorManager.instance.InactiveBackgroundColor : ColorManager.instance.BackgroundColor));
				tabButton.Image.color = color;
			}
		}
	}

	public void UpdatePositions()
	{
		int num = 0;
		Vector2 sizeDelta = this.EntriesParent.sizeDelta;
		Vector2 vector = this.EntriesParent.localPosition;
		Rect rect = this.EntriesParent.rect;
		float height = ((RectTransform)this.EntriesParent.parent).rect.height;
		float num2 = 0f - vector.y - height * 0.5f;
		for (int i = 0; i < this.listChildren.Count; i++)
		{
			object obj = this.listChildren[i];
			bool flag = false;
			RectTransform rectTransform = null;
			if (obj is ExpandableLabelCardopedia expandableLabelCardopedia)
			{
				flag = expandableLabelCardopedia.gameObject.activeInHierarchy;
				rectTransform = (RectTransform)expandableLabelCardopedia.transform;
			}
			if (obj is CardopediaEntryElement cardopediaEntryElement)
			{
				flag = cardopediaEntryElement.IsEnabled;
				rectTransform = (RectTransform)cardopediaEntryElement.transform;
				cardopediaEntryElement.Button.Image.raycastTarget = cardopediaEntryElement.IsEnabled;
			}
			if (flag)
			{
				Vector3 localPosition = rectTransform.localPosition;
				localPosition.x = 0f;
				localPosition.y = (float)(-num) * 50f;
				rectTransform.localPosition = localPosition;
				Vector2 sizeDelta2 = rectTransform.sizeDelta;
				sizeDelta2.x = rect.width;
				rectTransform.sizeDelta = sizeDelta2;
				num++;
			}
			else
			{
				Vector3 position = new Vector3(1000f, 1000f);
				rectTransform.position = position;
			}
			if (obj is CardopediaEntryElement cardopediaEntryElement2)
			{
				bool flag2 = Mathf.Abs(rectTransform.localPosition.y - num2) < height * 0.75f;
				cardopediaEntryElement2.Cull(!cardopediaEntryElement2.IsEnabled || !flag2);
			}
		}
		sizeDelta.y = (float)num * 50f;
		this.EntriesParent.sizeDelta = sizeDelta;
	}

	private string GetDropSummaryFromCard(CardData cardData)
	{
		if (cardData is Harvestable)
		{
			return BoosterpackData.GetSummaryFromAllCards(cardData.GetPossibleDrops(), "label_can_drop");
		}
		if (cardData is Enemy)
		{
			return BoosterpackData.GetSummaryFromAllCards(cardData.GetPossibleDrops(), "label_can_drop");
		}
		return "";
	}

	private void SetTempDemoCard(CardData data)
	{
		if (this.demoCard != null)
		{
			Object.Destroy(this.demoCard.gameObject);
		}
		this.demoCard = Object.Instantiate(PrefabManager.instance.GameCardPrefab);
		CardData cardData = Object.Instantiate(data);
		cardData.transform.SetParent(this.demoCard.transform);
		this.demoCard.CardData = cardData;
		cardData.MyGameCard = this.demoCard;
		this.demoCard.FaceUp = true;
		this.demoCard.IsDemoCard = true;
		this.demoCard.SetDemoCardRotation();
		this.demoCard.UpdateCardPalette();
		cardData.UpdateCard();
		this.demoCard.ForceUpdate();
		this.CardDescription.transform.parent.gameObject.SetActive(value: true);
		this.demoCard.CardData.UpdateCardText();
		string dropSummaryFromCard = this.GetDropSummaryFromCard(cardData);
		string description = this.demoCard.CardData.Description;
		description = description.Replace("\\d", "\n\n");
		if (cardData is Combatable combatable)
		{
			description += combatable.GetCombatableDescriptionAdvanced();
		}
		if (!string.IsNullOrEmpty(dropSummaryFromCard) && cardData.MyCardType != CardType.Locations)
		{
			description = description + "\n\n" + dropSummaryFromCard;
		}
		if (cardData is Blueprint blueprint)
		{
			description = blueprint.GetText();
		}
		this.CardDescription.text = description;
	}
}
