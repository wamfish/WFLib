//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

public class PoolStats
{
    static List<PoolStats> stats = new();
    private static List<string> StatsList(bool asHtml = false)
    {
        List<string> statsList = new();
        lock (stats)
        {
            foreach (var s in stats)
            {
                statsList.Add(s.BuildStats(asHtml));
            }
        }
        return statsList;
    }
    public static List<string> AllStats => StatsList();
    public static List<string> AllHtmlStats => StatsList(true);
    public string Stats => BuildStats();
    public string HtmlStats => BuildStats(true);
    private string BuildStats(bool asHtml = false)
    {
        string lineSep = "";

        var sb = StringBuilderPool.Rent();
        string title;
        if (asHtml) 
        {
            lineSep = "<br/>";
            title = $"{Name} Stats:";
            //we need to encode the title so it will display correctly in html
            title = System.Net.WebUtility.HtmlEncode(title);
            title = $"<b>{title}</b><br/>";
        }
        else
        {
            title = $"{Name} Stats:";
        }
        sb.AppendLine(title);
        if (PoolCount != null)
        {
            sb.AppendLine($"Pool Count: {PoolCount()}{lineSep}");
        }
        sb.AppendLine($"Rent Count: {RentCount}{lineSep}");
        sb.AppendLine($"Rent From Pool: {RentFromPoolCount}{lineSep}");
        sb.AppendLine($"Rent From New: {RentFromNewCount}{lineSep}");
        sb.AppendLine($"Return count: {ReturnCount}{lineSep}");
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public readonly string Name;
    public readonly Func<int> PoolCount;
    public PoolStats(string name, Func<int> poolCount)
    {
        Name = name;
        PoolCount = poolCount;
        lock (stats)
        {
            stats.Add(this);
        }
    }
    public long RentCount = 0;
    public long RentFromPoolCount = 0;
    public long RentFromNewCount = 0;
    public long ReturnCount = 0;
}
