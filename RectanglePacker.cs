using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RectanglePacker : MonoBehaviour
{
	private class SpriteMetadata
	{
		public Sprite Sprite;

		public Vector2 Size;

		public float Area => this.Size.x * this.Size.y;
	}

	public List<Sprite> Sprites;

	public Vector2 RectangleSize;

	public Vector2 CurrentRectangleSize;

	public Vector2 CurrentRectanglePivot;

	public float Padding = 0.4f;

	public int SpriteCount = 250;

	public Transform NewSpritesParent;

	public Transform ExistingSpritesParent;

	public SpriteRenderer SpriteRendererPrefab;

	[HideInInspector]
	public List<PlacedSprite> PlacedSprites;

	private Vector2 DetermineSpriteSize(Sprite spr)
	{
		return new Vector2(spr.rect.width, spr.rect.height) / spr.pixelsPerUnit;
	}

	private List<SpriteMetadata> CreateSpriteMetadatas(List<Sprite> sprites)
	{
		List<SpriteMetadata> list = new List<SpriteMetadata>();
		foreach (Sprite sprite in sprites)
		{
			Vector2 size = this.DetermineSpriteSize(sprite);
			SpriteMetadata item = new SpriteMetadata
			{
				Sprite = sprite,
				Size = size
			};
			list.Add(item);
		}
		return list;
	}

	public List<PlacedSprite> GetExistingSprites()
	{
		List<PlacedSprite> list = new List<PlacedSprite>();
		SpriteRenderer[] componentsInChildren = this.ExistingSpritesParent.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			list.Add(new PlacedSprite
			{
				Transform = spriteRenderer.transform,
				Size = this.PadSpriteSize(this.DetermineSpriteSize(spriteRenderer.sprite)),
				Sprite = spriteRenderer.sprite,
				Position = this.WorldPosToLocalPos(spriteRenderer.transform.position)
			});
		}
		return list;
	}

	private Vector2 WorldPosToLocalPos(Vector3 pos)
	{
		Vector3 vector = base.transform.position - new Vector3(this.RectangleSize.x * 0.5f, 0f, this.RectangleSize.y * 0.5f);
		return new Vector2(pos.x - vector.x, pos.z - vector.z);
	}

	private Vector2 PadSpriteSize(Vector2 size)
	{
		size += Vector2.one * this.Padding;
		size.x = Mathf.Max(0.02f, size.x);
		size.y = Mathf.Max(0.02f, size.y);
		return size;
	}

	public void Pack(List<PlacedSprite> initial)
	{
		this.PlacedSprites = this.SpawnNewSprites(initial);
		this.CreateSprites(this.PlacedSprites);
	}

	public List<PlacedSprite> SpawnNewSprites(List<PlacedSprite> initial)
	{
		List<SpriteMetadata> list = this.CreateSpriteMetadatas(this.Sprites);
		WeightedRandomBag<SpriteMetadata> weightedRandomBag = new WeightedRandomBag<SpriteMetadata>();
		foreach (SpriteMetadata item in list)
		{
			weightedRandomBag.AddEntry(item, 1f);
		}
		if (this.Sprites.Count > 0)
		{
			for (int i = 0; i < this.SpriteCount; i++)
			{
				SpriteMetadata spriteMetadata = weightedRandomBag.Choose();
				Vector2 size = spriteMetadata.Size;
				float x2 = Random.Range(size.x * 0.5f, this.RectangleSize.x - size.x * 0.5f);
				float y = Random.Range(size.y * 0.5f, size.y);
				Vector2 position = new Vector2(x2, y);
				PlacedSprite placedSprite = new PlacedSprite
				{
					Sprite = spriteMetadata.Sprite,
					Size = this.PadSpriteSize(size),
					Position = position
				};
				int num = 0;
				while (this.OverlapsWithAny(initial, placedSprite))
				{
					if (num % 5 == 0)
					{
						placedSprite.Position.y += 1f;
					}
					else
					{
						placedSprite.Position.x = Random.Range(size.x * 0.5f, this.RectangleSize.x - size.x * 0.5f);
					}
					num++;
				}
				initial.Add(placedSprite);
			}
		}
		initial.RemoveAll((PlacedSprite x) => x.Position.y > this.RectangleSize.y);
		return initial;
	}

	public void UpdateActiveSprites()
	{
		foreach (PlacedSprite placedSprite in this.PlacedSprites)
		{
			placedSprite.IsVisible = this.PositionInCurrentRectangle(placedSprite.Position);
			placedSprite.Transform.gameObject.SetActiveFast(placedSprite.IsVisible);
		}
	}

	private void CreateSprites(List<PlacedSprite> sprites)
	{
		foreach (Transform item in this.NewSpritesParent.Cast<Transform>().ToList())
		{
			Object.DestroyImmediate(item.gameObject);
		}
		foreach (PlacedSprite sprite in sprites)
		{
			if (!(sprite.Transform != null))
			{
				SpriteRenderer spriteRenderer = Object.Instantiate(this.SpriteRendererPrefab);
				spriteRenderer.transform.SetParent(this.NewSpritesParent);
				Vector3 worldPos = this.GetWorldPos(sprite);
				worldPos.y = -0.02f;
				spriteRenderer.transform.position = worldPos;
				spriteRenderer.gameObject.name = sprite.Sprite.name;
				spriteRenderer.sprite = sprite.Sprite;
				sprite.Transform = spriteRenderer.transform;
			}
		}
	}

	private bool OverlapsWithAny(List<PlacedSprite> existing, PlacedSprite newSprite)
	{
		foreach (PlacedSprite item in existing)
		{
			if (this.Overlaps(item, newSprite))
			{
				return true;
			}
		}
		return false;
	}

	private bool Overlaps(PlacedSprite a, PlacedSprite b)
	{
		if (a.Left < b.Right && a.Right > b.Left && a.Top > b.Bottom && a.Bottom < b.Top)
		{
			return true;
		}
		return false;
	}

	private Vector3 GetWorldPos(PlacedSprite p)
	{
		return this.LocalPosToWorldPos(p.Position);
	}

	private Vector3 LocalPosToWorldPos(Vector2 pos)
	{
		return base.transform.position - new Vector3(this.RectangleSize.x * 0.5f, 0f, this.RectangleSize.y * 0.5f) + new Vector3(pos.x, 0f, pos.y);
	}

	private Vector2 GetCurrentRectanglePosition()
	{
		Vector2 vector = new Vector2(this.CurrentRectanglePivot.x * this.RectangleSize.x, this.CurrentRectanglePivot.y * this.RectangleSize.y);
		Vector2 currentRectanglePivot = this.CurrentRectanglePivot;
		currentRectanglePivot.x = (1f - currentRectanglePivot.x) * 2f - 1f;
		currentRectanglePivot.y = (1f - currentRectanglePivot.y) * 2f - 1f;
		return currentRectanglePivot * this.CurrentRectangleSize * 0.5f + vector;
	}

	private bool PositionInCurrentRectangle(Vector2 pos)
	{
		Vector2 currentRectanglePosition = this.GetCurrentRectanglePosition();
		float num = currentRectanglePosition.x - this.CurrentRectangleSize.x * 0.5f;
		float num2 = currentRectanglePosition.x + this.CurrentRectangleSize.x * 0.5f;
		float num3 = currentRectanglePosition.y + this.CurrentRectangleSize.y * 0.5f;
		float num4 = currentRectanglePosition.y - this.CurrentRectangleSize.y * 0.5f;
		if (pos.x > num && pos.x < num2 && pos.y > num4)
		{
			return pos.y < num3;
		}
		return false;
	}

	private bool InCurrentRectangle(PlacedSprite spr)
	{
		Vector2 currentRectanglePosition = this.GetCurrentRectanglePosition();
		float num = currentRectanglePosition.x - this.CurrentRectangleSize.x * 0.5f;
		float num2 = currentRectanglePosition.x + this.CurrentRectangleSize.x * 0.5f;
		float num3 = currentRectanglePosition.y + this.CurrentRectangleSize.y * 0.5f;
		float num4 = currentRectanglePosition.y - this.CurrentRectangleSize.y * 0.5f;
		if (spr.Left < num2 && spr.Right > num && spr.Top > num4 && spr.Bottom < num3)
		{
			return true;
		}
		return false;
	}

	public Bounds GetCurrentWorldBounds()
	{
		return new Bounds(this.LocalPosToWorldPos(this.GetCurrentRectanglePosition()), new Vector3(this.CurrentRectangleSize.x, 0.1f, this.CurrentRectangleSize.y));
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.color = Color.red;
		Bounds currentWorldBounds = this.GetCurrentWorldBounds();
		Gizmos.DrawWireCube(currentWorldBounds.center, currentWorldBounds.size);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		if (this.PlacedSprites == null)
		{
			return;
		}
		foreach (PlacedSprite placedSprite in this.PlacedSprites)
		{
			Gizmos.color = (this.PositionInCurrentRectangle(placedSprite.Position) ? Color.yellow : Color.blue);
			Gizmos.DrawWireCube(this.GetWorldPos(placedSprite), new Vector3(placedSprite.Size.x, 0.1f, placedSprite.Size.y));
		}
	}
}
