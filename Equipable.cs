using System.Collections.Generic;
using UnityEngine;

public class Equipable : CardData
{
	public string VillagerTypeOverride;

	[Header("Equipment")]
	public EquipableType EquipableType;

	public List<AudioClip> AttackSounds;

	public AttackType AttackType;

	public Blueprint blueprint;

	public CombatStats MyStats;

	[ExtraData("level")]
	public int Level;

	private string _equipableInfo;

	public override bool CanBeDragged
	{
		get
		{
			if (base.MyGameCard.EquipmentHolder != null && base.MyGameCard.EquipmentHolder.Combatable.Team != 0)
			{
				return false;
			}
			return true;
		}
	}

	public virtual void Process(CombatStats stats)
	{
	}

	public override void OnLanguageChange()
	{
		this._equipableInfo = null;
		base.OnLanguageChange();
	}

	public override void StoppedDragging()
	{
		List<CardData> list = base.CardsInStackMatchingPredicate((CardData x) => x is Equipable);
		GameCard parent = base.MyGameCard.Parent;
		foreach (Equipable item in list)
		{
			item.TryEquipOnCard(parent);
		}
		base.StoppedDragging();
	}

	private void TryEquipOnCard(GameCard card)
	{
		if (base.MyGameCard.EquipmentHolder != null)
		{
			base.MyGameCard.EquipmentHolder.Unequip(this);
		}
		if (card != null && card.CardData.HasInventory)
		{
			card.OpenInventory(showInventory: true);
			card.CardData.EquipItem(this);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyCardType != CardType.Equipable && otherCard.MyCardType != CardType.Humans)
		{
			return otherCard.MyCardType == CardType.Resources;
		}
		return true;
	}

	private string GetAdvancedEquipableInfo()
	{
		string text = SokLoc.Translate("label_combat_speed");
		string text2 = SokLoc.Translate("label_hit_chance");
		string text3 = SokLoc.Translate("label_damage");
		string text4 = SokLoc.Translate("label_defence");
		string text5 = SokLoc.Translate("label_health");
		string text6 = "";
		if (this.MyStats.MaxHealth != 0)
		{
			text6 = text6 + text5 + ": " + this.NumberToStringWithPlus(this.MyStats.MaxHealth) + "\n";
		}
		if (this.MyStats.AttackSpeedIncrement != 0)
		{
			text6 = text6 + text + " " + this.NumberToStringWithPlus(this.MyStats.AttackSpeedIncrement) + "\n";
		}
		if (this.MyStats.HitChanceIncrement != 0)
		{
			text6 = text6 + text2 + " " + this.NumberToStringWithPlus(this.MyStats.HitChanceIncrement) + "\n";
		}
		if (this.MyStats.AttackDamageIncrement != 0)
		{
			text6 = text6 + text3 + " " + this.NumberToStringWithPlus(this.MyStats.AttackDamageIncrement) + "\n";
		}
		if (this.MyStats.DefenceIncrement != 0)
		{
			text6 = text6 + text4 + " " + this.NumberToStringWithPlus(this.MyStats.DefenceIncrement) + "\n";
		}
		return text6;
	}

	private string NumberToStringWithPlus(int n)
	{
		if (n > 0)
		{
			return $"+{n}";
		}
		if (n < 0)
		{
			return $"{n}";
		}
		return "0";
	}

	public string GetEquipableInfo()
	{
		if (!string.IsNullOrEmpty(this._equipableInfo))
		{
			return this._equipableInfo;
		}
		string text = "";
		text += SokLoc.Translate("label_itemlevel", LocParam.Create("level", Mathf.RoundToInt(this.MyStats.ItemLevel).ToString()));
		text += "\\d<size=90%>";
		string text2 = this.MyStats.SummarizeSpecialHits();
		if (text2.Length > 0)
		{
			text = text + text2 + "\n\n";
		}
		text += this.GetEquipableInfoAdvanced();
		this._equipableInfo = text;
		return this._equipableInfo;
	}

	public string GetEquipableInfoAdvanced()
	{
		return this.GetAdvancedEquipableInfo() ?? "";
	}

	public string GetEquipableCombatLevel()
	{
		return "" + SokLoc.Translate("label_itemlevel", LocParam.Create("level", Mathf.RoundToInt(this.MyStats.ItemLevel).ToString()));
	}

	public override void UpdateCard()
	{
		if (this.Level > 0)
		{
			base.nameOverride = $"{SokLoc.Translate(base.NameTerm)}  (+{this.Level})";
		}
		base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm) + "\n\n<i>" + this.GetEquipableInfo() + "</i>";
		base.MyGameCard.SpecialIcon.sprite = this.GetIconForEquipableType(this.EquipableType);
		base.MyGameCard.ShowSpecialIcon = true;
		base.UpdateCard();
	}

	private Sprite GetIconForEquipableType(EquipableType type)
	{
		return type switch
		{
			EquipableType.Head => SpriteManager.instance.HeadIconFilled, 
			EquipableType.Weapon => SpriteManager.instance.HandIconFilled, 
			EquipableType.Torso => SpriteManager.instance.TorsoIconFilled, 
			_ => null, 
		};
	}
}
