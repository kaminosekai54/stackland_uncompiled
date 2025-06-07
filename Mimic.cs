using UnityEngine;

public class Mimic : Enemy
{
	public Sprite TreasureChestIcon;

	public Sprite RealIcon;

	[ExtraData("was_detected")]
	public bool WasDetected;

	public override bool CanBeDragged => !this.WasDetected;

	public override void Clicked()
	{
		this.Detected();
		base.Clicked();
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.IsDemoCard)
		{
			this.Detected();
		}
		if (!this.WasDetected && (base.InConflict || base.MyGameCard.BeingDragged))
		{
			this.Detected();
		}
		base.UpdateCard();
		base.Icon = (this.WasDetected ? this.RealIcon : this.TreasureChestIcon);
		base.MyGameCard.UpdateIcon();
		base.nameOverride = (this.WasDetected ? SokLoc.Translate("card_mimic_name") : SokLoc.Translate("card_treasure_chest_name"));
		if (!this.WasDetected)
		{
			base.descriptionOverride = SokLoc.Translate("card_treasure_chest_description");
		}
		if (!this.WasDetected)
		{
			base.MyGameCard.SpecialValue = null;
		}
	}

	private void Detected()
	{
		if (!this.WasDetected)
		{
			if (!base.MyGameCard.IsDemoCard)
			{
				WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
			}
			base.MyGameCard.UpdateCardPalette();
			this.WasDetected = true;
		}
	}
}
