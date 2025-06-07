using Shapes;
using UnityEngine;

public class OnOffInteractable : Interactable
{
	public GameCard ParentCard;

	private Vector3 startScale;

	public Rectangle ButtonShape;

	public string TooltipTerm;

	public string gameObjectTerm;

	public override bool CanBeAutoMovedTo
	{
		get
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (WorldManager.instance.DraggingCard != null)
			{
				return false;
			}
			if (!this.ParentCard.ShowInventory)
			{
				return !this.ParentCard.BeingDragged;
			}
			return false;
		}
	}

	public override string GetTooltipText()
	{
		return SokLoc.Translate(this.TooltipTerm);
	}

	public override void Clicked()
	{
		this.ParentCard.ToggleCardOnOff();
	}

	public override bool CanBeDragged()
	{
		return false;
	}

	protected override void Start()
	{
		this.startScale = base.transform.localScale;
		base.Start();
	}

	protected override void Update()
	{
		base.MyBoard = this.ParentCard.MyBoard;
		base.gameObject.name = SokLoc.Translate(this.gameObjectTerm);
		Vector3 b = (this.IsHovered ? (this.startScale * 1.1f) : this.startScale);
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, b, Time.deltaTime * 12f);
		if (this.ParentCard.CardData != null)
		{
			if (this.ParentCard.CardData.IsOn)
			{
				this.ButtonShape.Color = ColorManager.instance.FloatingTextColorSuccess;
			}
			else
			{
				this.ButtonShape.Color = ColorManager.instance.FloatingTextColorFailed;
			}
		}
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		return false;
	}

	protected override void ClampPos()
	{
	}
}
