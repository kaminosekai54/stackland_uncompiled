public class Bone : Resource
{
	public override void StoppedDragging()
	{
		if (base.MyGameCard.HasParent && base.MyGameCard.Parent.CardData.Id == "wolf")
		{
			base.MyGameCard.Parent.DestroyCard();
			base.MyGameCard.DestroyCard();
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "dog");
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
		}
		else
		{
			base.StoppedDragging();
		}
	}
}
