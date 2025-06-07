using System.Collections.Generic;
using UnityEngine;

public class SelectResolutionScreen : SokScreen
{
	public RectTransform ButtonsParent;

	public CustomButton BackButton;

	private List<CustomButton> resolutionButtons = new List<CustomButton>();

	private List<Resolution> resolutions = new List<Resolution>();

	private int lastScreenWidth;

	private int lastHighestWidth;

	private void Start()
	{
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.lastScreenWidth = Screen.width;
	}

	private void Update()
	{
		if (this.lastScreenWidth != Screen.width && this.lastHighestWidth != Screen.currentResolution.width && this.resolutions != OptionsScreen.PossibleResolutions())
		{
			this.ResetResolutions();
		}
		this.lastHighestWidth = Screen.resolutions[Screen.resolutions.Length - 1].width;
		this.lastScreenWidth = Screen.width;
		this.resolutions = OptionsScreen.PossibleResolutions();
	}

	private void OnEnable()
	{
		this.ResetResolutions();
	}

	private void InitButtons()
	{
		Debug.Log("Reset resolutions");
		List<Resolution> list = OptionsScreen.PossibleResolutions();
		for (int i = 0; i < list.Count; i++)
		{
			Resolution res = list[i];
			CustomButton customButton = Object.Instantiate(PrefabManager.instance.ButtonPrefab);
			customButton.transform.SetParent(this.ButtonsParent);
			customButton.transform.localScale = Vector3.one;
			customButton.transform.localPosition = Vector3.zero;
			customButton.transform.localRotation = Quaternion.identity;
			customButton.TextMeshPro.text = res.width + "x" + res.height;
			customButton.Clicked += delegate
			{
				OptionsScreen.CurrentWidth = res.width;
				OptionsScreen.CurrentHeight = res.height;
				OptionsScreen.SetResolution();
			};
			this.resolutionButtons.Add(customButton);
		}
	}

	public void ResetResolutions()
	{
		foreach (CustomButton resolutionButton in this.resolutionButtons)
		{
			Object.Destroy(resolutionButton.gameObject);
		}
		this.resolutionButtons.Clear();
		this.InitButtons();
	}
}
