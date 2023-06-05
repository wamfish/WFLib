//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public static class Asset
{
    public static string AssetFolder => Directories.Assets;
    public static bool GetFullPath(string assetName, out string fullPath)
    {
        fullPath = Path.Combine(AssetFolder, assetName);
        if (File.Exists(fullPath)) return true;
        return false;
    }
    public static bool ReadTextAsset(string path, out string val)
    {
        path = Path.Combine(Asset.AssetFolder, path);
        if (!File.Exists(path))
        {
            LogError($"Asset Not Found: {path}");
            val = "";
            return false;
        }
        Log($"Asset Found: {path}");
        val = File.ReadAllText(path);
        return true;
    }
}