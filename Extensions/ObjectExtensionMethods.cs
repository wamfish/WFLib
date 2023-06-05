//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class ObjectExtensionMethods
{
    public static string ClassName(this System.Object obj)
    {
        Type myType = obj.GetType();
        return myType.Name;
    }
}
