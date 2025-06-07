using System.Collections.Generic;

public class CardStringSplitter
{
	private static CardStringSplitter _instance;

	private Dictionary<string, string[]> stringToSplit = new Dictionary<string, string[]>();

	public static CardStringSplitter me
	{
		get
		{
			if (CardStringSplitter._instance == null)
			{
				CardStringSplitter._instance = new CardStringSplitter();
			}
			return CardStringSplitter._instance;
		}
	}

	public string[] Split(string s)
	{
		if (this.stringToSplit.TryGetValue(s, out var value))
		{
			return value;
		}
		string[] array = s.Split('|');
		this.stringToSplit[s] = array;
		return array;
	}
}
