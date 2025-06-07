using UnityEngine;

public class StatusEffect_Radar : StatusEffect
{
	protected override string TermId => "radar";

	public override Sprite Sprite => SpriteManager.instance.RadarEffect;

	public override bool FadeInNonDefaultView => false;

	public override string Description => SokLoc.Translate("statuseffect_radar_description", LocParam.Create("amount", (CitiesManager.instance.NextConflictMonth - 1).ToString()));

	public override void Update()
	{
		bool flag = base.StatusTimer > 1f;
		base.FillAmount = (flag ? 1f : 0f);
		if (base.StatusTimer > 2f)
		{
			base.StatusTimer = 0f;
		}
		base.Update();
	}
}
