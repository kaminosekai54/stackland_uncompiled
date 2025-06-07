using UnityEngine;

public class DirectionCircleElement : Interactable
{
	private Vector3 startScale;

	public GameCard ParentCard;

	public Sprite DirectionArrow;

	public Sprite RandomArrow;

	public SpriteRenderer DirectionSpriteRenderer;

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
			return !this.ParentCard.BeingDragged;
		}
	}

	protected override void Start()
	{
		this.startScale = base.transform.localScale;
		base.Start();
	}

	public override string GetTooltipText()
	{
		return "Toggle output direction";
	}

	public override void Clicked()
	{
		this.ParentCard.ToggleDirection();
	}

	protected override void Update()
	{
		if (this.ParentCard.CardData.OutputDir == Vector3.zero)
		{
			this.DirectionSpriteRenderer.sprite = this.RandomArrow;
		}
		else
		{
			this.DirectionSpriteRenderer.sprite = this.DirectionArrow;
		}
		base.transform.rotation = Quaternion.LookRotation(Vector3.down, this.ParentCard.CardData.OutputDir);
		Vector3 b = (this.IsHovered ? (this.startScale * 1.1f) : this.startScale);
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, b, Time.deltaTime * 12f);
	}

	public override bool CanBeDragged()
	{
		return false;
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
