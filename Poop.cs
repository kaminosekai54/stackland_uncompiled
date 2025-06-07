using UnityEngine;

public class Poop : CardData
{
	public float SickChance = 20f;

	public AudioClip PoopSound;

	public bool CanMakeSick
	{
		get
		{
			bool result = true;
			if (base.MyGameCard.Parent != null && base.MyGameCard.Parent.CardData is Cesspool)
			{
				result = false;
			}
			if (base.MyGameCard.CardData.CreationMonth == WorldManager.instance.CurrentMonth)
			{
				result = false;
			}
			return result;
		}
	}

	public override void OnInitialCreate()
	{
		CardData nearestCardMatchingPred = WorldManager.instance.GetNearestCardMatchingPred(base.MyGameCard, (GameCard x) => x.CardData.Id == "sewer");
		if (nearestCardMatchingPred != null)
		{
			WorldManager.instance.StackSendTo(base.MyGameCard, nearestCardMatchingPred.MyGameCard);
		}
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyCardType != CardType.Resources && otherCard.MyCardType != CardType.Humans && otherCard.MyCardType != CardType.Food && !(otherCard.Id == base.Id))
		{
			if (otherCard.MyCardType == CardType.Structures)
			{
				return !otherCard.IsBuilding;
			}
			return false;
		}
		return true;
	}

	public override void UpdateCardText()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm) + "\n\n<i>" + SokLoc.Translate("card_poop_cant_sell") + "</i>";
		}
		else
		{
			base.descriptionOverride = null;
		}
		base.UpdateCardText();
	}

	public override void UpdateCard()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			base.Value = -1;
		}
		else
		{
			base.Value = 1;
		}
		base.UpdateCard();
	}
}
