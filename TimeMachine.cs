using System.Linq;
using UnityEngine;

public class TimeMachine : Landmark
{
	public AudioClip TimeMachineChargingSound;

	public AudioClip TimeMachineUseSound;

	public AudioClip TimeMachineDoneSound;

	[ExtraData("is_charged")]
	public bool IsCharged;

	[ExtraData("used_once")]
	public bool UsedOnce;

	public override void UpdateCard()
	{
		if (this.IsCharged)
		{
			if (base.MyGameCard.HasChild && (base.MyGameCard.Child.CardData is Worker || base.MyGameCard.Child.CardData is BaseVillager))
			{
				if (WorldManager.instance.GetCardCount<BaseVillager>() <= 1 && base.MyGameCard.Child.CardData is BaseVillager)
				{
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_take_time_machine");
					base.MyGameCard.Child.RemoveFromParent();
				}
				if (!base.MyGameCard.TimerRunning)
				{
					base.MyGameCard.StartTimer(20f, UseTimeMachine, SokLoc.Translate("card_time_machine_status_1"), base.GetActionId("UseTimeMachine"));
				}
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("UseTimeMachine"));
			}
		}
		else if (this.HasEnergyInput())
		{
			base.MyGameCard.StartTimer(240f, ChargeTimeMachine, SokLoc.Translate("card_time_machine_status_2"), base.GetActionId("ChargeTimeMachine"));
			AudioManager.me.PlaySound(this.TimeMachineChargingSound, base.transform, 1f, 0.5f);
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("ChargeTimeMachine"));
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.UpdateCardText();
		if (this.IsCharged)
		{
			base.nameOverride = SokLoc.Translate("card_time_machine_name_real");
			base.descriptionOverride = SokLoc.Translate("card_time_machine_desctiption_real");
		}
	}

	[TimedAction("start_time_machine")]
	public void ChargeTimeMachine()
	{
		AudioManager.me.PlaySound(this.TimeMachineDoneSound, base.transform, 1f, 0.5f);
		WorldManager.instance.QueueCutscene("cities_time_machine");
		this.IsCharged = true;
	}

	[TimedAction("use_time_machine")]
	public void UseTimeMachine()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			string destinationBoard = "main";
			if (WorldManager.instance.CurrentBoard.Id == "main")
			{
				destinationBoard = "cities";
			}
			GameCanvas.instance.ChangeLocationPrompt(GoAway, Stay, destinationBoard);
		}
	}

	public void Stay()
	{
		GameCard parent = base.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData c) => c is Worker || c is BaseVillager);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			base.MyGameCard.Parent = parent;
		}
	}

	private void GoAway()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameBoard targetBoard = WorldManager.instance.GetBoardWithId("main");
		if (WorldManager.instance.CurrentBoard.Id == "main")
		{
			targetBoard = WorldManager.instance.GetBoardWithId("cities");
		}
		AudioManager.me.PlaySound(this.TimeMachineUseSound, base.transform, 1f, 0.5f);
		WorldManager.instance.GoToBoard(targetBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			WorldManager.instance.SendToBoard(base.MyGameCard, targetBoard, new Vector2(0.4f, 0.5f));
			if (base.MyGameCard.GetChildCards().Any((GameCard x) => x.CardData is BaseVillager))
			{
				WorldManager.instance.QueueCutscene("cities_villager_timemachine");
			}
			this.UsedOnce = true;
			base.MyGameCard.RemoveFromStack();
		});
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!this.UsedOnce)
		{
			if (!(otherCard.Id == "genius"))
			{
				return otherCard.Id == "robot_genius";
			}
			return true;
		}
		if (otherCard is Worker || otherCard is BaseVillager)
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}
}
