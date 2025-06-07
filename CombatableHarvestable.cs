using UnityEngine;

public class CombatableHarvestable : CardData
{
	[Header("Harvestable")]
	public string StatusTerm;

	[ExtraData("amount")]
	public int Amount = 3;

	public bool IsUnlimited;

	public float HarvestTime = 10f;

	public CardBag MyCardBag;

	public string StatusText => SokLoc.Translate(this.StatusTerm);

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is BaseVillager;
	}

	public override void SetFoil()
	{
		base.SetFoil();
	}

	public override void UpdateCard()
	{
		if (base.HasCardOnTop(out BaseVillager card))
		{
			string actionId = base.GetActionId("CompleteHarvest");
			base.MyGameCard.StartTimer(card.GetActionTimeModifier(actionId, this) * this.HarvestTime, CompleteHarvest, this.StatusText, actionId);
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		base.UpdateCard();
	}

	[TimedAction("complete_harvest")]
	public void CompleteHarvest()
	{
		if (!this.IsUnlimited)
		{
			this.Amount--;
		}
		CardData cardData = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, this.MyCardBag.GetCard(), faceUp: false, checkAddToStack: false);
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
		if (base.HasCardOnTop(out BaseVillager card))
		{
			card.MyGameCard.RotWobble(0.5f);
		}
		if (!this.IsUnlimited && this.Amount <= 0)
		{
			base.MyGameCard.DestroyCard(spawnSmoke: true);
		}
	}
}
