//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class WfStopwatch
{
    System.Diagnostics.Stopwatch stopwatch;
    public bool IsStarted { get; private set; } = false;
    public WfStopwatch()
    {
        stopwatch = new System.Diagnostics.Stopwatch();
    }
    public void Start()
    {
        Clear();
        stopwatch.Reset();
        stopwatch.Start();
        IsStarted = true;
    }
    public void Stop()
    {
        if (!IsStarted)
            Start();
        IsStarted = false;
        stopwatch.Stop();
    }
    public void Clear()
    {
        if (IsStarted)
        {
            stopwatch.Stop();
            IsStarted = false;
        }
        stopwatch.Reset();
    }
    public int ElapsedMilliseconds
    {
        get { return (int)stopwatch.ElapsedMilliseconds; }

    }
    public String ElapsedMillisecondsAsString
    {
        get { return stopwatch.ElapsedMilliseconds.ToString(); }
    }
    public string ElapsedMillisecondsAsTimeString
    {
        get
        {
            var ts = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            float ms = ts.Milliseconds / 1000f;
            if (ts.Minutes > 0)
                return $"{ts.Minutes} Minutes and {ts.Seconds}{ms} Seconds";
            if (ts.Seconds > 0)
                return $"{ts.Seconds}{ms:#.###} Seconds";
            return $"{ms} Seconds";
        }
    }

}
