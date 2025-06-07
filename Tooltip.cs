using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
	public static string Text;

	public TextMeshProUGUI TextMesh;

	private RectTransform rectTransform;

	private float tooltipSameTime;

	private string lastTooltipText;

	private static Vector2[] pivots = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private void Awake()
	{
		this.rectTransform = base.GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		bool num = Tooltip.Text != "";
		if (!num)
		{
			base.transform.localScale = Vector3.zero;
		}
		else
		{
			if (Tooltip.Text == this.lastTooltipText)
			{
				this.tooltipSameTime += Time.deltaTime;
			}
			else
			{
				this.tooltipSameTime = 0f;
			}
			if (this.tooltipSameTime >= 0f)
			{
				base.transform.localScale = Vector3.one;
			}
			else
			{
				base.transform.localScale = Vector3.zero;
			}
		}
		this.TextMesh.text = Tooltip.Text;
		if (num)
		{
			Tooltip.SetRectTransformToSafePosition(this.rectTransform);
		}
		this.lastTooltipText = Tooltip.Text;
		Tooltip.Text = "";
	}

	public static void SetRectTransformToSafePosition(RectTransform rect)
	{
		Vector3 localScale = rect.transform.localScale;
		rect.transform.localScale = Vector3.one;
		Vector2[] array = Tooltip.pivots;
		foreach (Vector2 pivot in array)
		{
			rect.pivot = pivot;
			rect.localPosition = GameCanvas.instance.ScreenPosToLocalPos(InputController.instance.ClampedMousePosition());
			if (!Tooltip.IsOverflowing(rect))
			{
				break;
			}
		}
		rect.transform.localScale = localScale;
	}

	private static bool IsOverflowing(RectTransform rect)
	{
		Rect rect2 = new Rect(0f, 0f, Screen.width, Screen.height);
		Vector3[] array = new Vector3[4];
		rect.GetWorldCorners(array);
		bool result = false;
		Vector3[] array2 = array;
		foreach (Vector3 point in array2)
		{
			if (!rect2.Contains(point))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
