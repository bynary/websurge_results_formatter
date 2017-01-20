using System;
using System.Collections.Generic;

namespace Html_Formatter
{
	public class TestRunSummary
	{
		public int TotalRequest;
		public int TotalFailed;
		public int TotalSucceeded;
		public int ThreadCount;
		public int TotalTime;
		public int RequestsPerSecond;
		public int AverageRequestDuration;
		public int MinRequestDuration;
		public int MaxRequestDuration;
		public DateTime RunDateTime;
		public List<RequestSummary> RequestSummaryList;
	}
}
