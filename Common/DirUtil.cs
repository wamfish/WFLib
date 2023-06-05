namespace WFGenFix;

public class DirUtil
{
    private DirectoryInfo dirInfo;
    public bool IsValidFolder => dirInfo.Exists;
    List<string> files = new();
    public DirUtil(string folder)
    {
        dirInfo = new DirectoryInfo(folder);
        var a = dirInfo.Attributes;

    }
    public void ListDirs()
    {
        var dirs = dirInfo.EnumerateDirectories();
        foreach (var dir in dirs)
        {
            Console.WriteLine(dir.FullName);
        }
    }
    public void RemoveDirs()
    {
        var dirs = dirInfo.EnumerateDirectories();
        foreach (var dir in dirs)
        {

            Directory.Delete(dir.FullName, true);
            Console.WriteLine($"Remove: {dir.FullName}");
        }
    }
}
