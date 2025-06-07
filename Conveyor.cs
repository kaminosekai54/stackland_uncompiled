using UnityEngine;

public class Conveyor : CardData
{
	public float ExtraSideDistance = 0.01f;

	[ExtraData("direction")]
	[HideInInspector]
	public int Direction;

	public float TotalTime = 5f;

	private Vector2[] corners = new Vector2[4];

	private Vector3 directionVector
	{
		get
		{
			if (this.Direction == 0)
			{
				return Vector3.back;
			}
			if (this.Direction == 1)
			{
				return Vector3.left;
			}
			if (this.Direction == 2)
			{
				return Vector3.forward;
			}
			if (this.Direction == 3)
			{
				return Vector3.right;
			}
			return Vector3.back;
		}
	}

	protected override bool CanToggleOnOff()
	{
		if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			return true;
		}
		return false;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return false;
	}

	private bool CanBeInputCard(CardData card)
	{
		if (card.MyGameCard.Velocity.HasValue || card.MyGameCard.BounceTarget != null)
		{
			return false;
		}
		if (base.MyGameCard.IsParentOf(card.MyGameCard))
		{
			return false;
		}
		if (card is ResourceChest resourceChest)
		{
			if (string.IsNullOrEmpty(resourceChest.HeldCardId))
			{
				return false;
			}
			return this.CanBeConveyed(resourceChest.HeldCardId);
		}
		if (card is ResourceMagnet resourceMagnet)
		{
			if (string.IsNullOrEmpty(resourceMagnet.PullCardId))
			{
				return false;
			}
			return this.CanBeConveyed(resourceMagnet.PullCardId);
		}
		if (this.CanBeConveyed(card))
		{
			if (card.MyGameCard.HasChild)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CanBeConveyed(string cardId)
	{
		CardData cardPrefab = WorldManager.instance.GetCardPrefab(cardId);
		return this.CanBeConveyed(cardPrefab);
	}

	private CardData GetConveyableCardFromInputCard(CardData card)
	{
		if (card is ResourceChest { ResourceCount: >0 } resourceChest)
		{
			return resourceChest.RemoveResources(1).CardData;
		}
		if (card is ResourceMagnet resourceMagnet && resourceMagnet.MyGameCard.HasChild)
		{
			return resourceMagnet.MyGameCard.GetLeafCard().CardData;
		}
		if (this.CanBeConveyed(card))
		{
			return card;
		}
		return null;
	}

	private bool InputCardHasConveyableCard(CardData card)
	{
		if (card is ResourceChest resourceChest)
		{
			return resourceChest.ResourceCount > 0;
		}
		if (card is ResourceMagnet resourceMagnet && resourceMagnet.MyGameCard.HasChild)
		{
			return true;
		}
		if (this.CanBeConveyed(card))
		{
			return true;
		}
		return false;
	}

	private CardData GetPrefabForId(string id)
	{
		return WorldManager.instance.GetCardPrefab(id);
	}

	private CardData GetInputCardConveyablePrefab(CardData card)
	{
		if (card is ResourceChest resourceChest)
		{
			return this.GetPrefabForId(resourceChest.HeldCardId);
		}
		if (card is ResourceMagnet resourceMagnet)
		{
			return this.GetPrefabForId(resourceMagnet.PullCardId);
		}
		if (this.CanBeConveyed(card))
		{
			return this.GetPrefabForId(card.Id);
		}
		return null;
	}

	private CardData GetInputCard(bool allowDraggingCards)
	{
		return WorldManager.instance.GetBestCardInDirection(base.MyGameCard, this.directionVector, allowDraggingCards, (GameCard card) => this.CanBeInputCard(card.CardData))?.CardData;
	}

	private bool CanBeConveyed(CardData otherCard)
	{
		if (otherCard.MyCardType != CardType.Resources && otherCard.MyCardType != CardType.Food && otherCard.MyCardType != CardType.Humans)
		{
			if (otherCard is Mob mob)
			{
				return !mob.IsAggressive;
			}
			return false;
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.IsDemoCard)
		{
			return;
		}
		bool flag = true;
		if (base.MyGameCard.Velocity.HasValue)
		{
			flag = false;
		}
		CardData cardData = null;
		if (flag)
		{
			cardData = this.GetInputCard(allowDraggingCards: true);
		}
		if (cardData != null && this.InputCardHasConveyableCard(cardData))
		{
			CardData inputCardConveyablePrefab = this.GetInputCardConveyablePrefab(cardData);
			string status = SokLoc.Translate("card_conveyor_status", LocParam.Create("resource", inputCardConveyablePrefab.Name));
			base.MyGameCard.StartTimer(this.TotalTime, LoadCard, status, base.GetActionId("LoadCard"));
		}
		else
		{
			base.MyGameCard.CancelAnyTimer();
		}
		CardData outputCard = null;
		if (cardData != null)
		{
			CardData inputCardConveyablePrefab2 = this.GetInputCardConveyablePrefab(cardData);
			if (inputCardConveyablePrefab2 != null)
			{
				outputCard = WorldManager.instance.GetTargetCard(base.MyGameCard, inputCardConveyablePrefab2, -this.directionVector, allowDraggedCards: true, cardData.MyGameCard)?.CardData;
			}
		}
		this.DrawArrows(cardData, outputCard);
		base.UpdateCard();
	}

	public override void Clicked()
	{
		this.Direction = (this.Direction + 1) % 4;
		base.Clicked();
	}

	[TimedAction("load_card")]
	public void LoadCard()
	{
		CardData inputCard = this.GetInputCard(allowDraggingCards: false);
		if (inputCard == null)
		{
			return;
		}
		CardData conveyableCardFromInputCard = this.GetConveyableCardFromInputCard(inputCard);
		if (conveyableCardFromInputCard == null)
		{
			return;
		}
		conveyableCardFromInputCard.MyGameCard.RemoveFromStack();
		GameCard targetCard = WorldManager.instance.GetTargetCard(base.MyGameCard, conveyableCardFromInputCard, -this.directionVector, allowDraggedCards: false, inputCard.MyGameCard);
		if (targetCard != null)
		{
			this.SendToTargetCard(conveyableCardFromInputCard.MyGameCard, targetCard);
		}
		else
		{
			if (conveyableCardFromInputCard.MyGameCard.BounceTarget == inputCard.MyGameCard)
			{
				conveyableCardFromInputCard.MyGameCard.BounceTarget = null;
			}
			conveyableCardFromInputCard.MyGameCard.SendToPosition(base.MyGameCard.transform.position - this.directionVector);
		}
		QuestManager.instance.SpecialActionComplete("use_conveyor");
	}

	private void SendToTargetCard(GameCard card, GameCard targetCard)
	{
		Vector3 vector = targetCard.transform.position - card.transform.position;
		vector.y = 0f;
		Vector3 value = new Vector3(vector.x * 4f, 7f, vector.z * 4f);
		card.BounceTarget = targetCard.GetRootCard();
		card.Velocity = value;
	}

	private Vector2 GetPointOnCardEdge(Vector2 start, Vector2 end, GameCard card)
	{
		Bounds bounds = card.GetBounds();
		this.corners[0] = new Vector2(bounds.min.x, bounds.min.z);
		this.corners[1] = new Vector2(bounds.max.x, bounds.min.z);
		this.corners[2] = new Vector2(bounds.max.x, bounds.max.z);
		this.corners[3] = new Vector2(bounds.min.x, bounds.max.z);
		for (int i = 0; i < 4; i++)
		{
			Vector2 p = this.corners[i];
			Vector2 p2 = this.corners[(i + 1) % 4];
			if (MathHelper.LineSegmentsIntersection(start, end, p, p2, out var intersection, out var _))
			{
				return intersection;
			}
		}
		return start;
	}

	private Vector3 TransformToEdge(Vector3 start, Vector3 end, GameCard card, float dir)
	{
		Vector2 start2 = new Vector2(start.x, start.z);
		Vector2 end2 = new Vector2(end.x, end.z);
		Vector2 pointOnCardEdge = this.GetPointOnCardEdge(start2, end2, card);
		return new Vector3(pointOnCardEdge.x, 0f, pointOnCardEdge.y) + (start - end).normalized * this.ExtraSideDistance * dir;
	}

	private void DrawInputArrow(CardData inputCard)
	{
		Vector3 position = base.MyGameCard.transform.position;
		Vector3 start = ((!(inputCard != null)) ? (base.MyGameCard.transform.position + this.directionVector * 0.5f) : this.TransformToEdge(inputCard.transform.position, position, inputCard.MyGameCard, -1f));
		position = this.TransformToEdge(start, position, base.MyGameCard, 1f);
		DrawManager.instance.DrawShape(new ConveyorArrow
		{
			Start = start,
			End = position
		});
	}

	private void DrawOutputArrow(CardData outputCard)
	{
		Vector3 position = base.MyGameCard.transform.position;
		Vector3 end = ((!(outputCard != null)) ? (base.MyGameCard.transform.position - this.directionVector * 0.5f) : this.TransformToEdge(position, outputCard.transform.position, outputCard.MyGameCard, 1f));
		position = this.TransformToEdge(position, end, base.MyGameCard, -1f);
		DrawManager.instance.DrawShape(new ConveyorArrow
		{
			Start = position,
			End = end
		});
	}

	private void DrawArrows(CardData inputCard, CardData outputCard)
	{
		this.DrawInputArrow(inputCard);
		this.DrawOutputArrow(outputCard);
	}
}
