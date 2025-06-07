using UnityEngine;

public class StatusEffect_Frenzy : StatusEffect
{
	[ExtraData("frenzy_timer")]
	public float FrenzyTimer;

	protected override string TermId => "frenzy";

	public override Sprite Sprite => SpriteManager.instance.FrenzyEffect;

	public override void Update()
	{
		base.FillAmount = 1f - this.FrenzyTimer / 10f;
		this.FrenzyTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.FrenzyTimer >= 10f)
		{
			this.FrenzyTimer = 0f;
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}
}
