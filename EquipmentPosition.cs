using UnityEngine;

public class EquipmentPosition : MonoBehaviour
{
	public SpriteRenderer ShadowRenderer;

	public SpriteRenderer IconRenderer;

	private Vector3 startOffset;

	private GameCard parentCard;

	private float alpha;

	public bool IsWorkerPosition;

	private float timer;

	private void Awake()
	{
		this.parentCard = base.GetComponentInParent<GameCard>();
		this.startOffset = base.transform.localPosition;
		this.timer = Random.Range(0f, 10f);
		SpriteRenderer shadowRenderer = this.ShadowRenderer;
		Color color2 = (this.IconRenderer.color = new Color(1f, 1f, 1f, 0f));
		shadowRenderer.color = color2;
		SpriteRenderer shadowRenderer2 = this.ShadowRenderer;
		bool flag2 = (this.IconRenderer.enabled = false);
		shadowRenderer2.enabled = flag2;
	}

	private void Update()
	{
		if (!(this.parentCard.MyBoard == null) && this.parentCard.MyBoard.IsCurrent)
		{
			this.timer += Time.deltaTime * WorldManager.instance.TimeScale;
			this.alpha = Mathf.Lerp(this.alpha, (this.parentCard.ShowInventory && ((this.parentCard.IsWorkerInventory && this.IsWorkerPosition) || (!this.parentCard.IsWorkerInventory && !this.IsWorkerPosition))) ? 1f : 0f, Time.deltaTime * 20f);
			SpriteRenderer shadowRenderer = this.ShadowRenderer;
			Color color2 = (this.IconRenderer.color = new Color(1f, 1f, 1f, this.alpha));
			shadowRenderer.color = color2;
			SpriteRenderer shadowRenderer2 = this.ShadowRenderer;
			bool flag2 = (this.IconRenderer.enabled = this.alpha > 0.01f);
			shadowRenderer2.enabled = flag2;
			float x = this.timer * 0.5f;
			base.transform.localPosition = this.startOffset + new Vector3(this.Perlin(x, 0.2f), this.Perlin(x, 0.6f)) * 0.01f;
		}
	}

	private float Perlin(float x, float y)
	{
		return Mathf.PerlinNoise(x, y) * 2f - 1f;
	}
}
