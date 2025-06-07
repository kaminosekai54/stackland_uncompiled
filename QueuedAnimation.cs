using System;

public class QueuedAnimation
{
	public Action OnActivate;

	public string Id;

	public QueuedAnimation(Action act, string id = null)
	{
		this.OnActivate = act;
		this.Id = id;
	}
}
