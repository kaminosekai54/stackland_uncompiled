using System.Collections.Generic;

public class Charity : CardData
{
	public float GenerationTime = 5f;

	public int RequiredCoins = 3;

	private List<CardData> golds = new List<CardData>();

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "gold";
	}

	public override void UpdateCard()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Gold, this.golds);
		if (this.golds.Count >= this.RequiredCoins)
		{
			base.MyGameCard.StartTimer(this.GenerationTime, CompleteCharity, SokLoc.Translate("card_charity_status_active"), "complete_charity");
		}
		else
		{
			base.MyGameCard.CancelTimer("complete_charity");
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.descriptionOverride = SokLoc.Translate("card_charity_description", LocParam.Create("amount", this.RequiredCoins.ToString()));
		base.UpdateCardText();
	}

	[TimedAction("complete_charity")]
	public void CompleteCharity()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Gold, this.golds);
		if (this.golds.Count >= this.RequiredCoins)
		{
			base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => this.golds.Contains(x), this.RequiredCoins);
			WorldManager.instance.TryCreateHappiness(base.transform.position, 1);
		}
	}
}
