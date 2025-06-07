using Shapes;
using TMPro;
using UnityEngine;

public class SellBox : CardTarget
{
	public Transform GoldSpawnPosition;

	public SpriteRenderer ImageSpriteRenderer;

	public Rectangle HighlightRectangle;

	public TextMeshPro SellText;

	public override void CardDropped(GameCard card)
	{
		WorldManager.instance.SellCard(this.GoldSpawnPosition.position, card);
		base.CardDropped(card);
	}

	public override bool CanHaveCard(GameCard card)
	{
		if (!WorldManager.instance.CardCanBeSold(card))
		{
			return false;
		}
		return true;
	}

	protected override void Update()
	{
		this.ImageSpriteRenderer.sprite = WorldManager.instance.GetCurrencyIcon(WorldManager.instance.CurrentBoard?.BoardOptions?.Currency);
		if (WorldManager.instance.CurrentBoard != null)
		{
			this.SellText.text = SokLoc.Translate(WorldManager.instance.CurrentBoard.BoardOptions.SellBoxTerm);
			base.gameObject.name = SokLoc.Translate(WorldManager.instance.CurrentBoard.BoardOptions.SellBoxTerm);
		}
		else
		{
			this.SellText.text = SokLoc.Translate("label_sell");
			base.gameObject.name = SokLoc.Translate("label_sellbox_title");
		}
		if (WorldManager.instance.CurrentBoard != null)
		{
			this.HighlightRectangle.Color = WorldManager.instance.CurrentBoard.CardHighlightColor;
		}
		this.HighlightRectangle.enabled = WorldManager.instance.DraggingCard != null && this.CanHaveCard(WorldManager.instance.DraggingCard);
		this.HighlightRectangle.DashOffset += Time.deltaTime;
		if (this.HighlightRectangle.DashOffset >= 1f)
		{
			this.HighlightRectangle.DashOffset -= 1f;
		}
		base.Update();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = 0f;
		base.transform.localPosition = localPosition;
	}

	public override string GetTooltipText()
	{
		if (WorldManager.instance.CurrentBoard != null)
		{
			return SokLoc.Translate(WorldManager.instance.CurrentBoard.BoardOptions.SellBoxDescription);
		}
		return SokLoc.Translate("label_sellbox_description", Extensions.LocParam_Action("sell"));
	}
}
