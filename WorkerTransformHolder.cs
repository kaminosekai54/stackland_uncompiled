using System.Collections.Generic;
using UnityEngine;

public class WorkerTransformHolder : MonoBehaviour
{
	private List<GameObject> workers = new List<GameObject>();

	public GameObject WorkerPositionPrefab;

	public GameObject WorkerPositionMiddle;

	public float WorkerAmountOffset = 0.1f;

	private void Update()
	{
	}

	public Transform GetTransformAtIndex(int index)
	{
		return this.workers[Mathf.Clamp(index, 0, this.workers.Count - 1)].transform;
	}

	public void UpdateWorkerAmount(int workerAmount)
	{
		foreach (GameObject worker in this.workers)
		{
			Object.Destroy(worker.gameObject);
		}
		this.workers.Clear();
		for (int i = 0; i < workerAmount; i++)
		{
			Vector3 position = this.WorkerPositionMiddle.transform.position + new Vector3((float)i * this.WorkerAmountOffset - (float)(workerAmount / 2) * this.WorkerAmountOffset + this.WorkerAmountOffset / 2f * ((workerAmount % 2 == 0) ? 1f : 0f), 0f, 0f);
			GameObject item = Object.Instantiate(this.WorkerPositionPrefab, position, this.WorkerPositionMiddle.transform.rotation, base.transform);
			this.workers.Add(item);
		}
	}
}
