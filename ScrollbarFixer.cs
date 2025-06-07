using UnityEngine;
using UnityEngine.UI;

public class ScrollbarFixer : MonoBehaviour
{
	private Scrollbar scrollbar;

	private void Start()
	{
		this.scrollbar = base.GetComponent<Scrollbar>();
	}

	private void Update()
	{
		this.scrollbar.interactable = !InputController.instance.CurrentSchemeIsController;
	}
}
