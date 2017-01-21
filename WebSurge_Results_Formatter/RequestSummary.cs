using System;
namespace Html_Formatter
{
	public class RequestSummary
	{
		public string HttpVerb;
		public string Url;
		public int SuccessCount;
		public int FailureCount;
		public int AverageRequestDuration;
		public int MinRequestDuration;
		public int MaxRequestDuration;
		public string Timestamp;
		public DateTime StartTime;
		public DateTime EndTime;
		public TimeSpan Duration;
	}
}
