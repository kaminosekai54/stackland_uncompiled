using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameScreen : SokScreen
{
	public TextMeshProUGUI MoneyText;

	public TextMeshProUGUI FoodText;

	public TextMeshProUGUI CardText;

	public TextMeshProUGUI TimeText;

	public TextMeshProUGUI HappinessText;

	public TextMeshProUGUI EnergyText;

	public TextMeshProUGUI WellbeingText;

	public TextMeshProUGUI DollarText;

	public TextMeshProUGUI WorkerText;

	public TextMeshProUGUI InfoTitle;

	public TextMeshProUGUI InfoText;

	public GameObject InfoDividerPrefab;

	private List<GameObject> dividerList = new List<GameObject>();

	public RectTransform InfoLayoutGroup;

	public Image TimeFill;

	public GameObject InfoBox;

	public RectTransform ResourceRect;

	public RectTransform TimeRect;

	public RectTransform ViewRect;

	public TextMeshProUGUI Valuetext;

	public GameObject ValueParent;

	public GameObject FoodCardBox;

	public ShowInfoBox ShowInfoBoxMoney;

	public ShowInfoBox ShowInfoBoxFood;

	public ShowInfoBox ShowInfoBoxTime;

	public ShowInfoBox ShowInfoBoxCard;

	public ShowInfoBox ShowInfoBoxHappiness;

	public ShowInfoBox ShowInfoBoxEnergy;

	public ShowInfoBox ShowInfoBoxWellbeing;

	public ShowInfoBox ShowInfoBoxDollar;

	public ShowInfoBox ShowInfoBoxWorker;

	public ShowInfoBox ShowInfoBoxEnergyButton;

	public Image Crosshair;

	public static string InfoBoxTitle;

	public static string InfoBoxText;

	public Image DebugScreen;

	public CustomButton GameSpeedButton;

	public CustomButton ViewButton;

	public TextMeshProUGUI NextPackToUnlockText;

	public static GameScreen instance;

	public TextMeshProUGUI NoIdeasYetText;

	public RectTransform QuestsParent;

	public RectTransform QuestsTab;

	public RectTransform IdeasTab;

	public TextMeshProUGUI PausedText;

	public CustomButton QuestsButton;

	public CustomButton IdeasButton;

	public TMP_InputField IdeaSearchField;

	public RectTransform NotificationsParent;

	public CustomButton MinimizeButton;

	public GameObject IdeasTabNew;

	public GameObject QuestsTabNew;

	public Image FoldedNewIcon;

	public ScrollRect QuestsScrollRect;

	public ScrollRect IdeasScrollRect;

	private bool isMinimized;

	public bool ControllerIsInUI;

	private List<CustomButton> questButtons;

	private List<CustomButton> ideaButtons;

	private string previousInfoText = "";

	public string HappinessSummaryText;

	public string EnergySummaryText;

	public string WellbeingSummaryText;

	public RectTransform ViewDropdown;

	public CustomButton DefaultViewButton;

	public CustomButton EnergyViewButton;

	public CustomButton TransportViewButton;

	public CustomButton SewageViewButton;

	public CustomButton CalamityViewButton;

	private float pauseBlinkTimer;

	private bool gameSpeedButtonClicked;

	private bool questTabOpen = true;

	public List<AchievementElement> questElements;

	private List<QuestGroup> questGroupOrder = new List<QuestGroup>
	{
		QuestGroup.Starter,
		QuestGroup.MainQuest,
		QuestGroup.Island_Beginnings,
		QuestGroup.Island_Combat,
		QuestGroup.Island_Cooking,
		QuestGroup.Island_Misc,
		QuestGroup.Island_MainQuest,
		QuestGroup.Forest_MainQuest,
		QuestGroup.Fighting,
		QuestGroup.Equipment,
		QuestGroup.Cooking,
		QuestGroup.Exploration,
		QuestGroup.Resources,
		QuestGroup.Building,
		QuestGroup.Survival,
		QuestGroup.Discover_Spirits,
		QuestGroup.Other
	};

	public RectTransform IdeaElementsParent;

	private List<BlueprintGroup> groups = new List<BlueprintGroup>
	{
		BlueprintGroup.Basic,
		BlueprintGroup.Important,
		BlueprintGroup.Building,
		BlueprintGroup.Cooking,
		BlueprintGroup.Military,
		BlueprintGroup.Resources,
		BlueprintGroup.Island,
		BlueprintGroup.Sailing,
		BlueprintGroup.Fishing,
		BlueprintGroup.Happiness,
		BlueprintGroup.Greed,
		BlueprintGroup.Death,
		BlueprintGroup.Power,
		BlueprintGroup.Automation,
		BlueprintGroup.Landmark
	};

	private List<IdeaElement> ideaElements;

	private List<ExpandableLabel> ideaLabels;

	private int foundCount;

	public float prePauseSpeed = 1f;

	public RectTransform SideTransform;

	public ShowInfoBox MinimizeButtonInfoBox;

	private Dictionary<string, CardData> stackRequirements = new Dictionary<string, CardData>();

	private Dictionary<string, int> stackRequirementAmount = new Dictionary<string, int>();

	private float redTextBlinkTimer;

	private bool redBlink;

	public Image GameSpeedIcon;

	public override bool IsFrameRateUncapped => true;

	private void Awake()
	{
		GameScreen.instance = this;
		this.DebugScreen.gameObject.SetActive(value: false);
		this.GameSpeedButton.Clicked += delegate
		{
			this.gameSpeedButtonClicked = true;
		};
		this.ViewDropdown.gameObject.SetActive(value: false);
		this.ViewButton.Clicked += delegate
		{
			this.ViewDropdown.gameObject.SetActive(value: true);
		};
		this.DefaultViewButton.Clicked += delegate
		{
			this.SetView(ViewType.Default);
		};
		this.EnergyViewButton.Clicked += delegate
		{
			this.SetView(ViewType.Energy);
		};
		this.TransportViewButton.Clicked += delegate
		{
			this.SetView(ViewType.Transport);
		};
		this.SewageViewButton.Clicked += delegate
		{
			this.SetView(ViewType.Sewer);
		};
		this.CalamityViewButton.Clicked += delegate
		{
			this.SetView(ViewType.Calamity);
		};
		this.MinimizeButton.Clicked += delegate
		{
			this.ToggleMinimize();
		};
		this.IdeaSearchField.onValueChanged.AddListener(delegate
		{
			this.UpdateIdeasLog();
		});
		this.QuestsButton.Clicked += delegate
		{
			this.questTabOpen = true;
			this.QuestsTab.gameObject.SetActive(value: true);
			this.IdeasTab.gameObject.SetActive(value: false);
		};
		this.QuestsButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
		{
			List<CustomButton> list = (this.questTabOpen ? this.questButtons : this.ideaButtons);
			nav.selectOnDown = ((list != null && list.Count > 0) ? list[0] : null);
			nav.selectOnRight = this.IdeasButton;
			return nav;
		};
		this.IdeasButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
		{
			List<CustomButton> list2 = (this.questTabOpen ? this.questButtons : this.ideaButtons);
			nav.selectOnDown = ((list2 != null && list2.Count > 0) ? list2[0] : null);
			nav.selectOnLeft = this.QuestsButton;
			return nav;
		};
		this.IdeasButton.Clicked += delegate
		{
			this.questTabOpen = false;
			this.QuestsTab.gameObject.SetActive(value: false);
			this.IdeasTab.gameObject.SetActive(value: true);
		};
		this.GameSpeedButton.IsSelectableAction = () => false;
		this.MinimizeButton.IsSelectableAction = () => false;
		this.QuestsTab.gameObject.SetActive(value: true);
		this.IdeasTab.gameObject.SetActive(value: false);
		this.PausedText.gameObject.SetActive(value: false);
		this.NotificationsParent.gameObject.SetActive(value: true);
		this.SetViewdropdownTexts();
		this.InitIdeaElements();
		SokLoc.instance.LanguageChanged += Instance_LanguageChanged;
	}

	private void SetView(ViewType viewType)
	{
		WorldManager.instance.SetViewType(viewType);
		this.ViewDropdown.gameObject.SetActive(value: false);
	}

	public void CloseViewDropdown()
	{
		this.ViewDropdown.gameObject.SetActive(value: false);
	}

	private string GetIconForView(ViewType viewType)
	{
		return viewType switch
		{
			ViewType.Default => Icons.Card, 
			ViewType.Energy => Icons.Energy, 
			ViewType.Sewer => Icons.Sewer, 
			ViewType.Transport => Icons.Transport, 
			ViewType.Calamity => Icons.Calamity, 
			_ => throw new ArgumentException(), 
		};
	}

	private string GetLabelForViewType(ViewType viewType)
	{
		switch (viewType)
		{
		case ViewType.Default:
			return SokLoc.Translate("label_view_default");
		case ViewType.Energy:
			return SokLoc.Translate("label_view_energy");
		case ViewType.Sewer:
			return SokLoc.Translate("label_view_sewage");
		case ViewType.Calamity:
			return SokLoc.Translate("label_view_calamity");
		case ViewType.Transport:
			if (!(WorldManager.instance.GetCurrentBoardSafe().Id == "cities"))
			{
				return SokLoc.Translate("label_view_transport_default");
			}
			return SokLoc.Translate("label_view_transport");
		default:
			throw new ArgumentException();
		}
	}

	private void SetViewdropdownTexts()
	{
		this.DefaultViewButton.TextMeshPro.text = this.GetLabelForViewType(ViewType.Default) + this.GetIconForView(ViewType.Default);
		this.EnergyViewButton.TextMeshPro.text = this.GetLabelForViewType(ViewType.Energy) + this.GetIconForView(ViewType.Energy);
		this.TransportViewButton.TextMeshPro.text = this.GetLabelForViewType(ViewType.Transport) + this.GetIconForView(ViewType.Transport);
		this.SewageViewButton.TextMeshPro.text = this.GetLabelForViewType(ViewType.Sewer) + this.GetIconForView(ViewType.Sewer);
		this.CalamityViewButton.TextMeshPro.text = this.GetLabelForViewType(ViewType.Calamity) + this.GetIconForView(ViewType.Calamity);
	}

	public void OnBoardChange()
	{
		this.SetViewdropdownTexts();
	}

	private void Instance_LanguageChanged()
	{
		this.UpdateIdeasLog();
		this.UpdateQuestLog();
		this.SetViewdropdownTexts();
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= Instance_LanguageChanged;
		}
	}

	public DebugScreen GetDebugComponent()
	{
		return this.DebugScreen?.GetComponent<DebugScreen>();
	}

	public void SetQuestTab()
	{
		this.questTabOpen = true;
		this.QuestsTab.gameObject.SetActive(value: true);
		this.IdeasTab.gameObject.SetActive(value: false);
	}

	public void ScrollToQuest(Quest quest)
	{
		base.StartCoroutine(this.ScrollToQuestCoroutine(quest));
	}

	private IEnumerator ScrollToQuestCoroutine(Quest quest)
	{
		this.UpdateQuestLog();
		this.SetQuestTab();
		yield return null;
		ExpandableLabel expandableLabel = this.QuestsParent.GetComponentsInChildren<ExpandableLabel>().FirstOrDefault((ExpandableLabel x) => (QuestGroup)x.Tag == quest.QuestGroup);
		if (expandableLabel != null)
		{
			expandableLabel.SetExpanded(expanded: true);
		}
		AchievementElement achievementElement = this.questElements.FirstOrDefault((AchievementElement x) => x.MyQuest == quest);
		if (achievementElement != null)
		{
			GameCanvas.SetScrollRectPosition(this.QuestsScrollRect, achievementElement.transform as RectTransform);
		}
	}

	public void SetMinimize(bool minimized)
	{
		this.isMinimized = minimized;
		if (this.isMinimized)
		{
			this.QuestsTab.gameObject.SetActive(value: false);
			this.IdeasTab.gameObject.SetActive(value: false);
		}
		else
		{
			this.QuestsTab.gameObject.SetActive(this.questTabOpen);
			this.IdeasTab.gameObject.SetActive(!this.questTabOpen);
		}
	}

	public void ToggleMinimize()
	{
		this.SetMinimize(!this.isMinimized);
	}

	private void OnEnable()
	{
		if (!(QuestManager.instance == null))
		{
			this.UpdateQuestLog();
			this.UpdateIdeasLog();
		}
	}

	public void UpdateQuestLog()
	{
		if (QuestManager.instance == null)
		{
			return;
		}
		Dictionary<object, bool> dictionary = this.wasExpandedDict(this.QuestsParent.GetComponentsInChildren<ExpandableLabel>());
		IEnumerable<Quest> source = ((WorldManager.instance.CurrentBoard == null || WorldManager.instance.CurrentBoard.Id == "main") ? ((!QuestManager.instance.AllQuests.Any((Quest x) => x.QuestGroup == QuestGroup.Starter && !QuestManager.instance.QuestIsComplete(x))) ? QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation != Location.Death && x.QuestLocation != Location.Greed && x.QuestLocation != Location.Happiness && x.QuestLocation != Location.Cities) : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestGroup == QuestGroup.Starter)) : ((WorldManager.instance.CurrentBoard.Id == "island") ? ((!QuestManager.instance.AllQuests.Any((Quest x) => x.QuestGroup == QuestGroup.Island_Beginnings && !QuestManager.instance.QuestIsComplete(x))) ? QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation != Location.Death && x.QuestLocation != Location.Greed && x.QuestLocation != Location.Happiness && x.QuestLocation != Location.Cities) : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestGroup == QuestGroup.Island_Beginnings)) : ((!(WorldManager.instance.CurrentBoard.Id == "forest")) ? QuestManager.instance.AllQuests : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation == Location.Forest))));
		if (WorldManager.instance.CurrentRunVariables != null && !WorldManager.instance.CurrentRunVariables.VisitedIsland)
		{
			source = source.Where((Quest x) => x.QuestLocation != Location.Island);
		}
		if (WorldManager.instance.CurrentBoard?.Id == "happiness")
		{
			source = ((!QuestManager.instance.AllQuests.Any((Quest x) => x.QuestGroup == QuestGroup.Happiness_Starter && !QuestManager.instance.QuestIsComplete(x))) ? QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation == Location.Happiness) : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestGroup == QuestGroup.Happiness_Starter));
		}
		else if (WorldManager.instance.CurrentBoard?.Id == "greed")
		{
			source = ((!QuestManager.instance.AllQuests.Any((Quest x) => x.QuestGroup == QuestGroup.Greed_Starter && !QuestManager.instance.QuestIsComplete(x))) ? QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation == Location.Greed) : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestGroup == QuestGroup.Greed_Starter));
		}
		else if (WorldManager.instance.CurrentBoard?.Id == "death")
		{
			source = ((!QuestManager.instance.AllQuests.Any((Quest x) => x.QuestGroup == QuestGroup.Death_Starter && !QuestManager.instance.QuestIsComplete(x))) ? QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation == Location.Death) : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestGroup == QuestGroup.Death_Starter));
		}
		if (WorldManager.instance.CurrentBoard?.Id == "cities")
		{
			source = ((!QuestManager.instance.AllQuests.Any((Quest x) => x.QuestGroup == QuestGroup.Cities_Starter && !QuestManager.instance.QuestIsComplete(x))) ? QuestManager.instance.AllQuests.Where((Quest x) => x.QuestLocation == Location.Cities) : QuestManager.instance.AllQuests.Where((Quest x) => x.QuestGroup == QuestGroup.Cities_Starter));
			if (!WorldManager.instance.HasFoundCard("blueprint_barrack"))
			{
				source = source.Where((Quest x) => x.QuestGroup != QuestGroup.Cities_Freedom);
			}
		}
		bool flag = WorldManager.instance.CurrentRunVariables.FinishedDemon || QuestManager.instance.QuestIsComplete("kill_demon");
		if (!WorldManager.instance.IsSpiritDlcActive() || !flag)
		{
			source = source.Where((Quest x) => x.QuestGroup != QuestGroup.Discover_Spirits);
		}
		this.questElements = this.CreateQuestElements(this.QuestsParent, source.ToList());
		this.questButtons = (from x in this.QuestsParent.GetComponentsInChildren<CustomButton>()
			where x.enabled
			select x).ToList();
		for (int i = 0; i < this.questButtons.Count - 1; i++)
		{
			this.questButtons[i].ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
			{
				int num = this.questButtons.IndexOf(cb);
				nav.selectOnUp = ((num == 0) ? this.QuestsButton : this.questButtons[num - 1]);
				nav.selectOnDown = this.questButtons[num + 1];
				return nav;
			};
		}
		ExpandableLabel[] componentsInChildren = this.QuestsParent.GetComponentsInChildren<ExpandableLabel>();
		foreach (AchievementElement questElement in this.questElements)
		{
			if (questElement.IsNew)
			{
				dictionary[questElement.MyQuest.QuestGroup] = true;
			}
		}
		this.SetFromWasExpandedDict(componentsInChildren, dictionary);
	}

	private string GetAchievementGroupName(QuestGroup group)
	{
		string text = "questgroup_";
		return SokLoc.Translate(group switch
		{
			QuestGroup.Starter => text + "starter", 
			QuestGroup.MainQuest => text + "mainquest", 
			QuestGroup.Fighting => text + "fighting", 
			QuestGroup.Cooking => text + "cooking", 
			QuestGroup.Exploration => text + "exploration", 
			QuestGroup.Resources => text + "resources", 
			QuestGroup.Building => text + "building", 
			QuestGroup.Survival => text + "survival", 
			QuestGroup.Other => text + "other", 
			QuestGroup.Island_Misc => text + "island", 
			_ => text + group.ToString().ToLower(), 
		});
	}

	private List<AchievementElement> CreateQuestElements(RectTransform parent, List<Quest> quests, bool addLabels = true)
	{
		List<AchievementElement> list = new List<AchievementElement>();
		foreach (Transform item in parent)
		{
			if (!item.name.StartsWith("DontDestroy"))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		List<Quest> source = new List<Quest>(quests);
		quests = (from x in quests
			orderby !x.IsMainQuest, this.questGroupOrder.IndexOf(x.QuestGroup)
			select x).ToList();
		quests.RemoveAll((Quest x) => !QuestManager.instance.QuestIsVisible(x));
		Quest quest = null;
		ExpandableLabel expandableLabel = null;
		bool flag = (from x in quests
			group x by x.QuestGroup).Count() > 1;
		for (int i = 0; i < quests.Count; i++)
		{
			Quest cur = quests[i];
			Quest quest2 = ((i == quests.Count - 1) ? null : quests[i + 1]);
			if (addLabels)
			{
				if (flag && (quest == null || quest.IsMainQuest != cur.IsMainQuest))
				{
					RectTransform rectTransform = UnityEngine.Object.Instantiate(PrefabManager.instance.NormalLabelPrefab);
					rectTransform.transform.SetParentClean(parent);
					rectTransform.GetComponent<CustomButton>().enabled = false;
					rectTransform.GetComponent<Image>().enabled = false;
					TextMeshProUGUI componentInChildren = rectTransform.GetComponentInChildren<TextMeshProUGUI>();
					componentInChildren.fontStyle = FontStyles.Bold;
					componentInChildren.text = (cur.IsMainQuest ? SokLoc.Translate("label_main_quests") : SokLoc.Translate("label_side_quests"));
				}
				if (quest == null || quest.QuestGroup != cur.QuestGroup)
				{
					expandableLabel = UnityEngine.Object.Instantiate(PrefabManager.instance.AchievementElementLabelPrefab).GetComponent<ExpandableLabel>();
					expandableLabel.transform.SetParentClean(parent);
					expandableLabel.Tag = cur.QuestGroup;
					int num = source.Count((Quest x) => x.QuestGroup == cur.QuestGroup);
					int num2 = source.Count((Quest x) => x.QuestGroup == cur.QuestGroup && QuestManager.instance.QuestIsComplete(x));
					string achievementGroupName = this.GetAchievementGroupName(cur.QuestGroup);
					achievementGroupName = ((num != num2) ? (achievementGroupName + $" ({num2}/{num})") : (achievementGroupName + " " + Icons.Checkmark));
					expandableLabel.SetText(achievementGroupName);
					if (flag)
					{
						expandableLabel.SetExpanded(expanded: false);
					}
				}
			}
			AchievementElement achievementElement = UnityEngine.Object.Instantiate(PrefabManager.instance.AchievementElementPrefab);
			achievementElement.SetQuest(cur);
			expandableLabel.Children.Add(achievementElement.gameObject);
			if (flag)
			{
				achievementElement.gameObject.SetActive(value: false);
			}
			achievementElement.transform.SetParentClean(parent);
			list.Add(achievementElement);
			if ((quest2 == null || cur.QuestGroup != quest2.QuestGroup) && source.Count((Quest x) => x.QuestGroup == cur.QuestGroup && !QuestManager.instance.QuestIsVisible(x)) > 0)
			{
				AchievementElement achievementElement2 = UnityEngine.Object.Instantiate(PrefabManager.instance.EmptyAchievementElementPrefab);
				achievementElement2.transform.SetParentClean(parent);
				expandableLabel.Children.Add(achievementElement2.gameObject);
				if (flag)
				{
					achievementElement2.gameObject.SetActive(value: false);
				}
			}
			quest = cur;
		}
		return list;
	}

	private Dictionary<object, bool> wasExpandedDict(ExpandableLabel[] labels)
	{
		Dictionary<object, bool> dictionary = new Dictionary<object, bool>();
		foreach (ExpandableLabel expandableLabel in labels)
		{
			dictionary[expandableLabel.Tag] = expandableLabel.IsExpanded;
		}
		return dictionary;
	}

	private void SetFromWasExpandedDict(ExpandableLabel[] labels, Dictionary<object, bool> wasExpanded)
	{
		foreach (ExpandableLabel expandableLabel in labels)
		{
			if (wasExpanded.ContainsKey(expandableLabel.Tag))
			{
				expandableLabel.SetExpanded(wasExpanded[expandableLabel.Tag]);
			}
		}
	}

	public void UpdateIdeaElements()
	{
		this.UpdateIdeasLog();
	}

	public void InitIdeaElements()
	{
		List<IKnowledge> list = new List<IKnowledge>();
		IEnumerable<Rumor> collection = WorldManager.instance.CardDataPrefabs.OfType<Rumor>();
		list.AddRange(collection);
		List<Blueprint> list2 = new List<Blueprint>(WorldManager.instance.BlueprintPrefabs);
		list2.RemoveAll((Blueprint x) => x.HideFromIdeasTab);
		list.AddRange(list2.Cast<IKnowledge>());
		List<IKnowledge> source = new List<IKnowledge>(list);
		list = (from k in list
			orderby this.groups.IndexOf(k.Group), k.KnowledgeName
			select k).ToList();
		_ = list.Count;
		IKnowledge knowledge = null;
		ExpandableLabel expandableLabel = null;
		this.ideaElements = new List<IdeaElement>();
		this.ideaLabels = new List<ExpandableLabel>();
		for (int i = 0; i < list.Count; i++)
		{
			IKnowledge cur = list[i];
			if (knowledge == null || knowledge.Group != cur.Group)
			{
				expandableLabel = UnityEngine.Object.Instantiate(PrefabManager.instance.AchievementElementLabelPrefab).GetComponent<ExpandableLabel>();
				expandableLabel.transform.SetParentClean(this.IdeaElementsParent);
				expandableLabel.Tag = cur.Group;
				source.Count((IKnowledge k) => k.Group == cur.Group);
				source.Count((IKnowledge k) => k.Group == cur.Group && this.KnowledgeWasFound(k));
				expandableLabel.SetText(this.GetBlueprintGroupText(cur.Group));
				expandableLabel.SetCallback(UpdateIdeaElements);
				this.ideaLabels.Add(expandableLabel);
			}
			IdeaElement ideaElement = UnityEngine.Object.Instantiate(PrefabManager.instance.IdeaElementPrefab);
			ideaElement.transform.SetParentClean(this.IdeaElementsParent);
			expandableLabel.Children.Add(ideaElement.gameObject);
			ideaElement.SetKnowledge(cur);
			this.ideaElements.Add(ideaElement);
			knowledge = cur;
		}
	}

	public void UpdateIdeasLog()
	{
		string searchTerm = "";
		if (!string.IsNullOrEmpty(this.IdeaSearchField.text))
		{
			searchTerm = this.IdeaSearchField.text;
		}
		List<IKnowledge> currentKnowledges = new List<IKnowledge>();
		IEnumerable<Rumor> collection = WorldManager.instance.CardDataPrefabs.OfType<Rumor>();
		currentKnowledges.AddRange(collection);
		List<Blueprint> list = new List<Blueprint>(WorldManager.instance.BlueprintPrefabs);
		list.RemoveAll((Blueprint x) => x.HideFromIdeasTab);
		currentKnowledges.AddRange(list.Cast<IKnowledge>());
		new List<IKnowledge>(currentKnowledges);
		currentKnowledges = currentKnowledges.OrderBy((IKnowledge k) => this.groups.IndexOf(k.Group)).ThenBy((IKnowledge x) => x.KnowledgeName).ToList();
		if (WorldManager.instance.CurrentBoard?.Id == "cities")
		{
			currentKnowledges = currentKnowledges.Where((IKnowledge x) => WorldManager.instance.GameDataLoader.GetCardFromId(x.CardId).CardUpdateType == CardUpdateType.Cities).ToList();
		}
		else
		{
			currentKnowledges = currentKnowledges.Where((IKnowledge x) => WorldManager.instance.GameDataLoader.GetCardFromId(x.CardId).CardUpdateType != CardUpdateType.Cities).ToList();
		}
		_ = currentKnowledges.Count;
		Dictionary<object, bool> dictionary = this.wasExpandedDict(this.IdeaElementsParent.GetComponentsInChildren<ExpandableLabel>());
		foreach (IdeaElement element in this.ideaElements)
		{
			IKnowledge knowledge = currentKnowledges.Find((IKnowledge x) => x.CardId == element.MyKnowledge.CardId);
			if (knowledge == null)
			{
				element.gameObject.SetActive(value: false);
				continue;
			}
			element.SetKnowledge(knowledge);
			if (this.KnowledgeWasFound(element.MyKnowledge))
			{
				if (element.IsNew)
				{
					dictionary[element.MyKnowledge.Group] = true;
				}
				if (!string.IsNullOrEmpty(searchTerm))
				{
					if (this.KnowledgeMatchesSearch(element.MyKnowledge, searchTerm))
					{
						element.gameObject.SetActive(value: true);
						continue;
					}
				}
				else if (dictionary.ContainsKey(element.MyKnowledge.Group) && dictionary[element.MyKnowledge.Group])
				{
					element.gameObject.SetActive(value: true);
					continue;
				}
			}
			element.gameObject.SetActive(value: false);
		}
		foreach (ExpandableLabel ideaLabel in this.ideaLabels)
		{
			ideaLabel.SetText(this.GetBlueprintGroupText((BlueprintGroup)ideaLabel.Tag));
			if (ideaLabel.Children.Count((GameObject x) => this.HasFoundKnowledge(x, out var knowledge2) && currentKnowledges.Contains(knowledge2)) > 0)
			{
				if (string.IsNullOrEmpty(searchTerm))
				{
					ideaLabel.gameObject.SetActive(value: true);
					ideaLabel.IsExpanded = dictionary.ContainsKey(ideaLabel.Tag) && dictionary[ideaLabel.Tag];
					continue;
				}
				if (ideaLabel.Children.Count((GameObject x) => this.HasFoundKnowledge(x, out var _) && this.KnowledgeMatchesSearch(x.GetComponent<IdeaElement>().MyKnowledge, searchTerm) && currentKnowledges.Contains(x.GetComponent<IdeaElement>().MyKnowledge)) > 0)
				{
					ideaLabel.gameObject.SetActive(value: true);
					ideaLabel.IsExpanded = true;
					continue;
				}
			}
			ideaLabel.gameObject.SetActive(value: false);
		}
		this.foundCount = this.ideaElements.Where((IdeaElement x) => this.KnowledgeWasFound(x.MyKnowledge)).Count();
		this.ideaButtons = this.IdeaElementsParent.GetComponentsInChildren<CustomButton>().ToList();
		for (int i = 0; i < this.ideaButtons.Count - 1; i++)
		{
			this.ideaButtons[i].ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
			{
				int num = this.ideaButtons.IndexOf(cb);
				if (num == 0)
				{
					nav.selectOnUp = this.IdeasButton;
				}
				else if (this.ideaButtons[num - 1].gameObject.activeInHierarchy)
				{
					nav.selectOnUp = this.ideaButtons[num - 1];
				}
				else
				{
					nav.selectOnUp = this.getFirstActiveFromIndexUp(num - 1, this.ideaButtons);
				}
				if (this.ideaButtons[num + 1].gameObject.activeInHierarchy)
				{
					nav.selectOnDown = this.ideaButtons[num + 1];
				}
				else
				{
					nav.selectOnDown = this.getFirstActiveFromIndexDown(num + 1, this.ideaButtons);
				}
				return nav;
			};
		}
		this.NoIdeasYetText.gameObject.SetActive(this.foundCount == 0);
	}

	private bool HasFoundKnowledge(GameObject obj, out IKnowledge knowledge)
	{
		knowledge = null;
		IdeaElement component = obj.GetComponent<IdeaElement>();
		if (component == null)
		{
			return false;
		}
		knowledge = component.MyKnowledge;
		return this.KnowledgeWasFound(component.MyKnowledge);
	}

	private bool KnowledgeMatchesSearch(IKnowledge knowledge, string searchTerm)
	{
		string text;
		string value;
		if (this.ShouldKeepAccents())
		{
			text = knowledge.KnowledgeName.ToLower().Replace(" ", "");
			value = searchTerm.ToLower().Replace(" ", "");
		}
		else
		{
			text = GameScreen.RemoveAccents(knowledge.KnowledgeName.ToLower().Replace(" ", ""));
			value = GameScreen.RemoveAccents(searchTerm.ToLower().Replace(" ", ""));
		}
		if (text.Contains(value))
		{
			return true;
		}
		return false;
	}

	private bool ShouldKeepAccents()
	{
		if (!(SokLoc.instance.CurrentLanguage == "Chinese (Traditional)") && !(SokLoc.instance.CurrentLanguage == "Chinese (Simplified)") && !(SokLoc.instance.CurrentLanguage == "Japanese"))
		{
			return SokLoc.instance.CurrentLanguage == "Korean";
		}
		return true;
	}

	private static string RemoveAccents(string input)
	{
		string s = input.Normalize(NormalizationForm.FormKD);
		byte[] bytes = Encoding.GetEncoding(Encoding.ASCII.CodePage, new EncoderReplacementFallback(""), new DecoderReplacementFallback("")).GetBytes(s);
		return Encoding.ASCII.GetString(bytes);
	}

	private CustomButton getFirstActiveFromIndexDown(int index, List<CustomButton> buttonList)
	{
		for (int i = index; i < buttonList.Count - 1; i++)
		{
			if (buttonList[i].gameObject.activeInHierarchy)
			{
				return buttonList[i];
			}
		}
		return null;
	}

	private CustomButton getFirstActiveFromIndexUp(int index, List<CustomButton> buttonList)
	{
		for (int num = index; num >= 0; num--)
		{
			if (buttonList[num].gameObject.activeInHierarchy)
			{
				return buttonList[num];
			}
		}
		return null;
	}

	private bool KnowledgeWasFound(IKnowledge knowledge)
	{
		return WorldManager.instance.CurrentSave.FoundCardIds.Contains(knowledge.CardId);
	}

	private string GetBlueprintGroupText(BlueprintGroup group)
	{
		return SokLoc.Translate("ideagroup_" + group.ToString().ToLower());
	}

	private bool CompletedFirstAchievement()
	{
		return QuestManager.instance.QuestIsComplete(QuestManager.instance.AllQuests[0]);
	}

	public void SetControllerInUI(bool inUI)
	{
		if (this.ControllerIsInUI != inUI)
		{
			this.ControllerIsInUI = inUI;
			if (!this.ControllerIsInUI)
			{
				EventSystem.current.SetSelectedGameObject(null);
				WorldManager.instance.SpeedUp = this.prePauseSpeed;
			}
			else
			{
				this.prePauseSpeed = WorldManager.instance.SpeedUp;
				WorldManager.instance.SpeedUp = 0f;
			}
		}
	}

	private void Update()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Happiness))
		{
			this.ShowInfoBoxHappiness.gameObject.SetActiveFast(active: true);
		}
		else
		{
			this.ShowInfoBoxHappiness.gameObject.SetActiveFast(active: false);
		}
		this.ShowInfoBoxEnergy.gameObject.SetActiveFast(active: false);
		if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			this.ViewRect.gameObject.SetActiveFast(active: true);
			this.SewageViewButton.gameObject.SetActiveFast(active: true);
			this.CalamityViewButton.gameObject.SetActiveFast(active: true);
			this.EnergyViewButton.gameObject.SetActiveFast(active: true);
			this.ShowInfoBoxMoney.gameObject.SetActiveFast(active: false);
			this.ShowInfoBoxDollar.gameObject.SetActiveFast(active: true);
			this.ShowInfoBoxWorker.gameObject.SetActiveFast(active: true);
			this.ShowInfoBoxWellbeing.gameObject.SetActiveFast(active: true);
		}
		else
		{
			if (WorldManager.instance.GetCardCount<RoadBuilder>() > 0)
			{
				this.ViewRect.gameObject.SetActiveFast(active: true);
				this.SewageViewButton.gameObject.SetActiveFast(active: false);
				this.CalamityViewButton.gameObject.SetActiveFast(active: false);
				this.EnergyViewButton.gameObject.SetActiveFast(active: false);
			}
			else
			{
				this.ViewRect.gameObject.SetActiveFast(active: false);
			}
			this.ShowInfoBoxMoney.gameObject.SetActiveFast(active: true);
			this.ShowInfoBoxDollar.gameObject.SetActiveFast(active: false);
			this.ShowInfoBoxWorker.gameObject.SetActiveFast(active: false);
			this.ShowInfoBoxWellbeing.gameObject.SetActiveFast(active: false);
		}
		this.questButtons.RemoveAll((CustomButton x) => x == null);
		this.ideaButtons.RemoveAll((CustomButton x) => x == null);
		foreach (GameObject divider in this.dividerList)
		{
			divider.SetActive(value: false);
		}
		if (InputController.instance.PanelCollapse_Triggered())
		{
			this.ToggleMinimize();
		}
		if (InputController.instance.ActivateUI_Triggered())
		{
			this.SetControllerInUI(!this.ControllerIsInUI);
		}
		if (!InputController.instance.CurrentSchemeIsController)
		{
			this.SetControllerInUI(inUI: false);
		}
		this.QuestsTabNew.gameObject.SetActiveFast(this.questElements.Any((AchievementElement x) => x.IsNew));
		this.IdeasTabNew.gameObject.SetActiveFast(this.ideaElements.Any((IdeaElement x) => x.IsNew && x.gameObject.activeSelf));
		this.FoldedNewIcon.gameObject.SetActiveFast(this.isMinimized && (this.QuestsTabNew.gameObject.activeInHierarchy || this.IdeasTabNew.gameObject.activeInHierarchy));
		this.MinimizeButtonInfoBox.InfoBoxTitle = SokLoc.Translate("label_toggle_panel_title");
		this.MinimizeButtonInfoBox.InfoBoxText = SokLoc.Translate("label_toggle_panel_text", Extensions.LocParam_Action("panel_collapse"));
		this.UpdateSidePanelPosition();
		this.MinimizeButton.transform.localScale = (this.isMinimized ? Vector3.one : new Vector3(-1f, 1f, 1f));
		this.QuestsButton.Image.color = (this.QuestsTab.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.IdeasButton.Image.color = (this.IdeasTab.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.MoneyText.text = $"{WorldManager.instance.GetGoldCount(includeInChest: true)} {Icons.Gold}";
		this.DollarText.text = $"{WorldManager.instance.GetDollarCount(includeInChest: true)}{Icons.Dollar}";
		int foodCount = WorldManager.instance.GetFoodCount();
		int requiredFoodCount = WorldManager.instance.GetRequiredFoodCount();
		int cardCount = WorldManager.instance.GetCardCount();
		int maxCardCount = WorldManager.instance.GetMaxCardCount();
		int happinessCount = WorldManager.instance.GetHappinessCount();
		int requiredHappinessCount = WorldManager.instance.GetRequiredHappinessCount();
		int wellbeing = CitiesManager.instance.Wellbeing;
		CityState cityState = CitiesManager.instance.CityState;
		string text = GameCanvas.FormatTime(WorldManager.instance.MonthTime - WorldManager.instance.MonthTimer);
		this.ShowInfoBoxTime.InfoBoxTitle = SokLoc.Translate("label_time");
		this.ShowInfoBoxTime.InfoBoxText = SokLoc.Translate("label_time_infobox", LocParam.Create("time_left", text.ToString()), Extensions.LocParam_Action("time_pause"), Extensions.LocParam_Action("time_toggle"));
		this.ShowInfoBoxEnergyButton.InfoBoxTitle = SokLoc.Translate("label_energy_view");
		this.ShowInfoBoxEnergyButton.InfoBoxText = SokLoc.Translate("label_energy_view_infobox", Extensions.LocParam_Action("toggle_view"));
		this.FoodText.text = $"{foodCount}/{requiredFoodCount} {Icons.Food}";
		this.ShowInfoBoxMoney.InfoBoxTitle = SokLoc.Translate("label_coin_infobox_title");
		this.ShowInfoBoxMoney.InfoBoxText = SokLoc.Translate("label_coin_infobox_text");
		this.ShowInfoBoxFood.InfoBoxTitle = SokLoc.Translate("cardtype_food");
		this.ShowInfoBoxFood.InfoBoxText = SokLoc.Translate("label_food_infobox", LocParam.Create("foodicon", Icons.Food), LocParam.Create("required_food_count", requiredFoodCount.ToString()), LocParam.Create("food_count", foodCount.ToString()));
		this.CardText.text = $"{cardCount}/{maxCardCount} {Icons.Card}";
		this.ShowInfoBoxCard.InfoBoxTitle = SokLoc.Translate("label_card_cap");
		this.ShowInfoBoxCard.InfoBoxText = SokLoc.Translate("label_cards_infobox", LocParam.Create("cardicon", Icons.Card), LocParam.Create("card_count", cardCount.ToString()), LocParam.Create("max_card_count", maxCardCount.ToString()));
		if (WorldManager.instance.ForestMoonEnabled)
		{
			this.FoodCardBox.SetActiveFast(active: false);
		}
		else
		{
			this.FoodCardBox.SetActiveFast(active: true);
		}
		if (foodCount < requiredFoodCount || cardCount > maxCardCount || WorldManager.instance.DebugNoFoodEnabled || WorldManager.instance.DebugNoEnergyEnabled || wellbeing < 40 || (WorldManager.instance.CurseIsActive(CurseType.Happiness) && happinessCount < requiredHappinessCount))
		{
			this.redTextBlinkTimer += Time.deltaTime;
			if (this.redTextBlinkTimer >= 0.5f)
			{
				this.redTextBlinkTimer = 0f;
				this.redBlink = !this.redBlink;
			}
		}
		else
		{
			this.redTextBlinkTimer = 0f;
			this.redBlink = false;
		}
		if (foodCount < requiredFoodCount || WorldManager.instance.DebugNoFoodEnabled)
		{
			this.FoodText.color = (this.redBlink ? ColorManager.instance.RedTextColor : ColorManager.instance.TextColor);
			ShowInfoBox showInfoBoxFood = this.ShowInfoBoxFood;
			showInfoBoxFood.InfoBoxText = showInfoBoxFood.InfoBoxText + ". " + SokLoc.Translate("label_food_infobox_warning", LocParam.Create("foodicon", Icons.Food));
		}
		else
		{
			this.FoodText.color = ColorManager.instance.TextColor;
		}
		if (cardCount > maxCardCount)
		{
			this.CardText.color = (this.redBlink ? ColorManager.instance.RedTextColor : ColorManager.instance.TextColor);
			ShowInfoBox showInfoBoxCard = this.ShowInfoBoxCard;
			showInfoBoxCard.InfoBoxText = showInfoBoxCard.InfoBoxText + ". " + SokLoc.Translate("label_cards_infobox_warning", LocParam.Create("cardicon", Icons.Card));
		}
		else
		{
			this.CardText.color = ColorManager.instance.TextColor;
		}
		if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			this.TimeText.text = SokLoc.Translate("label_timetext_peaceful", LocParam.Create("moon", WorldManager.instance.CurrentMonth.ToString()));
		}
		else if (WorldManager.instance.ForestMoonEnabled)
		{
			string text2 = SokLoc.Translate("label_timetext", LocParam.Create("moon", "??"));
			string text3 = SokLoc.Translate("label_wave", LocParam.Create("wave", WorldManager.instance.CurrentRunVariables.ForestWave.ToString()));
			this.TimeText.text = text2 + " - " + text3;
		}
		else
		{
			this.TimeText.text = SokLoc.Translate("label_timetext", LocParam.Create("moon", WorldManager.instance.CurrentMonth.ToString()));
		}
		if (this.HappinessText.isActiveAndEnabled)
		{
			this.HappinessText.text = $"{happinessCount}/{requiredHappinessCount} {Icons.Happiness}";
			this.ShowInfoBoxHappiness.InfoBoxTitle = SokLoc.Translate("cardtype_happiness");
			this.ShowInfoBoxHappiness.InfoBoxText = SokLoc.Translate("label_happiness_infobox", LocParam.Create("happinessicon", Icons.Happiness), LocParam.Create("required_happiness_count", requiredHappinessCount.ToString()), LocParam.Create("happiness_count", happinessCount.ToString()));
			if (happinessCount < requiredHappinessCount || WorldManager.instance.DebugNoFoodEnabled)
			{
				this.HappinessText.color = (this.redBlink ? ColorManager.instance.RedTextColor : ColorManager.instance.TextColor);
				ShowInfoBox showInfoBoxHappiness = this.ShowInfoBoxHappiness;
				showInfoBoxHappiness.InfoBoxText = showInfoBoxHappiness.InfoBoxText + ". " + SokLoc.Translate("label_happiness_infobox_warning", LocParam.Create("happinessicon", Icons.Happiness));
			}
			else
			{
				this.HappinessText.color = ColorManager.instance.TextColor;
			}
			this.HappinessSummaryText = this.ShowInfoBoxHappiness.InfoBoxText;
		}
		if (this.WorkerText.isActiveAndEnabled)
		{
			int num = CitiesManager.instance.HousingConsumers.Sum((HousingConsumer x) => x.GetHousingSpaceRequired());
			int num2 = WorldManager.instance.GetCards<Apartment>().Sum((Apartment x) => x.HousingSpace);
			this.WorkerText.text = $"{num}/{num2}{Icons.Housing}";
			this.ShowInfoBoxWorker.InfoBoxTitle = SokLoc.Translate("label_info_worker_space_title");
			this.ShowInfoBoxWorker.InfoBoxText = SokLoc.Translate("label_info_worker_space_text", LocParam.Create("workers", num.ToString()), LocParam.Create("space", num2.ToString()), LocParam.Create("icon", Icons.Housing));
			if (num > num2)
			{
				ShowInfoBox showInfoBoxWorker = this.ShowInfoBoxWorker;
				showInfoBoxWorker.InfoBoxText = showInfoBoxWorker.InfoBoxText + ". " + SokLoc.Translate("label_info_worker_space_text_1", LocParam.Create("icon", Icons.Housing));
				this.WorkerText.color = (this.redBlink ? ColorManager.instance.RedTextColor : ColorManager.instance.TextColor);
			}
			else
			{
				this.WorkerText.color = ColorManager.instance.TextColor;
			}
		}
		if (this.WellbeingText.isActiveAndEnabled)
		{
			this.ShowInfoBoxWellbeing.InfoBoxTitle = SokLoc.Translate("label_wellbeing");
			this.ShowInfoBoxWellbeing.InfoBoxText = SokLoc.Translate("label_wellbeing_infobox_" + cityState.ToString().ToLower());
			this.WellbeingText.text = $"{wellbeing} {Icons.Wellbeing}";
			if (CitiesManager.instance.Wellbeing < 10)
			{
				this.WellbeingText.color = (this.redBlink ? ColorManager.instance.RedTextColor : ColorManager.instance.TextColor);
			}
			else
			{
				this.WellbeingText.color = ColorManager.instance.TextColor;
			}
		}
		if (this.ShowInfoBoxDollar.gameObject.activeSelf)
		{
			this.ShowInfoBoxDollar.InfoBoxTitle = SokLoc.Translate("label_dollar");
			this.ShowInfoBoxDollar.InfoBoxText = SokLoc.Translate("label_dollar_infobox", LocParam.Create("amount", WorldManager.instance.GetDollarCount(includeInChest: true).ToString()), LocParam.Create("icon", Icons.Dollar));
		}
		this.TimeFill.fillAmount = (WorldManager.instance.ForestMoonEnabled ? 0f : (WorldManager.instance.MonthTimer / WorldManager.instance.MonthTime));
		BoosterpackData boosterpackData = QuestManager.instance.NextPackUnlock();
		this.NextPackToUnlockText.gameObject.SetActiveFast(boosterpackData != null && this.CompletedFirstAchievement());
		if (boosterpackData != null)
		{
			int pluralCount = QuestManager.instance.RemainingQuestCountToComplete(boosterpackData);
			this.NextPackToUnlockText.text = SokLoc.Translate("label_complete_more_quests", LocParam.Plural("remaining", pluralCount));
		}
		GameCard gameCard = null;
		if (WorldManager.instance.DraggingCard != null)
		{
			gameCard = WorldManager.instance.DraggingCard;
		}
		else if (WorldManager.instance.HoveredCard != null)
		{
			gameCard = WorldManager.instance.HoveredCard;
		}
		string text4 = "";
		if (gameCard == null)
		{
			Boosterpack boosterpack = null;
			if (WorldManager.instance.DraggingDraggable is Boosterpack)
			{
				boosterpack = WorldManager.instance.DraggingDraggable as Boosterpack;
			}
			else if (WorldManager.instance.HoveredDraggable is Boosterpack)
			{
				boosterpack = WorldManager.instance.HoveredDraggable as Boosterpack;
			}
			if (boosterpack != null)
			{
				this.InfoTitle.text = boosterpack.Name ?? "";
				this.InfoText.text = SokLoc.Translate("label_click_this_pack");
			}
			else
			{
				this.InfoTitle.text = "";
				this.InfoText.text = "";
			}
		}
		else if (WorldManager.instance.CurrentHoverable == null)
		{
			CardValue stackValue = WorldManager.instance.GetStackValue(gameCard);
			List<GameCard> allCardsInStack = gameCard.GetAllCardsInStack();
			if (!gameCard.IsPartOfStack())
			{
				this.InfoTitle.text = gameCard.CardData.FullName;
				string text5 = gameCard.CardData.Description;
				if (gameCard.CardData.RequirementHolders.Count > 0 && WorldManager.instance.GetCurrentBoardSafe().Id == "cities")
				{
					string requirementDescription = gameCard.CardData.GetRequirementDescription(gameCard, 1, onlyShowCurrentlySatisfied: false);
					if (!string.IsNullOrEmpty(requirementDescription))
					{
						text5 = text5 + "\\d<i>" + SokLoc.Translate("label_at_end_moon") + "</i>\n" + requirementDescription;
					}
				}
				this.InfoText.text = text5;
				GameCard cardWithStatusInStack = gameCard.GetCardWithStatusInStack();
				if (cardWithStatusInStack != null)
				{
					this.InfoTitle.text = cardWithStatusInStack.Status + "..";
					this.InfoText.text = GameCanvas.FormatTimeLeft(cardWithStatusInStack.TargetTimerTime - cardWithStatusInStack.CurrentTimerTime) + " \n\n" + gameCard.CardData.Description;
				}
				else if (gameCard.CardData.GetValue() > 0)
				{
					if (!(WorldManager.instance.NearbyCardTarget is SellBox))
					{
						if (WorldManager.instance.NearbyCardTarget is BuyBoosterBox)
						{
							_ = (BuyBoosterBox)WorldManager.instance.NearbyCardTarget;
						}
						else
						{
							text4 = stackValue.ToValueString(WorldManager.instance.CurrentBoard);
						}
					}
				}
				else if (gameCard.CardData.GetValue() == -1)
				{
					text4 = SokLoc.Translate("label_cant_be_sold");
				}
			}
			else
			{
				GameCard cardWithStatusInStack2 = gameCard.GetCardWithStatusInStack();
				GameCard cardInCombatInStack = gameCard.GetCardInCombatInStack();
				if (cardWithStatusInStack2 != null)
				{
					this.InfoTitle.text = cardWithStatusInStack2.Status + "..";
					this.InfoText.text = GameCanvas.FormatTimeLeft(cardWithStatusInStack2.TargetTimerTime - cardWithStatusInStack2.CurrentTimerTime);
				}
				else if ((bool)cardInCombatInStack)
				{
					this.InfoTitle.text = gameCard.CardData.FullName;
					this.InfoText.text = gameCard.CardData.Description;
				}
				else
				{
					this.InfoTitle.text = SokLoc.Translate("label_stack_of_cards");
					this.InfoText.text = gameCard.GetStackSummary();
					string text6 = "";
					if (allCardsInStack.Any((GameCard x) => x.CardData.RequirementHolders.Count > 0) && WorldManager.instance.GetCurrentBoardSafe().Id == "cities")
					{
						this.stackRequirements.Clear();
						this.stackRequirementAmount.Clear();
						foreach (GameCard item in allCardsInStack)
						{
							if (this.stackRequirements.ContainsKey(item.CardData.Id))
							{
								this.stackRequirementAmount[item.CardData.Id]++;
							}
							else if (item.CardData.RequirementHolders.Count > 0)
							{
								this.stackRequirements[item.CardData.Id] = item.CardData;
								this.stackRequirementAmount[item.CardData.Id] = 1;
							}
						}
						bool flag = true;
						bool flag2 = this.stackRequirements.Count > 1;
						foreach (CardData value in this.stackRequirements.Values)
						{
							int num3 = this.stackRequirementAmount[value.Id];
							string requirementDescription2 = value.GetRequirementDescription(value.MyGameCard, num3, onlyShowCurrentlySatisfied: false);
							if (!string.IsNullOrEmpty(requirementDescription2))
							{
								if (flag)
								{
									flag = false;
									text6 = text6 + "\\d<i>" + SokLoc.Translate("label_at_end_moon") + "</i>";
								}
								else
								{
									text6 += "\n";
								}
								if (flag2)
								{
									string text7 = ((num3 != 1) ? $"{num3}x {SokLoc.Translate(value.NameTerm)}" : (value.Name ?? ""));
									text6 = text6 + "\n<i>(" + text7 + ")</i>\n" + requirementDescription2;
								}
								else
								{
									text6 = text6 + "\n" + requirementDescription2;
								}
							}
						}
					}
					this.InfoText.text += text6;
					CardData cardData = null;
					foreach (GameCard item2 in allCardsInStack)
					{
						if (item2.CardData.GetValue() == -1)
						{
							cardData = item2.CardData;
						}
					}
					if (cardData != null)
					{
						text4 = SokLoc.Translate("label_cant_be_sold");
						if (WorldManager.instance.NearbyCardTarget is BuyBoosterBox)
						{
							_ = (BuyBoosterBox)WorldManager.instance.NearbyCardTarget;
						}
					}
					else if (WorldManager.instance.NearbyCardTarget is SellBox)
					{
						TextMeshProUGUI infoText = this.InfoText;
						infoText.text = infoText.text + "\n" + SokLoc.Translate("label_drop_to_sell", LocParam.Create("value", stackValue.ToValueString(WorldManager.instance.CurrentBoard)));
					}
					else
					{
						text4 = stackValue.ToValueString(WorldManager.instance.CurrentBoard);
					}
				}
			}
			if (this.InfoText.text.Contains("\\d"))
			{
				string[] array = this.InfoText.text.Split(new string[1] { "\\d" }, StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					string text8 = array[i];
					if (i == 0)
					{
						this.InfoText.text = text8;
						continue;
					}
					GameObject obj = this.FindOrInstantiateDivider();
					obj.SetActiveFast(active: true);
					TextMeshProUGUI componentInChildren = obj.GetComponentInChildren<TextMeshProUGUI>();
					if (componentInChildren != null)
					{
						componentInChildren.text = text8;
					}
				}
			}
		}
		if (this.ControllerIsInUI)
		{
			this.InfoText.text = GameScreen.InfoBoxText;
			this.InfoTitle.text = GameScreen.InfoBoxTitle;
		}
		if (WorldManager.instance.CurrentHoverable != null)
		{
			this.InfoTitle.text = (GameScreen.InfoBoxTitle = WorldManager.instance.CurrentHoverable.GetTitle());
			this.InfoText.text = (GameScreen.InfoBoxText = WorldManager.instance.CurrentHoverable.GetDescription());
			text4 = null;
		}
		if (InputController.instance.CurrentSchemeIsMouseKeyboard && InputController.instance.InputCount > 0 && (!InputController.instance.GetInput(0) || !GameCanvas.instance.PositionIsOverUI(InputController.instance.GetInputPosition(0))))
		{
			this.CloseViewDropdown();
		}
		this.ViewButton.TextMeshPro.text = this.GetLabelForViewType(WorldManager.instance.CurrentView) + this.GetIconForView(WorldManager.instance.CurrentView);
		this.Crosshair.gameObject.SetActiveFast(InputController.instance.CurrentSchemeIsController);
		this.IdeaSearchField.gameObject.SetActiveFast(this.foundCount > 0 && !InputController.instance.CurrentSchemeIsController);
		this.ValueParent.gameObject.SetActiveFast(!string.IsNullOrEmpty(text4));
		this.Valuetext.text = text4;
		bool flag3 = true;
		if (WorldManager.instance.InAnimation || GameCanvas.instance.ModalIsOpen)
		{
			flag3 = false;
		}
		if (flag3)
		{
			if (InputController.instance.TimeToggleTriggered())
			{
				if (WorldManager.instance.SpeedUp == 0f)
				{
					WorldManager.instance.SpeedUp = 1f;
				}
				else if (WorldManager.instance.SpeedUp == 1f)
				{
					WorldManager.instance.SpeedUp = 5f;
				}
				else if (WorldManager.instance.SpeedUp == 5f)
				{
					WorldManager.instance.SpeedUp = 1f;
				}
			}
			if (this.gameSpeedButtonClicked)
			{
				this.gameSpeedButtonClicked = false;
				if (WorldManager.instance.SpeedUp == 0f)
				{
					WorldManager.instance.SpeedUp = 1f;
				}
				else if (WorldManager.instance.SpeedUp == 1f)
				{
					WorldManager.instance.SpeedUp = 5f;
				}
				else if (WorldManager.instance.SpeedUp == 5f)
				{
					WorldManager.instance.SpeedUp = 0f;
				}
			}
			if (InputController.instance.Time1_Triggered())
			{
				WorldManager.instance.SpeedUp = 1f;
			}
			if (InputController.instance.Time2_Triggered())
			{
				WorldManager.instance.SpeedUp = 5f;
			}
			if (InputController.instance.Time3_Triggered())
			{
				this.prePauseSpeed = WorldManager.instance.SpeedUp;
				WorldManager.instance.SpeedUp = 0f;
			}
			if (InputController.instance.TimePauseTriggered())
			{
				this.TimePause();
			}
		}
		if (WorldManager.instance.SpeedUp == 5f)
		{
			this.GameSpeedIcon.sprite = SpriteManager.instance.Speed10;
		}
		else if (WorldManager.instance.SpeedUp == 1f)
		{
			this.GameSpeedIcon.sprite = SpriteManager.instance.Speed1;
		}
		else if (WorldManager.instance.SpeedUp == 0f)
		{
			this.GameSpeedIcon.sprite = SpriteManager.instance.Speed0;
		}
		bool flag4 = WorldManager.instance.SpeedUp == 0f;
		this.PausedText.gameObject.SetActive(flag4);
		if (flag4)
		{
			QuestManager.instance.SpecialActionComplete("pause_game");
			this.pauseBlinkTimer += Time.deltaTime;
			if (this.pauseBlinkTimer >= 0.5f)
			{
				this.PausedText.enabled = !this.PausedText.enabled || !AccessibilityScreen.FlashingPausedEnabled;
				this.pauseBlinkTimer = 0f;
			}
		}
		else
		{
			this.pauseBlinkTimer = 0f;
		}
		_ = this.previousInfoText != this.InfoText.text;
		this.previousInfoText = this.InfoText.text;
	}

	private GameObject FindOrInstantiateDivider()
	{
		foreach (GameObject divider in this.dividerList)
		{
			if (!divider.activeSelf)
			{
				return divider;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(this.InfoDividerPrefab, this.InfoLayoutGroup);
		this.dividerList.Add(gameObject);
		return gameObject;
	}

	public void UpdateSidePanelPosition()
	{
		Vector2 anchoredPosition = this.SideTransform.anchoredPosition;
		anchoredPosition.x = (this.isMinimized ? (-340f) : 5f);
		this.SideTransform.anchoredPosition = anchoredPosition;
	}

	public void TimePause()
	{
		if (WorldManager.instance.SpeedUp >= 1f)
		{
			this.prePauseSpeed = WorldManager.instance.SpeedUp;
			WorldManager.instance.SpeedUp = 0f;
		}
		else
		{
			WorldManager.instance.SpeedUp = (this.ControllerIsInUI ? this.prePauseSpeed : ((this.prePauseSpeed != 0f) ? this.prePauseSpeed : 1f));
		}
	}

	public void AddNotification(string title, string text, Action onClicked = null)
	{
		NotificationElement notificationElement = UnityEngine.Object.Instantiate(PrefabManager.instance.NotificationElementPrefab);
		notificationElement.transform.SetParent(this.NotificationsParent);
		notificationElement.transform.localScale = Vector3.one;
		notificationElement.transform.localPosition = Vector3.zero;
		notificationElement.transform.localRotation = Quaternion.identity;
		notificationElement.NotificationText.text = text;
		notificationElement.NotificationTitle.text = title;
		notificationElement.OnClicked = onClicked;
		if (this.NotificationsParent.childCount > 5)
		{
			UnityEngine.Object.Destroy(this.NotificationsParent.GetChild(0).gameObject);
		}
	}

	private void LateUpdate()
	{
		if (this.InfoTitle.text == "")
		{
			this.InfoTitle.text = GameScreen.InfoBoxTitle;
			this.InfoText.text = GameScreen.InfoBoxText;
		}
		GameScreen.InfoBoxText = "";
		GameScreen.InfoBoxTitle = "";
	}
}
