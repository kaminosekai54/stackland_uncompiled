using System.Collections.Generic;

public class MonthlyRequirementResult
{
	public Dictionary<string, MonthlyResult> results;

	public MonthlyRequirementResult()
	{
		this.results = new Dictionary<string, MonthlyResult>();
	}
}
