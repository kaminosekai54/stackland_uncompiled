using UnityEngine;

public class Royal : CardData
{
	[HideInInspector]
	public float MoveTimer;

	public float MoveTime = 10f;

	[ExtraData("attack_tries")]
	public int AttackTries;

	public override void UpdateCard()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (!base.MyGameCard.IsDemoCard)
		{
			if (!WorldManager.instance.InAnimation)
			{
				this.UpdateInteractions();
			}
			if (activeDemand != null)
			{
				base.AddStatusEffect(new StatusEffect_Demand());
			}
			else if (base.HasStatusEffectOfType<StatusEffect_Demand>())
			{
				base.RemoveStatusEffect<StatusEffect_Demand>();
			}
			base.UpdateCard();
		}
	}

	private void UpdateInteractions()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (!(base.MyGameCard.Child != null))
		{
			return;
		}
		GameCard child = base.MyGameCard.Child;
		Demand demand = activeDemand?.Demand;
		if (child.CardData is BaseVillager)
		{
			this.AttackTries++;
			if (this.AttackTries >= 9)
			{
				WorldManager.instance.ChangeToCard(base.MyGameCard, "angry_royal");
				WorldManager.instance.CurrentRunVariables.ActiveDemand = null;
				DemandManager.instance.CanReceiveDemand = false;
			}
			else
			{
				WorldManager.instance.QueueCutscene(GreedCutscenes.TryAttackRoyal(this, this.AttackTries));
			}
			child.RemoveFromParent();
			child.SendIt();
		}
		else
		{
			if (!(demand != null) || !(child.CardData.Id == demand.CardToGet))
			{
				return;
			}
			foreach (GameCard childCard in base.MyGameCard.GetChildCards())
			{
				if (!(childCard.CardData.Id == demand.CardToGet))
				{
					continue;
				}
				if (demand.IsFinalDemand)
				{
					child.RemoveFromParent();
					child.SendIt();
					WorldManager.instance.QueueCutscene(GreedCutscenes.FinalDemandEndSuccess(shouldStop: true));
				}
				else if (WorldManager.instance.CurrentRunVariables.ActiveDemand.AmountGiven < demand.Amount)
				{
					childCard.RemoveFromStack();
					if (demand.ShouldDestroyOnComplete)
					{
						childCard.DestroyCard();
						WorldManager.instance.CreateSmoke(base.transform.position);
					}
					else
					{
						childCard.SendIt();
					}
					WorldManager.instance.CurrentRunVariables.ActiveDemand.AmountGiven++;
					if (demand.Amount == WorldManager.instance.CurrentRunVariables.ActiveDemand.AmountGiven)
					{
						WorldManager.instance.QueueCutscene(DemandManager.instance.FinishDemand(WorldManager.instance.CurrentRunVariables.ActiveDemand));
					}
				}
				else
				{
					child.RemoveFromParent();
					child.SendIt();
				}
			}
		}
	}

	public override void UpdateCardText()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (activeDemand != null && DemandManager.instance != null)
		{
			Demand demandById = DemandManager.instance.GetDemandById(activeDemand.DemandId);
			if (!(demandById != null))
			{
				return;
			}
			if (demandById.IsFinalDemand)
			{
				base.descriptionOverride = SokLoc.Translate("card_royal_description_demand_2");
				return;
			}
			base.descriptionOverride = DemandManager.instance.GetDemandStartDescription(demandById, activeDemand);
			if (demandById.Amount > 1)
			{
				base.descriptionOverride = base.descriptionOverride + "\n\n" + SokLoc.Translate("label_greed_given", LocParam.Create("given", $"{activeDemand.AmountGiven}/{demandById.Amount}"));
			}
		}
		else
		{
			base.descriptionOverride = "";
		}
	}

	public void Die()
	{
		WorldManager.instance.CreateCard(base.transform.position, "royal_crown");
		WorldManager.instance.CreateSmoke(base.transform.position);
		base.RemoveAllStatusEffects();
		WorldManager.instance.ChangeToCard(base.MyGameCard, "corpse");
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		Demand currentDemand = DemandManager.instance.GetCurrentDemand();
		if (otherCard is BaseVillager || (currentDemand != null && currentDemand.CardToGet == otherCard.Id))
		{
			return true;
		}
		return false;
	}
}
