using System;

public class Quest
{
	private Location? _questLocation;

	public string DescriptionTermOverride;

	public string Id;

	public int RequiredCount = -1;

	public bool DefaultVisible;

	public bool ShowCompleteAnimation = true;

	public Func<CardData, string, bool> OnActionComplete;

	public Func<CardData, bool> OnCardCreate;

	public Func<string, bool> OnSpecialAction;

	public bool IsSteamAchievement;

	public bool PossibleInPeacefulMode = true;

	public QuestGroup QuestGroup = QuestGroup.Other;

	public string DescriptionTerm => "quest_" + this.Id + "_text";

	public string Description
	{
		get
		{
			if (this.DescriptionTermOverride != null)
			{
				if (this.RequiredCount != -1)
				{
					return SokLoc.Translate(this.DescriptionTermOverride, LocParam.Create("count", this.RequiredCount.ToString()));
				}
				return SokLoc.Translate(this.DescriptionTermOverride);
			}
			return SokLoc.Translate(this.DescriptionTerm);
		}
	}

	public Location QuestLocation
	{
		get
		{
			if (!this._questLocation.HasValue)
			{
				if (this.QuestGroup.ToString().StartsWith("Island"))
				{
					this._questLocation = Location.Island;
				}
				else if (this.QuestGroup.ToString().StartsWith("Forest"))
				{
					this._questLocation = Location.Forest;
				}
				else if (this.QuestGroup.ToString().StartsWith("Death"))
				{
					this._questLocation = Location.Death;
				}
				else if (this.QuestGroup.ToString().StartsWith("Greed"))
				{
					this._questLocation = Location.Greed;
				}
				else if (this.QuestGroup.ToString().StartsWith("Happiness"))
				{
					this._questLocation = Location.Happiness;
				}
				else if (this.QuestGroup.ToString().StartsWith("Cities"))
				{
					this._questLocation = Location.Cities;
				}
				else
				{
					this._questLocation = Location.Mainland;
				}
			}
			return this._questLocation.Value;
		}
	}

	public bool IsMainQuest
	{
		get
		{
			if (this.QuestGroup != 0 && this.QuestGroup != QuestGroup.MainQuest && this.QuestGroup != QuestGroup.Island_Beginnings && this.QuestGroup != QuestGroup.Island_MainQuest && this.QuestGroup != QuestGroup.Forest_MainQuest && this.QuestGroup != QuestGroup.Death_MainQuest && this.QuestGroup != QuestGroup.Happiness_MainQuest && this.QuestGroup != QuestGroup.Greed_MainQuest && this.QuestGroup != QuestGroup.Happiness_Starter && this.QuestGroup != QuestGroup.Death_Starter && this.QuestGroup != QuestGroup.Death_MainQuest && this.QuestGroup != QuestGroup.Discover_Spirits && this.QuestGroup != QuestGroup.Greed_Starter && this.QuestGroup != QuestGroup.Cities_MainQuest)
			{
				return this.QuestGroup == QuestGroup.Cities_Starter;
			}
			return true;
		}
	}

	public Quest(string id)
	{
		this.Id = id;
	}
}
