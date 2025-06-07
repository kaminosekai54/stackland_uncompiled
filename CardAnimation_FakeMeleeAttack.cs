using UnityEngine;

public class CardAnimation_FakeMeleeAttack : CardAnimation
{
	private GameCard startCard;

	private GameCard endCard;

	private bool attacked;

	public CardAnimation_FakeMeleeAttack(GameCard start, GameCard end)
	{
		this.startCard = start;
		this.endCard = end;
		base.StartPosition = this.startCard.Position;
		base.EndPosition = this.endCard.Position;
		base.Position = (base.TargetPosition = base.StartPosition);
	}

	public override void Update()
	{
		base.timer += Time.deltaTime * WorldManager.instance.CombatSpeed;
		float t = WorldManager.instance.CombatFlatPositionCurve.Evaluate(base.timer);
		float num = WorldManager.instance.CombatYPosition.Evaluate(base.timer);
		Vector3 zero = Vector3.zero;
		zero.x = Mathf.Lerp(base.StartPosition.x, base.EndPosition.x, t);
		zero.y = base.EndPosition.y + num;
		zero.z = Mathf.Lerp(base.StartPosition.z, base.EndPosition.z, t);
		base.Position = (base.TargetPosition = zero);
		if (base.timer >= 0.5f && !this.attacked)
		{
			this.attacked = true;
			this.endCard.SetHitEffect();
			AudioManager.me.PlaySound2D(AudioManager.me.HitMelee, Random.Range(0.8f, 1.2f), 0.2f);
		}
		if (base.timer >= 1f)
		{
			base.IsDone = true;
		}
	}
}
