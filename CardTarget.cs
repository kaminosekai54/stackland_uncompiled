using UnityEngine;

public class CardTarget : Interactable
{
	private Vector3 startScale;

	public bool Hovered => WorldManager.instance.NearbyCardTarget == this;

	public override bool CanBePushed()
	{
		return false;
	}

	protected override void Start()
	{
		WorldManager.instance.CardTargets.Add(this);
		this.startScale = base.transform.localScale;
		base.PushDir = new Vector3(0f, 0f, 1f);
		base.TargetPosition = base.transform.position;
		base.MyBoard = base.DetermineParentBoard();
		base.Start();
	}

	protected override void OnDestroy()
	{
		if (WorldManager.instance != null)
		{
			WorldManager.instance.CardTargets.Remove(this);
		}
		base.OnDestroy();
	}

	protected override void Update()
	{
	}

	protected override void LateUpdate()
	{
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.Hovered ? (this.startScale * 1.05f) : this.startScale, Time.deltaTime * 20f);
		base.LateUpdate();
	}

	public virtual void CardDropped(GameCard card)
	{
	}

	public virtual bool CanHaveCard(GameCard card)
	{
		return true;
	}

	protected override void ClampPos()
	{
	}
}
