using System;

[Serializable]
public class SpecialHit
{
	public float Chance = 1f;

	public SpecialHitType HitType;

	public SpecialHitTarget Target;

	public string GetText()
	{
		string value = SokLoc.Translate("target_" + this.Target.ToString().ToLower());
		return SokLoc.Translate("specialhit_" + this.HitType.ToString().ToLower() + "_long", LocParam.Create("chance", this.Chance.ToString()), LocParam.Create("target", value));
	}

	public bool IsDebuff()
	{
		if (this.HitType == SpecialHitType.Poison || this.HitType == SpecialHitType.Stun || this.HitType == SpecialHitType.LifeSteal || this.HitType == SpecialHitType.Bleeding || this.HitType == SpecialHitType.Damage || this.HitType == SpecialHitType.Crit || this.HitType == SpecialHitType.Sick || this.HitType == SpecialHitType.Anxious)
		{
			return true;
		}
		return false;
	}
}
