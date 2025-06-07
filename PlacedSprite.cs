using System;
using UnityEngine;

[Serializable]
public class PlacedSprite
{
	public Vector2 Position;

	public Vector2 Size;

	public Sprite Sprite;

	public bool IsVisible;

	public Transform Transform;

	public float Left => this.Position.x - this.Size.x * 0.5f;

	public float Right => this.Position.x + this.Size.x * 0.5f;

	public float Top => this.Position.y + this.Size.y * 0.5f;

	public float Bottom => this.Position.y - this.Size.y * 0.5f;
}
