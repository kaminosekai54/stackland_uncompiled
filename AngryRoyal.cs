public class AngryRoyal : Enemy
{
	public override void Die()
	{
		WorldManager.instance.QueueCutscene(GreedCutscenes.KillRoyalLiftCurse());
		base.Die();
	}

	public void DieInCutscene()
	{
		WorldManager.instance.CreateCard(base.transform.position, "royal_crown");
		WorldManager.instance.CreateSmoke(base.transform.position);
		base.RemoveAllStatusEffects();
		WorldManager.instance.ChangeToCard(base.MyGameCard, "corpse");
	}
}
