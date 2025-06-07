using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creditcard : CardData, ICurrency
{
	[ExtraData("dollar_count")]
	[HideInInspector]
	public int DollarCount;

	public int MaxDollarCount = 1000;

	public string BankDescriptionTerm;

	public CardData Card => this;

	public int CurrencyValue
	{
		get
		{
			return this.DollarCount;
		}
		set
		{
			this.DollarCount = value;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Dollar || otherCard.Id == base.Id)
		{
			return true;
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (base.IsDamaged)
		{
			base.UpdateCard();
			return;
		}
		List<Dollar> list = (from x in base.MyGameCard.GetChildCards()
			where x.CardData is Dollar
			select x.CardData as Dollar).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			GameCard myGameCard = list[i].MyGameCard;
			if (myGameCard.CardData is Dollar dollar)
			{
				Creditcard creditcardWithSpace = this.GetCreditcardWithSpace();
				if (creditcardWithSpace != null)
				{
					int num = creditcardWithSpace.MaxDollarCount - creditcardWithSpace.DollarCount;
					if (num > 0)
					{
						if (dollar.DollarValue > num)
						{
							int value = dollar.DollarValue - num;
							creditcardWithSpace.DollarCount = creditcardWithSpace.MaxDollarCount;
							myGameCard.DestroyCard();
							list.AddRange(from x in WorldManager.instance.CreateDollarsFromValue(value, base.Position)
								select x.CardData as Dollar);
						}
						else
						{
							creditcardWithSpace.DollarCount += dollar.DollarValue;
							myGameCard.DestroyCard();
						}
						if (myGameCard.CardData == list.Last())
						{
							WorldManager.instance.CreateSmoke(base.Position);
						}
					}
				}
				else
				{
					myGameCard.RemoveFromParent();
				}
			}
			WorldManager.instance.Restack(list.Select((Dollar x) => x.MyGameCard).ToList());
		}
		base.CitiesValue = this.DollarCount;
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		GameCard myGameCard = base.MyGameCard;
		if ((object)myGameCard != null && myGameCard.CardConnectorChildren.Count > 0 && base.MyGameCard.IsHovered)
		{
			base.descriptionOverride = SokLoc.Translate(this.BankDescriptionTerm, LocParam.Create("count", this.DollarCount.ToString()), LocParam.Create("max_count", this.MaxDollarCount.ToString()), LocParam.Create("icon", Icons.Dollar));
			base.descriptionOverride = base.descriptionOverride + "\n\n<i>" + base.GetConnectorInfoString(base.MyGameCard) + "</i>";
		}
	}

	private Creditcard GetCreditcardWithSpace()
	{
		GameCard gameCard = base.MyGameCard.GetAllCardsInStack().FirstOrDefault((GameCard x) => x.CardData is Creditcard creditcard && creditcard.DollarCount < creditcard.MaxDollarCount);
		if (gameCard == null)
		{
			return null;
		}
		return gameCard.CardData as Creditcard;
	}

	public override void Clicked()
	{
		if (this.DollarCount > 0)
		{
			int num = Mathf.Min(this.DollarCount, 100);
			WorldManager.instance.CreateDollarsFromValue(num, base.Position, checkAddToStack: false);
			this.DollarCount -= num;
			WorldManager.instance.CreateSmoke(base.Position);
		}
	}

	public void UseCurrency(int currencyAmount, bool spawnSmoke = false)
	{
		if (spawnSmoke)
		{
			WorldManager.instance.CreateSmoke(base.Position);
		}
		this.DollarCount -= currencyAmount;
	}
}
