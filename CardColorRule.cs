using System;

public class CardColorRule
{
	public Predicate<CardData> Predicate;

	public CardPalette Palette;

	public CardColorRule(CardPalette palette, Predicate<CardData> pred)
	{
		this.Palette = palette;
		this.Predicate = pred;
	}
}
