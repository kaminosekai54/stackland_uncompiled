using System.Collections.Generic;
using System.Linq;

public class ArtGallery : Landmark
{
	public int ArtPrice = 50;

	[Card]
	public List<string> AcceptedCards;

	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.AcceptedCards.Contains(otherCard.Id);
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && base.MyGameCard.Child.CardData is ICurrency)
		{
			if (base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList()
				.Sum((ICurrency x) => x.CurrencyValue) >= this.ArtPrice)
			{
				if (!base.MyGameCard.TimerRunning)
				{
					base.MyGameCard.StartTimer(60f, CreatePainting, SokLoc.Translate("card_art_gallery_status_1"), base.GetActionId("CreatePainting"));
				}
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("CreatePainting"));
			}
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("CreatePainting"));
		}
		base.UpdateCard();
	}

	[TimedAction("create_painting")]
	public void CreatePainting()
	{
		List<ICurrency> list = base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList();
		if (list.Sum((ICurrency x) => x.CurrencyValue) >= this.ArtPrice)
		{
			CitiesManager.instance.TryUseDollars(list, this.ArtPrice, onlyTakeIfAmountMet: true, spawnSmoke: true, keepOnStack: true);
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "artwork");
			cardData.MyGameCard.RemoveFromStack();
			WorldManager.instance.StackSend(cardData.MyGameCard, base.OutputDir);
		}
	}
}
