using UnityEngine;

public class AttackAnimationMelee : AttackAnimation
{
	private bool attacked;

	private float timer;

	public override bool IsBlocking => true;

	public override void Update()
	{
		this.timer += Time.deltaTime * WorldManager.instance.TimeScale * WorldManager.instance.CombatSpeed;
		float t = WorldManager.instance.CombatFlatPositionCurve.Evaluate(this.timer);
		float num = WorldManager.instance.CombatYPosition.Evaluate(this.timer);
		Vector3 zero = Vector3.zero;
		zero.x = Mathf.Lerp(base.AttackStartPosition.x, base.AttackTargetPosition.x, t);
		zero.y = base.AttackTargetPosition.y + num;
		zero.z = Mathf.Lerp(base.AttackStartPosition.z, base.AttackTargetPosition.z, t);
		base.Position = (base.TargetPosition = zero);
		if (this.timer >= 0.5f && !this.attacked)
		{
			this.attacked = true;
			base.Origin.PerformAttack(base.Target, base.AttackTargetPosition);
		}
		if (this.timer >= 1f)
		{
			base.IsDone = true;
		}
		base.Update();
	}
}
