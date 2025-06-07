using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffect
{
	private CardData parentCard;

	public string ParentCardId;

	public float? FillAmount;

	public string NameTerm;

	public string DescriptionTerm;

	public string LoreTerm;

	public string ColorAName;

	public string ColorBName;

	private Color colorA;

	private Color colorB;

	[ExtraData("status_timer")]
	public float StatusTimer;

	private static List<Type> statusEffectTypes;

	public string Name => SokLoc.Translate(this.NameTerm);

	public virtual string Description => SokLoc.Translate(this.DescriptionTerm);

	public string Lore => SokLoc.Translate(this.LoreTerm);

	public virtual Color ColorA
	{
		get
		{
			if (WorldManager.instance.CurrentView != ViewType.Default && this.FadeInNonDefaultView)
			{
				return ColorManager.instance.GetColorWithName("statuseffect_energy_view_a");
			}
			return this.colorA;
		}
	}

	public virtual Color ColorB
	{
		get
		{
			if (WorldManager.instance.CurrentView != ViewType.Default && this.FadeInNonDefaultView)
			{
				return ColorManager.instance.GetColorWithName("statuseffect_energy_view_b");
			}
			return this.colorB;
		}
	}

	public virtual int? StatusNumber => null;

	public virtual Color? StatusNumberColor => null;

	protected virtual string TermId => "";

	public virtual bool FadeInNonDefaultView => true;

	public CardData ParentCard
	{
		get
		{
			if (this.parentCard == null && !string.IsNullOrEmpty(this.ParentCardId))
			{
				GameCard cardWithUniqueId = WorldManager.instance.GetCardWithUniqueId(this.ParentCardId);
				if (cardWithUniqueId != null)
				{
					this.parentCard = cardWithUniqueId.CardData;
				}
			}
			return this.parentCard;
		}
		set
		{
			this.parentCard = value;
			this.ParentCardId = value.Id;
		}
	}

	public virtual Sprite Sprite { get; }

	public StatusEffect()
	{
		this.NameTerm = "statuseffect_" + this.TermId + "_name";
		this.DescriptionTerm = "statuseffect_" + this.TermId + "_description";
		this.LoreTerm = "statuseffect_" + this.TermId + "_lore";
		this.ColorAName = "statuseffect_" + this.TermId + "_a";
		this.ColorBName = "statuseffect_" + this.TermId + "_b";
		this.colorA = ColorManager.instance.GetColorWithName(this.ColorAName);
		this.colorB = ColorManager.instance.GetColorWithName(this.ColorBName);
	}

	public virtual void Update()
	{
		this.StatusTimer += Time.deltaTime * WorldManager.instance.TimeScale;
	}

	public SavedStatusEffect ToSavedStatusEffect()
	{
		return new SavedStatusEffect
		{
			StatusEffectId = base.GetType().Name,
			ExtraDatas = CardData.GetExtraCardData(this)
		};
	}

	public static StatusEffect FromSavedStatusEffect(SavedStatusEffect savedEff)
	{
		StatusEffect.InitStatusEffectTypes();
		StatusEffect statusEffect = StatusEffect.CreateStatusEffectFromName(savedEff.StatusEffectId);
		if (statusEffect == null)
		{
			return null;
		}
		CardData.SetExtraCardData(statusEffect, savedEff.ExtraDatas);
		return statusEffect;
	}

	private static void InitStatusEffectTypes()
	{
		if (StatusEffect.statusEffectTypes == null)
		{
			StatusEffect.statusEffectTypes = (from type in typeof(StatusEffect).Assembly.GetTypes()
				where type.IsSubclassOf(typeof(StatusEffect))
				select type).ToList();
		}
	}

	public static bool StatusEffectExists(string name)
	{
		StatusEffect.InitStatusEffectTypes();
		return StatusEffect.statusEffectTypes.Any((Type x) => x.Name == name);
	}

	public static StatusEffect CreateStatusEffectFromName(string s)
	{
		StatusEffect.InitStatusEffectTypes();
		Type type = StatusEffect.statusEffectTypes.FirstOrDefault((Type x) => x.Name == s);
		if (type == null)
		{
			return null;
		}
		return Activator.CreateInstance(type) as StatusEffect;
	}
}
