using System.Collections.Generic;
using UnityEngine;

public class WishingWell : CardData
{
	public int WishCost = 500;

	public List<AudioClip> WishSound;

	public Sprite SpecialIcon;

	[ExtraData("coin_count")]
	[HideInInspector]
	public int CoinCount;

	[ExtraData("wish_count")]
	[HideInInspector]
	public int WishCount;

	private string HeldCardId = "gold";

	protected override bool CanHaveCard(CardData otherCard)
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

	public override void UpdateCardText()
	{
		if (this.WishCount > 0)
		{
			base.descriptionOverride = SokLoc.Translate("card_wishing_well_description_long", LocParam.Plural("amount", this.WishCount), LocParam.Create("count", this.WishCost.ToString()));
		}
		else
		{
			base.descriptionOverride = SokLoc.Translate("card_wishing_well_description", LocParam.Create("count", this.WishCost.ToString()));
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
					if (chest.CoinCount < this.WishCost - this.CoinCount)
					{
						this.CoinCount += chest.CoinCount;
						chest.CoinCount = 0;
						WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
					else if (chest.CoinCount >= this.WishCost - this.CoinCount)
					{
						chest.CoinCount -= this.WishCost - this.CoinCount;
						this.CoinCount = this.WishCost;
						WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
				}
				if (!(childCard.CardData.Id != this.HeldCardId))
				{
					if (this.CoinCount >= this.WishCost)
					{
						childCard.RemoveFromParent();
						break;
					}
					childCard.DestroyCard(spawnSmoke: true);
					this.CoinCount++;
				}
			}
			if (this.CoinCount == this.WishCost)
			{
				this.GiveWish();
			}
		}
		base.UpdateCard();
	}

	private void GiveWish()
	{
		AudioManager.me.PlaySound2D(this.WishSound, 1f, 0.1f);
		WorldManager.instance.CreateSmoke(base.transform.position);
		this.CoinCount = 0;
		this.WishCount++;
		switch (this.WishCount)
		{
		case 1:
			WorldManager.instance.QueueCutscene(Cutscenes.Wish1(this));
			break;
		case 2:
			WorldManager.instance.QueueCutscene(Cutscenes.Wish2(this));
			break;
		case 5:
			WorldManager.instance.QueueCutscene(Cutscenes.Wish5(this));
			break;
		case 10:
			WorldManager.instance.QueueCutscene(Cutscenes.Wish10(this));
			break;
		case 20:
			WorldManager.instance.QueueCutscene(Cutscenes.Wish20(this));
			break;
		case 50:
			WorldManager.instance.QueueCutscene(Cutscenes.Wish50(this));
			break;
		}
	}
}
