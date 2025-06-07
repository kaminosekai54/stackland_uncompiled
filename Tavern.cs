using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tavern : CardData
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
			base.MyGameCard.StartTimer(30f, ResearchedFood, SokLoc.Translate("card_tavern_status_0"), base.GetActionId("ResearchedFood"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("ResearchedFood"));
		}
	}

	[TimedAction("research_food")]
	public void ResearchedFood()
	{
		if (base.HasCardOnTop(out Food card))
		{
			base.RemoveFirstChildFromStack();
			card.MyGameCard.DestroyCard();
			WorldManager.instance.TryCreateHappiness(base.transform.position, Mathf.Max(1, card.FoodValue / 3));
			this.GiveCard(card);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Food { FoodValue: >0 })
		{
			return true;
		}
		return false;
	}
}
