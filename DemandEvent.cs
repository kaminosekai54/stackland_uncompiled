using System;

[Serializable]
public class DemandEvent
{
	public string DemandId;

	public int MonthStarted;

	public int Duration;

	public string BoardId;

	public bool Completed;

	public bool Successful;

	public int AmountGiven;

	public Demand Demand => DemandManager.instance.GetDemandById(this.DemandId);

	public int MonthCompleted => this.Duration + this.MonthStarted;

	public DemandEvent()
	{
	}

	public DemandEvent(string demandId, int monthStarted, int duration, string boardId)
	{
		this.DemandId = demandId;
		this.MonthStarted = monthStarted;
		this.Duration = duration;
		this.BoardId = boardId;
	}
}
