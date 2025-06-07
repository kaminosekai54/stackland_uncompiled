public class CardReferenceBooster : ICardReference
{
	public string BoosterId;

	public string ReferencedCardId { get; set; }

	public CardReferenceBooster(string referencedCardId, string boosterId)
	{
		this.ReferencedCardId = referencedCardId;
		this.BoosterId = boosterId;
	}

	public string GetKey()
	{
		return "booster_" + this.BoosterId + "_" + this.ReferencedCardId;
	}

	public override string ToString()
	{
		return "booster " + this.BoosterId;
	}
}
