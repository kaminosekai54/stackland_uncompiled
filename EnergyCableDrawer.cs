using System;
using UnityEngine;

public class EnergyCableDrawer : ShapeDrawer
{
	public Renderer CableRenderer;

	private MaterialPropertyBlock propBlock;

	public Material LowVoltageMaterial;

	public Material BehindMaterial;

	public Material HighVoltageMaterial;

	private int start = Shader.PropertyToID("_Start");

	private int end = Shader.PropertyToID("_End");

	private int middle = Shader.PropertyToID("_Middle");

	public override Type DrawingType => typeof(EnergyCable);

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	private Material GetCurrentMaterial(EnergyCable cable)
	{
		if (WorldManager.instance.CurrentView == ViewType.Energy)
		{
			if (cable.IsLowVoltage)
			{
				return this.LowVoltageMaterial;
			}
			return this.HighVoltageMaterial;
		}
		return this.BehindMaterial;
	}

	public override void UpdateShape()
	{
		EnergyCable cable = (EnergyCable)(object)base.MyShape;
		this.CableRenderer.sharedMaterial = this.GetCurrentMaterial(cable);
		this.CableRenderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector(this.start, new Vector4(cable.Start.x, cable.Start.z));
		this.propBlock.SetVector(this.end, new Vector4(cable.End.x, cable.End.z));
		this.propBlock.SetVector(this.middle, new Vector4(cable.Middle.x, cable.Middle.z));
		this.CableRenderer.SetPropertyBlock(this.propBlock);
		Vector3 position = Vector3.Lerp(cable.Start, cable.End, 0.5f);
		position.y = Mathf.Min(cable.Start.y, cable.End.y);
		if (WorldManager.instance.CurrentView != ViewType.Energy)
		{
			position.y = 0f;
		}
		Vector3 vector = new Vector3(Mathf.Abs(cable.End.x - cable.Start.x), 1f, Mathf.Abs(cable.End.z - cable.Start.z));
		base.transform.position = position;
		this.CableRenderer.transform.localScale = new Vector3(vector.x + 1.5f, vector.z + 1.5f, 1f);
	}
}
