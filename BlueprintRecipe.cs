using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlueprintRecipe : Blueprint
{
	[Header("Recipe")]
	public string[] Ingredients;

	public string[] ResultItems;

	public float CookingTime;

	public override void Init(GameDataLoader loader)
	{
		base.NeedsExactMatch = false;
		this.PopulateSubprints();
		base.Init(loader);
	}

	private void PopulateSubprints()
	{
		base.Subprints.Clear();
		string[] array = new string[2] { "campfire", "stove" };
		float[] array2 = new float[2] { 1f, 0.3f };
		for (int i = 0; i < array.Length; i++)
		{
			string item = array[i];
			List<string> list = new List<string>();
			list.Add(item);
			list.AddRange(this.Ingredients);
			base.Subprints.Add(new Subprint
			{
				StatusTerm = CardData.CardToTermId(this) + "_status_0",
				ExtraResultCards = this.ResultItems.ToArray(),
				RequiredCards = list.ToArray(),
				Time = this.CookingTime * array2[i]
			});
		}
	}
}
