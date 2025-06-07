using System;
using System.Collections;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
	private MeshRenderer myRenderer;

	private Material[] startMaterials;

	private Material[] currentMaterials;

	public void Init()
	{
		if (!(this.myRenderer != null))
		{
			this.myRenderer = base.GetComponent<MeshRenderer>();
			if (this.myRenderer == null)
			{
				Debug.Log(base.gameObject.name + " does not have a MeshRenderer");
			}
			this.startMaterials = this.myRenderer.sharedMaterials;
			this.currentMaterials = this.myRenderer.sharedMaterials;
		}
	}

	private void Awake()
	{
		this.Init();
	}

	public void SetMaterial(Material mat)
	{
		if (this.myRenderer == null)
		{
			return;
		}
		for (int i = 0; i < this.currentMaterials.Length; i++)
		{
			if (!(this.currentMaterials[i].name == "Invisible"))
			{
				this.currentMaterials[i] = mat;
			}
		}
		this.myRenderer.sharedMaterials = this.currentMaterials;
	}

	public void SetMaterialForTime(Material mat, float time, Action afterAction = null)
	{
		if (base.gameObject.activeInHierarchy)
		{
			this.SetMaterial(mat);
			base.StartCoroutine(this.ResetAfter(time, afterAction));
		}
	}

	private IEnumerator ResetAfter(float t, Action action)
	{
		yield return new WaitForSeconds(t);
		this.ResetMaterials();
		action?.Invoke();
	}

	public void ResetMaterials()
	{
		if (!(this.myRenderer == null))
		{
			this.myRenderer.sharedMaterials = this.startMaterials;
		}
	}
}
