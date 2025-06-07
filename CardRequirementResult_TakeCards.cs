using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CardRequirementResult_TakeCards : CardRequirementResult
{
	public bool TakeThisCard;

	[Card]
	public string CardId;

	public int Amount;

	public bool IsNegative;

	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		if (this.TakeThisCard)
		{
			card.DestroyCard(spawnSmoke: true);
		}
		else
		{
			List<CardData> cards = WorldManager.instance.GetCards(this.CardId);
			cards.Reverse();
			cards = cards.OrderBy((CardData x) => x.MyGameCard.GetCardIndex()).ToList();
			for (int i = 0; i < this.Amount; i++)
			{
				WorldManager.instance.CreateSmoke(cards.Last().Position);
				cards.Last().MyGameCard.DestroyCard();
			}
		}
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Card;
	}

	public override IEnumerator Perform(GameCard card)
	{
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		return "";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		return "";
	}
}
