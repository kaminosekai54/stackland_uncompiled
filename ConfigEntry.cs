using System;

public class ConfigEntry<T> : ConfigEntryBase
{
	internal object _cachedValue;

	internal bool _cached;

	public Action<T> OnChanged;

	public T Value
	{
		get
		{
			return (T)(this._cached ? this._cachedValue : ((object)base.Config.GetValue<T>(base.Name)));
		}
		set
		{
			this._cached = true;
			this._cachedValue = value;
			base.Config.SetValue(base.Name, value);
			this.OnChanged?.Invoke(value);
		}
	}

	public override object BoxedValue
	{
		get
		{
			return this.Value;
		}
		set
		{
			this.Value = (T)value;
		}
	}

	public ConfigEntry(string name, ConfigFile config, object defaultValue = null, ConfigUI ui = null)
	{
		base.Name = name;
		base.Config = config;
		this.BoxedValue = base.Config.GetValue(base.Name, typeof(T)) ?? defaultValue;
		base.ValueType = typeof(T);
		this._cached = true;
		if (ui != null)
		{
			base.UI = ui;
		}
		base.Config.Entries.Add(this);
	}
}
