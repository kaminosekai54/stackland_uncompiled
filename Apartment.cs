using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Apartment : EnergyConsumer
{
	public AudioClip SpawnWorkerSound;

	public int HousingSpace = 2;

	[HideInInspector]
	public int FreeSpace;

	[HideInInspector]
	[ExtraData("used_space")]
	public int UsedSpace;

	public bool CanHouseRobotWorkers;

	private float updateTimer = 1f;

	public void UpdateUsedSpace()
	{
		int num = 0;
		foreach (HousingConsumer housingConsumer in CitiesManager.instance.HousingConsumers)
		{
			if (housingConsumer.Housing == this)
			{
				num += housingConsumer.GetHousingSpaceRequired();
			}
		}
		this.UsedSpace = num;
	}

	public override void UpdateCard()
	{
		this.updateTimer -= Time.deltaTime;
		if (this.updateTimer <= 0f)
		{
			this.updateTimer = Random.Range(0.5f, 1f);
			this.UpdateUsedSpace();
		}
		this.FreeSpace = this.HousingSpace - this.UsedSpace;
		if (this.FreeSpace > 0)
		{
			if (!base.HasStatusEffectOfType<StatusEffect_Space>())
			{
				base.AddStatusEffect(new StatusEffect_Space());
			}
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_Space>();
		}
		if (CitiesManager.instance.HomelessHousingConsumers.Count > 0 && this.FreeSpace > 0)
		{
			for (int i = 0; i < CitiesManager.instance.HomelessHousingConsumers.Count; i++)
			{
				HousingConsumer housingConsumer = CitiesManager.instance.HomelessHousingConsumers[i];
				if (housingConsumer != null)
				{
					if (housingConsumer.GetGameCard().Destroyed || (housingConsumer.GetWorkerType() == WorkerType.Robot && !this.CanHouseRobotWorkers) || (this.CanHouseRobotWorkers && housingConsumer.GetWorkerType() != WorkerType.Robot))
					{
						continue;
					}
					if (housingConsumer.GetHousingSpaceRequired() <= this.FreeSpace)
					{
						housingConsumer.Housing = this;
						this.UsedSpace += housingConsumer.GetHousingSpaceRequired();
						this.FreeSpace = this.HousingSpace - this.UsedSpace;
						CitiesManager.instance.HomelessHousingConsumers.RemoveAt(i);
					}
				}
				if (this.FreeSpace <= 0)
				{
					break;
				}
			}
		}
		if (base.MyGameCard.HasChild && base.MyGameCard.Child.CardData is ICurrency)
		{
			if (base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList()
				.Sum((ICurrency x) => x.CurrencyValue) >= 20)
			{
				if (!base.MyGameCard.TimerRunning)
				{
					base.MyGameCard.StartTimer(5f, NewWorker, SokLoc.Translate("label_recruiting_worker"), base.GetActionId("NewWorker"));
				}
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("NewWorker"));
			}
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("NewWorker"));
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm, LocParam.Create("amount", this.HousingSpace.ToString()));
		if (this.FreeSpace != 0 && base.MyGameCard != null && !base.MyGameCard.IsDemoCard)
		{
			base.descriptionOverride = base.descriptionOverride + ". " + SokLoc.Translate("card_apartment_free_space", LocParam.Create("free", this.FreeSpace.ToString()));
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Apartment || otherCard is ICurrency || otherCard.Id == "copper_bar")
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}

	[TimedAction("new_worker")]
	public void NewWorker()
	{
		List<ICurrency> list = base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList();
		if (list.Sum((ICurrency x) => x.CurrencyValue) >= 20)
		{
			CitiesManager.instance.TryUseDollars(list, 20, onlyTakeIfAmountMet: true, spawnSmoke: true);
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "worker");
			cardData.MyGameCard.RemoveFromStack();
			WorldManager.instance.StackSend(cardData.MyGameCard, base.OutputDir);
		}
	}
}
