using UnityEngine;

public class AttackAnimationMagic : AttackAnimation
{
	public override void Start()
	{
		base.Origin.CreateProjectile(PrefabManager.instance.MagicProjectilePrefab, base.Target, this);
		AudioManager.me.PlaySound2D(AudioManager.me.MagicCharge, Random.Range(0.8f, 1.2f), 0.5f);
		base.Start();
	}

	public override void Update()
	{
		base.Position = (base.TargetPosition = base.AttackStartPosition + base.knockback);
		base.Update();
	}
}
