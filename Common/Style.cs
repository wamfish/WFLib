//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class Style
{
    public Style()
    {
        FontSize = 18f;
        ForeColor = WfColor.Color.Black;
        BackColor = WfColor.Color.White;
        LineColor = WfColor.Color.White;
        SetPadding(0, 0, 0, 0);
    }
    public Style(Style lsi)
    {
        lsi.CopyTo(this);
    }
    public void SetPaddingInInches(float lpInInches, float rpInInches, float tpInInches, float bpInInches)
    {
        LeftPadInInches = lpInInches;
        RightPadInInches = rpInInches;
        TopPadInInches = tpInInches;
        BotPadInInches = bpInInches;
    }
    public void SetPadding(float lp, float rp, float tp, float bp)
    {
        LeftPadInInches = lp;
        RightPadInInches = rp;
        TopPadInInches = tp;
        BotPadInInches = bp;
    }
    public float FontSize { get; set; }
    public float LeftPad { get; set; }
    public float LeftPadInInches { get => Util.WidthToInches(LeftPad); set => LeftPad = Util.WidthFromInches(value); }
    public int LeftPadInPixels { get => (int)LeftPad; set => LeftPad = value; }
    public float RightPad { get; set; }
    public float RightPadInInches { get => Util.WidthToInches(RightPad); set => RightPad = Util.WidthFromInches(value); }
    public int RightPadInPixels { get => (int)RightPad; set => RightPad = value; }
    public float TopPad { get; set; }
    public int TopPadInPixels { get => (int)TopPad; set => TopPad = value; }
    public float TopPadInInches { get => Util.HeightToInches(TopPad); set => TopPad = Util.HeightFromInches(value); }
    public float BotPad { get; set; }
    public int BotPadInPixels { get => (int)BotPad; set => BotPad = value; }
    public float BotPadInInches { get => Util.HeightToInches(BotPad); set => BotPad = Util.HeightFromInches(value); }
    public WfColor ForeColor { get; set; }
    public WfColor BackColor { get; set; }
    public WfColor LineColor { get; set; }
    public void CopyTo(Style dest)
    {

        dest.ForeColor = ForeColor;
        dest.BackColor = BackColor;
        dest.LineColor = LineColor;
        dest.SetPadding(LeftPad, RightPad, TopPad, BotPad);
        dest.FontSize = FontSize;
    }
    static Style _title = null;
    public static Style Title
    {
        get
        {
            if (_title == null)
            {
                var s = _title = new Style();
                s.ForeColor = WfColor.Color.Black;
                s.LineColor = WfColor.Color.Grey;

            }
            return _title;
        }
    }
    static Style _normal = null;
    public static Style Normal
    {
        get
        {
            if (_normal == null)
            {
                var s = _normal = new Style();
                s.ForeColor = WfColor.Color.Black;
                s.LineColor = WfColor.Color.White;
            }
            return _normal;
        }
    }
    static Style _reversed = null;
    public static Style Reversed
    {
        get
        {
            if (_reversed == null)
            {
                var s = _reversed = new Style();
                s.ForeColor = WfColor.Color.White;
                s.LineColor = WfColor.Color.Black;
            }
            return _reversed;
        }
    }
    static Style _red = null;
    public static Style Red
    {
        get
        {
            if (_red == null)
            {
                var s = _red = new Style();
                s.ForeColor = WfColor.Color.Red;
            }
            return _red;
        }
    }
    static Style _redReversed = null;
    public static Style RedReversed
    {
        get
        {
            if (_redReversed == null)
            {
                var s = _redReversed = new Style();
                s.ForeColor = WfColor.Color.Red;
                s.LineColor = WfColor.Color.Grey;
            }
            return _redReversed;
        }
    }
    static Style _green = null;
    public static Style Green
    {
        get
        {
            if (_green == null)
            {
                var s = _green = new Style();
                s.ForeColor = WfColor.Color.Green;
            }
            return _green;
        }
    }
    static Style _greenReversed = null;
    public static Style GreenReversed
    {
        get
        {
            if (_greenReversed == null)
            {
                var s = _greenReversed = new Style();
                s.ForeColor = WfColor.Color.Green;
                s.LineColor = WfColor.Color.Grey;
            }
            return _greenReversed;
        }
    }
    static Style _yellow = null;
    public static Style Yellow
    {
        get
        {
            if (_yellow == null)
            {
                var s = _yellow = new Style();
                s.ForeColor = WfColor.Color.Yellow;
            }
            return _yellow;
        }
    }
    static Style _exception = null;
    public static Style Exception
    {
        get
        {
            if (_exception == null)
            {
                var s = _exception = new Style();
                s.FontSize = 12;
            }
            return _exception;
        }
    }
}
