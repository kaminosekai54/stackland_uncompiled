using System.Collections;
using UnityEngine;

public class Altar : CardData
{
	public bool inCutscene;

	public AudioClip AltarActive;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard.Id == "charcoal") && !(otherCard.Id == "raw_meat"))
		{
			return otherCard.Id == "gold";
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (!this.inCutscene)
		{
			if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "charcoal") >= 1)
			{
				this.TryStartSpiritCutscene(Cutscenes.AltarIntro(this, CurseType.Happiness));
			}
			else if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "raw_meat") >= 1)
			{
				this.TryStartSpiritCutscene(Cutscenes.AltarIntro(this, CurseType.Death));
			}
			else if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "gold") >= 1)
			{
				this.TryStartSpiritCutscene(Cutscenes.AltarIntro(this, CurseType.Greed));
			}
		}
		base.UpdateCard();
	}

	private void TryStartSpiritCutscene(IEnumerator cutscene)
	{
		if (WorldManager.instance.IsSpiritDlcActive())
		{
			this.inCutscene = true;
			AudioManager.me.PlaySound2D(this.AltarActive, 1f, 0.2f);
			WorldManager.instance.QueueCutscene(cutscene);
		}
		else
		{
			GameCanvas.instance.ShowDlcNotInstalledModal();
			base.MyGameCard.Child.RemoveFromParent();
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}
}
