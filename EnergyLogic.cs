using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergyLogic : CardData
{
	[ExtraData("has_energy")]
	[HideInInspector]
	public bool HasEnergy;

	private bool prevHasEnergy;

	public override bool HasEnergyOutput(CardConnector connectedNode, List<CardConnector> nodeTracker)
	{
		if (nodeTracker.Contains(connectedNode))
		{
			return false;
		}
		nodeTracker.Add(connectedNode);
		if (base.MyGameCard.CardConnectorChildren.Where((CardConnector x) => x.CardDirection == CardDirection.input).Count() <= 0)
		{
			this.HasEnergy = false;
		}
		if (base.MyGameCard.CardConnectorChildren.Where((CardConnector x) => x.CardDirection == CardDirection.input).All((CardConnector x) => x.ConnectedNode != null && x.ConnectedNode.Parent.CardData.HasEnergyOutput(x.ConnectedNode, nodeTracker)))
		{
			this.HasEnergy = true;
		}
		else
		{
			this.HasEnergy = false;
		}
		if (this.HasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.HasEnergy;
		return this.HasEnergy;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return false;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}
}
