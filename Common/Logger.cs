//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using System;

namespace WFLib;
//public sealed class LoggerConfiguration
//{
//    public int EventId { get; set; }

//    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
//    {
//        [LogLevel.Critical] = ConsoleColor.Red,
//        [LogLevel.Debug] = ConsoleColor.DarkGray,
//        [LogLevel.Error] = ConsoleColor.Red,
//        [LogLevel.None] = ConsoleColor.Black,
//        [LogLevel.Trace] = ConsoleColor.Gray,
//        [LogLevel.Information] = ConsoleColor.Green,
//        [LogLevel.Warning] = ConsoleColor.Yellow,
//    };
//}
public sealed class Logger : ILogger
{
    private readonly string _name;
    static ConsoleColor[] colors = new ConsoleColor[7];
    static string[] logLevelNames = new string[7];
    static Logger()
    {
        logLevelNames[(int)LogLevel.Trace] = "Trace";
        logLevelNames[(int)LogLevel.Debug] = "Debug";
        logLevelNames[(int)LogLevel.Information] = "Info";
        logLevelNames[(int)LogLevel.Warning] = "Warn";
        logLevelNames[(int)LogLevel.Error] = "Error";
        logLevelNames[(int)LogLevel.Critical] = "Critical";
        logLevelNames[(int)LogLevel.None] = "N/A";
        colors[(int)LogLevel.Trace] = ConsoleColor.Gray;
        colors[(int)LogLevel.Debug] = ConsoleColor.DarkCyan;
        colors[(int)LogLevel.Information] = ConsoleColor.Green;
        colors[(int)LogLevel.Warning] = ConsoleColor.Yellow;
        colors[(int)LogLevel.Error] = ConsoleColor.Red;
        colors[(int)LogLevel.Critical] = ConsoleColor.DarkRed;
        colors[(int)LogLevel.None] = ConsoleColor.Blue;
    }
    //public Logger(string name, Func<LoggerConfiguration> getCurrentConfig)
    bool isValid;
    internal bool showName { get; set; } = false;
    public Logger(string name, bool isValid)
    {
        this.isValid = isValid;
        _name = name;
    }
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel logLevel)
    {
        if (!isValid) return false;
        switch(logLevel)
        {
            case LogLevel.Trace: return false;
            case LogLevel.Debug: return false;
            case LogLevel.Information: return true;
            case LogLevel.Warning: return true;
            case LogLevel.Error: return true;
            case LogLevel.Critical: return true;
            default: return false;
        }
    }
    public bool logToConsole { get; set; } = true;
    public bool logToFile { get; set; } = true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        int lli = (int)logLevel;
        if (logToConsole)
        {
            LogToConsole(state, exception, formatter, lli);
        }
        if (logToFile)
        {
            LogToFile(state, exception, formatter, lli);
        }
    }
    private void LogToConsole<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter, int lli)
    {
        lock (logLevelNames)
        {
            Console.ForegroundColor = colors[lli];
            Console.Write($"[{logLevelNames[lli]}] ");
            Console.ForegroundColor = ConsoleColor.White;
            if (lli == (int)LogLevel.Information)
            {
                var msg = state.ToString();
                var msgSpan = msg.AsSpan();
                bool searching = true;
                for(int i = 0; i < msgSpan.Length; i++)
                {
                    if (searching && (msgSpan[i] == ':' || msgSpan[i]== '!'))
                    {
                        searching = false;
                        Console.Write(":");
                        if (msgSpan[i] == '!')
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        continue;
                    }
                    Console.Write(msgSpan[i]);
                }
            }
            else
            {
                Console.Write($"{formatter(state, exception)}");
            }
            if (showName)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($" - {_name}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine();
        }
    }
    private void LogToFile<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter, int lli)
    {
        if (IsInvalidLogFile) return;
        lock (LogFile)
        {
            try
            {
                sb.Clear();
                var datestamp = DateTime.UtcNow.ToBinary();
                sb.Append(DateTime.UtcNow.AsString(DATETYPE.DisplayWithMSec));
                sb.Append('\t');
                sb.Append($"[{logLevelNames[lli]}]\t");
                sb.Append($"{formatter(state, exception)}\t");
                sb.Append($"{_name}");
                LogFile.WriteLine(sb.ToString());
                LogFile.Flush();
            }
            catch { }
        }
    }
    static Logger _logger = WFLoggerProvider.SCreateLogger("Default");
    static int LogDayOfYear = -1;
    static string LogFilePath = string.Empty;
    static WfFile LogFile = null;
    static StringBuilder sb = null;
    static bool IsInvalidLogFile
    {
        get
        {
            int dayOfYear = DateTime.UtcNow.DayOfYear;
            if (dayOfYear > LogDayOfYear)
            {
                LogDayOfYear = dayOfYear;
                LogFilePath = Path.Combine(Directories.Logs, $"LOG{DateTime.UtcNow.AsString(DATETYPE.FileDate)}.txt");
                if (LogFile != null)
                {
                    LogFile.Close();
                    LogFile.Dispose();
                }
                LogFile = new WfFile(LogFilePath);
                LogFile.OpenAppend();
            }
            if (sb == null)
            {
                sb = new StringBuilder();
            }
            if (LogFile != null && LogFile.IsOpen) return false;
            return true;
        }
    }
    public static void Message(string message, Action<string> logger = null)
    {
        if (logger != null)
        {
            logger(message);
            return;
        }
        _logger.LogInformation(message);
    }
    public static void Exception(Exception ex, Action<string> logger = null)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append("Exception: ");
        sb.Append("Source: ");
        sb.Append(ex.Source);
        sb.Append(" Message:");
        sb.AppendLine(ex.Message);
        sb.Append("Call Stack:");
        sb.AppendLine(ex.StackTrace);
        Error(sb.ToString(), logger);
        sb.Return();
    }
    public static void Error(string message, Action<string> logger = null)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append(message);
        if (logger != null)
        {
            logger(message);
            return;
        }
        _logger.LogError(message);
        sb.Return();
    }
    public static void Warning(string message, Action<string> logger = null)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append(message);
        if (logger != null)
        {
            logger(message);
            return;
        }
        _logger.LogWarning(message);
        sb.Return();
    }
}
[ProviderAlias("WFLogger")]
public sealed class WFLoggerProvider : ILoggerProvider
{
    private static readonly HashSet<string> validNames = new();

    private static readonly Dictionary<string,Logger> loggers = new();
    public WFLoggerProvider()
    {
        validNames.Add("Microsoft.Hosting.Lifetime");
        //validNames.Add("Microsoft.AspNetCore.Hosting.Diagnostics");
    }
    private bool IsValid(string categoryName)
    {
        if (categoryName.StartsWith("Microsoft"))
        {
            if (!validNames.Contains(categoryName))
                return false;
            return true;
        }

        return true;
    }
    public static Logger SCreateLogger(string categoryName, bool isValid=true)
    {
        var logger = new Logger(categoryName, isValid);
        loggers.Add(categoryName, logger);
        return logger;
    }
    public ILogger CreateLogger(string categoryName)
    {
        bool isValid = IsValid(categoryName);
        lock(loggers)
        {
            if (!loggers.TryGetValue(categoryName, out var logger))
            {
                logger =  SCreateLogger(categoryName, isValid);
            }
            return logger;
        }
    }
    
    public void Dispose()
    {
        loggers.Clear();
    }
}
public static class LoggerExtensions
{
    public static ILoggingBuilder AddWFLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, WFLoggerProvider>());
        return builder;
    }
}