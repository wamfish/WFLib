using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFLib;

public enum ValidationMessageEnum {
    None, Required, PasswordLength, 
    PasswordMatch, Email, Phone, 
    Zip, Date, Time, DateTime, Number, 
    Decimal, Currency, Integer, 
    IntegerRange, DecimalRange, CurrencyRange, 
    NumberRange, StringLength, 
    StringLengthRange, StringLengthMin, StringLengthMax, 
    StringLengthRangeMin, StringLengthRangeMax, StringLengthRangeMinMax,COUNT }
public static class Validation
{
    public static string[] Message = new string[(int)ValidationMessageEnum.COUNT];
    static Validation()
    {
        Message[(int)ValidationMessageEnum.None] = "";
        Message[(int)ValidationMessageEnum.Required] = "Required";
        Message[(int)ValidationMessageEnum.PasswordLength] = "Password must be at least 8 characters";
        Message[(int)ValidationMessageEnum.PasswordMatch] = "Passwords do not match";
        Message[(int)ValidationMessageEnum.Email] = "Invalid email address";
        Message[(int)ValidationMessageEnum.Phone] = "Invalid phone number";
        Message[(int)ValidationMessageEnum.Zip] = "Invalid zip code";
        Message[(int)ValidationMessageEnum.Date] = "Invalid date";
        Message[(int)ValidationMessageEnum.Time] = "Invalid time";
        Message[(int)ValidationMessageEnum.DateTime] = "Invalid date/time";
        Message[(int)ValidationMessageEnum.Number] = "Invalid number";
        Message[(int)ValidationMessageEnum.Decimal] = "Invalid decimal";
        Message[(int)ValidationMessageEnum.Currency] = "Invalid currency";
        Message[(int)ValidationMessageEnum.Integer] = "Invalid integer";
        Message[(int)ValidationMessageEnum.IntegerRange] = "Invalid integer range";
        Message[(int)ValidationMessageEnum.DecimalRange] = "Invalid decimal range";
        Message[(int)ValidationMessageEnum.CurrencyRange] = "Invalid currency range";
        Message[(int)ValidationMessageEnum.NumberRange] = "Invalid number range";
        Message[(int)ValidationMessageEnum.StringLength] = "Invalid string length";
        Message[(int)ValidationMessageEnum.StringLengthRange] = "Invalid string length range";
        Message[(int)ValidationMessageEnum.StringLengthMin] = "Invalid string length min";
        Message[(int)ValidationMessageEnum.StringLengthMax] = "Invalid string length max";
        Message[(int)ValidationMessageEnum.StringLengthRangeMin] = "Invalid string length range min";
        Message[(int)ValidationMessageEnum.StringLengthRangeMax] = "Invalid string length range max";
        Message[(int)ValidationMessageEnum.StringLengthRangeMinMax] = "Invalid string length range min/max";
    }   
}
