using System.Collections.Generic;
using UnityEngine;

public class WickedWitch : Enemy
{
	public List<AudioClip> WitchDieSounds;

	public bool IsOldLady;

	public Sprite NormalIcon;

	public Sprite OldLadyIcon;

	public override void Die()
	{
		AudioManager.me.PlaySound2D(this.WitchDieSounds, Random.Range(1.1f, 1.3f), 0.5f);
		WorldManager.instance.CreateSmoke(base.MyGameCard.transform.position);
		QuestManager.instance.SpecialActionComplete("fight_wicked_witch");
		WorldManager.instance.CurrentRunVariables.FinishedWickedWitch = true;
		base.Die();
	}

	public override void UpdateCard()
	{
		base.Icon = (this.IsOldLady ? this.OldLadyIcon : this.NormalIcon);
		base.NameTerm = (this.IsOldLady ? "card_wicked_witch_name_2" : "card_wicked_witch_name");
		base.MyGameCard.UpdateIcon();
		base.UpdateCard();
		if (this.IsOldLady)
		{
			base.MyGameCard.SpecialValue = null;
		}
	}
}
