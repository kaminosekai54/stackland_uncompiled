using TMPro;
using UnityEngine;

public class GameOverScreen : SokScreen
{
	public CustomButton BackButton;

	public TextMeshProUGUI StatsText;

	private float timer;

	private void Awake()
	{
		this.BackButton.Clicked += delegate
		{
			TransitionScreen.instance.StartTransition(delegate
			{
				WorldManager.instance.ClearRoundAndRestart();
			});
		};
	}

	private void OnEnable()
	{
		this.StatsText.maxVisibleLines = 0;
	}

	private void Update()
	{
		string text = "";
		text = text + SokLoc.Translate("label_you_reached_moon", LocParam.Create("moon", WorldManager.instance.CurrentMonth.ToString())) + "\n";
		text = text + SokLoc.Translate("label_quests_completed", LocParam.Plural("count", WorldManager.instance.QuestsCompleted)) + "\n";
		text = text + SokLoc.Translate("label_new_cards_found", LocParam.Plural("count", WorldManager.instance.NewCardsFound)) + "\n";
		this.StatsText.text = text;
		this.timer += Time.deltaTime;
		if (this.timer >= 0.3f)
		{
			this.timer = 0f;
			this.StatsText.maxVisibleLines++;
		}
	}
}
