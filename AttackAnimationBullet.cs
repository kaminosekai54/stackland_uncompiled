using UnityEngine;

public class AttackAnimationBullet : AttackAnimation
{
	public override void Start()
	{
		Projectile projectile = base.Origin.CreateProjectile(PrefabManager.instance.BulletProjectilePrefab, base.Target, this);
		this.SetKnockback(projectile);
		AudioManager.me.PlaySound2D(AudioManager.me.RangedRelease, Random.Range(0.8f, 1.2f), 0.3f);
		base.Start();
	}

	public override void Update()
	{
		base.Position = (base.TargetPosition = base.AttackStartPosition + base.knockback);
		base.Update();
	}
}
