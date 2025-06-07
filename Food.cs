using UnityEngine;

public class Food : CardData
{
	[Header("Food")]
	[ExtraData("food_value")]
	public int FoodValue = 1;

	public bool CanBePlacedOnVillager;

	[ExtraData("spoil_time")]
	[HideInInspector]
	public float SpoilTime;

	public bool CanSpoil = true;

	[Header("Special Actions")]
	public string ResultAction;

	public string FullyConsumeResultAction;

	[HideInInspector]
	public bool IsReserved;

	[HideInInspector]
	public bool IsConsumed;

	public bool IsSpoiling => base.HasStatusEffectOfType<StatusEffect_Spoiling>();

	public override void UpdateCard()
	{
		if (this.FoodValue > 0)
		{
			base.MyGameCard.SpecialValue = this.FoodValue;
			base.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.FoodIcon;
		}
		if (this.FoodValue <= 0)
		{
			this.FoodValue = 0;
		}
		if (!base.MyGameCard.IsDemoCard && base.MyGameCard.MyBoard.BoardOptions.FoodSpoils && !this.IsSpoiling && this.CanSpoil)
		{
			this.SpoilTime += Time.deltaTime * WorldManager.instance.TimeScale;
			float num = WorldManager.instance.MonthTime;
			if (base.IsCookedFood)
			{
				num = WorldManager.instance.MonthTime * 2f;
			}
			if (this.SpoilTime >= num)
			{
				base.AddStatusEffect(new StatusEffect_Spoiling());
			}
		}
		base.UpdateCard();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is Food) && otherCard.MyCardType != CardType.Resources)
		{
			if (otherCard is BaseVillager || otherCard is Worker)
			{
				return this.CanBePlacedOnVillager;
			}
			return false;
		}
		return true;
	}

	public virtual void ConsumedBy(Combatable vill)
	{
		vill.ParseAction(this.ResultAction);
	}

	private bool CanGiveFoodPoisoning()
	{
		return WorldManager.instance.CurseIsActive(CurseType.Death);
	}

	public virtual void FullyConsumed(CardData c)
	{
		c.ParseAction(this.FullyConsumeResultAction);
	}

	public override void OnSellCard()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Happiness) && this.FoodValue > 0)
		{
			WorldManager.instance.TryCreateUnhappiness(base.transform.position, 1);
			WorldManager.instance.QueueCutsceneIfNotPlayed("happiness_food");
		}
		base.OnSellCard();
	}
}
