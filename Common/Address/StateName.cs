//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class StateName
{
    public string Name { get; private set; }
    public string Abbreviation { get; private set; }
    public StateName(string name, string abbrev)
    {
        Name = name.ToUpper();
        Abbreviation = abbrev.ToUpper();
    }
}
