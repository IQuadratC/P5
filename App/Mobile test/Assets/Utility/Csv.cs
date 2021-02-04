
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
                string[] fields = lines[i].Split(',',';');
                csvData.Add(new List<string>());
                for (var j = 0; j < fields.Length; j++)
                {
                    csvData[i].Add(fields[j].Replace("\r",""));
                }
            }

            return csvData;
        } 
    }
}
