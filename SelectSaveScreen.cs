using System.Collections.Generic;
using UnityEngine;

public class SelectSaveScreen : SokScreen
{
	public RectTransform ButtonsParent;

	public CustomButton BackButton;

	private List<CustomButton> saveButtons = new List<CustomButton>();

	private void Start()
	{
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.UpdateButtonText();
	}

	private void OnEnable()
	{
		this.UpdateButtonText();
	}

	private void TryCreateSaveButtons()
	{
		if (this.saveButtons.Count <= 0)
		{
			this.saveButtons.Clear();
			for (int i = 0; i < 5; i++)
			{
				CustomButton customButton = Object.Instantiate(PrefabManager.instance.ButtonPrefab);
				customButton.transform.SetParentClean(this.ButtonsParent);
				this.saveButtons.Add(customButton);
			}
		}
	}

	private void UpdateButtonText()
	{
		this.TryCreateSaveButtons();
		List<SaveGame> allSaves = SaveManager.instance.GetAllSaves();
		for (int i = 0; i < 5; i++)
		{
			int index = i;
			SaveGame save = allSaves[index];
			CustomButton customButton = this.saveButtons[index];
			if (save != null)
			{
				customButton.TextMeshPro.text = SaveManager.instance.GetSaveSummary(save);
			}
			else
			{
				customButton.TextMeshPro.text = SokLoc.Translate("label_start_new_save", LocParam.Create("save_index", (index + 1).ToString()));
			}
			customButton.Clicked += delegate
			{
				if (save == null)
				{
					SaveManager.instance.Save(new SaveGame
					{
						SaveId = index.ToString()
					});
					this.Restart();
				}
				else if (PlatformHelper.HasModdingSupport && !new HashSet<string>(save.DisabledMods).SetEquals(new HashSet<string>(WorldManager.instance.CurrentSave.DisabledMods)))
				{
					ModalScreen.instance.Clear();
					ModalScreen.instance.SetTexts(SokLoc.Translate("label_restart_required"), SokLoc.Translate("label_different_mods"));
					ModalScreen.instance.AddOption(SokLoc.Translate("label_restart"), delegate
					{
						SaveManager.instance.Save(save);
						WorldManager.RebootGame();
					});
					GameCanvas.instance.OpenModal();
				}
				else
				{
					this.SetSave(save);
				}
			};
		}
	}

	private void Restart()
	{
		TransitionScreen.instance.StartTransition(delegate
		{
			WorldManager.RestartGame();
		});
	}

	private void SetSave(SaveGame save)
	{
		SaveManager.instance.Save(save);
		this.Restart();
	}
}
