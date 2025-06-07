using System.Collections.Generic;

public class House : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is BaseVillager || otherCard is Kid)
		{
			return true;
		}
		if (otherCard.Id == base.Id)
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}

	public override void UpdateCard()
	{
		if (base.HasCardOnTop<Kid>())
		{
			base.MyGameCard.StartTimer(120f, GrowUpKid, SokLoc.Translate("new_growing_up"), base.GetActionId("GrowUpKid"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("GrowUpKid"));
		}
		base.UpdateCard();
	}

	[TimedAction("growup_kid")]
	public void GrowUpKid()
	{
		base.HasCardOnTop(out Kid card);
		List<ExtraCardData> extraCardData = card.GetExtraCardData();
		card.MyGameCard.DestroyCard(spawnSmoke: true);
		CardData cardData;
		if (WorldManager.instance.IsSpiritDlcActive())
		{
			cardData = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, "teenage_villager", faceUp: true, checkAddToStack: false);
			(cardData as BaseVillager).UpdateLifeStage();
		}
		else
		{
			cardData = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, "villager", faceUp: true, checkAddToStack: false);
		}
		cardData.SetExtraCardData(extraCardData);
		cardData.MyGameCard.SendIt();
	}
}
