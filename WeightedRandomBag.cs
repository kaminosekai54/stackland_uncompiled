using System.Collections.Generic;
using UnityEngine;

public class WeightedRandomBag<T>
{
	public struct Entry
	{
		public float accumulatedWeight;

		public float weight;

		public T item;
	}

	private List<Entry> entries = new List<Entry>();

	private float totalWeight;

	private Entry lastPickedEntry;

	public int Count => this.entries.Count;

	public void AddEntry(T item, float weight)
	{
		this.totalWeight += weight;
		this.entries.Add(new Entry
		{
			item = item,
			accumulatedWeight = this.totalWeight,
			weight = weight
		});
	}

	public T Choose()
	{
		float num = Random.value * this.totalWeight;
		foreach (Entry entry in this.entries)
		{
			if (entry.accumulatedWeight >= num)
			{
				this.lastPickedEntry = entry;
				return entry.item;
			}
		}
		return default(T);
	}

	public Entry GetLastPickedEntry()
	{
		return this.lastPickedEntry;
	}

	public void Clear()
	{
		this.entries.Clear();
		this.totalWeight = 0f;
	}
}
