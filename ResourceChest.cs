using UnityEngine;
using UnityEngine.InputSystem;

public class ResourceChest : CardData
{
	[ExtraData("resource_count")]
	[HideInInspector]
	public int ResourceCount;

	[ExtraData("resource_id")]
	[HideInInspector]
	public string HeldCardId = "";

	public Sprite SpecialIcon;

	[Term]
	public string ChestTermOverride = "card_storage_container_name_override";

	[Term]
	public string ChestDescriptionLong = "card_storage_container_description_long";

	public int MaxResourceCount = 100;

	private CardConnector outputConnector;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!string.IsNullOrEmpty(this.HeldCardId) && otherCard.Id != this.HeldCardId)
		{
			return false;
		}
		if (otherCard is Food { FoodValue: <=0 } && WorldManager.instance.CurrentBoard.Id == "cities")
		{
			return true;
		}
		if (otherCard.MyCardType != CardType.Resources || otherCard.Id == "gold" || otherCard.Id == "shell")
		{
			return false;
		}
		if (WorldManager.instance.CurrentBoard.Id == "cities" && (otherCard.Id == "poop" || otherCard.Id == "quantum_entangled_uranium"))
		{
			return false;
		}
		if (otherCard is Dollar)
		{
			return false;
		}
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		base.MyGameCard.SpecialValue = this.ResourceCount;
		base.MyGameCard.SpecialIcon.sprite = this.SpecialIcon;
		if (!base.MyGameCard.HasParent || base.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard childCard in base.MyGameCard.GetChildCards())
			{
				if (string.IsNullOrEmpty(this.HeldCardId))
				{
					this.HeldCardId = childCard.CardData.Id;
				}
				if (!(childCard.CardData.Id != this.HeldCardId))
				{
					if (this.ResourceCount >= this.MaxResourceCount)
					{
						childCard.RemoveFromParent();
						break;
					}
					childCard.DestroyCard(spawnSmoke: true);
					this.ResourceCount++;
					if (this.ResourceCount == this.MaxResourceCount)
					{
						QuestManager.instance.SpecialActionComplete("full_chest");
					}
				}
			}
		}
		if (this.outputConnector == null)
		{
			this.outputConnector = this.GetOutputConnector();
		}
		if (this.ResourceCount > 0 && this.outputConnector?.ConnectedNode != null)
		{
			base.MyGameCard.StartTimer(10f, OutputCard, SokLoc.Translate("idea_resourcechest_status_2"), base.GetActionId("OutputCard"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("OutputCard"));
		}
		base.UpdateCard();
		if (string.IsNullOrEmpty(this.HeldCardId))
		{
			base.Icon = SpriteManager.instance.EmptyTexture;
		}
		else
		{
			base.Icon = WorldManager.instance.GetCardPrefab(this.HeldCardId).Icon;
		}
		base.MyGameCard.UpdateIcon();
	}

	[TimedAction("output_card")]
	public void OutputCard()
	{
		if (this.ResourceCount > 0)
		{
			CardData cardData = WorldManager.instance.CreateCard(base.Position, this.HeldCardId, faceUp: true, checkAddToStack: false);
			WorldManager.instance.StackSendCheckTarget(base.MyGameCard, cardData.MyGameCard, Vector3.right);
			this.ResourceCount--;
		}
	}

	public CardConnector GetOutputConnector()
	{
		CardConnector result = null;
		for (int i = 0; i < base.MyGameCard.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = base.MyGameCard.CardConnectorChildren[i];
			if (cardConnector != null && cardConnector.ConnectionType == ConnectionType.Transport && cardConnector.CardDirection == CardDirection.output)
			{
				result = cardConnector;
			}
		}
		return result;
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.HeldCardId))
		{
			CardData cardFromId = WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId);
			base.nameOverride = SokLoc.Translate(this.ChestTermOverride, LocParam.Create("resource", cardFromId.Name));
			if (base.MyGameCard.IsHovered)
			{
				base.descriptionOverride = SokLoc.Translate(this.ChestDescriptionLong, LocParam.Create("resource", cardFromId.Name), LocParam.Create("amount", this.ResourceCount.ToString()));
			}
		}
		else
		{
			base.nameOverride = null;
			base.descriptionOverride = null;
		}
	}

	public GameCard RemoveResources(int count)
	{
		count = Mathf.Min(count, this.ResourceCount);
		GameCard gameCard = WorldManager.instance.CreateCardStack(base.transform.position + Vector3.up * 0.2f, count, this.HeldCardId, checkAddToStack: false);
		WorldManager.instance.StackSend(gameCard.GetRootCard(), Vector3.right);
		this.ResourceCount -= count;
		return gameCard.GetRootCard();
	}

	public override void Clicked()
	{
		if (!base.IsDamaged)
		{
			int count = 1;
			if (InputController.instance.GetKey(Key.LeftShift) || InputController.instance.GetKey(Key.RightShift))
			{
				count = 5;
			}
			if (this.ResourceCount > 0)
			{
				this.RemoveResources(count);
			}
			if (this.ResourceCount == 0)
			{
				this.HeldCardId = null;
			}
			base.Clicked();
		}
	}
}
