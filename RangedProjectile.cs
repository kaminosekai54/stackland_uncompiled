using UnityEngine;

public class RangedProjectile : Projectile
{
	protected override void Update()
	{
		base.position += (base.TargetPosition - base.StartPosition).normalized * base.Speed * Time.deltaTime * WorldManager.instance.TimeScale;
		base.Update();
	}
}
