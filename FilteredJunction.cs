using UnityEngine;

public class FilteredJunction : CardData
{
	[ExtraData("filtered_card")]
	[HideInInspector]
	public string FilteredCard;

	[Term]
	public string NameOverride;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyCardType == CardType.Structures)
		{
			return false;
		}
		if (otherCard.MyCardType == CardType.Humans)
		{
			return false;
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild)
		{
			if (string.IsNullOrEmpty(this.FilteredCard))
			{
				this.FilteredCard = base.MyGameCard.Child.CardData.Id;
				base.MyGameCard.Child.DestroyCard(spawnSmoke: true);
				return;
			}
			for (int num = base.MyGameCard.GetChildCards().Count - 1; num >= 0; num--)
			{
				int outputIndex = -1;
				GameCard gameCard = base.MyGameCard.GetChildCards()[num];
				if (!string.IsNullOrEmpty(this.FilteredCard))
				{
					outputIndex = ((gameCard.CardData.Id == this.FilteredCard) ? 1 : 0);
				}
				gameCard.RemoveFromStack();
				WorldManager.instance.StackSendCheckTarget(base.MyGameCard, gameCard, base.OutputDir, null, sendToChest: true, outputIndex);
			}
		}
		base.UpdateCard();
	}

	public override void Clicked()
	{
		if (!string.IsNullOrEmpty(this.FilteredCard))
		{
			WorldManager.instance.CreateCard(base.Position, this.FilteredCard, faceUp: true, checkAddToStack: false).MyGameCard.SendIt();
			this.FilteredCard = "";
		}
		base.Clicked();
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.FilteredCard))
		{
			base.nameOverride = SokLoc.Translate(this.NameOverride, LocParam.Create("card", WorldManager.instance.GameDataLoader.GetCardFromId(this.FilteredCard).Name));
		}
		else
		{
			base.nameOverride = null;
		}
	}
}
