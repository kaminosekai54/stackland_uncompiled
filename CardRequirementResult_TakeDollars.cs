using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_TakeDollars : CardRequirementResult
{
	public int Amount;

	public bool IsNegative;

	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Dollar;
	}

	public override IEnumerator Perform(GameCard card)
	{
		AudioManager.me.PlaySound2D(AudioManager.me.Dollar, 1f, 0.5f);
		CitiesManager.instance.TryUseDollars(WorldManager.instance.GetCardsImplementingInterface<ICurrency>(), this.Amount, onlyTakeIfAmountMet: true);
		card.CardData.UpdateRequirementResultsInStack(RequirementType.Dollar, -this.Amount, card);
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed)}>-{this.Amount * multiplier}{Icons.Dollar}</nobr></color>";
		}
		return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess)}>-{this.Amount * multiplier}{Icons.Dollar}</nobr></color>";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed)}>-{this.Amount * multiplier}{Icons.Dollar}</nobr></color>";
		}
		return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess)}>-{this.Amount * multiplier}{Icons.Dollar}</nobr></color>";
	}
}
