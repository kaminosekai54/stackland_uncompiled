public class ActionTimeBase
{
	public delegate bool ActionTimeBaseSpeedFunc(ActionTimeParams parameters);

	public ActionTimeBaseSpeedFunc Matches;

	public float BaseSpeed;

	public ActionTimeBase(ActionTimeBaseSpeedFunc baseSpeedFunc, float baseSpeed)
	{
		this.Matches = baseSpeedFunc;
		this.BaseSpeed = baseSpeed;
	}
}
