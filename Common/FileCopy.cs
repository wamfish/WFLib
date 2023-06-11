namespace WFLib;
public static partial class Global
{
    public static void FileCopyToDirectory(string source, string destination, bool overwrite = false)
    {
        if (overwrite)
        {
            File.Copy(source, Path.Combine(destination, Path.GetFileName(source)), true);
        }
        else
        {
            if (File.Exists(Path.Combine(destination, Path.GetFileName(source))))
            {
                throw new IOException($"File {Path.Combine(destination, Path.GetFileName(source))} already exists.");
            }
            File.Copy(source, Path.Combine(destination, Path.GetFileName(source)));
        }
    }
    public static void FileCopy(string source, string destination, bool overwrite = false)
    {
        if (overwrite)
        {
            File.Copy(source, destination, true);
        }
        else
        {
            if (File.Exists(destination))
            {
                throw new IOException($"File {destination} already exists.");
            }
            File.Copy(source, destination);
        }
    }
}
