public class Junction : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyCardType == CardType.Structures)
		{
			return false;
		}
		return true;
	}

	public bool AnyTransportConnected()
	{
		for (int i = 0; i < base.MyGameCard.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = base.MyGameCard.CardConnectorChildren[i];
			if (cardConnector.ConnectionType == ConnectionType.Transport && cardConnector.ConnectedNode != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild)
		{
			for (int num = base.MyGameCard.GetChildCards().Count - 1; num >= 0; num--)
			{
				GameCard gameCard = base.MyGameCard.GetChildCards()[num];
				gameCard.RemoveFromStack();
				if (this.AnyTransportConnected())
				{
					WorldManager.instance.StackSendCheckTarget(base.MyGameCard, gameCard, base.OutputDir);
				}
				else
				{
					gameCard.SendIt();
				}
			}
		}
		base.UpdateCard();
	}
}
