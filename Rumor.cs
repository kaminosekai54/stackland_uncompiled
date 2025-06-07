public class Rumor : CardData, IKnowledge
{
	public BlueprintGroup KnowledgeGroup;

	public BlueprintGroup Group => this.KnowledgeGroup;

	public string CardId => base.Id;

	public string KnowledgeName => base.FullName;

	public string KnowledgeText => base.Description;

	public bool IsIslandKnowledge
	{
		get
		{
			if (this.KnowledgeGroup != BlueprintGroup.Island)
			{
				return this.KnowledgeGroup == BlueprintGroup.Sailing;
			}
			return true;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard is Blueprint))
		{
			return otherCard is Rumor;
		}
		return true;
	}
}
