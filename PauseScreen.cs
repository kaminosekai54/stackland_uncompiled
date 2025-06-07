using UnityEngine;

public class PauseScreen : SokScreen
{
	public CustomButton ContinueButton;

	public CustomButton CardopediaButton;

	public CustomButton OptionsButton;

	public CustomButton MainMenuButton;

	public CustomButton AbandonCityButton;

	public RectTransform NewCardopediaEntryRect;

	public override bool IsFrameRateUncapped => true;

	private void Awake()
	{
		this.ContinueButton.Clicked += delegate
		{
			WorldManager.instance.TogglePause();
		};
		this.OptionsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.AbandonCityButton.Clicked += delegate
		{
			WorldManager.instance.ModalAbandonCity();
		};
		this.CardopediaButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<CardopediaScreen>();
		};
		this.MainMenuButton.Clicked += delegate
		{
			TransitionScreen.instance.StartTransition(delegate
			{
				SaveManager.instance.Save(saveRound: true);
				WorldManager.RestartGame();
			});
		};
		this.NewCardopediaEntryRect.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		this.NewCardopediaEntryRect.gameObject.SetActive(WorldManager.instance.CurrentSave.NewCardopediaIds.Count > 0);
		if (WorldManager.instance.GetCurrentBoardSafe().Id == "cities")
		{
			this.AbandonCityButton.gameObject.SetActive(value: true);
		}
		else
		{
			this.AbandonCityButton.gameObject.SetActive(value: false);
		}
	}
}
