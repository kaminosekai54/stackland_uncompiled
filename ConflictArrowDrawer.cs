using System;
using Shapes;
using UnityEngine;

public class ConflictArrowDrawer : ShapeDrawer
{
	public Transform VeryEffectiveText;

	public Rectangle VeryEffectiveRect;

	public Color OutlineColor = Color.black;

	public float Length = 0.1f;

	public float Thickness = 0.1f;

	public float OutlineThickness = 0.05f;

	public Renderer ArrowRenderer;

	private MaterialPropertyBlock propBlock;

	public ConflictArrow Arrow => (ConflictArrow)(object)base.MyShape;

	public override Type DrawingType => typeof(ConflictArrow);

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		this.ArrowRenderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector("_Start", new Vector4(this.Arrow.Start.x, this.Arrow.Start.z));
		this.propBlock.SetVector("_End", new Vector4(this.Arrow.End.x, this.Arrow.End.z));
		this.propBlock.SetColor("_Color", this.Arrow.Color);
		this.propBlock.SetColor("_OutlineColor", this.OutlineColor);
		this.ArrowRenderer.SetPropertyBlock(this.propBlock);
		Vector3 position = Vector3.Lerp(this.Arrow.Start, this.Arrow.End, 0.5f);
		Vector3 vector = new Vector3(Mathf.Abs(this.Arrow.End.x - this.Arrow.Start.x), 1f, Mathf.Abs(this.Arrow.End.z - this.Arrow.Start.z));
		base.transform.position = position;
		this.ArrowRenderer.transform.localScale = new Vector3(vector.x + 1f, vector.z + 1f, 1f);
		this.VeryEffectiveText.transform.position = Vector3.Lerp(this.Arrow.Start, this.Arrow.End, 0.5f) + Vector3.up * 0.03f;
		this.VeryEffectiveText.gameObject.SetActive(this.Arrow.VeryEffective);
		this.VeryEffectiveRect.gameObject.SetActive(this.Arrow.VeryEffective);
		this.VeryEffectiveText.transform.rotation = Camera.main.transform.rotation;
	}
}
