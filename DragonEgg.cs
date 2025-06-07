using UnityEngine;

public class DragonEgg : CardData
{
	public int CrackedState;

	public Sprite NormalIcon;

	public Sprite CrackedIcon;

	public Sprite CrackedIcon_2;

	public AudioClip CrackedSound;

	public AudioClip CrackedSound2;

	public override void UpdateCard()
	{
		base.Icon = this.NormalIcon;
		if (this.CrackedState == 1)
		{
			base.Icon = this.CrackedIcon;
		}
		if (this.CrackedState == 2)
		{
			base.Icon = this.CrackedIcon_2;
		}
		base.NameTerm = ((this.CrackedState == 0) ? "card_dragon_egg_name" : "card_dragon_egg_name_cracked");
		base.MyGameCard.UpdateIcon();
		base.UpdateCard();
	}
}
