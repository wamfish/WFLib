//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
namespace WFLib;
public class WFZipWriter : IDisposable
{
    private string zipPassword = "";
    private FileStream fs;
    private ZipOutputStream OutputStream;
    public enum Status { Ok, ZipFileDoesNotExist, OutputFolderDoesNotExist, ZipFileNotOpen, ZipFileOpenError, ExceptionError, ErrorNotOpen, EntryNotFound }
    string zipFilePath = string.Empty;
    bool IsOpen = false;
    public WFZipWriter(string zipFileName, string password = "")
    {
        IsOpen = true;
        zipFilePath = zipFileName;
        zipPassword = password;
        try
        {
            fs = File.Open(zipFilePath, FileMode.Create, System.IO.FileAccess.ReadWrite);
            OutputStream = new ZipOutputStream(fs);
            OutputStream.Password = zipPassword;
            OutputStream.IsStreamOwner = false;
            OutputStream.SetLevel(9);
            IsOpen = true;
        }
        catch (Exception ex)
        {
            fs = null;
            IsOpen = false;
            LogException(ex);
        }

    }
    public Status Close()
    {
        if (!IsOpen) return Status.ErrorNotOpen;
        IsOpen = false;
        try
        {
            OutputStream.Finish();
            OutputStream.Close();
            OutputStream.Dispose();
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Status.ExceptionError;
        }
        finally
        {
            fs.Dispose();
            fs = null;
        }
        return Status.Ok;
    }
    public Status WriteByteArray(string entryName, ByteArray ba)
    {
        if (!IsOpen) return Status.ZipFileNotOpen;
        try
        {
            ZipEntry entry = new ZipEntry(entryName);
            entry.DateTime = DateTime.Now;
            if (zipPassword != "")
            {
                entry.AESKeySize = 256;
            }
            OutputStream.PutNextEntry(entry);
            OutputStream.Write(ba.Data, 0, ba.BytesUsed);
            return Status.Ok;
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Status.ExceptionError;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
    }
    ~WFZipWriter()
    {
        if (IsOpen)
        {
            Close();
        }
    }

}
public class WFZip : IDisposable
{
    private string zipPassword = "";
    private FileStream fs;
    public bool IsOpen { get; private set; } = false;
    public enum Status { Ok, ZipFileDoesNotExist, OutputFolderDoesNotExist, ZipFileNotOpen, ZipFileOpenError, ExceptionError, ErrorNotOpen, EntryNotFound }
    string zipFilePath = string.Empty;
    public WFZip()
    {
    }
    public Status Open(string zipFile, string password = "")
    {
        if (IsOpen) Close();
        zipFilePath = zipFile;
        zipPassword = password;
        try
        {
            IsOpen = true;
            fs = File.Open(zipFilePath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            return Status.Ok;
        }
        catch (Exception ex)
        {
            fs = null;
            IsOpen = false;
            LogException(ex);
            return Status.ExceptionError;
        }
    }
    public Status Close()
    {
        if (!IsOpen) return Status.ErrorNotOpen;
        IsOpen = false;
        try
        {
            fs.Flush();
            fs.Close();
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Status.ExceptionError;
        }
        finally
        {
            fs.Dispose();
            fs = null;
        }
        return Status.Ok;
    }
    public Status ReadByteArray(string entryName, ByteArray ba)
    {
        if (!IsOpen) return Status.ZipFileNotOpen;
        try
        {
            using var zf = new ZipFile(fs);
            zf.IsStreamOwner = false;
            zf.Password = zipPassword;
            foreach (ZipEntry zipEntry in zf)
            {
                if (zipEntry.Name != entryName) continue;
                int size = (int)zipEntry.Size;
                ba.Resize(size);
                using (var inStream = zf.GetInputStream(zipEntry))
                {
                    StreamUtils.ReadFully(inStream, ba.Data, 0, size);
                    ba.SetWriteIndex(size);
                }
                return Status.Ok;
            }
            Log($"ZipFile: {zipFilePath} Entry: {entryName} not found");
            return Status.EntryNotFound;
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Status.ExceptionError;
        }
    }
    public Status ExtractAll(string outFolder, string password = "")
    {
        if (!IsOpen) return Status.ZipFileNotOpen;
        if (!Directory.Exists(outFolder)) return Status.OutputFolderDoesNotExist;
        FileStream zffs = null;
        ZipFile zf = null;
        Stream inStream = null;
        FileStream outStream = null;
        try
        {
            zffs = File.OpenRead(zipFilePath);
            if (zffs == null) return Status.ZipFileOpenError;
            zf = new ZipFile(zffs);
            if (zf == null) return Status.ZipFileOpenError;
            if (!String.IsNullOrEmpty(password))
            {
                zf.Password = password;     // AES encrypted entries are handled automatically
            }
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile)
                {
                    continue;           // Ignore directories
                }
                String entryFileName = zipEntry.Name;
                byte[] buffer = new byte[4096];
                inStream = zf.GetInputStream(zipEntry);
                String fullZipToPath = Path.Combine(outFolder, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);
                outStream = File.Create(fullZipToPath);
                StreamUtils.Copy(inStream, outStream, buffer);
                outStream.Flush(true);
                outStream.Close();
                outStream.Dispose();
                outStream = null;
                inStream.Close();
                inStream.Dispose();
                inStream = null;
            }
            return Status.Ok;
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Status.ExceptionError;
        }
        finally
        {
            if (outStream != null)
            {
                outStream.Flush(true);
                outStream.Close();
                outStream.Dispose();
            }
            if (inStream != null)
            {
                inStream.Close();
                inStream.Dispose();
            }
            if (zf != null)
            {
                zf.Close();
            }
            if (zffs != null)
            {
                zffs.Close();
                zffs.Dispose();
            }
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
    }
    ~WFZip()
    {
        if (IsOpen)
        {
            Close();
        }
    }
}
