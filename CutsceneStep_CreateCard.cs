using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CutsceneStep_CreateCard : CutsceneStep
{
	public enum SpawnLocation
	{
		Random,
		MiddleOfBoard,
		AtCard,
		AtFocussed
	}

	[Card]
	public string CardId;

	public SpawnLocation Location;

	[Card]
	public string OtherCardId;

	[Header("Options")]
	public bool FindOrCreate;

	public bool SendCard;

	public bool MakeSmoke;

	public override IEnumerator Process()
	{
		if (!this.FindOrCreate || !(WorldManager.instance.GetCard(this.CardId) != null))
		{
			Vector3 vector = Vector3.zero;
			if (this.Location == SpawnLocation.MiddleOfBoard)
			{
				vector = WorldManager.instance.MiddleOfBoard();
			}
			else if (this.Location == SpawnLocation.Random)
			{
				vector = WorldManager.instance.GetRandomSpawnPosition();
			}
			else if (this.Location == SpawnLocation.AtCard)
			{
				vector = WorldManager.instance.GetCard(this.OtherCardId).transform.position;
			}
			else if (this.Location == SpawnLocation.AtFocussed)
			{
				IGameCardOrCardData targetCardOverride = GameCamera.instance.TargetCardOverride;
				vector = ((targetCardOverride == null) ? WorldManager.instance.MiddleOfBoard() : (targetCardOverride.Position + Vector3.left * 1.5f));
			}
			CardData cardData = WorldManager.instance.CreateCard(vector, this.CardId, faceUp: true, checkAddToStack: false);
			if (this.MakeSmoke)
			{
				WorldManager.instance.CreateSmoke(vector);
			}
			if (this.SendCard)
			{
				cardData.MyGameCard.SendIt();
			}
		}
		yield break;
	}
}
