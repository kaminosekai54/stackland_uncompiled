public struct SubprintMatchInfo
{
	public int FullyMatchedAt;

	public int MatchCount;

	public SubprintMatchInfo(int matchedAt, int matchCount)
	{
		this.FullyMatchedAt = matchedAt;
		this.MatchCount = matchCount;
	}
}
