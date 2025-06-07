public class Demon : Enemy
{
	public override void Die()
	{
		if (base.Id == "demon")
		{
			WorldManager.instance.QueueCutscene(Cutscenes.BossFightComplete(this));
			WorldManager.instance.CurrentRunVariables.FinishedDemon = true;
			GameScreen.instance.UpdateQuestLog();
		}
		else
		{
			WorldManager.instance.QueueCutscene(Cutscenes.BossFight2Complete(this));
			WorldManager.instance.CurrentRunVariables.FinishedDemonLord = true;
		}
		base.TryDropItems();
	}
}
