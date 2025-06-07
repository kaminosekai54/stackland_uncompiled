using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodWarehouse : Food
{
	private int MaxFoodValue = 999;

	[ExtraData("resource_id")]
	[HideInInspector]
	public string HeldCardId = "";

	private CardConnector outputConnector;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Hotpot)
		{
			return false;
		}
		if (otherCard is Food { FoodValue: >0 })
		{
			if (string.IsNullOrEmpty(this.HeldCardId))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(this.HeldCardId) && otherCard.Id == this.HeldCardId)
			{
				return true;
			}
			return false;
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
		if ((!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation) && base.MyGameCard.HasChild && !(base.MyGameCard.Child.CardData is FoodWarehouse) && (string.IsNullOrEmpty(this.HeldCardId) || (!string.IsNullOrEmpty(this.HeldCardId) && base.MyGameCard.Child.CardData.Id == this.HeldCardId)))
		{
			this.StoreFood();
		}
		if (!string.IsNullOrEmpty(this.HeldCardId))
		{
			Food food = WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId) as Food;
			base.nameOverride = SokLoc.Translate("card_food_warehouse_name_long", LocParam.Create("food", WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId).Name));
			base.descriptionOverride = SokLoc.Translate("card_food_warehouse_description_long", LocParam.Create("food", WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId).Name), LocParam.Create("amount", (base.FoodValue / food.FoodValue).ToString()));
		}
		else
		{
			base.nameOverride = SokLoc.Translate("card_food_warehouse_name");
			base.descriptionOverride = null;
		}
		if (this.outputConnector == null)
		{
			this.outputConnector = this.GetOutputConnector();
		}
		if (base.FoodValue > 0 && this.outputConnector?.ConnectedNode != null)
		{
			base.MyGameCard.StartTimer(10f, OutputCard, SokLoc.Translate("idea_resourcechest_status_2"), base.GetActionId("OutputCard"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("OutputCard"));
		}
		base.UpdateCard();
		if (string.IsNullOrEmpty(this.HeldCardId))
		{
			base.Icon = SpriteManager.instance.EmptyTexture;
		}
		else
		{
			base.Icon = WorldManager.instance.GetCardPrefab(this.HeldCardId).Icon;
		}
		base.MyGameCard.UpdateIcon();
	}

	public CardConnector GetOutputConnector()
	{
		CardConnector result = null;
		for (int i = 0; i < base.MyGameCard.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = base.MyGameCard.CardConnectorChildren[i];
			if (cardConnector != null && cardConnector.ConnectionType == ConnectionType.Transport && cardConnector.CardDirection == CardDirection.output)
			{
				result = cardConnector;
			}
		}
		return result;
	}

	public void StoreFood()
	{
		foreach (GameCard childCard in base.MyGameCard.GetChildCards())
		{
			if (string.IsNullOrEmpty(this.HeldCardId))
			{
				this.HeldCardId = childCard.CardData.Id;
			}
			if (childCard.CardData.Id != this.HeldCardId || childCard.CardData is Hotpot || childCard.CardData is FoodWarehouse)
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
				else
				{
					childCard.RemoveFromParent();
				}
			}
		}
	}

	public GameCard RemoveFood(int count, bool checkOutput = false)
	{
		Food food = WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId) as Food;
		List<GameCard> list = new List<GameCard>();
		for (int i = 0; i < count; i++)
		{
			CardData cardData;
			if (base.FoodValue >= food.FoodValue)
			{
				cardData = WorldManager.instance.CreateCard(base.transform.position, this.HeldCardId, faceUp: true, checkAddToStack: false);
				base.FoodValue -= food.FoodValue;
			}
			else
			{
				int num = Mathf.Min(base.FoodValue, food.FoodValue);
				cardData = WorldManager.instance.CreateCard(base.transform.position, this.HeldCardId, faceUp: true, checkAddToStack: false);
				if (cardData is Food food2)
				{
					food2.FoodValue = num;
				}
				base.FoodValue -= num;
			}
			if (cardData != null)
			{
				list.Add(cardData.MyGameCard);
			}
			if (base.FoodValue <= 0)
			{
				base.FoodValue = 0;
				break;
			}
		}
		WorldManager.instance.Restack(list);
		if (checkOutput)
		{
			WorldManager.instance.StackSendCheckTarget(base.MyGameCard, list[0], base.OutputDir);
		}
		else
		{
			WorldManager.instance.StackSend(list[0], base.OutputDir);
		}
		return list[0].GetRootCard();
	}

	[TimedAction("output_card")]
	public void OutputCard()
	{
		if (base.FoodValue > 0)
		{
			this.RemoveFood(1, checkOutput: true);
		}
	}

	public override void Clicked()
	{
		int count = 1;
		if (InputController.instance.GetKey(Key.LeftShift) || InputController.instance.GetKey(Key.RightShift))
		{
			count = 5;
		}
		if (base.FoodValue > 0)
		{
			this.RemoveFood(count);
		}
		if (base.FoodValue == 0)
		{
			this.HeldCardId = null;
		}
		base.Clicked();
	}
}
