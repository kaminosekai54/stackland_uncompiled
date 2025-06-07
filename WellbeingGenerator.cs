public class WellbeingGenerator : Landmark, IEnergyConsumer
{
	public int WellbeingAmountPerCycle;

	public int HarvestTime = 10;

	[Term]
	public string StatusResultTerm;

	[Term]
	public string StatusTerm;

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Worker;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (base.WorkerAmountMet() && !base.MyGameCard.TimerRunning)
		{
			base.MyGameCard.StartTimer(this.HarvestTime, Complete, SokLoc.Translate(this.StatusTerm), base.GetActionId("Complete"));
		}
		else if (!base.WorkerAmountMet())
		{
			base.MyGameCard.CancelAnyTimer();
		}
		if (!base.MyGameCard.TimerRunning && !this.HasEnergyInput())
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
			{
				base.AddStatusEffect(new StatusEffect_NoEnergy());
			}
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_NoEnergy>();
		}
		base.UpdateCard();
	}

	[TimedAction("complete")]
	public void Complete()
	{
		CitiesManager.instance.AddWellbeing(this.WellbeingAmountPerCycle);
		WorldManager.instance.CreateFloatingText(base.MyGameCard, this.WellbeingAmountPerCycle > 0, this.WellbeingAmountPerCycle, SokLoc.Translate(this.StatusResultTerm), Icons.Wellbeing, desiredBehaviour: true, 0, 0f, closeOnHover: true);
	}

	string IEnergyConsumer.GetEnergyConsumptionString()
	{
		return base.GetEnergyInputString();
	}
}
