using System.Collections.Generic;
using UnityEngine;

public class Boat : CardData
{
	public int MaxCapacity = 5;

	public float TravelTime = 30f;

	public bool InSailOffPrompt;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public bool IsSailingOff
	{
		get
		{
			if (base.MyGameCard.TimerRunning)
			{
				return base.MyGameCard.TimerActionId == base.GetActionId("SailOff");
			}
			return false;
		}
	}

	public bool InSailOff => this.InSailOffPrompt;

	public override bool CanBeDragged
	{
		get
		{
			if (this.IsSailingOff)
			{
				return false;
			}
			return true;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (base.GetChildCount() + (otherCard.GetChildCount() + 1) > this.MaxCapacity)
		{
			return false;
		}
		if (this.CardIsAllowedOnBoat(otherCard))
		{
			return otherCard.AllChildrenMatchPredicate((CardData x) => this.CardIsAllowedOnBoat(x));
		}
		return false;
	}

	private bool CardIsAllowedOnBoat(CardData otherCard)
	{
		if (otherCard.Id == "trained_monkey")
		{
			return false;
		}
		if (otherCard.MyCardType != CardType.Food && otherCard.MyCardType != CardType.Humans)
		{
			return otherCard.MyCardType == CardType.Resources;
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
			if (num > 0)
			{
				if (!WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToIsland)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					this.Stay();
					return;
				}
				this.RemoveTrainedMonkeys();
				int cardCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
				int requiredFoodCount = WorldManager.instance.GetRequiredFoodCount();
				if (WorldManager.instance.GetFoodCount() < requiredFoodCount)
				{
					base.MyGameCard.Child.RemoveFromParent();
					GameCanvas.instance.NotEnoughFoodToSailOffPrompt();
				}
				else if (WorldManager.instance.CurrentBoard.Id == "main" && num == cardCount)
				{
					base.MyGameCard.CancelTimer(base.GetActionId("SailOff"));
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_sailing_off_title");
					this.RemoveLastVillagerOnBoat();
				}
				else
				{
					base.MyGameCard.StartTimer(this.TravelTime, SailOff, SokLoc.Translate("card_boat_status"), base.GetActionId("SailOff"));
				}
			}
			else
			{
				base.MyGameCard.CancelTimer(base.GetActionId("SailOff"));
			}
		}
		base.UpdateCard();
	}

	private void RemoveTrainedMonkeys()
	{
		List<GameCard> allCardsInStack = base.MyGameCard.GetAllCardsInStack();
		for (int num = allCardsInStack.Count - 1; num >= 0; num--)
		{
			if (allCardsInStack[num].CardData.Id == "trained_monkey")
			{
				allCardsInStack.RemoveAt(num);
				break;
			}
		}
		WorldManager.instance.Restack(allCardsInStack);
	}

	private void RemoveLastVillagerOnBoat()
	{
		List<GameCard> allCardsInStack = base.MyGameCard.GetAllCardsInStack();
		for (int num = allCardsInStack.Count - 1; num >= 0; num--)
		{
			if (allCardsInStack[num].CardData is BaseVillager)
			{
				allCardsInStack.RemoveAt(num);
				break;
			}
		}
		WorldManager.instance.Restack(allCardsInStack);
	}

	[TimedAction("sail_off")]
	public void SailOff()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			GameCanvas.instance.ChangeLocationPrompt(GoAway, Stay, "island");
		}
	}

	private void Stay()
	{
		GameCard parent = base.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData c) => c is BaseVillager);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			base.MyGameCard.SetParent(parent);
		}
	}

	private void RemoveStacksFromAllBoats()
	{
		foreach (Boat card in WorldManager.instance.GetCards<Boat>())
		{
			if (!(card == this) && card.MyGameCard.HasChild)
			{
				card.MyGameCard.Child.RemoveFromParent();
			}
		}
	}

	private void GoAway()
	{
		EndOfMonthParameters endOfMonthParameters = new EndOfMonthParameters();
		endOfMonthParameters.SkipSpecialEvents = true;
		endOfMonthParameters.CutsceneTitle = SokLoc.Translate("label_sailing_off_full");
		endOfMonthParameters.SkipEndConfirmation = true;
		this.InSailOffPrompt = true;
		this.RemoveStacksFromAllBoats();
		endOfMonthParameters.OnDone = delegate
		{
			this.InSailOffPrompt = false;
			GameCanvas.instance.SetScreen<CutsceneScreen>();
			string id = ((!(WorldManager.instance.CurrentBoard.Id == "main")) ? "main" : "island");
			GameBoard targetBoard = WorldManager.instance.GetBoardWithId(id);
			WorldManager.instance.GoToBoard(targetBoard, delegate
			{
				GameCanvas.instance.SetScreen<GameScreen>();
				WorldManager.instance.SendStackToBoard(base.MyGameCard, targetBoard, new Vector2(0.4f, 0.5f));
				base.RestackChildrenMatchingPredicate((CardData v) => v is BaseVillager);
			});
		};
		WorldManager.instance.ForceEndOfMoon(endOfMonthParameters);
	}
}
