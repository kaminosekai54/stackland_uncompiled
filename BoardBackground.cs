using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardBackground : MonoBehaviour
{
	public List<Transform> SpriteParents;

	public GameBoard MyBoard;

	public List<SpriteArea> SpriteAreas;

	[HideInInspector]
	public List<RectanglePacker> RectanglePackers;

	public RectanglePacker RectanglePackerPrefab;

	public List<Sprite> SpritesToPlace;

	public List<Sprite> InsideSpritesToPlace;

	public SpriteRenderer NormalSpritePrefab;

	public SpriteRenderer InsideSpritePrefab;

	public float InsidePadding = 1f;

	public float NormalPadding = 0.1f;

	private const float extraWidth = 20f;

	private const float extraHeight = 10f;

	private const float maxWidth = 20f;

	private const float maxHeight = 12f;

	public void SetUpDefaultSpriteAreas()
	{
		if (this.SpriteAreas.Count > 0)
		{
			Debug.LogError("Clear sprite areas before setting up default ones!");
			return;
		}
		this.SpriteAreas.Clear();
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.BottomLeft,
			Padding = this.NormalPadding
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.BottomCenter,
			Padding = this.NormalPadding,
			Expansion = 10f
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.BottomRight,
			Padding = this.NormalPadding
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.MiddleLeft,
			Padding = this.NormalPadding,
			Expansion = 20f
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.MiddleCenter,
			Padding = this.InsidePadding
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.MiddleRight,
			Padding = this.NormalPadding,
			Expansion = 20f
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.TopLeft,
			Padding = this.NormalPadding
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.TopCenter,
			Padding = this.NormalPadding,
			Expansion = 10f
		});
		this.SpriteAreas.Add(new SpriteArea
		{
			Region = BackgroundRegion.TopRight,
			Padding = this.NormalPadding
		});
	}

	public void CreateRectanglePackers()
	{
		foreach (Transform item in base.transform.Cast<Transform>().ToList())
		{
			UnityEngine.Object.DestroyImmediate(item.gameObject);
		}
		this.RectanglePackers = new List<RectanglePacker>();
		for (int i = 0; i < this.SpriteAreas.Count; i++)
		{
			SpriteArea spriteArea = this.SpriteAreas[i];
			RectanglePacker rectanglePacker = UnityEngine.Object.Instantiate(this.RectanglePackerPrefab);
			rectanglePacker.transform.SetParent(base.transform);
			List<Sprite> sprites;
			if (spriteArea.Region == BackgroundRegion.MiddleCenter)
			{
				sprites = this.InsideSpritesToPlace;
				rectanglePacker.SpriteRendererPrefab = this.InsideSpritePrefab;
			}
			else
			{
				sprites = this.SpritesToPlace;
				rectanglePacker.SpriteRendererPrefab = this.NormalSpritePrefab;
			}
			rectanglePacker.Padding = spriteArea.Padding;
			if (spriteArea.OverrideAllowedSprites)
			{
				sprites = spriteArea.AllowedSprites;
			}
			rectanglePacker.Sprites = sprites;
			rectanglePacker.gameObject.name = spriteArea.Region.ToString();
			Vector2 spriteAreaPosition = this.GetSpriteAreaPosition(spriteArea);
			rectanglePacker.transform.position = new Vector3(spriteAreaPosition.x, 0f, spriteAreaPosition.y);
			rectanglePacker.CurrentRectanglePivot = this.GetRegionPivot(spriteArea.Region);
			rectanglePacker.RectangleSize = this.GetSpriteAreaMaxSize(spriteArea);
			rectanglePacker.CurrentRectangleSize = this.GetSpriteAreaCurrentSize(spriteArea);
			this.RectanglePackers.Add(rectanglePacker);
		}
		List<SpriteRenderer> list = new List<SpriteRenderer>();
		foreach (Transform spriteParent in this.SpriteParents)
		{
			list.AddRange(spriteParent.GetComponentsInChildren<SpriteRenderer>(includeInactive: true));
		}
		list = list.Distinct().ToList();
		foreach (SpriteRenderer item2 in list)
		{
			SpriteArea spriteArea2 = this.DetermineSpriteArea(new Vector2(item2.transform.position.x, item2.transform.position.z));
			if (spriteArea2 == null)
			{
				Debug.LogError("No sprite area found for " + item2.name);
				continue;
			}
			SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate(item2);
			spriteRenderer.gameObject.SetActive(value: true);
			RectanglePacker rectanglePacker2 = this.RectanglePackers[this.SpriteAreas.IndexOf(spriteArea2)];
			spriteRenderer.transform.SetParent(rectanglePacker2.ExistingSpritesParent, worldPositionStays: true);
			spriteRenderer.transform.position = item2.transform.position;
		}
		foreach (RectanglePacker rectanglePacker3 in this.RectanglePackers)
		{
			rectanglePacker3.Pack(rectanglePacker3.GetExistingSprites());
			rectanglePacker3.UpdateActiveSprites();
		}
	}

	public void UpdateBoardBackground()
	{
		for (int i = 0; i < this.SpriteAreas.Count; i++)
		{
			SpriteArea spriteArea = this.SpriteAreas[i];
			RectanglePacker rectanglePacker = this.RectanglePackers[i];
			rectanglePacker.CurrentRectangleSize = this.GetSpriteAreaCurrentSize(spriteArea);
			Vector2 spriteAreaPosition = this.GetSpriteAreaPosition(spriteArea);
			rectanglePacker.transform.position = new Vector3(spriteAreaPosition.x, 0f, spriteAreaPosition.y);
			rectanglePacker.UpdateActiveSprites();
		}
	}

	private void Awake()
	{
		this.UpdateBoardBackground();
	}

	private Vector2 GetRegionPivot(BackgroundRegion region)
	{
		Vector2 regionDirection = this.GetRegionDirection(region);
		if (regionDirection.x == 1f)
		{
			regionDirection.x = 0f;
		}
		else if (regionDirection.x == 0f)
		{
			regionDirection.x = 1f;
		}
		if (regionDirection.y == 1f)
		{
			regionDirection.y = 0f;
		}
		else if (regionDirection.y == 0f)
		{
			regionDirection.y = 1f;
		}
		return regionDirection;
	}

	private float GetPrevExpansion(SpriteArea spriteArea)
	{
		float num = 0f;
		for (int i = 0; i < this.SpriteAreas.Count; i++)
		{
			if (this.SpriteAreas[i] == spriteArea)
			{
				return num;
			}
			if (this.SpriteAreas[i].Region == spriteArea.Region)
			{
				num += this.SpriteAreas[i].Expansion;
			}
		}
		return num;
	}

	private Vector2 GetSpriteAreaCurrentSize(SpriteArea spriteArea)
	{
		Bounds worldBounds = this.MyBoard.WorldBounds;
		BackgroundRegion region = spriteArea.Region;
		if (this.IsCornerRegion(spriteArea.Region))
		{
			return new Vector2(20f, 10f);
		}
		switch (region)
		{
		case BackgroundRegion.BottomCenter:
		case BackgroundRegion.TopCenter:
			return new Vector2(worldBounds.size.x, spriteArea.Expansion);
		case BackgroundRegion.MiddleLeft:
		case BackgroundRegion.MiddleRight:
			return new Vector2(spriteArea.Expansion, worldBounds.size.z);
		case BackgroundRegion.MiddleCenter:
			return new Vector2(worldBounds.size.x, worldBounds.size.z);
		default:
			throw new Exception($"{region} is not a valid region");
		}
	}

	private Vector2 GetSpriteAreaMaxSize(SpriteArea spriteArea)
	{
		BackgroundRegion region = spriteArea.Region;
		if (this.IsCornerRegion(region))
		{
			return new Vector2(20f, 10f);
		}
		switch (region)
		{
		case BackgroundRegion.BottomCenter:
		case BackgroundRegion.TopCenter:
			return new Vector2(20f, spriteArea.Expansion);
		case BackgroundRegion.MiddleLeft:
		case BackgroundRegion.MiddleRight:
			return new Vector2(spriteArea.Expansion, 12f);
		case BackgroundRegion.MiddleCenter:
			return new Vector2(20f, 12f);
		default:
			throw new Exception($"{region} is not a valid region");
		}
	}

	private Vector2 GetSpriteAreaPosition(SpriteArea spriteArea)
	{
		Vector2 spriteAreaMaxSize = this.GetSpriteAreaMaxSize(spriteArea);
		Vector2 regionDirection = this.GetRegionDirection(spriteArea.Region);
		Bounds worldBounds = this.MyBoard.WorldBounds;
		Vector2 vector = new Vector2(Mathf.Lerp(worldBounds.min.x, worldBounds.max.x, regionDirection.x), Mathf.Lerp(worldBounds.min.z, worldBounds.max.z, regionDirection.y));
		Vector2 vector2 = regionDirection - new Vector2(0.5f, 0.5f);
		return vector + vector2 * this.GetPrevExpansion(spriteArea) * 2f + new Vector2(vector2.x * spriteAreaMaxSize.x, vector2.y * spriteAreaMaxSize.y);
	}

	private Vector2 GetRegionDirection(BackgroundRegion region)
	{
		return new Vector2((float)((int)region % 3) * 0.5f, (float)((int)region / 3) * 0.5f);
	}

	private bool IsCornerRegion(BackgroundRegion region)
	{
		if (region != BackgroundRegion.TopLeft && region != BackgroundRegion.TopRight && region != 0)
		{
			return region == BackgroundRegion.BottomRight;
		}
		return true;
	}

	private Bounds GetSpriteAreaBounds(SpriteArea spriteArea)
	{
		int index = this.SpriteAreas.IndexOf(spriteArea);
		return this.RectanglePackers[index].GetCurrentWorldBounds();
	}

	private SpriteArea DetermineSpriteArea(Vector2 pos)
	{
		foreach (SpriteArea spriteArea in this.SpriteAreas)
		{
			Bounds spriteAreaBounds = this.GetSpriteAreaBounds(spriteArea);
			float x = spriteAreaBounds.min.x;
			float x2 = spriteAreaBounds.max.x;
			float z = spriteAreaBounds.min.z;
			float z2 = spriteAreaBounds.max.z;
			if (pos.x > x && pos.x < x2 && pos.y > z && pos.y < z2)
			{
				return spriteArea;
			}
		}
		return null;
	}
}
