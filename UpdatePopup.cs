using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpdatePopup : MonoBehaviour
{
	public TextMeshProUGUI UpdateText;

	public TextMeshProUGUI UpdateTitle;

	public CustomButton CloseUpdateInfoButton;

	public CustomButton BuyDLCButton;

	private void Awake()
	{
		this.CloseUpdateInfoButton.Clicked += delegate
		{
			if (PlatformHelper.IsTestBuild && WorldManager.instance.IsCitiesDlcActive())
			{
				GameCanvas.instance.ShowEarlyAccessModal();
			}
			base.gameObject.SetActive(value: false);
		};
		this.CloseUpdateInfoButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
		{
			nav.selectOnUp = (this.BuyDLCButton.gameObject.activeInHierarchy ? this.BuyDLCButton : null);
			Selectable selectable2 = (nav.selectOnRight = null);
			Selectable selectOnDown = (nav.selectOnLeft = selectable2);
			nav.selectOnDown = selectOnDown;
			return nav;
		};
		this.BuyDLCButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
		{
			Selectable selectable5 = (nav.selectOnRight = null);
			Selectable selectOnUp = (nav.selectOnLeft = selectable5);
			nav.selectOnUp = selectOnUp;
			nav.selectOnDown = this.CloseUpdateInfoButton;
			return nav;
		};
		this.BuyDLCButton.Clicked += delegate
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/2867570/Stacklands_2000");
		};
		this.UpdatePopupText();
	}

	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(this.CloseUpdateInfoButton.gameObject);
	}

	private void Update()
	{
		this.UpdatePopupText();
	}

	private void UpdatePopupText()
	{
		this.UpdateTitle.text = SokLoc.Translate("label_update_title_cities");
		if (WorldManager.instance.IsCitiesDlcActive())
		{
			this.UpdateText.text = SokLoc.Translate("label_update_text_cities");
			this.BuyDLCButton.gameObject.SetActive(value: false);
		}
		else
		{
			this.UpdateText.text = SokLoc.Translate("label_update_text_cities_locked");
			this.BuyDLCButton.gameObject.SetActive(value: true);
		}
	}
}
