using System.Collections.Generic;
using System.Linq;

public class TransmissionTower : CardData
{
	[ExtraData("has_energy")]
	public bool hasEnergy;

	private bool prevHasEnergy;

	public override bool HasEnergyOutput(CardConnector outputConnector, List<CardConnector> nodeTracker)
	{
		if (nodeTracker.Contains(outputConnector))
		{
			return false;
		}
		nodeTracker.Add(outputConnector);
		if (base.MyGameCard.CardConnectorChildren.Where((CardConnector x) => x.CardDirection == CardDirection.input).Count() <= 0)
		{
			this.hasEnergy = false;
		}
		if (outputConnector != null)
		{
			int num = base.MyGameCard.CardConnectorChildren.Where((CardConnector x) => x.CardDirection == CardDirection.output).ToList().IndexOf(outputConnector);
			List<CardConnector> list = base.MyGameCard.CardConnectorChildren.Where((CardConnector x) => x.CardDirection == CardDirection.input).ToList();
			if (num >= 0 && num < list.Count)
			{
				CardConnector cardConnector = list[num];
				if (cardConnector != null)
				{
					if (cardConnector.ConnectedNode != null)
					{
						this.hasEnergy = cardConnector.ConnectedNode.Parent.CardData.HasEnergyOutput(cardConnector, nodeTracker);
					}
					else
					{
						this.hasEnergy = false;
					}
				}
				else
				{
					this.hasEnergy = false;
				}
			}
			else
			{
				this.hasEnergy = false;
			}
		}
		else
		{
			this.hasEnergy = false;
		}
		if (this.hasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.hasEnergy;
		return this.hasEnergy;
	}

	public override bool HasEnergyInput(CardConnector inputConnector)
	{
		if (inputConnector != null && inputConnector.ConnectedNode != null && inputConnector.ConnectedNode.HasEnergyOutput())
		{
			return true;
		}
		return false;
	}
}
