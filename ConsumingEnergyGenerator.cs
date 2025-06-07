using System.Collections.Generic;
using System.Linq;

public class ConsumingEnergyGenerator : EnergyGenerator
{
	public List<CardAmountPair> CardsToConsume = new List<CardAmountPair>();

	public int PollutionAmount;

	public float CycleTime = 15f;

	[ExtraData("has_energy")]
	public bool hasEnergy;

	private bool prevHasEnergy;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (this.CardsToConsume.Select((CardAmountPair x) => x.CardId).Contains(otherCard.Id))
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && (!base.MyGameCard.TimerRunning || base.MyGameCard.TimerActionId == base.GetActionId("StopEnergy")) && this.CardsToConsume.All((CardAmountPair pair) => base.CardsInStackMatchingPredicate((CardData x) => x.Id == pair.CardId).Count >= pair.Amount))
		{
			if (!base.IsDamaged)
			{
				base.MyGameCard.StartTimer(this.CycleTime, GenerateEnergy, SokLoc.Translate("card_energy_status_0"), base.GetActionId("GenerateEnergy"));
			}
			if (base.MyGameCard.TimerRunning && base.MyGameCard.TimerActionId == base.GetActionId("GenerateEnergy"))
			{
				base.MyGameCard.CancelTimer(base.GetActionId("StopEnergy"));
				AudioManager.me.PlaySound2D(AudioManager.me.CardDestroy, 1f, 0.4f);
				foreach (CardAmountPair pair2 in this.CardsToConsume)
				{
					base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == pair2.CardId, pair2.Amount);
				}
			}
		}
		if (base.MyGameCard.TimerRunning)
		{
			this.hasEnergy = true;
		}
		if (!base.MyGameCard.TimerRunning && this.hasEnergy)
		{
			base.MyGameCard.StartTimer(5f, StopEnergy, SokLoc.Translate("card_energy_status_0"), base.GetActionId("StopEnergy"), withStatusBar: false, skipWorkerEnergyCheck: true, skipDamageOnOffCheck: true);
		}
		if (this.hasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.hasEnergy;
		base.UpdateCard();
	}

	public override bool HasEnergyOutput(CardConnector connectedNode, List<CardConnector> nodeTracker)
	{
		return this.hasEnergy;
	}

	[TimedAction("generate_energy")]
	public void GenerateEnergy()
	{
		if (this.PollutionAmount > 0)
		{
			WorldManager.instance.CreateCardStack(base.Position, this.PollutionAmount, "pollution", checkAddToStack: false);
		}
	}

	[TimedAction("stop_energy")]
	public void StopEnergy()
	{
		this.hasEnergy = false;
		AudioManager.me.PlaySound2D(AudioManager.me.PowerOutage, 1f, 0.4f);
	}
}
