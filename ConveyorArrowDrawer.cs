using System;
using UnityEngine;

public class ConveyorArrowDrawer : ShapeDrawer
{
	public Color OutlineColor = Color.black;

	public float Length = 0.1f;

	public float Thickness = 0.1f;

	public float OutlineThickness = 0.05f;

	public Renderer ArrowRenderer;

	private MaterialPropertyBlock propBlock;

	public ConveyorArrow Arrow => (ConveyorArrow)(object)base.MyShape;

	public override Type DrawingType => typeof(ConveyorArrow);

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		this.ArrowRenderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector("_Start", new Vector4(this.Arrow.Start.x, this.Arrow.Start.z));
		this.propBlock.SetVector("_End", new Vector4(this.Arrow.End.x, this.Arrow.End.z));
		this.propBlock.SetColor("_OutlineColor", this.OutlineColor);
		this.ArrowRenderer.SetPropertyBlock(this.propBlock);
		Vector3 position = Vector3.Lerp(this.Arrow.Start, this.Arrow.End, 0.5f);
		Vector3 vector = new Vector3(Mathf.Abs(this.Arrow.End.x - this.Arrow.Start.x), 1f, Mathf.Abs(this.Arrow.End.z - this.Arrow.Start.z));
		base.transform.position = position;
		this.ArrowRenderer.transform.localScale = new Vector3(vector.x + 1f, vector.z + 1f, 1f);
	}
}
