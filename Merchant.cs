using UnityEngine;

public class Merchant : CardData
{
	public int AmountNeeded = 100;

	[ExtraData("amountGiven")]
	public int AmountGiven;

	private string HeldCardId = "gold";

	public AudioClip BuySound;

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

	public override void UpdateCard()
	{
		if (!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard childCard in base.MyGameCard.GetChildCards())
			{
				if (childCard.CardData is Chest chest)
				{
					if (chest.CoinCount < this.AmountNeeded - this.AmountGiven)
					{
						this.AmountGiven += chest.CoinCount;
						chest.CoinCount = 0;
						WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
					else if (chest.CoinCount >= this.AmountNeeded - this.AmountGiven)
					{
						chest.CoinCount -= this.AmountNeeded - this.AmountGiven;
						this.AmountGiven = this.AmountNeeded;
						WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
				}
				if (!(childCard.CardData.Id != this.HeldCardId))
				{
					if (this.AmountGiven >= this.AmountNeeded)
					{
						childCard.RemoveFromParent();
						break;
					}
					childCard.DestroyCard(spawnSmoke: true);
					this.AmountGiven++;
				}
			}
			if (this.AmountGiven == this.AmountNeeded)
			{
				WorldManager.instance.CreateCard(base.Position, "dragon_egg").MyGameCard.SendIt();
				WorldManager.instance.CreateSmoke(base.Position);
				AudioManager.me.PlaySound2D(this.BuySound, 1f, 0.3f);
				base.MyGameCard.DestroyCard();
			}
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		if (this.AmountGiven > 0)
		{
			base.descriptionOverride = SokLoc.Translate("card_merchant_description_2", LocParam.Create("coinsNeeded", (this.AmountNeeded - this.AmountGiven).ToString()));
		}
		else
		{
			base.descriptionOverride = "";
		}
		base.UpdateCardText();
	}
}
