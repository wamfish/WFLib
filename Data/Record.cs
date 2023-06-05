//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

using System.Data;
using System.Diagnostics;

namespace WFLib;

public abstract class Record : Data
{
    public byte StatusCode { get; set; }
    public int ID  { get => _id; set => _id = value; }
    public DateTime Timestamp { get; set; }
    public int EditByID { get; set; }
    public bool IsActive
    {
        get
        {
            if (StatusCode == 'A') return true;
            if (StatusCode == 'U') return true;
            return false;
        }
    }
    public abstract void InitContextFactory();

}
