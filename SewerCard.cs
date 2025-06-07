using UnityEngine;

public class SewerCard : CardData
{
	[ExtraData("poop_timer")]
	[HideInInspector]
	public float PoopTimer;

	private bool shouldRunTimer;

	private bool HasSewerConnector()
	{
		foreach (CardConnectorData energyConnector in base.EnergyConnectors)
		{
			if (energyConnector.EnergyConnectionStrength == ConnectionType.Sewer)
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (this.HasSewerConnector())
		{
			if (!base.HasSewerConnected())
			{
				if (!base.HasStatusEffectOfType<StatusEffect_NoSewer>())
				{
					base.AddStatusEffect(new StatusEffect_NoSewer());
				}
				this.shouldRunTimer = true;
			}
			else
			{
				base.RemoveStatusEffect<StatusEffect_NoSewer>();
				this.shouldRunTimer = false;
			}
		}
		this.CheckSpawnPoop();
		base.UpdateCard();
	}

	[TimedAction("check_spawn_poop")]
	public void CheckSpawnPoop()
	{
		if (this.shouldRunTimer)
		{
			this.PoopTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		}
		if (this.PoopTimer >= 30f && (double)Random.value > 0.5)
		{
			CardData cardData = WorldManager.instance.CreateCard(base.Position, "poop");
			WorldManager.instance.StackSend(cardData.MyGameCard, base.OutputDir);
			this.PoopTimer = 0f;
		}
	}
}
