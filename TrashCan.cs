using System.Collections.Generic;
using UnityEngine;

public class TrashCan : CardData
{
	public float DestroyTime;

	public List<AudioClip> DestroySounds;

	public override bool DetermineCanHaveCardsWhenIsRoot => true;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Curse || otherCard is Spirit)
		{
			return false;
		}
		if (otherCard is Royal || otherCard is Shaman || otherCard is Unhappiness || otherCard.Id == "royal_crown")
		{
			return false;
		}
		if (otherCard is Poop && WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			return false;
		}
		if (otherCard.MyCardType != CardType.Humans && otherCard.MyCardType != CardType.Mobs)
		{
			return otherCard.MyCardType != CardType.Fish;
		}
		return false;
	}

	public override bool CanBePushedBy(CardData otherCard)
	{
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (base.MyGameCard.HasChild)
		{
			base.MyGameCard.StartTimer(this.DestroyTime, DestroyChild, SokLoc.Translate("card_trash_can_status_0"), base.GetActionId("DestroyChild"));
		}
		else
		{
			base.MyGameCard.CancelTimer(base.GetActionId("DestroyChild"));
		}
		base.UpdateCard();
	}

	[TimedAction("destroy_child")]
	public void DestroyChild()
	{
		if (!base.MyGameCard.HasChild)
		{
			return;
		}
		foreach (GameCard childCard in base.MyGameCard.GetChildCards())
		{
			childCard.DestroyCard();
		}
		AudioManager.me.PlaySound2D(this.DestroySounds, Random.Range(1.2f, 1.3f), 0.1f);
		WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
	}
}
