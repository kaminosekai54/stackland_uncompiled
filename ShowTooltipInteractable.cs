public class ShowTooltipInteractable : Interactable
{
	public string TooltipTextTerm;

	public string TooltipTitleTerm;

	protected override void ClampPos()
	{
	}

	protected override void Update()
	{
	}

	protected override void LateUpdate()
	{
	}

	public override string GetTooltipText()
	{
		base.name = SokLoc.Translate(this.TooltipTitleTerm);
		return SokLoc.Translate(this.TooltipTextTerm);
	}
}
