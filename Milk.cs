public class Milk : Food
{
	public override void StoppedDragging()
	{
		if (base.MyGameCard.HasParent && base.MyGameCard.Parent.CardData.Id == "feral_cat" && WorldManager.instance.IsSpiritDlcActive())
		{
			base.MyGameCard.Parent.DestroyCard();
			base.MyGameCard.DestroyCard();
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "cat");
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
		}
		else
		{
			base.StoppedDragging();
		}
	}
}
