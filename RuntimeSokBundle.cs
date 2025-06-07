using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RuntimeSokBundle : ISokBundle
{
	private AssetBundle myAssetBundle;

	public bool Load(string id)
	{
		string path = "";
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			path = Path.Combine(Application.dataPath, "../", id, "PC", id);
		}
		else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			path = Path.Combine(Application.dataPath, "../../", id, "macOS", id);
		}
		this.myAssetBundle = AssetBundle.LoadFromFile(path);
		if (this.myAssetBundle == null)
		{
			Debug.LogError("No asset bundle found at path " + Path.GetFullPath(path));
			return false;
		}
		return true;
	}

	public List<T> LoadAssets<T>() where T : Object
	{
		return this.myAssetBundle.LoadAllAssets<T>().ToList();
	}
}
