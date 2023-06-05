//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public abstract class DataUtil<D> where D : Data, new()
{
    //public static void Initialize(D d)
    //{
    //    using var context = RentContext();
    //    context.Initialize(d);
    //}
    //public static void Clear(D d)
    //{
    //    using var context = RentContext();
    //    context.Clear(d);
    //}
    //public static void Serialize(D d, SerializationBuffer sb)
    //{
    //    using var context = RentContext();
    //    context.Serialize(d,sb);
    //}
    //public static void Deserialize(D d, SerializationBuffer sb)
    //{
    //    using var context = RentContext();
    //    context.Deserialize(d, sb);
    //}

    public static DataContext<D> RentContext()
    {
        return DataContextFactory<D>.Rent();
    }
    public static D RentData()
    {
        var data = DataFactory<D>.Rent();
        data.Init();
        return data;
    }
    public static void ReturnData(D data)
    {
        data.Clear();
        DataFactory<D>.Return(data);
    }
}
