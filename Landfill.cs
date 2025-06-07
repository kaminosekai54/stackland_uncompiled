using UnityEngine;

public class Landfill : SewerCard
{
	public int PollutionOverflowMin;

	public int PollutionOverflowMax;

	[ExtraData("is_overflowing")]
	public bool IsOverflowing;

	public Sprite EmptyIcon;

	public Sprite HalfFullIcon;

	public Sprite FullIcon;

	[HideInInspector]
	[ExtraData("stored_pollution")]
	public int StoredPollution;

	[HideInInspector]
	[ExtraData("pollution_overflow")]
	public int PollutionOverflow;

	public int PollutionRemovalRate = 5;

	public override void OnInitialCreate()
	{
		this.PollutionOverflow = Random.Range(this.PollutionOverflowMin, this.PollutionOverflowMax);
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!this.IsOverflowing)
		{
			return otherCard.Id == "pollution";
		}
		return false;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCardText()
	{
		if (!this.IsOverflowing)
		{
			if (this.StoredPollution <= 0)
			{
				base.descriptionOverride = SokLoc.Translate("card_landfill_description", LocParam.Create("amount", this.PollutionOverflowMin.ToString()));
			}
			else
			{
				base.descriptionOverride = SokLoc.Translate("card_landfill_description_long", LocParam.Create("amount", this.PollutionOverflowMin.ToString()), LocParam.Create("current", this.StoredPollution.ToString()));
			}
		}
		else
		{
			base.nameOverride = SokLoc.Translate("card_overflowing_landfill_name");
			base.descriptionOverride = SokLoc.Translate("card_overflowing_landfill_description");
		}
	}

	public override void UpdateCard()
	{
		if (!this.IsOverflowing)
		{
			if (base.MyGameCard.HasChild && base.AllChildrenMatchPredicate((CardData x) => x is Pollution))
			{
				foreach (Pollution item in base.ChildrenMatchingPredicate((CardData x) => x is Pollution))
				{
					this.StoredPollution += item.PollutionAmount;
					item.PollutionAmount -= item.PollutionAmount;
					if (item.PollutionAmount == 0)
					{
						item.MyGameCard.DestroyCard(spawnSmoke: true);
					}
					if (this.StoredPollution >= this.PollutionOverflow)
					{
						this.IsOverflowing = true;
						GameCamera.instance.Screenshake = 1f;
						item.MyGameCard.RemoveFromParent();
						AudioManager.me.PlaySound(AudioManager.me.LandfillOverflow, base.transform, 1f, 0.3f);
						WorldManager.instance.QueueCutscene("cities_landfill_overflow");
						break;
					}
				}
			}
			if (!base.MyGameCard.TimerRunning && this.StoredPollution > 0)
			{
				base.MyGameCard.StartTimer(60f, DumpPollution, SokLoc.Translate("card_landfill_status_1", LocParam.Create("amount", this.PollutionRemovalRate.ToString())), base.GetActionId("DumpPollution"));
			}
			if (this.StoredPollution >= this.PollutionOverflowMin / 2)
			{
				base.Icon = this.HalfFullIcon;
				base.MyGameCard.UpdateIcon();
			}
			else
			{
				base.Icon = this.EmptyIcon;
				base.MyGameCard.UpdateIcon();
			}
			base.MyGameCard.SpecialValue = this.StoredPollution;
		}
		else
		{
			base.Icon = this.FullIcon;
			base.MyGameCard.UpdateIcon();
			base.MyGameCard.CancelTimer(base.GetActionId("DumpPollution"));
			if (!base.MyGameCard.TimerRunning)
			{
				base.MyGameCard.StartTimer(120f, ResolveOverflow, SokLoc.Translate("card_landfill_status_2"), base.GetActionId("ResolveOverflow"));
			}
		}
		base.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.PollutionIcon;
		base.UpdateCard();
	}

	[TimedAction("resolve_overflow")]
	public void ResolveOverflow()
	{
		WorldManager.instance.CreateSmoke(base.Position);
		this.StoredPollution = this.PollutionOverflow - 10;
		this.IsOverflowing = false;
	}

	[TimedAction("dump_pollution")]
	public void DumpPollution()
	{
		if (this.StoredPollution > 0)
		{
			int num = Mathf.Min(this.StoredPollution, this.PollutionRemovalRate);
			this.StoredPollution -= num;
			AudioManager.me.PlaySound(AudioManager.me.ClearPollution, base.transform, 1f, 0.3f);
			WorldManager.instance.CreateSmoke(base.Position);
		}
	}
}
