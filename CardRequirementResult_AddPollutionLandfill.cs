using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_AddPollutionLandfill : CardRequirementResult
{
	public int Amount;

	public bool IsNegative;

	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Pollution;
	}

	public override IEnumerator Perform(GameCard card)
	{
		Landfill landfill = card.CardData as Landfill;
		if (landfill != null && landfill.StoredPollution >= this.Amount)
		{
			landfill.StoredPollution += this.Amount;
		}
		card.CardData.UpdateRequirementResultsInStack(RequirementType.Pollution, -this.Amount, card);
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Pollution}</nobr></color>";
		}
		return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Pollution}</nobr></color>";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Pollution}</nobr></color>";
		}
		return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess)}><nobr>{CitiesManager.GetAmountPrefix(this.Amount)}{this.Amount * multiplier}{Icons.Pollution}</nobr></color>";
	}
}
