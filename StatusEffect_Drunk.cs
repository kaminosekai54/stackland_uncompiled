using UnityEngine;

public class StatusEffect_Drunk : StatusEffect
{
	protected override string TermId => "drunk";

	public override Sprite Sprite => SpriteManager.instance.DrunkEffect;

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
