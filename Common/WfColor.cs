//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public struct WfColor
{
    public WfColor(uint color)
    {
        R = 0;
        G = 0;
        B = 0;
        A = 0;
        Set(color);
    }
    public byte R;
    public byte G;
    public byte B;
    public byte A;
    public uint UInt => (uint)(R << 24 & G << 16 & B << 8 & A);
    public uint Color32 => (uint)(R << 24 & G << 16 & B << 8 & A);
    public void Set(uint color)
    {
        uint r = color & 0xFF000000;
        uint g = color & 0x00FF0000;
        uint b = color & 0x0000FF00;
        uint a = color & 0x000000FF;
        R = (byte)(r >> 24);
        G = (byte)(g >> 16);
        B = (byte)(b >> 8);
        A = (byte)a;
    }
    public static class Color
    {
        public static WfColor Black => new WfColor(0x000000ff);
        public static WfColor White => new WfColor(0xffffffff);
        public static WfColor Grey => new WfColor(0x9e9e9eff);
        public static WfColor Red => new WfColor(0xb71c1cff);
        public static WfColor Green => new WfColor(0x1b5e20ff);
        public static WfColor Yellow => new WfColor(0xffeb3bff);
    }
}
