public class Kid : CardData
{
	public override void UpdateCardText()
	{
		string text = SokLoc.Translate(base.NameTerm);
		if (!string.IsNullOrEmpty(base.CustomName))
		{
			text = text + " " + base.CustomName;
		}
		base.nameOverride = text;
	}
}
