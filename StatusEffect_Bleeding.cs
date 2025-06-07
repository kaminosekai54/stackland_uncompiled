using UnityEngine;

public class StatusEffect_Bleeding : StatusEffect
{
	[ExtraData("damage_timer")]
	public float DamageTimer;

	protected override string TermId => "bleeding";

	public override Sprite Sprite => SpriteManager.instance.BleedingEffect;

	public override void Update()
	{
		base.FillAmount = 1f - base.StatusTimer / 10f;
		this.DamageTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.DamageTimer >= 2f)
		{
			Combatable combatable = base.ParentCard as Combatable;
			if (combatable != null)
			{
				combatable.Damage(1);
				AudioManager.me.PlaySound2D(AudioManager.me.Bleed, Random.Range(0.8f, 1.2f), 0.2f);
				combatable.CreateHitText("1", PrefabManager.instance.BleedHitText);
			}
			this.DamageTimer = 0f;
		}
		if (base.StatusTimer >= 10f)
		{
			this.DamageTimer = 0f;
			base.StatusTimer = 0f;
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}
}
