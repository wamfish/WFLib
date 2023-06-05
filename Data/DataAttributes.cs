//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KAttribute : Attribute
{
    public KAttribute()
    {
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class XAttribute : Attribute
{
    public XAttribute()
    {
    }
}
//[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
//public sealed class RangeAttribute : Attribute
//{
//    public double Min { get; private set; }
//    public double Max { get; private set; }
//    public RangeAttribute(float min, float max)
//    {
//        Min = min;
//        Max = max;
//    }
//}
