using System;
using UnityEngine;

public class ModLogger
{
	public ModManifest Manifest;

	public ModLogger(ModManifest manifest)
	{
		this.Manifest = manifest;
	}

	public void Log(LogType logType, string message)
	{
		Debug.unityLogger.Log(logType, $"[{ModLogger.FormatTime(DateTime.Now)}] [{logType} : {this.Manifest.Id}] {message}");
	}

	public void Log(string message)
	{
		this.Log(LogType.Log, message);
	}

	public void LogWarning(string message)
	{
		this.Log(LogType.Warning, message);
	}

	public void LogError(string message)
	{
		this.Log(LogType.Error, message);
	}

	public void LogException(string message)
	{
		this.Log(LogType.Exception, message);
	}

	public static string FormatTime(DateTime dt)
	{
		return dt.ToString("HH:mm:ss");
	}
}
