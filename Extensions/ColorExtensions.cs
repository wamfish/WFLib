//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class ColorExtensions
{
    public static string AsString(this Godot.Color color) => color.ToHtml();
    public static Godot.Color AsColor(this string val) => new Godot.Color(val);
}
