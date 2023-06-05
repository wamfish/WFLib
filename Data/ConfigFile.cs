namespace WFLib;
public class ConfigFile<D> : IDisposable where D : Data, new()
{
    public readonly string FilePath;
    Dictionary<string, D> configData = new();

    public ConfigFile()
    {
        FilePath = Directories.Config;
        FilePath = Path.Combine(FilePath, $"{typeof(D).Name}.txt");
        ReadConfigFile();
    }

    public D RentData()
    {
        var d = DataFactory<D>.Rent();
        return d;
    }
    public void ReturnData(D data) => DataFactory<D>.Return(data);
    public void RebuildConfigFile()
    {
        BackupConfig();
        lock (configData)
        {
            using var sw = new StreamWriter(FilePath, false);
            foreach (var kvp in configData)
            {
                WriteDataToConfigFile(sw, kvp.Key, kvp.Value);
            }
        }
    }
    private void BackupConfig()
    {
        var backupPath = Path.Combine(Directories.Config, $"{typeof(D).Name}.bak");
        using var sw = new StreamWriter(backupPath, true);
        if (!File.Exists(FilePath)) return;
        using var sr = new StreamReader(FilePath);
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            sw.WriteLine(line);
        }
    }
    public void UpdateConfig(string key, D rec)
    {
        lock (configData)
        {
            if (configData.ContainsKey(key))
            {
                rec.CopyTo(configData[key]);
            }
            else
            {
                var addRec = RentData();
                rec.CopyTo(addRec);
                configData.Add(key, addRec);
            }
            UpdateConfigFile(key, rec);
        }
    }
    public void GetConfig(string key, D rec)
    {
        lock (configData)
        {
            if (configData.ContainsKey(key))
            {
                configData[key].CopyTo(rec);
                return;
            }
            var config = RentData();
            configData.Add(key, config);
            config.CopyTo(rec);
            UpdateConfigFile(key, rec);
        }
    }
    public bool ContainsKey(string key)
    {
        lock (configData)
        {
            if (configData.ContainsKey(key)) return true;
            return false;
        }
    }
    public void Dispose()
    {
        lock (configData)
        {
            foreach (var d in configData.Values)
            {
                ReturnData(d);
            }
            configData.Clear();
        }
    }

    private void UpdateConfigFile(string key, D rec)
    {
        using (var sw = new StreamWriter(FilePath, true))
        {
            WriteDataToConfigFile(sw, key, rec);
        }
    }
    private void WriteDataToConfigFile(StreamWriter sw, string key, D rec)
    {
        sw.WriteLine($"[{key}] Update On {DateTime.Now.AsString(DATETYPE.DisplayWithMSec)}");
        sw.WriteLine("{");
        for (int f = 0; f < rec.FieldCount; f++)
        {
            sw.WriteLine($"{rec.FieldName(f)}={rec.FieldAsString(f)}");
        }
        sw.WriteLine("}");

    }
    /// <summary>
    /// Use only from constructor
    /// </summary>
    private void ReadConfigFile()
    {
        bool Rebuild = false;
        if (!File.Exists(FilePath)) return;
        using (var sr = new StreamReader(FilePath))
        {
            string key = "";
            var d = RentData();
            List<int> fields = new();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("//")) continue;
                if (ParseLine(line.AsSpan(), ref key, ref d, fields))
                {
                    if (configData.ContainsKey(key))
                    {
                        d.CopyTo(configData[key]); //update the existing record
                        d.Init();
                    }
                    else
                    {
                        configData.Add(key, d);
                        d = RentData();
                    }
                    if (fields.Count != d.FieldCount) Rebuild = true;
                    key = "";
                }
            }
            ReturnData(d);
        }
        if (Rebuild) RebuildConfigFile();
    }
    private bool ParseLine(ReadOnlySpan<char> line, ref string key, ref D data, List<int> fields)
    {
        if (line[0] == '[')
        {
            int endPos = line.IndexOf(']');
            if (key != "")
            {
                LogError("Config Read Key Error");
                return false;
            }
            key = line.Slice(1, endPos - 1).ToString().Trim();
            return false;
        }
        if (line[0] == '}')
        {
            if (key == "")
            {
                data.Init();
                fields.Clear();
                LogError("Config Read Key Error");
                return false;
            }
            return true;
        }
        if (line[0] == '{') return false;
        int pos = line.IndexOf('=');
        if (pos < 1) return false;
        var fieldName = line.Slice(0, pos).ToString().Trim();
        var fieldValue = line.Slice(pos + 1).ToString().Trim();
        int fieldId = data.FieldIdFromName(fieldName);
        if (fieldId < 0)
        {
            //the config file may contain fields that are no longer used
            //by adding to fields we may trigger a rebuild of the config file
            fields.Add(fieldId);
            LogError("Config Read Field Error");
            return false;
        }
        fields.Add(fieldId);
        data.FieldFromString(fieldValue, fieldId);
        return false;
    }

}
