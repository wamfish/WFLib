//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

public static class TableFactory<T, R> where T : Table<R>, new() where R : Record, new()
{
    static T table = new T();
    public static T Rent()
    {
        return table;
    }
}
