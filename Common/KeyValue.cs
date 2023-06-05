//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class KeyValue
{
    public bool HasChanged { get; private set; } = false;
    public Dictionary<string, string> keyValueDict { get; set; }
    public KeyValue()
    {
        keyValueDict = new Dictionary<string, string>();
    }
    public void UpdateOrAdd(string key, string value)
    {
        lock (keyValueDict)
        {
            HasChanged = true;
            if (keyValueDict.ContainsKey(key))
            {
                keyValueDict[key] = value;
                return;
            }
            keyValueDict.Add(key, value);
            return;
        }
    }
    public string GetValue(string key)
    {
        lock (keyValueDict)
        {
            if (keyValueDict.TryGetValue(key, out string value))
            {
                return value;
            }
            return string.Empty;
        }
    }
}