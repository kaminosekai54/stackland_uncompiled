using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

public class ConfigFile
{
	public Mod Mod;

	public JObject Data;

	public Action OnSave;

	public List<ConfigEntryBase> Entries = new List<ConfigEntryBase>();

	public ConfigFile(Mod mod, string jsonPath)
	{
		this.Mod = mod;
		string text = File.ReadAllText(jsonPath);
		if (string.IsNullOrEmpty(text))
		{
			text = "{}";
		}
		this.Data = JObject.Parse(text);
	}

	public ConfigEntry<T> GetEntry<T>(string property, object defaultValue = null, ConfigUI ui = null)
	{
		return ((ConfigEntry<T>)this.Entries.FirstOrDefault((ConfigEntryBase e) => e.Name == property)) ?? new ConfigEntry<T>(property, this, defaultValue, ui);
	}

	public T GetValue<T>(string property)
	{
		if (this.Data.TryGetValue(property, out JToken value))
		{
			return value.ToObject<T>();
		}
		return default(T);
	}

	public object GetValue(string property, Type valueType)
	{
		if (this.Data.TryGetValue(property, out JToken value))
		{
			return value.ToObject(valueType);
		}
		return null;
	}

	internal void SetValue(string property, object data)
	{
		this.Data[property] = ((data == null) ? JValue.CreateNull() : JToken.FromObject(data));
	}

	public void Save()
	{
		File.WriteAllText(Path.Combine(this.Mod.Path, "config.json"), this.Data.ToString());
		this.OnSave?.Invoke();
	}
}
