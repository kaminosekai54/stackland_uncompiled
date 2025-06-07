using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_AddWellbeing : CardRequirementResult
{
	public int Amount;

	public bool IsNegative;

	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.WellBeing;
	}

	public override IEnumerator Perform(GameCard card)
	{
		CitiesManager.instance.AddWellbeing(this.Amount);
		card.CardData.UpdateRequirementResultsInStack(RequirementType.WellBeing, this.Amount, card);
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Wellbeing}</nobr></color>";
		}
		return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Wellbeing}</nobr></color>";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Wellbeing}</nobr></color>";
		}
		return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Wellbeing}</nobr></color>";
	}
}
