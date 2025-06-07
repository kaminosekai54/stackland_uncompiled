using UnityEngine;

public class Animal : Mob
{
	[Header("Animal")]
	public float CreateTime = 10f;

	[Card]
	public string CreateCard;

	public bool IsBreedable;

	[ExtraData("age")]
	public int Age;

	public bool IsOld;

	[ExtraData("createtimer")]
	[HideInInspector]
	public float CreateTimer;

	[ExtraData("itemscreated")]
	[HideInInspector]
	public int ItemsCreated;

	public override bool CanBeDragged => true;

	public override bool CanMove
	{
		get
		{
			if (!this.InAnimalPen && !base.MyGameCard.HasParent && !base.MyGameCard.HasChild)
			{
				return base.MyGameCard.GetCardWithStatusInStack() == null;
			}
			return false;
		}
	}

	public AnimalPen RootPen
	{
		get
		{
			if (!base.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as AnimalPen;
		}
	}

	public BreedingPen RootBreedingPen
	{
		get
		{
			if (!base.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as BreedingPen;
		}
	}

	public ResourceMagnet RootMagnet
	{
		get
		{
			if (!base.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as ResourceMagnet;
		}
	}

	public SlaughterHouse RootSlaughterHouse
	{
		get
		{
			if (!base.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as SlaughterHouse;
		}
	}

	public PettingZoo RootPettingZoo
	{
		get
		{
			if (!base.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as PettingZoo;
		}
	}

	private CardData rootStructure
	{
		get
		{
			GameCard gameCard = base.MyGameCard.GetRootCard();
			if (gameCard.CardData is HeavyFoundation && gameCard.HasChild)
			{
				gameCard = gameCard.Child;
			}
			return gameCard.CardData;
		}
	}

	public bool InAnimalPen
	{
		get
		{
			if (!(this.RootPen != null) && !(this.RootBreedingPen != null) && !(this.RootMagnet != null) && !(this.RootSlaughterHouse != null))
			{
				return this.RootPettingZoo != null;
			}
			return true;
		}
	}

	public virtual bool CanCreate
	{
		get
		{
			if (string.IsNullOrEmpty(this.CreateCard))
			{
				return false;
			}
			if (base.InConflict)
			{
				return false;
			}
			return true;
		}
	}

	public float TimeUntilCreate
	{
		get
		{
			if (!this.CanCreate)
			{
				return -1f;
			}
			return this.CreateTime - this.CreateTimer;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is NamingStone)
		{
			return true;
		}
		if (otherCard is Animal)
		{
			return false;
		}
		if (otherCard.Id == "wheat")
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(base.CustomName))
		{
			if (this.IsOld)
			{
				base.nameOverride = SokLoc.Translate("card_animal_old_name", LocParam.Create("name", base.CustomName));
			}
			else
			{
				base.nameOverride = base.CustomName;
			}
		}
		else if (this.IsOld)
		{
			base.nameOverride = SokLoc.Translate(base.NameTerm + "_old");
		}
		else
		{
			base.nameOverride = SokLoc.Translate(base.NameTerm);
		}
		base.UpdateCardText();
	}

	[TimedAction("eat_wheat")]
	private void EatWheat()
	{
		if (base.HasCardOnTop("wheat", out var cardData))
		{
			this.ConsumeWheat(cardData);
		}
	}

	public void ConsumeWheat(CardData wheat)
	{
		base.MyGameCard.GetRootCard().CardData.RestackChildrenMatchingPredicate((CardData x) => x == wheat);
		wheat.MyGameCard.DestroyCard();
		this.CreateTimer = 0f;
		this.TryCreateItem();
		base.MyGameCard.RotWobble(1f);
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
		if (this.CanCreate)
		{
			this.CreateTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		}
		if (this.CreateTimer >= this.CreateTime && (base.moveFlag || this.InAnimalPen || base.MyGameCard.GetRootCard().CardData is HeavyFoundation))
		{
			this.CreateTimer -= this.CreateTime;
			this.TryCreateItem();
		}
		if (base.HasCardOnTop("wheat", out var _) && !this.InAnimalPen)
		{
			base.MyGameCard.StartTimer(5f, EatWheat, SokLoc.Translate("card_animal_eating_status"), "eat_wheat");
		}
		else
		{
			base.MyGameCard.CancelTimer("eat_wheat");
		}
		if (!base.MyGameCard.BeingDragged && !this.InAnimalPen && !base.InConflict && base.HasCardOnTop(out Animal card))
		{
			card.MyGameCard.RemoveFromStack();
		}
	}

	private void TryCreateItem()
	{
		if (!this.CanCreate)
		{
			return;
		}
		CardData cardData = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, this.CreateCard, faceUp: true, checkAddToStack: false);
		if (this.RootBreedingPen != null || this.RootSlaughterHouse != null || this.RootPen != null)
		{
			WorldManager.instance.StackSendCheckTarget(this.rootStructure.MyGameCard, cardData.MyGameCard, base.OutputDir);
		}
		else
		{
			WorldManager.instance.StackSend(cardData.MyGameCard, base.OutputDir);
		}
		this.ItemsCreated++;
		if (WorldManager.instance.CurseIsActive(CurseType.Death) && this.ItemsCreated % 4 == 0 && this.CreateCard != "poop")
		{
			CardData cardData2 = WorldManager.instance.CreateCard(base.MyGameCard.transform.position, "poop", faceUp: true, checkAddToStack: false);
			if (this.RootBreedingPen != null || this.RootSlaughterHouse != null || this.RootPen != null)
			{
				WorldManager.instance.StackSendCheckTarget(this.rootStructure.MyGameCard, cardData2.MyGameCard, base.OutputDir);
			}
			else
			{
				WorldManager.instance.StackSend(cardData2.MyGameCard, base.OutputDir);
			}
		}
	}

	public override void Clicked()
	{
		if (!this.InAnimalPen)
		{
			if (!base.MyGameCard.Velocity.HasValue)
			{
				base.MoveTimer = base.MoveTime;
			}
			base.Clicked();
		}
	}

	protected override void Move()
	{
		AudioManager.me.PlaySound2D(AudioManager.me.AnimalMove, Random.Range(0.8f, 1.2f), 0.2f);
		base.Move();
	}

	public override void Die()
	{
		if (!base.IsAggressive)
		{
			WorldManager.instance.TryCreateUnhappiness(base.transform.position, 2);
		}
		base.Die();
	}

	private bool CanGrowOld()
	{
		return WorldManager.instance.CurseIsActive(CurseType.Death);
	}
}
