using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PluralsightBot.Helpers
{
    public static class AIRecogniser
    {
        public static (DateTime, TimeSpan) RecogniseDateTimeRange(string source, out string rawString)
        {
            List<ModelResult> aiResults = DateTimeRecognizer.RecognizeDateTime(source, Culture.English);
            if (aiResults.Count == 0)
                throw new Exception("Error: Couldn't recognise any time ranges in that source string.");

            /* Example contents of the below dictionary:
				[0]: {[timex, 2018-11-11T06:15]}
				[1]: {[type, datetime]}
				[2]: {[value, 2018-11-11 06:15:00]}
			*/

            rawString = aiResults[0].Text;
            Dictionary<string, string> aiResult = unwindResult(aiResults[0]);
            foreach (KeyValuePair<string, string> kvp in aiResult)
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            string type = aiResult["type"];

            if (type != "datetimerange")
                throw new Exception($"Error: An invalid type of {type} was encountered ('datetimerange' expected).");


            return (
                DateTime.Parse(aiResult["start"]),
                DateTime.Parse(aiResult["end"]) - DateTime.Parse(aiResult["start"])
            );
        }

        public static DateTime RecogniseDateTime(string source, out string rawString)
        {
            List<ModelResult> aiResults = DateTimeRecognizer.RecognizeDateTime(source, Culture.English);
            if (aiResults.Count == 0)
                throw new Exception("Error: Couldn't recognise any dates or times in that source string.");

            /* Example contents of the below dictionary:
				[0]: {[timex, 2018-11-11T06:15]}
				[1]: {[type, datetime]}
				[2]: {[value, 2018-11-11 06:15:00]}
			*/

            rawString = aiResults[0].Text;
            Dictionary<string, string> aiResult = unwindResult(aiResults[0]);
            string type = aiResult["type"];
            if (!(new string[] { "datetime", "date", "time", "datetimerange", "daterange", "timerange" }).Contains(type))
                throw new Exception($"Error: An invalid type of {type} was encountered ('datetime' expected).");


            string result = Regex.IsMatch(type, @"range$") ? aiResult["start"] : aiResult["value"];
            return DateTime.Parse(result);
        }


        private static Dictionary<string, string> unwindResult(ModelResult modelResult)
        {
            return (modelResult.Resolution["values"] as List<Dictionary<string, string>>)[0];
        }
    }
}
