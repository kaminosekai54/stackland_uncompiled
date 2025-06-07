public class Laboratory : EnergyHarvestable
{
	public bool InCutscene;

	public override void UpdateCard()
	{
		if (base.HasCardOnTop("fossil", out var cardData) && !this.InCutscene)
		{
			this.InCutscene = true;
			WorldManager.instance.QueueCutscene(CitiesCutscenes.DinoBoss(this, cardData));
		}
		base.UpdateCard();
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard.Id == "science"))
		{
			return otherCard.Id == "fossil";
		}
		return true;
	}
}
