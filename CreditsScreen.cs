using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class CreditsScreen : SokScreen
{
	public TextMeshProUGUI CreditsText;

	public float ScrollSpeed;

	private StringBuilder sb;

	private void OnEnable()
	{
		this.CreditsText.text = this.GenerateCredits();
		Vector2 anchoredPosition = this.CreditsText.rectTransform.anchoredPosition;
		anchoredPosition.y = this.GetHeight();
		this.CreditsText.rectTransform.anchoredPosition = anchoredPosition;
	}

	private float GetHeight()
	{
		return 0f - ((RectTransform)base.transform.parent).sizeDelta.y;
	}

	private void Update()
	{
		Vector2 anchoredPosition = this.CreditsText.rectTransform.anchoredPosition;
		anchoredPosition.y += this.ScrollSpeed * Time.deltaTime;
		if (anchoredPosition.y >= this.CreditsText.rectTransform.sizeDelta.y)
		{
			anchoredPosition.y = this.GetHeight();
		}
		this.CreditsText.rectTransform.anchoredPosition = anchoredPosition;
		if (InputController.instance.AnyInputDone())
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		}
	}

	private void AddTitle(string term)
	{
		this.sb.Append("<color=#AAAAAA>");
		this.sb.Append(SokLoc.Translate(term));
		this.sb.Append("</color>");
		this.sb.AppendLine();
	}

	private void AddName(string name)
	{
		this.sb.Append(name);
		this.sb.AppendLine();
	}

	private void AddName(params string[] names)
	{
		foreach (string item in names.OrderBy((string x) => x))
		{
			this.AddName(item);
		}
	}

	private void AddNewLine()
	{
		this.sb.AppendLine();
	}

	public string GenerateCredits()
	{
		this.sb = new StringBuilder();
		this.sb.Append("<b><size=150%>Stacklands</size></b>");
		this.sb.AppendLine();
		this.sb.Append(SokLoc.Translate("credits_sokpop"));
		this.sb.AppendLine();
		this.sb.AppendLine();
		this.AddTitle("credits_aran");
		this.AddName("Aran Koning");
		this.AddNewLine();
		this.AddTitle("credits_lisa");
		this.AddName("Lisa Mantel");
		this.AddNewLine();
		this.AddTitle("credits_wouter");
		this.AddName("Wouter Janssen");
		this.AddNewLine();
		this.AddTitle("credits_cyber");
		this.AddName("Cyber");
		this.AddNewLine();
		this.AddTitle("credits_tumult");
		this.AddName("Tumult Kollektiv");
		this.AddNewLine();
		this.AddTitle("credits_local_heroes");
		this.AddName("Local Heroes");
		this.AddNewLine();
		this.AddTitle("language_chinese");
		this.AddName("Active Gaming Media");
		this.AddNewLine();
		this.AddTitle("language_dutch");
		this.AddName("Vincent Leeuw", "Iris Kuppen", "Lotte Busch");
		this.AddNewLine();
		this.AddTitle("language_french");
		this.AddName("Manuel Deroulers");
		this.AddNewLine();
		this.AddTitle("language_german");
		this.AddName("Jan Schäfer", "Regina Lurz", "Janina Zaghli");
		this.AddNewLine();
		this.AddTitle("language_italian");
		this.AddName("Michele Fantoni", "Gian Maria Battistini", "Gaetano Fabozzi");
		this.AddNewLine();
		this.AddTitle("language_japanese");
		this.AddName("Ziya Sarper Ekim", "Eugene Kamei-Oser", "Moeka Shimada");
		this.AddNewLine();
		this.AddTitle("language_korean");
		this.AddName("Ziya Sarper Ekim", "Junglim Kim", "Lim Yoon");
		this.AddNewLine();
		this.AddTitle("language_polish");
		this.AddName("Aleksandra Lubińska");
		this.AddNewLine();
		this.AddTitle("language_portuguese");
		this.AddName("Fábio Ludwig", "Thierry Banhete");
		this.AddNewLine();
		this.AddTitle("language_spanish");
		this.AddName("Isabel de la Mota Mendiola", "Alba Salgado Rivas", "Pedro Cortázar Pagalday");
		this.AddNewLine();
		this.AddTitle("credits_betatesting");
		this.AddName("Arjan \"Starchip\" Schipstra");
		this.AddName("Bor den Breejen");
		this.AddName("Benedikt \"1vader\" Werner");
		this.AddName("Lopidav");
		this.AddName("Marc de Jong");
		this.AddName("Margmas");
		this.AddName("NBK_RedSpy");
		this.AddName("Titouan \"Tit\" Nizet");
		this.AddName("Vsevolod \"Damglador\" Stopchanskyi");
		this.AddNewLine();
		this.AddTitle("credits_special_thanks");
		this.AddName("Boomhut", "Esther Bouma", "Neander Giljam", "Simon Naus", "Adriaan de Jongh", "Andel van Ophem", "Qkrisi");
		return this.sb.ToString();
	}
}
