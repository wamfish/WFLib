using System.Reflection.Metadata.Ecma335;

public static class Directories
{
    /// <summary>
    ///		The path to the UserData directory.
    /// </summary>
    public static string UserData { get; private set; } = MakeUserDir("");
    /// <summary>
    ///		The path to the Logs folder inside of UserData.
    /// </summary>
    public static string Logs { get; private set; } = MakeUserDir("Logs");
    /// <summary>
    ///		The path to the Tables folder inside of UserData.
    /// </summary>
    public static string Tables { get; private set; } = MakeUserDir("Tables");
    /// <summary>
    ///		The path to the Config folder inside of UserData.
    /// </summary>
    public static string Config { get; private set; } = MakeUserDir("Config");
    /// <summary>
    ///		The path to the Assets folder. This folder is usually in the same directory as the app.exe.
    /// </summary>
    /// 
    private static string _assets = "";
    public static string Assets 
    { 
        get
        {
            if (_assets.Length > 0) return _assets;
            if (IsWebAssembly) return "Assets";
            string dir = Directory.GetCurrentDirectory();
            string path = Path.Combine(dir, "Assets");
            while (dir.Length > 0)
            {
                if (Directory.Exists(path)) return path;
                dir = Path.GetDirectoryName(dir);
                path = Path.Combine(dir, "Assets");
            }
            path = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
            _assets = path;
            return _assets;
        }
    }
    public static void SetNearApp(string subdir)
    {
        string dir = Directory.GetCurrentDirectory();
        dir = Path.GetDirectoryName(dir);
        string path = Path.Combine(dir, subdir);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        Log($"Data Path: {path}");
        SetDirs(path);
    }
    //SetDirs(newUserDataPath) changes the default location for UserData directory.
    public static void SetDirs(string newUserDataPath)
    {
        if (!Directory.Exists(newUserDataPath)) return;
        UserData = MakeUserDir("", newUserDataPath);
        Logs = MakeUserDir("Logs", newUserDataPath);
        Tables = MakeUserDir("Tables", newUserDataPath);
        Config = MakeUserDir("Config", newUserDataPath);
    }
    public static void SetDirs() => SetDirs(string.Empty);
    //SetAssestDir(newAssetsPath) changes the location of the Assets directory;
    public static void SetAssetsDir(string newAssetsPath)
    {
        _assets = newAssetsPath;
    }
    private static string MakeUserDir(string dir, string userDataPath = "")
    {
        string fullPath;
        if (userDataPath != null && userDataPath.Length > 0)
        {
            fullPath = userDataPath;
        }
        else
        {

            fullPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Wamfish");
        }
        if (dir.Length > 0) fullPath = Path.Combine(fullPath, dir);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        return fullPath;
    }
    //private static string GetAssetsDir()
    //{
    //    if (IsWebAssembly) return "Assets";
    //    string dir = Directory.GetCurrentDirectory();
    //    string path = Path.Combine(dir, "Assets");
    //    while (dir.Length > 0)
    //    {
    //        if (Directory.Exists(path)) return path;
    //        dir = Path.GetDirectoryName(dir);
    //        path = Path.Combine(dir, "Assets");
    //    }
    //    path = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
    //    return path;
    //}
}