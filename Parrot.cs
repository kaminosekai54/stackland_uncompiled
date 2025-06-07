public class Parrot : Animal
{
	public override void StoppedDragging()
	{
		if (base.MyGameCard.HasParent && base.MyGameCard.Parent.CardData.Id == "pirate")
		{
			CardData cardData = WorldManager.instance.ChangeToCard(base.MyGameCard.Parent, "friendly_pirate");
			base.MyGameCard.DestroyCard();
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
			QuestManager.instance.SpecialActionComplete("befriend_pirate");
		}
		else
		{
			base.StoppedDragging();
		}
	}
}
