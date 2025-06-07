using UnityEngine;

public class Farmland : CardData
{
	public bool CanDeplete;

	[Card]
	public string HarvestCardId;

	public int HarvestAmount = 3;

	[HideInInspector]
	[ExtraData("amount_harvested")]
	public int AmountHarvested;

	public bool IsDepleted;

	public float DepletedTime = 30f;

	public float HarvestTime = 10f;

	public AudioClip WateringSound;

	public Sprite DepletedIcon;

	public Sprite NormalIcon;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is Worker))
		{
			return otherCard.Id == "water";
		}
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (!this.IsDepleted)
		{
			base.RemoveStatusEffect<StatusEffect_Depleted>();
			if (!base.WorkerAmountMet() && base.MyGameCard.TimerRunning)
			{
				base.MyGameCard.CancelTimer(base.GetActionId("Harvest"));
			}
			if (!this.CanDeplete || this.AmountHarvested < this.HarvestAmount)
			{
				if (base.WorkerAmountMet() && !base.MyGameCard.TimerRunning)
				{
					base.MyGameCard.StartTimer(this.HarvestTime, Harvest, SokLoc.Translate("card_farmland_status"), base.GetActionId("Harvest"));
				}
			}
			else
			{
				if (this.CanDeplete)
				{
					this.IsDepleted = true;
				}
				this.AmountHarvested = 0;
			}
			base.MyGameCard.IconRenderer.sprite = this.NormalIcon;
		}
		else
		{
			base.AddStatusEffect(new StatusEffect_Depleted());
			if (base.MyGameCard.HasChild && base.ChildrenMatchingPredicateCount((CardData x) => x.Id == "water") >= 1)
			{
				if (!base.MyGameCard.TimerRunning)
				{
					base.MyGameCard.StartTimer(this.DepletedTime, WaterFarmland, SokLoc.Translate("card_farmland_status_0"), base.GetActionId("WaterFarmland"));
				}
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("WaterFarmland"));
			}
			base.MyGameCard.IconRenderer.sprite = this.DepletedIcon;
		}
		base.MyGameCard.UpdateIcon();
		base.UpdateCard();
	}

	[TimedAction("harvest")]
	public void Harvest()
	{
		this.AmountHarvested++;
		CardData cardData = WorldManager.instance.CreateCard(base.Position, this.HarvestCardId, faceUp: true, checkAddToStack: false);
		WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, base.OutputDir);
	}

	[TimedAction("water_farmland")]
	public void WaterFarmland()
	{
		base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == "water", 1);
		this.IsDepleted = false;
		AudioManager.me.PlaySound2D(this.WateringSound, 1f, 0.3f);
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}
}
