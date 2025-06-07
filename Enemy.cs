using UnityEngine;

public class Enemy : Mob
{
	protected override void Move()
	{
		Vector3 vector;
		if (base.CurrentTarget != null)
		{
			vector = base.transform.position - base.CurrentTarget.transform.position;
			vector.y = 0f;
			vector.Normalize();
			Debug.DrawLine(base.transform.position, base.transform.position - vector, Color.red, 1f);
			vector = this.Wiggle(vector, 45f);
			Debug.DrawLine(base.transform.position, base.transform.position - vector, Color.green, 1f);
			vector = -vector * 4f;
		}
		else if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			vector = Vector3.zero;
		}
		else
		{
			Vector2 vector2 = Random.insideUnitCircle.normalized * 4f;
			vector = new Vector3(vector2.x, 0f, vector2.y);
		}
		base.MyGameCard.Velocity = new Vector3(vector.x, 0f, vector.z);
	}

	public override void UpdateCard()
	{
		CardData cardData2;
		if (base.Id == "wolf")
		{
			if (base.HasCardOnTop("bone", out var cardData))
			{
				cardData.MyGameCard.DestroyCard();
				base.MyGameCard.DestroyCard();
				WorldManager.instance.CreateCard(base.transform.position, "dog", faceUp: true, checkAddToStack: false);
			}
		}
		else if (base.Id == "feral_cat" && base.HasCardOnTop("milk", out cardData2) && WorldManager.instance.IsSpiritDlcActive())
		{
			cardData2.MyGameCard.DestroyCard();
			base.MyGameCard.DestroyCard();
			WorldManager.instance.CreateCard(base.transform.position, "cat", faceUp: true, checkAddToStack: false);
		}
		base.UpdateCard();
	}

	private Vector3 Wiggle(Vector3 vec, float angle)
	{
		return Quaternion.AngleAxis(Random.Range(0f - angle, angle), Vector3.up) * vec;
	}
}
