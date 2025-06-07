using System;
using System.Collections;
using System.Linq;

[Serializable]
public class CutsceneStep_FocusCamera : CutsceneStep
{
	[Card]
	public string CardId;

	public bool FocusType;

	public CardType Type;

	public override IEnumerator Process()
	{
		if (!this.FocusType && !string.IsNullOrEmpty(this.CardId))
		{
			GameCamera.instance.TargetCardOverride = WorldManager.instance.GetCard(this.CardId);
		}
		else if (this.FocusType)
		{
			GameCamera.instance.TargetCardOverride = WorldManager.instance.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id).FirstOrDefault((GameCard x) => x.CardData.MyCardType == this.Type);
		}
		yield break;
	}
}
