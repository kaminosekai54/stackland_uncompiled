using UnityEngine;

public class ResourceMagnet : CardData
{
	[ExtraData("resource_id")]
	[HideInInspector]
	public string PullCardId;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if ((otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Food || otherCard is Animal) && (otherCard.Id == this.PullCardId || !base.MyGameCard.HasChild))
		{
			return true;
		}
		return base.CanHaveCard(otherCard);
	}

	public override void Clicked()
	{
		this.PullCardId = null;
	}

	protected override bool CanToggleOnOff()
	{
		if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			return true;
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild && (string.IsNullOrEmpty(this.PullCardId) || this.PullCardId != base.MyGameCard.Child.CardData.Id))
		{
			this.PullCardId = base.MyGameCard.Child.CardData.Id;
		}
		if (!string.IsNullOrEmpty(this.PullCardId))
		{
			base.nameOverride = SokLoc.Translate("card_resource_magnet_name_override", LocParam.Create("resource", WorldManager.instance.GameDataLoader.GetCardFromId(this.PullCardId).Name));
			base.descriptionOverride = SokLoc.Translate("card_resource_magnet_description_long", LocParam.Create("resource", WorldManager.instance.GameDataLoader.GetCardFromId(this.PullCardId).Name));
		}
		else
		{
			base.nameOverride = SokLoc.Translate("card_resource_magnet_name");
			base.descriptionOverride = null;
		}
		base.UpdateCard();
		if (string.IsNullOrEmpty(this.PullCardId))
		{
			base.Icon = SpriteManager.instance.EmptyTexture;
		}
		else
		{
			base.Icon = WorldManager.instance.GetCardPrefab(this.PullCardId).Icon;
		}
		base.MyGameCard.UpdateIcon();
	}
}
