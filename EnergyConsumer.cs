using System.Linq;

public class EnergyConsumer : SewerCard, IEnergyConsumer
{
	public override void OnInitialCreate()
	{
		if (base.MyGameCard.CardConnectorChildren.Count((CardConnector x) => x.CardDirection == CardDirection.input && x.ConnectionType == ConnectionType.LV) > 0)
		{
			WorldManager.instance.QueueCutsceneIfNotPlayed("cities_first_energy");
		}
		base.OnInitialCreate();
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (!base.WorkerAmountMet() && base.MyGameCard.TimerRunning && !base.MyGameCard.SkipCitiesChecks)
		{
			base.MyGameCard.CancelAnyTimer();
		}
		if (!this.HasEnergyInput())
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
			{
				base.AddStatusEffect(new StatusEffect_NoEnergy());
			}
			if (base.MyGameCard.TimerRunning && !base.MyGameCard.SkipCitiesChecks)
			{
				base.MyGameCard.CancelAnyTimer();
			}
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_NoEnergy>();
		}
		base.UpdateCard();
	}

	string IEnergyConsumer.GetEnergyConsumptionString()
	{
		return base.GetEnergyInputString();
	}
}
