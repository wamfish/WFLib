//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public static partial class AddressUtil
{
    static AddressUtil()
    {
        InitAddressData();
    }
    static Dictionary<string, string> secUnitAbbrevLookup = new Dictionary<string, string>();
    static Dictionary<string, string> addressDirAbbrevLookup = new Dictionary<string, string>();
    static Dictionary<string, string> addressDirLookup = new Dictionary<string, string>();
    static List<StateName> stateNames = new List<StateName>();
    public static Dictionary<string, string> stateAbbrevLookup = new Dictionary<string, string>();
    public static Dictionary<string, string> stateLookup = new Dictionary<string, string>();
    public static string GetStateAbbrev(string state)
    {
        if (stateAbbrevLookup.TryGetValue(state, out string abbrev))
        {
            return abbrev;
        }
        return state;
    }
    public static string GetStateFromAbbrev(string abbrev)
    {
        if (stateLookup.TryGetValue(abbrev, out string state))
        {
            return state;
        }
        return abbrev;
    }
    public static string GetDirAbbrev(string dir)
    {
        if (addressDirAbbrevLookup.TryGetValue(dir, out string abbrev))
        {
            return abbrev;
        }
        return dir;
    }
    public static string GetDir(string abbrev)
    {
        if (addressDirLookup.TryGetValue(abbrev, out string dir))
        {
            return dir;
        }
        return abbrev;
    }
    public static string GetSecondayUnitAbbrev(string unit)
    {
        if (secUnitAbbrevLookup.TryGetValue(unit.ToUpper(), out string abbrev))
        {
            return abbrev;
        }
        return unit;
    }
    static void AddUnitAbbrev(string unit, string abbrev)
    {
        secUnitAbbrevLookup.Add(unit.ToUpper(), abbrev.ToUpper());
    }
    static void InitSecondaryUnitDesignator()
    {
        AddUnitAbbrev("APARTMENT", "APT");
        AddUnitAbbrev("BASEMENT", "BSMT");
        AddUnitAbbrev("", "#");
        AddUnitAbbrev("BUILDING", "BLDG");
        AddUnitAbbrev("Department", "DEPT");
        AddUnitAbbrev("Floor", "FL");
        AddUnitAbbrev("Front", "FRNT");
        AddUnitAbbrev("Hanger", "HNGR");
        AddUnitAbbrev("Key", "KEY");
        AddUnitAbbrev("Lobby", "LBBY");
        AddUnitAbbrev("Lot", "LOT");
        AddUnitAbbrev("Lower", "LOWR");
        AddUnitAbbrev("Office", "OFC");
        AddUnitAbbrev("Penthouse", "PH");
        AddUnitAbbrev("Pier", "PIER");
        AddUnitAbbrev("Rear", "REAR");
        AddUnitAbbrev("Room", "RM");
        AddUnitAbbrev("Side", "SIDE");
        AddUnitAbbrev("Slip", "SLIP");
        AddUnitAbbrev("Space", "SPC");
        AddUnitAbbrev("Stop", "STOP");
        AddUnitAbbrev("Suite", "STE");
        AddUnitAbbrev("Trailer", "TRLR");
        AddUnitAbbrev("Unit", "UNIT");
        AddUnitAbbrev("Upper", "UPPR");
    }
    static void InitAddressData()
    {
        {
            stateNames.Add(new StateName("Alabama", "AL"));
            stateNames.Add(new StateName("Alaska", "AK"));
            stateNames.Add(new StateName("American Samoa", "AS"));
            stateNames.Add(new StateName("Arizona", "AZ"));
            stateNames.Add(new StateName("Arkansas", "AR"));
            stateNames.Add(new StateName("California", "CA"));
            stateNames.Add(new StateName("Colorado", "CO"));
            stateNames.Add(new StateName("Connecticut", "CT"));
            stateNames.Add(new StateName("Delaware", "DE"));
            stateNames.Add(new StateName("District of Columbia", "DC"));
            stateNames.Add(new StateName("Federated States of Micronesia", "FM"));
            stateNames.Add(new StateName("Florida", "FL"));
            stateNames.Add(new StateName("Georgia", "GA"));
            stateNames.Add(new StateName("Guam", "GU"));
            stateNames.Add(new StateName("Hawaii", "HI"));
            stateNames.Add(new StateName("Idaho", "ID"));
            stateNames.Add(new StateName("Illinois", "IL"));
            stateNames.Add(new StateName("Indiana", "IN"));
            stateNames.Add(new StateName("Iowa", "IA"));
            stateNames.Add(new StateName("Kansas", "KS"));
            stateNames.Add(new StateName("Kentucky", "KY"));
            stateNames.Add(new StateName("Louisiana", "LA"));
            stateNames.Add(new StateName("Maine", "ME"));
            stateNames.Add(new StateName("Marshall Islands", "MH"));
            stateNames.Add(new StateName("Maryland", "MD"));
            stateNames.Add(new StateName("Massachusetts", "MA"));
            stateNames.Add(new StateName("Michigan", "MI"));
            stateNames.Add(new StateName("Minnesota", "MN"));
            stateNames.Add(new StateName("Mississippi", "MS"));
            stateNames.Add(new StateName("Missouri", "MO"));
            stateNames.Add(new StateName("Montana", "MT"));
            stateNames.Add(new StateName("Nebraska", "NE"));
            stateNames.Add(new StateName("Nevada", "NV"));
            stateNames.Add(new StateName("New Hampshire", "NH"));
            stateNames.Add(new StateName("New Jersey", "NJ"));
            stateNames.Add(new StateName("New Mexico", "NM"));
            stateNames.Add(new StateName("New York", "NY"));
            stateNames.Add(new StateName("North Carolina", "NC"));
            stateNames.Add(new StateName("North Dakota", "ND"));
            stateNames.Add(new StateName("Northern Mariana Islands", "MP"));
            stateNames.Add(new StateName("Ohio", "OH"));
            stateNames.Add(new StateName("Oklahoma", "OK"));
            stateNames.Add(new StateName("Oregon", "OR"));
            stateNames.Add(new StateName("Palau", "PW"));
            stateNames.Add(new StateName("Pennsylvania", "PA"));
            stateNames.Add(new StateName("Puerto Rico", "PR"));
            stateNames.Add(new StateName("Rhode Island", "RI"));
            stateNames.Add(new StateName("South Carolina", "SC"));
            stateNames.Add(new StateName("South Dakota", "SD"));
            stateNames.Add(new StateName("Tennessee", "TN"));
            stateNames.Add(new StateName("Texas", "TX"));
            stateNames.Add(new StateName("Utah", "UT"));
            stateNames.Add(new StateName("Vermont", "VT"));
            stateNames.Add(new StateName("Virgin Islands", "VI"));
            stateNames.Add(new StateName("Virginia", "VA"));
            stateNames.Add(new StateName("Washington", "WA"));
            stateNames.Add(new StateName("West Virginia", "WV"));
            stateNames.Add(new StateName("Wisconsin", "WI"));
            stateNames.Add(new StateName("Wyoming", "WY"));
        }
        foreach (var sn in stateNames)
        {
            stateAbbrevLookup.Add(sn.Name, sn.Abbreviation);
            stateLookup.Add(sn.Abbreviation, sn.Name);
        }
        addressDirAbbrevLookup.Add("NORTH", "N");
        addressDirAbbrevLookup.Add("N", "N");
        addressDirAbbrevLookup.Add("EAST", "E");
        addressDirAbbrevLookup.Add("E", "E");
        addressDirAbbrevLookup.Add("SOUTH", "S");
        addressDirAbbrevLookup.Add("S", "S");
        addressDirAbbrevLookup.Add("WEST", "W");
        addressDirAbbrevLookup.Add("W", "W");
        addressDirAbbrevLookup.Add("NORTHWEST", "NW");
        addressDirAbbrevLookup.Add("NW", "NW");
        addressDirAbbrevLookup.Add("NORTHEAST", "NE");
        addressDirAbbrevLookup.Add("NE", "NE");
        addressDirAbbrevLookup.Add("SOUTHWEST", "SW");
        addressDirAbbrevLookup.Add("SW", "SW");
        addressDirAbbrevLookup.Add("SOUTHEAST", "SE");
        addressDirAbbrevLookup.Add("SE", "SE");
        addressDirLookup.Add("N", "NORTH");
        addressDirLookup.Add("NORTH", "NORTH");
        addressDirLookup.Add("E", "EAST");
        addressDirLookup.Add("EAST", "EAST");
        addressDirLookup.Add("SOUTH", "SOUTH");
        addressDirLookup.Add("S", "SOUTH");
        addressDirLookup.Add("WEST", "WEST");
        addressDirLookup.Add("W", "WEST");
        addressDirLookup.Add("NORTHWEST", "NORTHWEST");
        addressDirLookup.Add("NW", "NORTHWEST");
        addressDirLookup.Add("NORTHEAST", "NORTHEAST");
        addressDirLookup.Add("NE", "NORTHEAST");
        addressDirLookup.Add("SOUTHWEST", "SOUTHWEST");
        addressDirLookup.Add("SW", "SOUTHWEST");
        addressDirLookup.Add("SOUTHEAST", "SOUTHEAST");
        addressDirLookup.Add("SE", "SOUTHEAST");
        InitSecondaryUnitDesignator();
    }
}