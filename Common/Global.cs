//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using Microsoft.AspNetCore.Identity;

namespace WFLib;
public enum COLORRANGE
{
    Amber, Blue, LightBlue, Brown, Cyan,
    Gray, BlueGray, Green, LightGreen, Indigo,
    Lime, Red, Pink, Orange, DeepOrange, Purple, DeepPurple,
    Teal, Yellow, COUNT
}
public static partial class Global
{
    
    public static readonly bool IsWebAssembly = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier.Contains("wasm");

    public static LogLevel GlobalLogLevel = LogLevel.Information;

    public static bool Log(string message)
    {
        if (GlobalLogLevel > LogLevel.Information) return true;
        Logger.Message(message);
        return true;
    }
    public static bool LogMessage(string message)
    {
        if (GlobalLogLevel > LogLevel.Information) return true;
        Logger.Message(message);
        return true;
    }
    public static bool LogException(Exception ex)
    {
        if (GlobalLogLevel > LogLevel.Critical) return true;
        Logger.Exception(ex);
        return true;
    }
    public static bool LogError(string error)
    {
        if (GlobalLogLevel > LogLevel.Error) return true;
        Logger.Error(error);
        return true;
    }
    public static bool LogWarning(string msg)
    {
        if (GlobalLogLevel > LogLevel.Warning) return true;
        Logger.Warning(msg);
        return true;
    }
    public const char LISTSEP = (char)1;
    public const char VALUESEP = (char)2;
    public const char FIELDSEP = (char)3;
    public const char DATASTART = (char)4;
    public const char DATAEND = (char)5;
    public static void ParseFloats(ReadOnlySpan<char> value, Span<float> floats, char sepChar)
    {
        if (value.Length == 0)
        {
            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] = 0f;
            }
            return;
        }
        int startPos;
        int endPos;
        for (int i = 0, f = 0; i < value.Length && f < floats.Length; i++, f++)
        {
            startPos = i;
            for (i++; i < value.Length && value[i] != sepChar; i++) { }
            endPos = i;
            var s = value.Slice(startPos, endPos - startPos);
            floats[f] = s.AsFloat();
        }
    }
    public static void ParseDoubles(ReadOnlySpan<char> value, Span<double> doubles, char sepChar)
    {
        if (value.Length == 0)
        {
            for (int i = 0; i < doubles.Length; i++)
            {
                doubles[i] = 0;
            }
            return;
        }
        int startPos;
        int endPos;
        for (int i = 0, d = 0; i < value.Length && d < doubles.Length; i++, d++)
        {
            startPos = i;
            for (i++; i < value.Length && value[i] != sepChar; i++) { }
            endPos = i;
            var s = value.Slice(startPos, endPos - startPos);
            doubles[d] = s.AsDouble();
        }
    }
    public static void ParseInts(ReadOnlySpan<char> value, Span<int> ints, char sepChar)
    {
        if (value.Length == 0)
        {
            for (int i = 0; i < ints.Length; i++)
            {
                ints[i] = 0;
            }
            return;
        }
        int startPos;
        int endPos;
        for (int i = 0, f = 0; i < value.Length && f < ints.Length; i++, f++)
        {
            startPos = i;
            for (i++; i < value.Length && value[i] != sepChar; i++) { }
            endPos = i;
            var s = value.Slice(startPos, endPos - startPos);
            ints[f] = s.AsInt();
        }
    }
    public static class Date
    {
        public static DateTime Current => DateTime.UtcNow;
        public static string CurrentYear => DateTime.UtcNow.Year.AsString();
    }
    public static string GetStackTrace(int count)
    {
        var st2 = System.Environment.StackTrace;
        var stlines = st2.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
        var sb = StringBuilderPool.Rent();
        for (int i = 2; i < count + 2 && i < stlines.Count(); i++)
        {
            sb.AppendLine(stlines[i]);
        }
        var r = sb.ToString();
        sb.Return();
        return r;
    }
    private static PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
    public static string PasswordHash(string password)
    {
        return _passwordHasher.HashPassword("n/a", password);
    }
    public static bool PasswordVerify(string hashedPassword, string providedPassword)
    {
        var r = _passwordHasher.VerifyHashedPassword("n/a", hashedPassword, providedPassword);
        if (r == PasswordVerificationResult.Success) return true;
        return false;
    }

    public static Color GetColor(COLORRANGE range, int level)
    {
        var cr = ColorRanges[(int)range];
        return new Color(cr[level]);
    }
    static uint[][] ColorRanges = new uint[(int)COLORRANGE.COUNT][];
    static void SetRange(COLORRANGE range, uint[] colors)
    {
        ColorRanges[(int)range] = colors;
    }
    static Global()
    {
        SetRange(COLORRANGE.Amber, Amber);
        SetRange(COLORRANGE.Blue, Blue);
        SetRange(COLORRANGE.LightBlue, LightBlue);
        SetRange(COLORRANGE.Brown, Brown);
        SetRange(COLORRANGE.Cyan, Cyan);
        //Grey,BlueGray,Green,LightGreen,Indigo,
        SetRange(COLORRANGE.Gray, Gray);
        SetRange(COLORRANGE.BlueGray, BlueGray);
        SetRange(COLORRANGE.Green, Green);
        SetRange(COLORRANGE.LightGreen, LightGreen);
        SetRange(COLORRANGE.Indigo, Indigo);
        //Lime,Red,Pink,Orange,DeepOrange,Purple,DeepPurple,
        SetRange(COLORRANGE.Lime, Lime);
        SetRange(COLORRANGE.Red, Red);
        SetRange(COLORRANGE.Pink, Pink);
        SetRange(COLORRANGE.Orange, Orange);
        SetRange(COLORRANGE.DeepOrange, DeepOrange);
        SetRange(COLORRANGE.Purple, Purple);
        SetRange(COLORRANGE.DeepPurple, DeepPurple);
        //Teal, Yellow, COUNT
        SetRange(COLORRANGE.Teal, Teal);
        SetRange(COLORRANGE.Yellow, Yellow);
    }
    public readonly static Color Transparent = new Color(0, 0, 0, 0);
    static uint[] Amber = { 0xFFF8E1ff, 0xFFECB3ff, 0xFFE082ff, 0xFFD54Fff, 0xFFCA28ff, 0xFFC107ff, 0xFFB300ff, 0xFFA000ff, 0xFF8F00ff, 0xFF6F00ff };
    public readonly static Color Amber0 = new Color(Amber[0]);
    public readonly static Color Amber1 = new Color(Amber[1]);
    public readonly static Color Amber2 = new Color(Amber[2]);
    public readonly static Color Amber3 = new Color(Amber[3]);
    public readonly static Color Amber4 = new Color(Amber[4]);
    public readonly static Color Amber5 = new Color(Amber[5]);
    public readonly static Color Amber6 = new Color(Amber[6]);
    public readonly static Color Amber7 = new Color(Amber[7]);
    public readonly static Color Amber8 = new Color(Amber[8]);
    public readonly static Color Amber9 = new Color(Amber[9]);

    static uint[] Blue = { 0xE3F2FDff, 0xBBDEFBff, 0x90CAF9ff, 0x64B5F6ff, 0x42A5F5ff, 0x2196F3ff, 0x1E88E5ff, 0x1976D2ff, 0x1565C0ff, 0x0D47A1ff };
    public readonly static Color Blue0 = new Color(Blue[0]);
    public readonly static Color Blue1 = new Color(Blue[1]);
    public readonly static Color Blue2 = new Color(Blue[2]);
    public readonly static Color Blue3 = new Color(Blue[3]);
    public readonly static Color Blue4 = new Color(Blue[4]);
    public readonly static Color Blue5 = new Color(Blue[5]);
    public readonly static Color Blue6 = new Color(Blue[6]);
    public readonly static Color Blue7 = new Color(Blue[7]);
    public readonly static Color Blue8 = new Color(Blue[8]);
    public readonly static Color Blue9 = new Color(Blue[9]);

    static uint[] LightBlue = { 0xE1F5FEff, 0xB3E5FCff, 0x81D4FAff, 0x4FC3F7ff, 0x29B6F6ff, 0x03A9F4ff, 0x039BE5ff, 0x0288D1ff, 0x0277BDff, 0x01579Bff };
    public readonly static Color LightBlue0 = new Color(LightBlue[0]);
    public readonly static Color LightBlue1 = new Color(LightBlue[1]);
    public readonly static Color LightBlue2 = new Color(LightBlue[2]);
    public readonly static Color LightBlue3 = new Color(LightBlue[3]);
    public readonly static Color LightBlue4 = new Color(LightBlue[4]);
    public readonly static Color LightBlue5 = new Color(LightBlue[5]);
    public readonly static Color LightBlue6 = new Color(LightBlue[6]);
    public readonly static Color LightBlue7 = new Color(LightBlue[7]);
    public readonly static Color LightBlue8 = new Color(LightBlue[8]);
    public readonly static Color LightBlue9 = new Color(LightBlue[9]);

    static uint[] Brown = { 0xEFEBE9ff, 0xD7CCC8ff, 0xBCAAA4ff, 0xA1887Fff, 0x8D6E63ff, 0x795548ff, 0x6D4C41ff, 0x5D4037ff, 0x4E342Eff, 0x3E2723ff };
    public readonly static Color Brown0 = new Color(Brown[0]);
    public readonly static Color Brown1 = new Color(Brown[1]);
    public readonly static Color Brown2 = new Color(Brown[2]);
    public readonly static Color Brown3 = new Color(Brown[3]);
    public readonly static Color Brown4 = new Color(Brown[4]);
    public readonly static Color Brown5 = new Color(Brown[5]);
    public readonly static Color Brown6 = new Color(Brown[6]);
    public readonly static Color Brown7 = new Color(Brown[7]);
    public readonly static Color Brown8 = new Color(Brown[8]);
    public readonly static Color Brown9 = new Color(Brown[9]);

    static uint[] Cyan = { 0xE0F7FAff, 0xB2EBF2ff, 0x80DEEAff, 0x4DD0E1ff, 0x26C6DAff, 0x00BCD4ff, 0x00ACC1ff, 0x0097A7ff, 0x00838Fff, 0x006064ff };
    public readonly static Color Cyan0 = new Color(Cyan[0]);
    public readonly static Color Cyan1 = new Color(Cyan[1]);
    public readonly static Color Cyan2 = new Color(Cyan[2]);
    public readonly static Color Cyan3 = new Color(Cyan[3]);
    public readonly static Color Cyan4 = new Color(Cyan[4]);
    public readonly static Color Cyan5 = new Color(Cyan[5]);
    public readonly static Color Cyan6 = new Color(Cyan[6]);
    public readonly static Color Cyan7 = new Color(Cyan[7]);
    public readonly static Color Cyan8 = new Color(Cyan[8]);
    public readonly static Color Cyan9 = new Color(Cyan[9]);

    public readonly static Color Black = Colors.Black;
    public readonly static Color White = Colors.White;

    static uint[] Gray = { 0xFAFAFAff, 0xF5F5F5ff, 0xEEEEEEff, 0xE0E0E0ff, 0xBDBDBDff, 0x9E9E9Eff, 0x757575ff, 0x616161ff, 0x424242ff, 0x212121ff };
    public readonly static Color Gray0 = new Color(Gray[0]);
    public readonly static Color Gray1 = new Color(Gray[1]);
    public readonly static Color Gray2 = new Color(Gray[2]);
    public readonly static Color Gray3 = new Color(Gray[3]);
    public readonly static Color Gray4 = new Color(Gray[4]);
    public readonly static Color Gray5 = new Color(Gray[5]);
    public readonly static Color Gray6 = new Color(Gray[6]);
    public readonly static Color Gray7 = new Color(Gray[7]);
    public readonly static Color Gray8 = new Color(Gray[8]);
    public readonly static Color Gray9 = new Color(Gray[9]);


    static uint[] BlueGray = { 0xECEFF1FF, 0xCFD8DCFF, 0xB0BEC5FF, 0x90A4AEFF, 0x78909CFF, 0x607D8BFF, 0x546E7AFF, 0x455A64FF, 0x37474FFF, 0x263238FF };
    public readonly static Color BlueGray0 = new Color(BlueGray[0]);
    public readonly static Color BlueGray1 = new Color(BlueGray[1]);
    public readonly static Color BlueGray2 = new Color(BlueGray[2]);
    public readonly static Color BlueGray3 = new Color(BlueGray[3]);
    public readonly static Color BlueGray4 = new Color(BlueGray[4]);
    public readonly static Color BlueGray5 = new Color(BlueGray[5]);
    public readonly static Color BlueGray6 = new Color(BlueGray[6]);
    public readonly static Color BlueGray7 = new Color(BlueGray[7]);
    public readonly static Color BlueGray8 = new Color(BlueGray[8]);
    public readonly static Color BlueGray9 = new Color(BlueGray[9]);

    static uint[] Green = { 0xE8F5E9ff, 0xC8E6C9ff, 0xA5D6A7ff, 0x81C784ff, 0x66BB6Aff, 0x4CAF50ff, 0x43A047ff, 0x388E3Cff, 0x2E7D32ff, 0x1B5E20ff };
    public readonly static Color Green0 = new Color(Green[0]);
    public readonly static Color Green1 = new Color(Green[1]);
    public readonly static Color Green2 = new Color(Green[2]);
    public readonly static Color Green3 = new Color(Green[3]);
    public readonly static Color Green4 = new Color(Green[4]);
    public readonly static Color Green5 = new Color(Green[5]);
    public readonly static Color Green6 = new Color(Green[6]);
    public readonly static Color Green7 = new Color(Green[7]);
    public readonly static Color Green8 = new Color(Green[8]);
    public readonly static Color Green9 = new Color(Green[9]);

    static uint[] LightGreen = { 0xF1F8E9ff, 0xDCEDC8ff, 0xC5E1A5ff, 0xAED581ff, 0x9CCC65ff, 0x8BC34Aff, 0x7CB342ff, 0x689F38ff, 0x558B2Fff, 0x33691Eff };
    public readonly static Color LightGreen0 = new Color(LightGreen[0]);
    public readonly static Color LightGreen1 = new Color(LightGreen[1]);
    public readonly static Color LightGreen2 = new Color(LightGreen[2]);
    public readonly static Color LightGreen3 = new Color(LightGreen[3]);
    public readonly static Color LightGreen4 = new Color(LightGreen[4]);
    public readonly static Color LightGreen5 = new Color(LightGreen[5]);
    public readonly static Color LightGreen6 = new Color(LightGreen[6]);
    public readonly static Color LightGreen7 = new Color(LightGreen[7]);
    public readonly static Color LightGreen8 = new Color(LightGreen[8]);
    public readonly static Color LightGreen9 = new Color(LightGreen[9]);

    static uint[] Indigo = { 0xE8EAF6ff, 0xC5CAE9ff, 0x9FA8DAff, 0x7986CBff, 0x5C6BC0ff, 0x3F51B5ff, 0x3949ABff, 0x303F9Fff, 0x283593ff, 0x1A237Eff };
    public readonly static Color Indigo0 = new Color(Indigo[0]);
    public readonly static Color Indigo1 = new Color(Indigo[1]);
    public readonly static Color Indigo2 = new Color(Indigo[2]);
    public readonly static Color Indigo3 = new Color(Indigo[3]);
    public readonly static Color Indigo4 = new Color(Indigo[4]);
    public readonly static Color Indigo5 = new Color(Indigo[5]);
    public readonly static Color Indigo6 = new Color(Indigo[6]);
    public readonly static Color Indigo7 = new Color(Indigo[7]);
    public readonly static Color Indigo8 = new Color(Indigo[8]);
    public readonly static Color Indigo9 = new Color(Indigo[9]);

    static uint[] Lime = { 0xF9FBE7ff, 0xF0F4C3ff, 0xE6EE9Cff, 0xDCE775ff, 0xD4E157ff, 0xCDDC39ff, 0xC0CA33ff, 0xAFB42Bff, 0x9E9D24ff, 0x827717ff };
    public readonly static Color Lime0 = new Color(Lime[0]);
    public readonly static Color Lime1 = new Color(Lime[1]);
    public readonly static Color Lime2 = new Color(Lime[2]);
    public readonly static Color Lime3 = new Color(Lime[3]);
    public readonly static Color Lime4 = new Color(Lime[4]);
    public readonly static Color Lime5 = new Color(Lime[5]);
    public readonly static Color Lime6 = new Color(Lime[6]);
    public readonly static Color Lime7 = new Color(Lime[7]);
    public readonly static Color Lime8 = new Color(Lime[8]);
    public readonly static Color Lime9 = new Color(Lime[9]);

    static uint[] Red = { 0xFFEBEEff, 0xFFCDD2ff, 0xEF9A9Aff, 0xE57373ff, 0xEF5350ff, 0xF44336ff, 0xE53935ff, 0xD32F2Fff, 0xC62828ff, 0xB71C1Cff };
    public readonly static Color Red0 = new Color(Red[0]);
    public readonly static Color Red1 = new Color(Red[1]);
    public readonly static Color Red2 = new Color(Red[2]);
    public readonly static Color Red3 = new Color(Red[3]);
    public readonly static Color Red4 = new Color(Red[4]);
    public readonly static Color Red5 = new Color(Red[5]);
    public readonly static Color Red6 = new Color(Red[6]);
    public readonly static Color Red7 = new Color(Red[7]);
    public readonly static Color Red8 = new Color(Red[8]);
    public readonly static Color Red9 = new Color(Red[9]);

    static uint[] Pink = { 0xFCE4ECff, 0xF8BBD0ff, 0xF48FB1ff, 0xF06292ff, 0xEC407Aff, 0xE91E63ff, 0xD81B60ff, 0xC2185Bff, 0xAD1457ff, 0x880E4Fff };
    public readonly static Color Pink0 = new Color(Pink[0]);
    public readonly static Color Pink1 = new Color(Pink[1]);
    public readonly static Color Pink2 = new Color(Pink[2]);
    public readonly static Color Pink3 = new Color(Pink[3]);
    public readonly static Color Pink4 = new Color(Pink[4]);
    public readonly static Color Pink5 = new Color(Pink[5]);
    public readonly static Color Pink6 = new Color(Pink[6]);
    public readonly static Color Pink7 = new Color(Pink[7]);
    public readonly static Color Pink8 = new Color(Pink[8]);
    public readonly static Color Pink9 = new Color(Pink[9]);

    static uint[] Orange = { 0xFFF3E0ff, 0xFFE0B2ff, 0xFFCC80ff, 0xFFB74Dff, 0xFFA726ff, 0xFF9800ff, 0xFB8C00ff, 0xF57C00ff, 0xEF6C00ff, 0xE65100ff };
    public readonly static Color Orange0 = new Color(Orange[0]);
    public readonly static Color Orange1 = new Color(Orange[1]);
    public readonly static Color Orange2 = new Color(Orange[2]);
    public readonly static Color Orange3 = new Color(Orange[3]);
    public readonly static Color Orange4 = new Color(Orange[4]);
    public readonly static Color Orange5 = new Color(Orange[5]);
    public readonly static Color Orange6 = new Color(Orange[6]);
    public readonly static Color Orange7 = new Color(Orange[7]);
    public readonly static Color Orange8 = new Color(Orange[8]);
    public readonly static Color Orange9 = new Color(Orange[9]);

    static uint[] DeepOrange = { 0xFBE9E7ff, 0xFFCCBCff, 0xFFAB91ff, 0xFF8A65ff, 0xFF7043ff, 0xFF5722ff, 0xF4511Eff, 0xE64A19ff, 0xD84315ff, 0xBF360Cff };
    public readonly static Color DeepOrange0 = new Color(DeepOrange[0]);
    public readonly static Color DeepOrange1 = new Color(DeepOrange[1]);
    public readonly static Color DeepOrange2 = new Color(DeepOrange[2]);
    public readonly static Color DeepOrange3 = new Color(DeepOrange[3]);
    public readonly static Color DeepOrange4 = new Color(DeepOrange[4]);
    public readonly static Color DeepOrange5 = new Color(DeepOrange[5]);
    public readonly static Color DeepOrange6 = new Color(DeepOrange[6]);
    public readonly static Color DeepOrange7 = new Color(DeepOrange[7]);
    public readonly static Color DeepOrange8 = new Color(DeepOrange[8]);
    public readonly static Color DeepOrange9 = new Color(DeepOrange[9]);
    static uint[] Purple = { 0xF3E5F5ff, 0xE1BEE7ff, 0xCE93D8ff, 0xBA68C8ff, 0xAB47BCff, 0x9C27B0ff, 0x8E24AAff, 0x7B1FA2ff, 0x6A1B9Aff, 0x4A148Cff };
    public readonly static Color Purple0 = new Color(Purple[0]);
    public readonly static Color Purple1 = new Color(Purple[1]);
    public readonly static Color Purple2 = new Color(Purple[2]);
    public readonly static Color Purple3 = new Color(Purple[3]);
    public readonly static Color Purple4 = new Color(Purple[4]);
    public readonly static Color Purple5 = new Color(Purple[5]);
    public readonly static Color Purple6 = new Color(Purple[6]);
    public readonly static Color Purple7 = new Color(Purple[7]);
    public readonly static Color Purple8 = new Color(Purple[8]);
    public readonly static Color Purple9 = new Color(Purple[9]);

    static uint[] DeepPurple = { 0xEDE7F6ff, 0xD1C4E9ff, 0xB39DDBff, 0x9575CDff, 0x7E57C2ff, 0x673AB7ff, 0x5E35B1ff, 0x512DA8ff, 0x4527A0ff, 0x311B92ff };
    public readonly static Color DeepPurple0 = new Color(DeepPurple[0]);
    public readonly static Color DeepPurple1 = new Color(DeepPurple[1]);
    public readonly static Color DeepPurple2 = new Color(DeepPurple[2]);
    public readonly static Color DeepPurple3 = new Color(DeepPurple[3]);
    public readonly static Color DeepPurple4 = new Color(DeepPurple[4]);
    public readonly static Color DeepPurple5 = new Color(DeepPurple[5]);
    public readonly static Color DeepPurple6 = new Color(DeepPurple[6]);
    public readonly static Color DeepPurple7 = new Color(DeepPurple[7]);
    public readonly static Color DeepPurple8 = new Color(DeepPurple[8]);
    public readonly static Color DeepPurple9 = new Color(DeepPurple[9]);

    static uint[] Teal = { 0xE0F2F1ff, 0xB2DFDBff, 0x80CBC4ff, 0x4DB6ACff, 0x26A69Aff, 0x009688ff, 0x00897Bff, 0x00796Bff, 0x00695Cff, 0x004D40ff };
    public readonly static Color Teal0 = new Color(Teal[0]);
    public readonly static Color Teal1 = new Color(Teal[1]);
    public readonly static Color Teal2 = new Color(Teal[2]);
    public readonly static Color Teal3 = new Color(Teal[3]);
    public readonly static Color Teal4 = new Color(Teal[4]);
    public readonly static Color Teal5 = new Color(Teal[5]);
    public readonly static Color Teal6 = new Color(Teal[6]);
    public readonly static Color Teal7 = new Color(Teal[7]);
    public readonly static Color Teal8 = new Color(Teal[8]);
    public readonly static Color Teal9 = new Color(Teal[9]);

    static uint[] Yellow = { 0xFFFDE7ff, 0xFFF9C4ff, 0xFFF59Dff, 0xFFF176ff, 0xFFEE58ff, 0xFFEB3Bff, 0xFDD835ff, 0xFBC02Dff, 0xF9A825ff, 0xF57F17ff };
    public readonly static Color Yellow0 = new Color(Yellow[0]);
    public readonly static Color Yellow1 = new Color(Yellow[1]);
    public readonly static Color Yellow2 = new Color(Yellow[2]);
    public readonly static Color Yellow3 = new Color(Yellow[3]);
    public readonly static Color Yellow4 = new Color(Yellow[4]);
    public readonly static Color Yellow5 = new Color(Yellow[5]);
    public readonly static Color Yellow6 = new Color(Yellow[6]);
    public readonly static Color Yellow7 = new Color(Yellow[7]);
    public readonly static Color Yellow8 = new Color(Yellow[8]);
    public readonly static Color Yellow9 = new Color(Yellow[9]);


}
