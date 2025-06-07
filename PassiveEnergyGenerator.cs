using System.Collections.Generic;

public class PassiveEnergyGenerator : EnergyGenerator
{
	public int EnergyGeneratedPerCycle = 1;

	public float CycleTime = 30f;

	[ExtraData("has_energy")]
	public bool hasEnergy;

	private bool prevHasEnergy;

	public override void UpdateCard()
	{
		if (!base.MyGameCard.TimerRunning && !base.IsDamaged)
		{
			base.MyGameCard.StartTimer(this.CycleTime, EndCycle, SokLoc.Translate("card_energy_status_0"), base.GetActionId("EndCycle"));
		}
		base.UpdateCard();
	}

	public override bool HasEnergyOutput(CardConnector connectedNode, List<CardConnector> nodeTracker)
	{
		if (base.WorkerAmountMet())
		{
			this.hasEnergy = true;
		}
		else
		{
			this.hasEnergy = false;
		}
		if (this.hasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.hasEnergy;
		return this.hasEnergy;
	}

	[TimedAction("end_cycle")]
	public void EndCycle()
	{
	}
}
