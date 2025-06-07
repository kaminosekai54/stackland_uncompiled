using System.Collections.Generic;
using UnityEngine;

public class University : CardData
{
	public int InventionCost = 50;

	[Card]
	public List<string> BlueprintDrops = new List<string>();

	public List<AudioClip> InventionSound;

	public Sprite SpecialIcon;

	[ExtraData("coin_count")]
	[HideInInspector]
	public int CoinCount;

	private string HeldCardId = "gold";

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!this.AllInventionsFound())
		{
			if (!(otherCard.Id == this.HeldCardId))
			{
				if (otherCard is Chest chest)
				{
					return chest.HeldCardId == this.HeldCardId;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override void UpdateCardText()
	{
		if (this.AllInventionsFound())
		{
			base.descriptionOverride = SokLoc.Translate("card_university_description_completed");
		}
		else if (this.CoinCount > 0)
		{
			base.descriptionOverride = SokLoc.Translate("card_university_description_long", LocParam.Create("count", this.CoinCount.ToString()), LocParam.Create("max_count", this.InventionCost.ToString()));
		}
		else
		{
			base.descriptionOverride = SokLoc.Translate("card_university_description", LocParam.Create("max_count", this.InventionCost.ToString()));
		}
	}

	public override void UpdateCard()
	{
		if (!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard childCard in base.MyGameCard.GetChildCards())
			{
				if (childCard.CardData is Chest chest)
				{
					if (chest.CoinCount < this.InventionCost - this.CoinCount)
					{
						this.CoinCount += chest.CoinCount;
						chest.CoinCount = 0;
						WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
					else if (chest.CoinCount >= this.InventionCost - this.CoinCount)
					{
						chest.CoinCount -= this.InventionCost - this.CoinCount;
						this.CoinCount = this.InventionCost;
						WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
				}
				if (!(childCard.CardData.Id != this.HeldCardId))
				{
					if (this.CoinCount >= this.InventionCost)
					{
						childCard.RemoveFromParent();
						break;
					}
					childCard.DestroyCard(spawnSmoke: true);
					this.CoinCount++;
				}
			}
			if (this.CoinCount == this.InventionCost)
			{
				base.MyGameCard.StartTimer(10f, GiveInvention, SokLoc.Translate("card_university_status"), base.GetActionId("GiveInvention"));
			}
		}
		if (this.AllInventionsFound())
		{
			base.MyGameCard.CancelTimer(base.GetActionId("GiveInvention"));
		}
		base.UpdateCard();
	}

	private bool AllInventionsFound()
	{
		bool result = true;
		foreach (string blueprintDrop in this.BlueprintDrops)
		{
			if (!WorldManager.instance.HasFoundCard(blueprintDrop))
			{
				result = false;
				break;
			}
		}
		if (WorldManager.instance.IsCitiesDlcActive() && !WorldManager.instance.HasFoundCard("industrial_revolution"))
		{
			return false;
		}
		return result;
	}

	[TimedAction("give_invention")]
	public void GiveInvention()
	{
		if (WorldManager.instance.IsCitiesDlcActive() && !WorldManager.instance.HasFoundCard("industrial_revolution"))
		{
			CardData cardData = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, "industrial_revolution", faceUp: true, checkAddToStack: false);
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
			AudioManager.me.PlaySound2D(this.InventionSound, 1f, 0.1f);
			this.CoinCount = 0;
			return;
		}
		foreach (string blueprintDrop in this.BlueprintDrops)
		{
			Blueprint blueprint = WorldManager.instance.GameDataLoader.GetCardFromId(blueprintDrop) as Blueprint;
			if ((bool)blueprint && !WorldManager.instance.HasFoundCard(blueprint.Id))
			{
				CardData cardData2 = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, blueprint, faceUp: true, checkAddToStack: false);
				WorldManager.instance.CreateSmoke(cardData2.transform.position);
				cardData2.MyGameCard.SendIt();
				AudioManager.me.PlaySound2D(this.InventionSound, 1f, 0.1f);
				this.CoinCount = 0;
				break;
			}
		}
	}
}
