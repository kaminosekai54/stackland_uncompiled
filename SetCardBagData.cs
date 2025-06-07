using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SetCardBag", menuName = "ScriptableObjects/SetCardBag", order = 1)]
public class SetCardBagData : ScriptableObject
{
	public SetCardBagType SetCardBagType;

	public List<SimpleCardChance> Chances;

	[SerializeReference]
	public VariableFilter Filter;

	public virtual bool IsActive()
	{
		if (this.Filter != null)
		{
			return this.Filter.IsMet();
		}
		return true;
	}
}
