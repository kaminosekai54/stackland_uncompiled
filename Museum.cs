using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Museum : CardData
{
	[ExtraData("given_card_ids")]
	[HideInInspector]
	public string SavedGivenCardIds;

	private List<string> _givenCards;

	private List<string> givenCards
	{
		get
		{
			if (this._givenCards == null)
			{
				this._givenCards = this.SavedGivenCardIds.Split(',').ToList();
			}
			return this._givenCards;
		}
	}

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public void GiveCard(CardData card)
	{
		if (!this.CardWasGiven(card))
		{
			this.givenCards.Add(card.Id);
			this.UpdateData();
		}
	}

	public bool CardWasGiven(CardData card)
	{
		return this.givenCards.Contains(card.Id);
	}

	private void UpdateData()
	{
		this.SavedGivenCardIds = string.Join(",", this.givenCards);
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
		if (base.HasCardOnTop(out Food _))
		{
			base.MyGameCard.StartTimer(5f, ResearchedItem, SokLoc.Translate("card_tavern_status_0"), base.GetActionId("ResearchedItem"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("ResearchedItem"));
		}
	}

	[TimedAction("research_food")]
	public void ResearchedItem()
	{
		if (base.HasCardOnTop(out CardData card) && !this.CardWasGiven(card))
		{
			base.RemoveFirstChildFromStack();
			card.MyGameCard.DestroyCard();
			WorldManager.instance.TryCreateHappiness(base.transform.position, 2);
			this.GiveCard(card);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Resource && !this.CardWasGiven(otherCard))
		{
			return true;
		}
		return false;
	}
}
