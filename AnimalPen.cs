using System.Collections.Generic;
using UnityEngine;

public class AnimalPen : CardData
{
	[Header("Animals")]
	public int MaxAnimalCount = 5;

	public bool IsForFish;

	private List<CardData> animals = new List<CardData>();

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (base.Id == "animal_cage" && otherCard.Id == "animal_cage")
		{
			return true;
		}
		if (otherCard.Id == "wheat")
		{
			return true;
		}
		if (otherCard.Id == "egg")
		{
			return true;
		}
		if (otherCard.Id == "magic_dust" || otherCard.Id == "soil")
		{
			return true;
		}
		int num = base.GetChildCount() + (1 + otherCard.GetChildCount());
		if (!this.IsForFish)
		{
			if (otherCard is Animal && otherCard.MyCardType != CardType.Fish)
			{
				return num <= this.MaxAnimalCount;
			}
			return false;
		}
		if (otherCard is Animal && otherCard.MyCardType == CardType.Fish)
		{
			return num <= this.MaxAnimalCount;
		}
		return false;
	}

	private Animal GetAnimalInStack()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Animal, this.animals);
		if (this.animals.Count == 0)
		{
			return null;
		}
		return this.animals.Choose() as Animal;
	}

	public override void UpdateCard()
	{
		if (base.AnyChildMatchesPredicate((CardData x) => x.Id == "wheat", out var _))
		{
			if (this.GetAnimalInStack() != null)
			{
				base.MyGameCard.StartTimer(5f, EatWheat, SokLoc.Translate("card_animal_eating_status"), "eat_wheat");
			}
		}
		else
		{
			base.MyGameCard.CancelTimer("eat_wheat");
			this.ShowFakeAnimalProgressBar();
		}
		base.UpdateCard();
	}

	private void ShowFakeAnimalProgressBar()
	{
		Animal firstAnimalToProduce = this.GetFirstAnimalToProduce();
		if (firstAnimalToProduce != null)
		{
			CardData cardPrefab = WorldManager.instance.GetCardPrefab(firstAnimalToProduce.CreateCard);
			string status = SokLoc.Translate("card_animal_pen_status", LocParam.Create("card", cardPrefab.Name), LocParam.Create("name", firstAnimalToProduce.Name));
			base.MyGameCard.StartTimer(firstAnimalToProduce.CreateTime, AnimalCreate, status, "animal_create");
			base.MyGameCard.CurrentTimerTime = firstAnimalToProduce.CreateTimer;
		}
		else
		{
			base.MyGameCard.CancelTimer("animal_create");
		}
	}

	[TimedAction("animal_create")]
	public void AnimalCreate()
	{
	}

	private Animal GetFirstAnimalToProduce()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Animal, this.animals);
		Animal result = null;
		float num = float.MaxValue;
		foreach (Animal animal in this.animals)
		{
			if (animal.CanCreate && animal.TimeUntilCreate < num)
			{
				num = animal.TimeUntilCreate;
				result = animal;
			}
		}
		return result;
	}

	[TimedAction("eat_wheat")]
	public void EatWheat()
	{
		Animal animalInStack = this.GetAnimalInStack();
		if (base.AnyChildMatchesPredicate((CardData x) => x.Id == "wheat", out var match) && animalInStack != null)
		{
			animalInStack.ConsumeWheat(match);
		}
	}
}
