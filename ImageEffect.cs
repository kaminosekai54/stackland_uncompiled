using UnityEngine;

public class ImageEffect : MonoBehaviour
{
	public Material PostProcessingMaterial;

	public float Weight;

	private Material realMaterial;

	private void Awake()
	{
		this.realMaterial = new Material(this.PostProcessingMaterial);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.realMaterial.SetFloat("_Weight", this.Weight);
		Graphics.Blit(source, destination, this.realMaterial);
	}
}
