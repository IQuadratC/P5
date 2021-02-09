
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public static class Csv
    {
        public static string[][] ParseCVSFile(string csvString)
        {
            string[] lines = csvString.Split('\n');
            string[][] csvData = new string[lines.Length][];
            
            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(',',';');
                fields[fields.Length - 1] = fields[fields.Length - 1].Replace("\r","");
                csvData[i] = fields;
            }
            return csvData;
        } 
    }
}
