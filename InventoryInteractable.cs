using UnityEngine;

public class InventoryInteractable : Interactable
{
	public GameCard ParentCard;

	private Vector3 startScale;

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
		this.ParentCard.ToggleInventory();
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
