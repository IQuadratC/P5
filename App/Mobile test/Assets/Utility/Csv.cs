
using System.Collections.Generic;

namespace Utility
{
    public static class Csv
    {
        public static List<List<string>> ParseCVSFile(string csvString)
        {
            List<List<string>> csvData = new List<List<string>>();
            string[] lines = csvString.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                csvData.Add(new List<string>());
                string[] fields = lines[i].Split(',',';');
                for (var j = 0; j < fields.Length; j++)
                {
                    csvData[i].Add(fields[j].Replace("\r",""));
                }
            }

            return csvData;
        } 
    }
}
