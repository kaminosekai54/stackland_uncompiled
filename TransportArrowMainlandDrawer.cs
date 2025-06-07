using System;
using UnityEngine;

public class TransportArrowMainlandDrawer : ShapeDrawer
{
	public Renderer Renderer;

	private MaterialPropertyBlock propBlock;

	public Material FrontMaterial;

	public Material BehindMaterial;

	public TransportArrowMainland Cable => (TransportArrowMainland)(object)base.MyShape;

	public override Type DrawingType => typeof(TransportArrowMainland);

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		this.Renderer.sharedMaterial = ((WorldManager.instance.CurrentView == ViewType.Transport) ? this.FrontMaterial : this.BehindMaterial);
		this.Renderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector("_Start", new Vector4(this.Cable.Start.x, this.Cable.Start.z));
		this.propBlock.SetVector("_End", new Vector4(this.Cable.End.x, this.Cable.End.z));
		this.propBlock.SetVector("_Middle", new Vector4(this.Cable.Middle.x, this.Cable.Middle.z));
		this.Renderer.SetPropertyBlock(this.propBlock);
		Vector3 position = Vector3.Lerp(this.Cable.Start, this.Cable.End, 0.5f);
		position.y = Mathf.Min(this.Cable.Start.y, this.Cable.End.y);
		if (WorldManager.instance.CurrentView != ViewType.Transport)
		{
			position.y = 0f;
		}
		Vector3 vector = new Vector3(Mathf.Abs(this.Cable.End.x - this.Cable.Start.x), 1f, Mathf.Abs(this.Cable.End.z - this.Cable.Start.z));
		base.transform.position = position;
		this.Renderer.transform.localScale = new Vector3(vector.x + 1.5f, vector.z + 1.5f, 1f);
	}
}
