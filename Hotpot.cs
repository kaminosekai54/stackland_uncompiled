using UnityEngine;

public class Hotpot : Food
{
	private int MaxFoodValue = 50;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Food { FoodValue: 0 })
		{
			return false;
		}
		if (otherCard.MyCardType == CardType.Food)
		{
			return true;
		}
		return false;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		base.MyGameCard.SpecialValue = base.FoodValue;
		base.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.FoodIcon;
		if (!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			if (base.MyGameCard.HasChild && !base.MyGameCard.TimerRunning && !(base.MyGameCard.Child.CardData is Hotpot))
			{
				base.MyGameCard.StartTimer(10f, CookFood, SokLoc.Translate("card_hotpot_name"), base.GetActionId("CookFood"));
			}
			if (!base.MyGameCard.HasChild && base.MyGameCard.TimerRunning)
			{
				base.MyGameCard.CancelTimer(base.GetActionId("CookFood"));
			}
		}
		GameCard rootCard = base.MyGameCard.GetRootCard();
		if (rootCard != null && rootCard.CardData is MessHall)
		{
			base.MyGameCard.CancelTimer(base.GetActionId("CookFood"));
		}
		if (base.FoodValue > 0)
		{
			base.descriptionOverride = "";
		}
		base.UpdateCard();
	}

	[TimedAction("cook_food")]
	public void CookFood()
	{
		foreach (GameCard childCard in base.MyGameCard.GetChildCards())
		{
			if (childCard.CardData is Hotpot)
			{
				continue;
			}
			if (childCard.SpecialValue.HasValue && base.FoodValue + childCard.SpecialValue <= this.MaxFoodValue)
			{
				base.FoodValue += childCard.SpecialValue.Value;
				childCard.DestroyCard(spawnSmoke: true);
			}
			else if (childCard.CardData is Food food)
			{
				int num = Mathf.Min(this.MaxFoodValue - base.FoodValue, food.FoodValue);
				base.FoodValue += num;
				food.FoodValue -= num;
				if (food.FoodValue <= 0)
				{
					childCard.DestroyCard(spawnSmoke: true);
				}
			}
		}
	}
}
