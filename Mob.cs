using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mob : Combatable
{
	private delegate bool DropFilter(string cardId);

	[HideInInspector]
	public float MoveTimer;

	[Header("Mob")]
	public float MoveTime = 3f;

	public bool IsAggressive;

	public bool AlwaysDrop;

	[HideInInspector]
	public Combatable CurrentTarget;

	public CardBag Drops;

	protected bool moveFlag;

	private List<Combatable> overlappingCombatables = new List<Combatable>();

	public virtual bool CanMove => base.MyGameCard.GetCardWithStatusInStack() == null;

	public override bool CanBeDragged => false;

	public override bool CanBePushedBy(CardData otherCard)
	{
		return otherCard is Mob;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == "bone" && base.Id == "wolf")
		{
			return true;
		}
		if (otherCard.Id == "milk" && base.Id == "feral_cat")
		{
			return true;
		}
		return base.AllCardsInStackMatchPred(otherCard, (CardData x) => x is Combatable { CanAttack: not false } combatable && !(combatable is Animal));
	}

	public override void UpdateCard()
	{
		this.moveFlag = false;
		bool flag = this.CanMove && !base.InConflict && this.MoveTime != -1f;
		if (base.MyGameCard.BounceTarget != null)
		{
			flag = false;
		}
		if (flag)
		{
			float num = ((this is Enemy) ? 2f : 1f);
			if (WorldManager.instance.CurrentBoard != null && WorldManager.instance.CurrentBoard.Id == "forest")
			{
				num = 3f;
			}
			this.MoveTimer += Time.deltaTime * WorldManager.instance.TimeScale * num;
			if (this.MoveTimer >= this.MoveTime)
			{
				this.moveFlag = true;
				this.MoveTimer = 0f;
				this.Move();
			}
		}
		else
		{
			this.MoveTimer = 0f;
		}
		if (this.IsAggressive && !base.InConflict && WorldManager.instance.TimeScale > 0f)
		{
			if (this.CurrentTarget == null || this.CurrentTarget.MyGameCard.MyBoard != base.MyGameCard.MyBoard)
			{
				this.CurrentTarget = this.FindTarget();
			}
			foreach (Combatable overlappingCombatable in this.GetOverlappingCombatables())
			{
				if (overlappingCombatable.InConflict)
				{
					overlappingCombatable.MyConflict.JoinConflict(this);
					break;
				}
				if (overlappingCombatable.Team != base.Team)
				{
					List<GameCard> list = (from x in overlappingCombatable.MyGameCard.GetAllCardsInStack()
						where x.Combatable != null
						select x).ToList();
					list.Add(base.MyGameCard);
					list = list.Distinct().ToList();
					WorldManager.instance.Restack(list);
					break;
				}
			}
		}
		base.UpdateCard();
	}

	public override void Die()
	{
		this.TryDropItems();
		base.Die();
	}

	public void TryDropItems()
	{
		List<GameCard> list = new List<GameCard>();
		bool flag = false;
		if (WorldManager.instance.CurrentRunVariables.CanDropItem)
		{
			flag = this.TryDropEquipment();
			if (flag)
			{
				Debug.Log("Dropped special equipment!");
				WorldManager.instance.CurrentRunVariables.CanDropItem = false;
			}
		}
		if (flag)
		{
			return;
		}
		Debug.Log("Dropped normal item!");
		if (this.Drops.CardBagType == CardBagType.Chances && this.Drops.Chances.Count == 0)
		{
			base.Die();
			return;
		}
		for (int i = 0; i < this.Drops.CardsInPack; i++)
		{
			ICardId cardFiltered = this.Drops.GetCardFiltered(Filter, removeCard: false);
			if (cardFiltered != null && !string.IsNullOrWhiteSpace(cardFiltered.Id))
			{
				CardData cardFromId = WorldManager.instance.GameDataLoader.GetCardFromId(cardFiltered.Id);
				if (cardFromId is Equipable)
				{
					WorldManager.instance.CurrentRunVariables.CanDropItem = false;
				}
				CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardFromId, faceUp: true, checkAddToStack: false);
				cardData.MyGameCard.SendIt();
				list.Add(cardData.MyGameCard);
			}
		}
		if (list.Count <= 0 || !WorldManager.instance.StackAllSame(list[0]))
		{
			return;
		}
		foreach (GameCard item in list)
		{
			item.Velocity = null;
		}
		WorldManager.instance.Restack(list);
		WorldManager.instance.StackSend(list[0], base.OutputDir);
	}

	private bool Filter(string cardId)
	{
		if (this.AlwaysDrop)
		{
			return true;
		}
		if (WorldManager.instance.GameDataLoader.GetCardFromId(cardId) is Equipable && !WorldManager.instance.CurrentRunVariables.CanDropItem)
		{
			return false;
		}
		if (WorldManager.instance.CurrentBoard != null && WorldManager.instance.CurrentBoard.Id == "forest")
		{
			return ForestCombatManager.instance.CanDropCard(cardId);
		}
		return true;
	}

	public bool TryDropEquipment()
	{
		if (!this.HasInventory)
		{
			return false;
		}
		List<Equipable> list = (from x in base.GetAllEquipables()
			orderby Random.value
			select x).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		List<Equipable> list2 = new List<Equipable>();
		foreach (Equipable item in list)
		{
			list2.Add(item);
		}
		list2.RemoveAll((Equipable x) => x.blueprint != null && WorldManager.instance.HasFoundCard(x.Id));
		if (list2.Count == 0)
		{
			return false;
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, list2[0], faceUp: true, checkAddToStack: false);
		if (cardData is Equipable equipable && equipable.blueprint != null)
		{
			WorldManager.instance.CreateCard(base.transform.position, equipable.blueprint, faceUp: true, checkAddToStack: false).MyGameCard.SetChild(cardData.MyGameCard);
		}
		cardData.MyGameCard.SendIt();
		return true;
	}

	public List<Combatable> GetOverlappingCombatables()
	{
		this.overlappingCombatables.Clear();
		foreach (GameCard overlappingCard in base.MyGameCard.GetOverlappingCards())
		{
			foreach (GameCard item in overlappingCard.GetAllCardsInStack())
			{
				if (item.Combatable != null && !item.BeingDragged)
				{
					this.overlappingCombatables.Add(item.Combatable);
				}
			}
		}
		return this.overlappingCombatables;
	}

	protected virtual void Move()
	{
		base.MyGameCard.SendIt();
	}

	protected virtual Combatable FindTarget()
	{
		if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			return WorldManager.instance.GetCard<CitiesCombatable>();
		}
		return WorldManager.instance.GetCard<BaseVillager>();
	}
}
