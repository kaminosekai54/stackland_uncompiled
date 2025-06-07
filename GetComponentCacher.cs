using System.Collections.Generic;
using UnityEngine;

public class GetComponentCacher<T> where T : MonoBehaviour
{
	private Dictionary<int, T> gameObjectToComponent = new Dictionary<int, T>();

	public T GetComponent(GameObject go)
	{
		int instanceID = go.GetInstanceID();
		if (!this.gameObjectToComponent.TryGetValue(instanceID, out var value))
		{
			T component = go.GetComponent<T>();
			this.gameObjectToComponent[instanceID] = component;
			return component;
		}
		return value;
	}
}
