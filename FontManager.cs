using System;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class FontManager : MonoBehaviour
{
	public static FontManager instance;

	public TMP_FontAsset RegularFontAsset;

	public TMP_FontAsset TitleFontAsset;

	public TMP_FontAsset WorldFontAsset;

	public TMP_FontAsset ChineseTraditionalFontAsset;

	public TMP_FontAsset ChineseSimplifiedFontAsset;

	public TMP_FontAsset JapaneseFontAsset;

	public TMP_FontAsset KoreanFontAsset;

	public Material WorldFontMaterial;

	public Shader WorldFontShader;

	private void Awake()
	{
		FontManager.instance = this;
		this.UpdateWorldFontMaterial();
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged += Instance_LanguageChanged;
		}
	}

	private void Start()
	{
		this.UpdateWorldFontMaterial();
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= Instance_LanguageChanged;
		}
	}

	private void Instance_LanguageChanged()
	{
		this.UpdateWorldFontMaterial();
	}

	private void Update()
	{
		FontManager.instance = this;
	}

	public void UpdateWorldFontMaterial()
	{
		if (SokLoc.instance != null)
		{
			TMP_FontAsset font = this.GetFont(FontType.World, SokLoc.instance.CurrentLanguage);
			this.WorldFontMaterial.CopyPropertiesFromMaterial(font.material);
			this.WorldFontMaterial.shader = this.WorldFontShader;
		}
	}

	public TMP_FontAsset GetFont(FontType fontType, string languageOverride = null)
	{
		if (SokLoc.instance != null)
		{
			string text = SokLoc.instance.CurrentLanguage;
			if (!string.IsNullOrEmpty(languageOverride))
			{
				text = languageOverride;
			}
			switch (text)
			{
			case "Chinese (Traditional)":
				return this.ChineseTraditionalFontAsset;
			case "Chinese (Simplified)":
				return this.ChineseSimplifiedFontAsset;
			case "Japanese":
				return this.JapaneseFontAsset;
			case "Korean":
				return this.KoreanFontAsset;
			}
		}
		return fontType switch
		{
			FontType.Regular => this.RegularFontAsset, 
			FontType.Rounded => this.TitleFontAsset, 
			FontType.World => this.WorldFontAsset, 
			_ => throw new ArgumentException("Unknown fontType"), 
		};
	}
}
