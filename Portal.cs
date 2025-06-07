using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Portal : CardData
{
	public int MaxVillagerCount = 7;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public bool IsTakingPortal
	{
		get
		{
			if (base.MyGameCard.TimerRunning)
			{
				return base.MyGameCard.TimerActionId == base.GetActionId("TakePortal");
			}
			return false;
		}
	}

	public override bool CanBeDragged
	{
		get
		{
			if (this.IsTakingPortal)
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
		if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			return false;
		}
		if (this.CardIsAllowedInPortal(otherCard))
		{
			return otherCard.AllChildrenMatchPredicate((CardData x) => this.CardIsAllowedInPortal(x));
		}
		return false;
	}

	private bool CardIsAllowedInPortal(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Humans;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
	}

	public void RemoveNonHuman()
	{
		GameCard parent = base.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData x) => x.MyCardType != CardType.Humans);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			base.MyGameCard.Parent = parent;
		}
	}

	public void RemoveLastVillagerInPortal()
	{
		GameCard parent = base.MyGameCard.Parent;
		List<GameCard> childCards = base.MyGameCard.GetChildCards();
		for (int num = childCards.Count - 1; num >= 0; num--)
		{
			if (childCards[num].CardData is BaseVillager)
			{
				childCards[num].RemoveFromParent();
				break;
			}
		}
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			base.MyGameCard.Parent = parent;
		}
	}

	public void RemoveExcessVillagersInPortal()
	{
		GameCard parent = base.MyGameCard.Parent;
		List<GameCard> childCards = base.MyGameCard.GetChildCards();
		for (int num = childCards.Count - 1; num >= this.MaxVillagerCount; num--)
		{
			if (childCards[num].CardData is BaseVillager)
			{
				childCards[num].RemoveFromParent();
			}
		}
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			base.MyGameCard.Parent = parent;
		}
	}

	[TimedAction("take_portal")]
	public void TakePortal()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			GameCanvas.instance.ChangeLocationPrompt(GoAway, Stay, "forest");
		}
	}

	public void Stay()
	{
		GameCard parent = base.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData c) => c is BaseVillager);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			base.MyGameCard.Parent = parent;
		}
	}

	private void RemoveStacksFromAllPortals()
	{
		foreach (CardData item in WorldManager.instance.GetCards<StablePortal>().Cast<CardData>().Concat(WorldManager.instance.GetCards<StrangePortal>().Cast<CardData>()))
		{
			if (!(item == this) && item.MyGameCard.HasChild)
			{
				item.MyGameCard.Child.RemoveFromParent();
			}
		}
	}

	private void GoAway()
	{
		this.RemoveStacksFromAllPortals();
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameBoard targetBoard = WorldManager.instance.GetBoardWithId("forest");
		WorldManager.instance.GoToBoard(targetBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			WorldManager.instance.SendToBoard(base.MyGameCard.Child, targetBoard, new Vector2(0.4f, 0.5f));
			base.RestackChildrenMatchingPredicate((CardData v) => v is BaseVillager);
			if (this is StrangePortal)
			{
				base.MyGameCard.DestroyCard();
			}
		});
	}
}
