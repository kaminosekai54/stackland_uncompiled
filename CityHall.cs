using System.Linq;
using UnityEngine;

public class CityHall : Landmark
{
	[HideInInspector]
	[ExtraData("dollar_amount")]
	public int DollarAmount = 100;

	public static int DollarPerCardcap = 5;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is Dollar) && !(otherCard is Worker))
		{
			return otherCard is CitiesCombatable;
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && this.HasEnergyInput())
		{
			if (base.AllChildrenMatchPredicate((CardData x) => x is Dollar))
			{
				int num = (from x in base.MyGameCard.GetChildCards()
					select x.CardData into x
					where x is Dollar
					select x).Cast<Dollar>().Sum((Dollar x) => x.DollarValue);
				this.DollarAmount += num;
				base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x is Dollar, base.ChildrenMatchingPredicateCount((CardData x) => x is Dollar));
				QuestManager.instance.SpecialActionComplete("card_cap_increased");
				if (base.MyGameCard.HasChild)
				{
					GameCard child = base.MyGameCard.Child;
					child.RemoveFromParent();
					child.SendIt();
				}
			}
			else if (base.MyGameCard.GetChildCount() == 1 && (base.MyGameCard.Child.CardData is Worker || base.MyGameCard.Child.CardData is CitiesCombatable))
			{
				CardData card = null;
				CardData card2 = null;
				if ((base.HasCardOnTop(out card) || base.IsOnCard<CardData>(out card2)) && !GameCanvas.instance.ModalIsOpen)
				{
					CardData bs = ((card != null) ? card : card2);
					if (this.CanHaveCard(bs))
					{
						GameCanvas.instance.ShowNameCombatableModal(bs, delegate
						{
							bs.MyGameCard.RemoveFromStack();
							bs.MyGameCard.SendIt();
						});
					}
					else
					{
						bs.MyGameCard.RemoveFromStack();
					}
				}
			}
		}
		if (this.DollarAmount > 0)
		{
			base.descriptionOverride = SokLoc.Translate("card_city_hall_description_long", LocParam.Create("amount", this.DollarAmount.ToString()));
		}
		base.UpdateCard();
	}
}
