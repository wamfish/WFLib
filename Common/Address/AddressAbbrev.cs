//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

using System.Reflection;
using System.Resources;
namespace WFLib;
public class AddressAbbrev
{
    static Dictionary<string, AddressAbbrev> abbrevLookup = new Dictionary<string, AddressAbbrev>();
    static void Add(string name, string abbrev, params string[] otherNames)
    {
        name = name.ToUpper();
        abbrev = abbrev.ToUpper();
        for (int i = 0; i < otherNames.Length; i++)
        {
            otherNames[i] = otherNames[i].ToUpper();
        }
        var aa = new AddressAbbrev(name, abbrev, otherNames);
        //Console.WriteLine(name);
        abbrevLookup.Add(name, aa);
        if (name != abbrev)
        {
            //Console.WriteLine(abbrev);
            abbrevLookup.Add(abbrev, aa);
        }
        for (int i = 0; i < otherNames.Length; i++)
        {
            abbrevLookup.Add(otherNames[i], aa);
        }
    }
    public static string GetAddressAbbrev(string abbrev)
    {
        if (abbrevLookup.TryGetValue(abbrev, out AddressAbbrev val))
        {
            return val.Abbrev;
        }
        return abbrev;
    }
    static AddressAbbrev()
    {
        Assembly assem = typeof(AddressAbbrev).Assembly;
        var rns = assem.GetManifestResourceNames();
        var rr = new ResourceReader(assem.GetManifestResourceStream(rns[0]));
        rr.GetResourceData("StreetSuffixTable", out string resType, out byte[] res);
        char check = Convert.ToChar(res[0]);
        string[] suffixTable;
        if (check >= 'a' && check <= 'z' || check >= 'A' && check <= 'Z')
        {
            suffixTable = res.AsString().Split('\n');
        }
        else
        {
            suffixTable = Encoding.UTF8.GetString(res, 2, res.Length - 2).Split('\n');
        }

        foreach (var line in suffixTable)
        {
            var nline = line.Replace("\r", "");
            var parts = nline.Split(',');
            string[] other = new string[parts.Length - 2];
            for (int i = 2; i < parts.Length; i++)
            {
                other[i - 2] = parts[i];
            }
            Add(parts[0], parts[1], other);
        }
        rr.Close();
        //Console.WriteLine($"Lookup Count:{abbrevLookup.Count}");
    }
    public string Name { get; private set; }
    public string Abbrev { get; private set; }
    public string[] OtherNames { get; private set; }
    private AddressAbbrev(string name, string abbrev, params string[] otherNames)
    {
        Name = name;
        Abbrev = abbrev;
        OtherNames = otherNames;
    }

}
