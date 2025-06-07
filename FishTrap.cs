using System.Collections.Generic;
using System.Linq;

public class FishTrap : CardData
{
	public BaitBag DefaultBaitBag;

	public List<BaitBag> BaitBags;

	public float FishTime = 30f;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Food;
	}

	public override void UpdateCard()
	{
		if (base.HasCardOnTop(out Food _))
		{
			base.MyGameCard.StartTimer(this.FishTime, CompleteFishing, SokLoc.Translate("card_fish_trap_status"), "complete_fishing");
		}
		else
		{
			base.MyGameCard.CancelTimer("complete_fishing");
		}
		base.UpdateCard();
	}

	[TimedAction("complete_fishing")]
	public void CompleteFishing()
	{
		base.HasCardOnTop(out Food food);
		BaitBag baitBag = this.BaitBags.FirstOrDefault((BaitBag x) => x.BaitId == food.Id);
		if (baitBag == null)
		{
			baitBag = this.DefaultBaitBag;
		}
		ICardId cardId = baitBag.GetCard(removeCard: false);
		if (cardId == null)
		{
			cardId = (CardId)"cod";
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardId, faceUp: false, checkAddToStack: false);
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
		base.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c == food, 1);
	}
}
