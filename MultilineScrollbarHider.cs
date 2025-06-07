using UnityEngine;
using UnityEngine.UI;

public class MultilineScrollbarHider : MonoBehaviour
{
	public Scrollbar Scrollbar;

	private void Update()
	{
		bool active = this.Scrollbar.size < 1f || this.Scrollbar.value != 0f;
		this.Scrollbar.gameObject.SetActive(active);
	}
}
