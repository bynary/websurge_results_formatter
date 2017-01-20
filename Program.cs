using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Html_Formatter;

class Program
{
	static readonly string htmlOutputFileName = "./test_results.htm";
	static readonly string cssOutputFileName = "websurge_results.css";
	static string webSurgeResultsInputFile = string.Empty;

	static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			ExitWithMessage("An argument specifying the input json file is required.");
		}

		if (args.Length > 1)
		{
			ExitWithMessage("Currently only one argument is supported. This argument must be the filepath to the json test results file.");
		}

		string path = args[0];

		//If the file path exists, load the json into an object and generate the HTML file
		if (!string.IsNullOrWhiteSpace(path))
		{
			webSurgeResultsInputFile = path;
			List<TestResult> TestResultList = LoadJson(path);
			TestRunSummary testRunSummary = GenerateTestRunSummary(TestResultList);
			GenerateHtml(testRunSummary);
			WriteCssFileToOutputFolder();
		}
		else
		{
			ExitWithMessage("File path is empty. A path to the json file is required as the first parameter.");
		}

		ExitWithMessage($"Html file {htmlOutputFileName} was successfully generated.");
	}

	/// <summary>
	/// Loads the json.
	/// </summary>
	/// <returns>Deserialized json as a TestResultList object</returns>
	/// <param name="path">Path.</param>
	static List<TestResult> LoadJson(string path)
	{
		//Check for null path parameter
		if (string.IsNullOrWhiteSpace(path))
		{
			Console.WriteLine("path: " + path);
			ExitWithMessage("path is either null or an empty string.");
		}

		//Load and deserialize the json from a file
		try
		{
			using (StreamReader streamReader = File.OpenText(path))
			{
				Console.WriteLine("Loading json...");

				string rawJson = streamReader.ReadToEnd();
				List<TestResult> testResultList = JsonConvert.DeserializeObject<List<TestResult>>(rawJson);

				Console.WriteLine("Json loaded");

				return testResultList;
			}
		}
		catch(Exception ex)
		{
			ExitWithMessage("An error occurred while reading the json file from \"" + path + "\"" + Environment.NewLine + ex.Message);
		}

		return null;
	}

	static TestRunSummary GenerateTestRunSummary(List<TestResult> testResultList)
	{
		TestRunSummary testRunSummary = new TestRunSummary();
		testRunSummary.RequestSummaryList = new List<RequestSummary>();

		var distinctRequests = testResultList.Select(x => new { x.Url, x.HttpVerb } ).Distinct();

		foreach (var request in distinctRequests)
		{
			RequestSummary requestSummary = new RequestSummary();
			requestSummary.Url = request.Url;
			requestSummary.HttpVerb = request.HttpVerb;
			requestSummary.SuccessCount = testResultList.Where(x => x.Url.Equals(request.Url) && x.IsError.Equals("false")).Count();
			requestSummary.FailureCount = testResultList.Where(x => x.Url.Equals(request.Url) && x.IsError.Equals("true")).Count();
			requestSummary.AverageRequestDuration = (int)testResultList.Where(x => x.Url.Equals(request.Url)).Average(x => Convert.ToInt32(x.TimeTakenMs));
			requestSummary.MaxRequestDuration = testResultList.Where(x => x.Url.Equals(request.Url)).Max(x => Convert.ToInt32(x.TimeTakenMs));
			requestSummary.MinRequestDuration = testResultList.Where(x => x.Url.Equals(request.Url)).Min(x => Convert.ToInt32(x.TimeTakenMs));

			testRunSummary.RequestSummaryList.Add(requestSummary);
		}

		return testRunSummary;
	}

	/// <summary>
	/// Generates the html.
	/// </summary>
	/// <param name="testRunSummary">Test result list.</param>
	static void GenerateHtml(TestRunSummary testRunSummary)
	{
		StringBuilder html = new StringBuilder();
		html.Append("<html>");
		html.Append("<head>");
		html.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"websurge_results.css\">");
		html.Append("</head>");
		html.Append("<body>");
		html.Append("<header>");
		html.Append("<h1>");
		html.Append("WebSurge Test Results");
		html.Append("</h1>");
		html.Append("<div class=\"detail\">");
		html.Append($"test results source: {webSurgeResultsInputFile}");
		html.Append("</div>");
		html.Append("<div class=\"detail\">");
		html.Append($"date time generated: {DateTime.Now}");
		html.Append("</div>");
		html.Append("</header>");
		//html.Append("<main>");

		foreach (var requestSummary in testRunSummary.RequestSummaryList)
		{
			html.Append("<div class=\"row\">");
			html.Append("<div class=\"request\">");
			html.Append("<div class=\"http-verb\">");
			html.Append($"{requestSummary.HttpVerb}");
			html.Append("</div>");
			html.Append("<div class=\"url\">");
			html.Append($"{requestSummary.Url}");
			html.Append("</div>");
			html.Append("</div>");
			html.Append("<div class=\"results\">");
			html.Append("<div class=\"duration\">");
			html.Append("<span class=\"duration-header\">Duration</span>");
			html.Append("<div class=\"\">");
			html.Append($"Avg: {requestSummary.AverageRequestDuration}ms");
			html.Append("</div>");
			html.Append("<div class=\"\">");
			html.Append($"Min: {requestSummary.MinRequestDuration}ms");
			html.Append("</div>");
			html.Append("<div class=\"\">");
			html.Append($"Max: {requestSummary.MaxRequestDuration}ms");
			html.Append("</div>");
			html.Append("</div>");
			html.Append("<div class=\"result-count success\">");
			html.Append($"Success: {requestSummary.SuccessCount}");
			html.Append("</div>");
			html.Append("<div class=\"result-count fail\">");
			html.Append($"Fail: {requestSummary.FailureCount}");
			html.Append("</div>");
			html.Append("</div>");
			html.Append("</div>");
		}

		//html.Append("</main>");
		html.Append("<footer>");
		html.Append("<p>");
		html.Append($"HTML WebSurge test results generated using WebSurge_Results_Formatter - &copy;{DateTime.Now.Year} Coding Inertia");
		html.Append("</p>");
		html.Append("</footer>");
		html.Append("</body>");
		html.Append("</html>");

		File.WriteAllText(htmlOutputFileName, html.ToString());
	}

	static void WriteCssFileToOutputFolder()
	{
		Console.WriteLine($"Writing css to {cssOutputFileName}...");

		var assembly = typeof(Program).GetTypeInfo().Assembly;
		using (StreamReader streamReader = new StreamReader(assembly.GetManifestResourceStream($"WebSurge_Results_Formatter.Styles.{cssOutputFileName}")))
		{
			string rawCss = streamReader.ReadToEnd();
			File.WriteAllText($"./{cssOutputFileName}", rawCss);
		}

		Console.WriteLine($"Done writing css");
	}

	static void ExitWithMessage(string message)
	{
		Console.WriteLine(message);
		Environment.Exit(0);
	}

	static void PauseConsole()
	{
		Console.ReadKey();
	}
}

