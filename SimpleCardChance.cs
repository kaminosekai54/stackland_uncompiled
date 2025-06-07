using System;

[Serializable]
public class SimpleCardChance
{
	[Card]
	public string CardId;

	public int Chance = 1;

	public SimpleCardChance(string cardId, int chance)
	{
		this.CardId = cardId;
		this.Chance = chance;
	}

	public SimpleCardChance()
	{
	}
}
