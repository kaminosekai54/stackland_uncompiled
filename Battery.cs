using UnityEngine;

public class Battery : CardData, IEnergy
{
	public int EnergyCapacity = 50;

	[ExtraData("stored_energy")]
	public int StoredEnergy;

	public Sprite SpecialIcon;

	public int EnergyAmount => this.StoredEnergy;

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Energy;
	}

	public override void UpdateCard()
	{
		base.MyGameCard.SpecialIcon.sprite = this.SpecialIcon;
		base.MyGameCard.SpecialValue = this.StoredEnergy;
		if (base.MyGameCard.HasChild)
		{
			foreach (GameCard childCard in base.MyGameCard.GetChildCards())
			{
				if (this.StoredEnergy < this.EnergyCapacity)
				{
					this.StoredEnergy++;
					childCard.DestroyCard(spawnSmoke: true);
					continue;
				}
				childCard.RemoveFromParent();
				break;
			}
		}
		base.UpdateCard();
	}

	public void UseEnergy(int energyAmount)
	{
		this.StoredEnergy -= energyAmount;
		WorldManager.instance.CreateMinusElectricity(base.Position);
	}

	public CardData GetCardData()
	{
		return this;
	}
}
