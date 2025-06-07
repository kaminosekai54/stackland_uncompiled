using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
	[Serializable]
	public class ShapeDrawerPrefab
	{
		public ShapeDrawer Prefab;

		public int Count;
	}

	public static DrawManager instance;

	public List<ShapeDrawerPrefab> Prefabs;

	public List<IShape> ShapesToDraw = new List<IShape>();

	private List<ShapeDrawer> takenShapeDrawers = new List<ShapeDrawer>();

	private Dictionary<Type, List<ShapeDrawer>> shapeObjectPools = new Dictionary<Type, List<ShapeDrawer>>();

	public int ShapesToDrawCount = -1;

	private void Awake()
	{
		DrawManager.instance = this;
		foreach (ShapeDrawerPrefab prefab in this.Prefabs)
		{
			for (int i = 0; i < prefab.Count; i++)
			{
				if (!this.shapeObjectPools.ContainsKey(prefab.Prefab.DrawingType))
				{
					this.shapeObjectPools.Add(prefab.Prefab.DrawingType, new List<ShapeDrawer>());
				}
				this.MakeShapeObject(prefab.Prefab);
			}
		}
	}

	private ShapeDrawer GetPrefabFromShape(IShape shape)
	{
		if (shape == null)
		{
			return null;
		}
		return this.Prefabs.Find((ShapeDrawerPrefab x) => x.Prefab.DrawingType == shape.GetType())?.Prefab;
	}

	private ShapeDrawer MakeShapeObject(ShapeDrawer prefab)
	{
		ShapeDrawer shapeDrawer = UnityEngine.Object.Instantiate(prefab);
		shapeDrawer.transform.SetParentClean(base.transform);
		shapeDrawer.gameObject.SetActive(value: false);
		this.shapeObjectPools[shapeDrawer.DrawingType].Add(shapeDrawer);
		return shapeDrawer;
	}

	private void Update()
	{
		this.ShapesToDraw.Clear();
		foreach (ShapeDrawer takenShapeDrawer in this.takenShapeDrawers)
		{
			takenShapeDrawer.gameObject.SetActive(value: false);
			this.shapeObjectPools[takenShapeDrawer.DrawingType].Insert(0, takenShapeDrawer);
		}
		this.takenShapeDrawers.Clear();
	}

	private ShapeDrawer GetShapeDrawerForShape(IShape shape)
	{
		List<ShapeDrawer> list = this.shapeObjectPools[shape.GetType()];
		ShapeDrawer shapeDrawer = null;
		if (list.Count > 0)
		{
			shapeDrawer = list[0];
		}
		else
		{
			ShapeDrawer prefabFromShape = this.GetPrefabFromShape(shape);
			if (prefabFromShape != null)
			{
				shapeDrawer = this.MakeShapeObject(prefabFromShape);
			}
		}
		this.takenShapeDrawers.Add(shapeDrawer);
		list.Remove(shapeDrawer);
		return shapeDrawer;
	}

	private void LateUpdate()
	{
		this.ShapesToDrawCount = this.ShapesToDraw.Count;
		foreach (IShape item in this.ShapesToDraw)
		{
			ShapeDrawer shapeDrawerForShape = this.GetShapeDrawerForShape(item);
			if (shapeDrawerForShape == null)
			{
				Debug.LogError($"ShapeDrawer pool is empty, could not draw {item.GetType()}!");
				continue;
			}
			shapeDrawerForShape.gameObject.SetActive(value: true);
			shapeDrawerForShape.MyShape = item;
			shapeDrawerForShape.UpdateShape();
		}
	}

	public void DrawShape(IShape shape)
	{
		this.ShapesToDraw.Add(shape);
	}
}
