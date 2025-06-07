public class StablePortal : Portal
{
	public float TravelTime = 30f;

	public override void UpdateCard()
	{
		if (string.IsNullOrWhiteSpace(base.descriptionOverride))
		{
			base.descriptionOverride = SokLoc.Translate("card_stable_portal_description", LocParam.Create("amount", base.MaxVillagerCount.ToString()));
		}
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
			if (num > 0)
			{
				if (!WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToForest)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					base.Stay();
					return;
				}
				base.RemoveNonHuman();
				int cardCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
				if (base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager) > base.MaxVillagerCount && !GameCanvas.instance.ModalIsOpen)
				{
					base.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.MaxVillagerCountPrompt("label_taking_portal_title", base.MaxVillagerCount);
					base.RemoveExcessVillagersInPortal();
				}
				if (num == cardCount && !GameCanvas.instance.ModalIsOpen)
				{
					base.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_taking_portal_title");
					base.RemoveLastVillagerInPortal();
				}
				else
				{
					base.MyGameCard.StartTimer(this.TravelTime, base.TakePortal, SokLoc.Translate("card_stable_portal_status"), base.GetActionId("TakePortal"));
				}
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
			}
		}
		base.UpdateCard();
	}
}
