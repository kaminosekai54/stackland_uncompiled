using System.Linq;
using UnityEngine;

public class EnergyHarvestable : Harvestable, IEnergyConsumer
{
	[Header("Cities options")]
	public int PollutionPerHarvest;

	public override void OnInitialCreate()
	{
		if (base.MyGameCard.CardConnectorChildren.Count((CardConnector x) => x.CardDirection == CardDirection.input && x.ConnectionType == ConnectionType.LV) > 0)
		{
			WorldManager.instance.QueueCutsceneIfNotPlayed("cities_first_energy");
		}
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return base.CanHaveCard(otherCard);
	}

	public override void OnHarvestComplete()
	{
		if (this.PollutionPerHarvest > 0)
		{
			(WorldManager.instance.CreateCard(base.Position, "pollution", faceUp: true, checkAddToStack: false) as Pollution).PollutionAmount = this.PollutionPerHarvest;
		}
		if (base.Id == "uranium_mine")
		{
			CardData cardData = WorldManager.instance.CreateCard(base.Position, "gravel", faceUp: true, checkAddToStack: false);
			this.SendCard(cardData.MyGameCard);
		}
	}

	public override void SendCard(GameCard card)
	{
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, card, base.OutputDir);
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	protected virtual bool CanStartHarvesting()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (!base.WorkerAmountMet())
		{
			base.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		else if (base.WorkerAmountMet() && base.RequiredVillagerCount <= 0 && !base.MyGameCard.TimerRunning && this.CanStartHarvesting())
		{
			base.MyGameCard.StartTimer(base.HarvestTime, base.CompleteHarvest, base.StatusText, base.GetActionId("CompleteHarvest"));
		}
		if (!this.HasEnergyInput())
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
			{
				base.AddStatusEffect(new StatusEffect_NoEnergy());
			}
			base.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		else if (base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
		{
			base.RemoveStatusEffect<StatusEffect_NoEnergy>();
		}
		if (!base.HasSewerConnected())
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoSewer>())
			{
				base.AddStatusEffect(new StatusEffect_NoSewer());
			}
			base.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		else if (base.HasStatusEffectOfType<StatusEffect_NoSewer>())
		{
			base.RemoveStatusEffect<StatusEffect_NoSewer>();
		}
		base.UpdateCard();
	}

	string IEnergyConsumer.GetEnergyConsumptionString()
	{
		return base.GetEnergyInputString();
	}
}
