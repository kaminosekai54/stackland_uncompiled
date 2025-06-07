using UnityEngine;

public class StatusEffect_Stunned : StatusEffect
{
	protected override string TermId => "stunned";

	public override Sprite Sprite => SpriteManager.instance.StunnedEffect;

	public override void Update()
	{
		base.FillAmount = 1f - base.StatusTimer / 5f;
		if (base.StatusTimer >= 5f)
		{
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}
}
