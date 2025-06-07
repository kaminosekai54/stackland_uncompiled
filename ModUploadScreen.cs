using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TMPro;
using UnityEngine;

public class ModUploadScreen : SokScreen
{
	public CustomButton BackButton;

	public CustomButton UploadButton;

	public CustomButton TogglePreviewButton;

	public CustomButton SelectTagsButton;

	public bool SetPreviewImage = true;

	public TextMeshProUGUI ChangeNotesText;

	public List<string> Tags = new List<string>();

	private CallResult<CreateItemResult_t> crCreateItemResult;

	private CallResult<SubmitItemUpdateResult_t> crSubmitItemUpdateResult;

	private UGCUpdateHandle_t ugcUpdateHandle = UGCUpdateHandle_t.Invalid;

	private EItemUpdateStatus lastUpdateStatus;

	private void Awake()
	{
		this.TogglePreviewButton.Clicked += delegate
		{
			this.SetPreviewImage = !this.SetPreviewImage;
			this.TogglePreviewButton.TextMeshPro.text = "Set preview image: " + OptionsScreen.YesNo(this.SetPreviewImage);
		};
		this.SelectTagsButton.Clicked += delegate
		{
			ModalScreen instance = ModalScreen.instance;
			this.Tags.Clear();
			instance.Clear();
			instance.SetTexts("Select Tags", "Click tags below to enable them.\n\n<color=#0000ff><u><link=\"https://modding.stacklands.co/en/latest/guides/publishing.html#adding-tags\">What are tags?</link></u></color>");
			this.CreateTagButton("Cards");
			this.CreateTagButton("Gameplay");
			this.CreateTagButton("Language");
			this.CreateTagButton("Quality of Life");
			this.CreateTagButton("Content");
			this.CreateTagButton("Development");
			instance.AddOption("Back", delegate
			{
				GameCanvas.instance.CloseModal();
			});
			GameCanvas.instance.OpenModal();
		};
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ModOptionsScreen>();
		};
	}

	private void OnEnable()
	{
		Mod mod = ModOptionsScreen.SelectedMod;
		if (mod == null)
		{
			return;
		}
		this.crCreateItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItem);
		this.crSubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnUpdateItem);
		if (ModUploadScreen.GetWorkshopId(mod) == 0L)
		{
			ModalScreen instance = ModalScreen.instance;
			instance.Clear();
			instance.SetTexts("Create Workshop Item?", "It appears this mod has not been uploaded to the Steam Workshop yet. Would you like to create a new Workshop Item now?\n\nBy submitting this item, you agree to the <color=#0000ff><u><link=\"https://steamcommunity.com/workshop/workshoplegalagreement/\">Steam Workshop Terms Of Service</link></u></color>");
			instance.AddOption("Create Item", delegate
			{
				if (!this.crCreateItemResult.IsActive())
				{
					this.CreateWorkshopItem();
				}
			});
			instance.AddOption("Back", delegate
			{
				GameCanvas.instance.SetScreen<ModOptionsScreen>();
				GameCanvas.instance.CloseModal();
			});
			GameCanvas.instance.OpenModal();
		}
		this.UploadButton.Clicked += delegate
		{
			if (!(this.ugcUpdateHandle != UGCUpdateHandle_t.Invalid))
			{
				ModalScreen modal = ModalScreen.instance;
				modal.Clear();
				modal.SetTexts("Upload mod?", "You are about to upload the contents of the following folder to the Steam Workshop: " + mod.Path);
				modal.AddOption("Upload", delegate
				{
					modal.Clear();
					this.UploadWorkshopItem();
				});
				modal.AddOption("Open Folder", delegate
				{
					Application.OpenURL("file:///" + mod.Path);
				});
				modal.AddOption("Back", delegate
				{
					GameCanvas.instance.CloseModal();
				});
				GameCanvas.instance.OpenModal();
			}
		};
	}

	private void Update()
	{
		this.UpdateUploading();
		if (InputController.instance.CancelTriggered())
		{
			GameCanvas.instance.SetScreen<ModOptionsScreen>();
		}
	}

	private void UpdateUploading()
	{
		if (!(this.ugcUpdateHandle == UGCUpdateHandle_t.Invalid))
		{
			ModalScreen instance = ModalScreen.instance;
			ulong punBytesProcessed;
			ulong punBytesTotal;
			EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(this.ugcUpdateHandle, out punBytesProcessed, out punBytesTotal);
			if (punBytesTotal != 0L)
			{
				instance.SetTexts("Uploading...", $"Status: {itemUpdateProgress} ({ModUploadScreen.FormatBytes(punBytesProcessed)}/{ModUploadScreen.FormatBytes(punBytesTotal)})");
			}
			else
			{
				instance.SetTexts("Uploading...", $"Status: {itemUpdateProgress}");
			}
			if (itemUpdateProgress != this.lastUpdateStatus)
			{
				Debug.Log(itemUpdateProgress);
				this.lastUpdateStatus = itemUpdateProgress;
			}
		}
	}

	private void CheckForConfig()
	{
		string configPath = Path.Combine(ModOptionsScreen.SelectedMod.Path, "config.json");
		ModalScreen modal = ModalScreen.instance;
		if (File.Exists(configPath))
		{
			modal.Clear();
			modal.SetTexts("config.json detected", "A config.json file has been detected in the mod folder. If this file is uploaded, all players will have your current settings instead of the default ones. Would you like to delete the file and proceed with the upload?");
			modal.AddOption("Delete config.json & Upload", delegate
			{
				File.Delete(configPath);
				this.UploadWorkshopItem();
				modal.Clear();
			});
			modal.AddOption("Keep config.json & Upload (Not Recommended)", delegate
			{
				this.UploadWorkshopItem();
				modal.Clear();
			});
			modal.AddOption("Open Folder", delegate
			{
				Application.OpenURL("file:///" + ModOptionsScreen.SelectedMod.Path);
			});
			modal.AddOption("Cancel", delegate
			{
				GameCanvas.instance.CloseModal();
			});
		}
		else
		{
			this.UploadWorkshopItem();
			modal.Clear();
		}
	}

	private void CreateWorkshopItem()
	{
		Debug.Log("Creating workshop item..");
		SteamAPICall_t hAPICall = SteamUGC.CreateItem(new AppId_t(1948280u), EWorkshopFileType.k_EWorkshopFileTypeFirst);
		this.crCreateItemResult.Set(hAPICall);
	}

	private void UploadWorkshopItem()
	{
		Mod selectedMod = ModOptionsScreen.SelectedMod;
		string text = Path.Combine(selectedMod.Path, "config.json");
		if (File.Exists(text))
		{
			File.Move(text, Path.Combine(Application.persistentDataPath, "Mods", "config.json"));
		}
		this.ugcUpdateHandle = SteamUGC.StartItemUpdate(new AppId_t(1948280u), new PublishedFileId_t(ModUploadScreen.GetWorkshopId(selectedMod)));
		SteamUGC.SetItemTitle(this.ugcUpdateHandle, selectedMod.Manifest.Name);
		SteamUGC.SetItemContent(this.ugcUpdateHandle, selectedMod.Path);
		if (this.Tags.Count > 0)
		{
			SteamUGC.SetItemTags(this.ugcUpdateHandle, this.Tags);
		}
		if (this.SetPreviewImage && File.Exists(Path.Combine(selectedMod.Path, "icon.png")))
		{
			SteamUGC.SetItemPreview(this.ugcUpdateHandle, Path.Combine(selectedMod.Path, "icon.png"));
		}
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(this.ugcUpdateHandle, this.ChangeNotesText.text);
		this.crSubmitItemUpdateResult.Set(hAPICall);
	}

	private void OnCreateItem(CreateItemResult_t result, bool failed)
	{
		if (result.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError($"uh oh, result of CreateItem is {result.m_eResult}");
			return;
		}
		Debug.Log($"Item has been created: {result.m_nPublishedFileId}");
		if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{result.m_nPublishedFileId}");
		}
		Mod mod = ModOptionsScreen.SelectedMod;
		ModUploadScreen.SetWorkshopId(mod, (ulong)result.m_nPublishedFileId);
		ModalScreen instance = ModalScreen.instance;
		instance.Clear();
		instance.SetTexts("Item created", "A workshop.txt file has been created in the mod folder, be sure to copy this to your source folder!");
		instance.AddOption("Open folder", delegate
		{
			Application.OpenURL("file:///" + mod.Path);
		});
		instance.AddOption("Back", delegate
		{
			GameCanvas.instance.CloseModal();
		});
	}

	private void OnUpdateItem(SubmitItemUpdateResult_t result, bool failed)
	{
		this.ugcUpdateHandle = UGCUpdateHandle_t.Invalid;
		this.lastUpdateStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
		ModalScreen instance = ModalScreen.instance;
		instance.Clear();
		if (result.m_eResult == EResult.k_EResultOK)
		{
			instance.SetTexts("Upload finished", "The files have been successfully uploaded");
			instance.AddOption(SokLoc.Translate("label_okay"), delegate
			{
				GameCanvas.instance.CloseModal();
			});
		}
		else
		{
			instance.SetTexts("uh oh", $"something went wrong :( {result.m_eResult}");
			instance.AddOption(SokLoc.Translate("label_okay"), delegate
			{
				GameCanvas.instance.CloseModal();
			});
		}
		string text = Path.Combine(Application.persistentDataPath, "Mods", "config.json");
		if (File.Exists(text))
		{
			File.Move(text, Path.Combine(ModOptionsScreen.SelectedMod.Path, "config.json"));
		}
		SteamFriends.ActivateGameOverlayToWebPage($"https://steamcommunity.com/sharedfiles/filedetails/?id={result.m_nPublishedFileId}");
	}

	private void CreateTagButton(string tag)
	{
		CustomButton but = UnityEngine.Object.Instantiate(ModalScreen.instance.ButtonPrefab);
		but.transform.SetParentClean(ModalScreen.instance.ButtonParent);
		but.TextMeshPro.text = "<color=#A1A1A1><s>" + tag + "</s>";
		but.Clicked += delegate
		{
			if (this.Tags.Contains(tag))
			{
				this.Tags.Remove(tag);
				but.TextMeshPro.text = "<color=#A1A1A1><s>" + tag + "</s>";
			}
			else
			{
				this.Tags.Add(tag);
				but.TextMeshPro.text = tag;
			}
		};
	}

	public static string FormatBytes(ulong bytes)
	{
		if (bytes < 1024)
		{
			return $"{bytes} B";
		}
		int num = (int)(Math.Log(bytes) / Math.Log(1024.0));
		return string.Format("{0:F2} {1}B", (double)bytes / Math.Pow(1024.0, num), "KMGT"[num - 1]);
	}

	public static ulong GetWorkshopId(Mod mod)
	{
		string path = Path.Combine(mod.Path, "workshop.txt");
		if (!File.Exists(path))
		{
			return 0uL;
		}
		if (ulong.TryParse(File.ReadAllText(path), out var result))
		{
			return result;
		}
		return 0uL;
	}

	public static void SetWorkshopId(Mod mod, ulong id)
	{
		File.WriteAllText(Path.Combine(mod.Path, "workshop.txt"), id.ToString());
	}
}
