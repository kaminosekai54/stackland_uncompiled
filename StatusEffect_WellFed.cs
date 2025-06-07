using UnityEngine;

public class StatusEffect_WellFed : StatusEffect
{
	protected override string TermId => "wellfed";

	public override Sprite Sprite => SpriteManager.instance.WellFedEffect;

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
