using TMPro;
using UnityEngine;

[ExecuteAlways]
public class FontSetter : MonoBehaviour
{
	public FontType MyFontType;

	[HideInInspector]
	public string LanguageOverride;

	private TextMeshPro tmPro;

	private void Start()
	{
		this.SetFont();
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged += SetFont;
		}
	}

	private void OnEnable()
	{
		this.SetFont();
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= SetFont;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			this.SetFont();
		}
		if (this.tmPro != null && FontManager.instance != null)
		{
			this.tmPro.fontSharedMaterial = FontManager.instance.WorldFontMaterial;
		}
	}

	private void SetFont()
	{
		if (!(FontManager.instance == null))
		{
			TMP_FontAsset font = FontManager.instance.GetFont(this.MyFontType, this.LanguageOverride);
			TextMeshProUGUI component = base.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				component.font = font;
				component.material = font.material;
			}
			this.tmPro = base.GetComponent<TextMeshPro>();
			if (this.tmPro != null)
			{
				this.tmPro.font = font;
				this.tmPro.fontSharedMaterial = FontManager.instance.WorldFontMaterial;
			}
		}
	}
}
