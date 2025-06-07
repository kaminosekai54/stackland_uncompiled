using UnityEngine;

public class StatusEffect_Anxious : StatusEffect
{
	protected override string TermId => "anxious";

	public override Sprite Sprite => SpriteManager.instance.AnxiousEffect;

	public override void Update()
	{
		base.FillAmount = 1f - base.StatusTimer / WorldManager.instance.MonthTime;
		if (base.StatusTimer >= WorldManager.instance.MonthTime)
		{
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}
}
