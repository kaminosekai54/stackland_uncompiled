using System.Linq;
using UnityEngine;

public class BaseVillager : Combatable
{
	public bool CanOverrideCardFromEquipment;

	[ExtraData("age")]
	public int Age;

	public bool ChangesCardOnStage;

	[HideInInspector]
	public bool AteUncookedFood;

	public bool CanBreed = true;

	public LifeStage MyLifeStage
	{
		get
		{
			if (!WorldManager.instance.CurseIsActive(CurseType.Death))
			{
				return LifeStage.Adult;
			}
			return this.DetermineLifeStageFromAge(this.Age);
		}
	}

	public override bool HasInventory
	{
		get
		{
			if (base.Id == "trained_monkey")
			{
				return false;
			}
			return true;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is BaseVillager) && otherCard.MyCardType != CardType.Resources && otherCard.MyCardType != CardType.Equipable && !(otherCard is Food { CanBePlacedOnVillager: not false }))
		{
			return otherCard.Id == "naming_stone";
		}
		return true;
	}

	public override void UpdateCardText()
	{
		base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm);
		base.descriptionOverride += "\n\n";
		if (WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			base.descriptionOverride = base.descriptionOverride + "<i>" + SokLoc.Translate("label_villager_age_description", LocParam.Plural("age", this.Age + 1)) + "<i>\n";
		}
		base.descriptionOverride = base.descriptionOverride + "<i>" + base.GetCombatableDescription() + "</i>";
		if (AdvancedSettingsScreen.AdvancedCombatStatsEnabled || GameCanvas.instance.CurrentScreen is CardopediaScreen)
		{
			base.descriptionOverride = base.descriptionOverride + "\n\n<i>" + base.GetCombatableDescriptionAdvanced() + "</i>";
		}
		bool flag = !this.ChangesCardOnStage || !string.IsNullOrEmpty(base.CustomName);
		string termId = base.NameTerm;
		if (flag)
		{
			if (this.MyLifeStage == LifeStage.Adult)
			{
				termId = base.NameTerm;
			}
			else if (this.MyLifeStage == LifeStage.Teenager)
			{
				termId = base.NameTerm + "_young";
			}
			else if (this.MyLifeStage == LifeStage.Elderly)
			{
				termId = base.NameTerm + "_old";
			}
			else if (this.MyLifeStage == LifeStage.Dead)
			{
				termId = base.NameTerm + "_old";
			}
		}
		base.nameOverride = SokLoc.Translate(termId);
		if (string.IsNullOrEmpty(base.CustomName))
		{
			return;
		}
		if (flag)
		{
			if (this.MyLifeStage == LifeStage.Adult)
			{
				base.nameOverride = base.CustomName;
			}
			else if (this.MyLifeStage == LifeStage.Teenager)
			{
				base.nameOverride = SokLoc.Translate("label_villager_young", LocParam.Create("villager", base.CustomName));
			}
			else if (this.MyLifeStage == LifeStage.Elderly)
			{
				base.nameOverride = SokLoc.Translate("label_villager_old", LocParam.Create("villager", base.CustomName));
			}
			else if (this.MyLifeStage == LifeStage.Dead)
			{
				base.nameOverride = SokLoc.Translate("label_villager_old", LocParam.Create("villager", base.CustomName));
			}
		}
		else
		{
			base.nameOverride = base.CustomName;
		}
	}

	public override void UpdateCard()
	{
		if (WorldManager.instance.TimeScale > 0f && !WorldManager.instance.InAnimation)
		{
			this.UpdateLifeStage();
		}
		base.UpdateCard();
	}

	public virtual int GetRequiredFoodCount()
	{
		if (base.Id == "trained_monkey")
		{
			return 0;
		}
		if (base.Id == "dog")
		{
			return 1;
		}
		return 2;
	}

	public override void Die()
	{
		WorldManager.instance.KillVillager(this);
		if (WorldManager.instance.GetCardCount<BaseVillager>() == 2 && this.MyLifeStage == LifeStage.Elderly)
		{
			WorldManager.instance.QueueCutsceneIfNotPlayed("death_middle_villager");
		}
		if (base.MyConflict != null)
		{
			base.MyConflict.LeaveConflict(this);
		}
	}

	public float GetActionTimeModifier(string actionId, CardData baseCard)
	{
		float num = 1f;
		ActionTimeParams parameters = new ActionTimeParams(this, actionId, baseCard);
		foreach (ActionTimeBase actionTimeBasis in WorldManager.instance.actionTimeBases)
		{
			if (actionTimeBasis.Matches(parameters))
			{
				num = actionTimeBasis.BaseSpeed;
			}
		}
		foreach (ActionTimeModifier actionTimeModifier in WorldManager.instance.actionTimeModifiers)
		{
			if (actionTimeModifier.Matches(parameters))
			{
				num *= actionTimeModifier.SpeedModifier;
			}
		}
		return num;
	}

	public override void OnEquipItem(Equipable equipable)
	{
		if (equipable.Id == "royal_crown")
		{
			WorldManager.instance.QueueCutscene(GreedCutscenes.GreedWearCrown());
			base.MyGameCard.Unequip(equipable);
			return;
		}
		if (this.CanOverrideCardFromEquipment && !string.IsNullOrEmpty(equipable.VillagerTypeOverride) && equipable.VillagerTypeOverride != base.Id)
		{
			WorldManager.instance.ChangeToCard(base.MyGameCard, equipable.VillagerTypeOverride);
		}
		base.OnEquipItem(equipable);
	}

	private Equipable GetOverrideEquipable()
	{
		if (!this.CanOverrideCardFromEquipment)
		{
			return null;
		}
		return base.GetAllEquipables().FirstOrDefault((Equipable x) => !string.IsNullOrEmpty(x.VillagerTypeOverride));
	}

	public override void OnUnequipItem(Equipable equipable)
	{
		if (this.CanOverrideCardFromEquipment)
		{
			if (this.GetOverrideEquipable() == null && base.Id != "villager")
			{
				(WorldManager.instance.ChangeToCard(base.MyGameCard, "villager") as Villager).UpdateLifeStage();
			}
			else if (this.GetOverrideEquipable() != null && this.GetOverrideEquipable().VillagerTypeOverride != base.Id)
			{
				(WorldManager.instance.ChangeToCard(base.MyGameCard, this.GetOverrideEquipable().VillagerTypeOverride) as BaseVillager).UpdateLifeStage();
			}
		}
		base.OnUnequipItem(equipable);
	}

	public void UpdateLifeStage()
	{
		if (this.ChangesCardOnStage)
		{
			string text = this.DetermineCardFromStage(this.MyLifeStage);
			if (text != null && text != base.Id)
			{
				WorldManager.instance.ChangeToCard(base.MyGameCard, text);
			}
		}
	}

	public string DetermineCardFromStage(LifeStage stage)
	{
		if (base.Id == "teenage_villager" || base.Id == "villager" || base.Id == "old_villager")
		{
			switch (stage)
			{
			case LifeStage.Teenager:
				return "teenage_villager";
			case LifeStage.Adult:
				return "villager";
			case LifeStage.Elderly:
				QuestManager.instance.SpecialActionComplete("villager_old");
				return "old_villager";
			}
		}
		if (base.Id == "puppy" || base.Id == "dog" || base.Id == "old_dog")
		{
			switch (stage)
			{
			case LifeStage.Teenager:
				return "puppy";
			case LifeStage.Adult:
				return "dog";
			case LifeStage.Elderly:
				return "old_dog";
			}
		}
		if (base.Id == "kitten" || base.Id == "cat" || base.Id == "old_cat")
		{
			switch (stage)
			{
			case LifeStage.Teenager:
				return "kitten";
			case LifeStage.Adult:
				return "cat";
			case LifeStage.Elderly:
				return "old_cat";
			}
		}
		return null;
	}

	public LifeStage DetermineLifeStageFromAge(int age)
	{
		if (age < 2)
		{
			return LifeStage.Teenager;
		}
		if (age <= 6)
		{
			return LifeStage.Adult;
		}
		if (age <= 8)
		{
			return LifeStage.Elderly;
		}
		return LifeStage.Dead;
	}

	public bool WillChangeLifeStage()
	{
		return this.DetermineLifeStageFromAge(this.Age) != this.DetermineLifeStageFromAge(this.Age + 1);
	}
}
