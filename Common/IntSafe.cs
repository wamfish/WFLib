//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using static WFLib.IntSafe;
namespace WFLib;
// Doubles and Floats can contain very large numbers. Numbers much larger than an int, or
// even a long. This comes at a cost of loss of precision. After a certain Min/Max value
// the loss of precision means that not all integer values beyound the Min/Max value can
// be represented when using floats or doubles. This makes Doubles and Floats a bad choice
// for counting things, and their mainly used for calculating large numbers that don't require
// 100% precision. The IntSafe class defines the Min/Max const values that can be used if
// counting with a float or a double is needed.
//
// I created these mostly for the IntSafeDoubleMin and IntSafeDoubleMax to be used in
// conjuction with Godot's slider control. The slider control uses a double for its Min and
// Max value, and I needed to know the Range where it was safe to use the slider.
//
// In short using the "--" or "++" with a float or a double can lead to bugs or even endless
// loops if you are not aware of the limits of float and double. Read the IntSafeTest class
// below for and example of how to use these const values.
public static class IntSafe
{
    public const int IntSafeFloatMax = 16_777_217;
    public const int IntSafeFloatMin = -16_777_217;
    public const long IntSafeDoubleMax = 9_007_199_254_740_992;
    public const long IntSafeDoubleMin = -9_007_199_254_740_992;
}
public static class IntSafeTest
{
    const string IntSafeFloatMaxTestStr = "IntSafeFloatMax Test";
    const string IntSafeFloatMinTestStr = "IntSafeFloatMin Test";
    const string IntSafeDoubleMaxTestStr = "IntSafeDoubleMax Test";
    const string IntSafeDoubleMinTestStr = "IntSafeDoubleMin Test";
    public static void RunIntSafeTest()
    {
        IntSafeFloatMaxTest();
        IntSafeFloatMinTest();
        IntSafeDoubleMaxTest();
        IntSafeDoubleMinTest();
    }
    static void IntSafeFloatMaxTest()
    {
        Console.WriteLine($"\nRunning {IntSafeFloatMaxTestStr} ...\n");
        float fval;
        int ival;
        for (ival = 0, fval = 0; ival <= IntSafeFloatMax; ival++, fval++)
        {
            if (ival != fval) //shoule never be true
            {
                Console.WriteLine($"\t{IntSafeFloatMaxTestStr} Failed");
                return;
            }
        }
        ival++;
        fval++; //this does not work because of the loss of precision
        if (ival != fval)
        {
            Console.WriteLine($"\t{IntSafeFloatMaxTestStr} Passed");
        }
        else
        {
            Console.WriteLine($"\t{IntSafeFloatMaxTestStr} Failed with Unexpected Result");
        }
    }
    static void IntSafeFloatMinTest()
    {
        Console.WriteLine($"\nRunning {IntSafeFloatMinTestStr} ...\n");
        float fval;
        int ival;
        for (ival = 0, fval = 0; ival >= IntSafeFloatMin; ival--, fval--)
        {
            if (ival != fval) //should never happen
            {
                Console.WriteLine($"\t{IntSafeFloatMinTestStr} Failed");
                return;
            }
        }
        fval--; //this does not work because of the loss of precision
        ival--;
        if (ival != fval)
        {
            Console.WriteLine($"\t{IntSafeFloatMinTestStr} Passed");
        }
        else
        {
            Console.WriteLine($"\t{IntSafeFloatMinTestStr} Failed with Unexpected Result");
        }
    }
    static void IntSafeDoubleMaxTest()
    {
        Console.WriteLine($"\nRunning {IntSafeDoubleMaxTestStr} ...\n");
        double fval;
        long lval;
        long start = IntSafeDoubleMax - 100000000;

        for (lval = start, fval = start; lval <= IntSafeDoubleMax; lval++, fval++)
        {
            if (lval != fval) //shoule never be true
            {
                Console.WriteLine($"\t{IntSafeDoubleMaxTestStr} Failed");
                return;
            }
        }
        lval++;
        fval++; //this does not work because of the loss of precision
        if (lval != fval)
        {
            Console.WriteLine($"\t{IntSafeDoubleMaxTestStr} Passed");
        }
        else
        {
            Console.WriteLine($"\t{IntSafeDoubleMaxTestStr} Failed with Unexpected Result");
        }
    }
    static void IntSafeDoubleMinTest()
    {
        Console.WriteLine($"\nRunning {IntSafeDoubleMinTestStr} ...\n");
        double fval;
        long lval;
        long start = IntSafeDoubleMin + 100000000;
        for (lval = start, fval = start; lval >= IntSafeDoubleMin; lval--, fval--)
        {
            if (lval != fval) //should never happen
            {
                Console.WriteLine($"\t{IntSafeDoubleMinTestStr} Failed");
                return;
            }
        }
        fval--; //this does not work because of the loss of precision
        lval--;
        if (lval != fval)
        {
            Console.WriteLine($"\t{IntSafeDoubleMinTestStr} Passed");
        }
        else
        {
            Console.WriteLine($"\t{IntSafeDoubleMinTestStr} Failed with Unexpected Result");
        }
    }
}
