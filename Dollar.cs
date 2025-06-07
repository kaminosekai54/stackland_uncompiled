public class Dollar : Resource, ICurrency
{
	public int DollarValue;

	public CardData Card => this;

	public int CurrencyValue
	{
		get
		{
			return this.DollarValue;
		}
		set
		{
			this.DollarValue = value;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is Dollar) && !(otherCard is Worker))
		{
			return otherCard is Resource;
		}
		return true;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.nameOverride = SokLoc.Translate(base.NameTerm, LocParam.Create("icon", Icons.Dollar));
		base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm, LocParam.Create("icon", Icons.Dollar));
	}

	public void UseCurrency(int currencyAmount, bool spawnSmoke = false)
	{
		if (spawnSmoke)
		{
			WorldManager.instance.CreateSmoke(base.Position);
		}
		base.MyGameCard.DestroyCard();
	}
}
