using System.Collections.Generic;

public static class IntToStringCacher
{
	public static Dictionary<int, string> cache = new Dictionary<int, string>();

	public static string ToStringCached(this int i)
	{
		if (!IntToStringCacher.cache.TryGetValue(i, out var value))
		{
			IntToStringCacher.cache[i] = i.ToString();
			return IntToStringCacher.cache[i];
		}
		return value;
	}
}
