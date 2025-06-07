using System.Linq;
using UnityEngine;

public class Chest : CardData
{
	[ExtraData("coin_count")]
	[HideInInspector]
	public int CoinCount;

	public int MaxCoinCount = 100;

	public string HeldCardId = "gold";

	public string ChestTerm = "card_coin_chest_description_long";

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == base.Id)
		{
			return true;
		}
		if (otherCard.Id == this.HeldCardId)
		{
			return this.GetChestWithSpace() != null;
		}
		return false;
	}

	private Chest GetChestWithSpace()
	{
		GameCard gameCard = base.MyGameCard.GetAllCardsInStack().FirstOrDefault((GameCard x) => x.CardData is Chest chest && chest.CoinCount < chest.MaxCoinCount);
		if (gameCard == null)
		{
			return null;
		}
		return gameCard.CardData as Chest;
	}

	public override void UpdateCard()
	{
		base.Value = this.CoinCount;
		if (!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard childCard in base.MyGameCard.GetChildCards())
			{
				if (!(childCard.CardData.Id != this.HeldCardId))
				{
					Chest chestWithSpace = this.GetChestWithSpace();
					if (!(chestWithSpace != null))
					{
						childCard.RemoveFromParent();
						break;
					}
					if (chestWithSpace.CoinCount < chestWithSpace.MaxCoinCount)
					{
						childCard.DestroyCard(spawnSmoke: true);
						chestWithSpace.CoinCount++;
					}
				}
			}
		}
		base.descriptionOverride = SokLoc.Translate(this.ChestTerm, LocParam.Create("count", this.CoinCount.ToString()), LocParam.Create("max_count", this.MaxCoinCount.ToString()), LocParam.Create("goldicon", Icons.Gold), LocParam.Create("shellicon", Icons.Shell));
		base.UpdateCard();
	}

	public override void Clicked()
	{
		int a = 5;
		if (this.CoinCount > 0)
		{
			int num = Mathf.Min(a, this.CoinCount);
			GameCard gameCard = WorldManager.instance.CreateCardStack(base.transform.position + Vector3.up * 0.2f, num, this.HeldCardId, checkAddToStack: false);
			WorldManager.instance.StackSend(gameCard.GetRootCard(), base.OutputDir, null, sendToChest: false);
			this.CoinCount -= num;
		}
		base.Clicked();
	}
}
