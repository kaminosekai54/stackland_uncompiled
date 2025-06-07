using System;
using UnityEngine;

[Serializable]
public class CardPalette
{
	public Color Color;

	public Color Color2;

	public Color Icon;

	public CardPalette(Color color, Color color2, Color icon)
	{
		this.Color = color;
		this.Color2 = color2;
		this.Icon = icon;
	}
}
