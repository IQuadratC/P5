using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CSV
{
    public static string[,] ParseCVSFile(string csvString)
    {
        string[] lines = csvString.Split('\n');
        string[] fields = lines[0].Split(',',';');
        
        string[,] csvData = new string[lines.Length,fields.Length];
        
        for (var i = 0; i < lines.Length; i++)
        {
            fields = lines[i].Split(',',';');
            for (var j = 0; j < fields.Length; i++)
            {
                csvData[i,j] = fields[j];
            }
        }
        return csvData;
    }
}
