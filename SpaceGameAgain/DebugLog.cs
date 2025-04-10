using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;

internal static class DebugLog
{
    private static FileStream? logFile;
    private static TextWriter? writer;

    [Conditional("DEBUG")]
    public static void Initialize()
    {
        try
        {
            if (File.Exists("latest.log"))
            {
                DateTime timestamp = File.GetLastWriteTime("latest.log");
                string newName = "./logs/" + timestamp.ToString("yyyy-MM-dd-hh-mm-ss") + ".log";
                Directory.CreateDirectory("logs");
                File.Copy("latest.log", newName, true);
            }
        }
        catch
        {
            Console.WriteLine("error preserving last log; overwriting");
        }
        
        try
        {
            logFile = new FileStream("latest.log", FileMode.OpenOrCreate);
            logFile.SetLength(0);
            writer = new StreamWriter(logFile);
        }
        catch
        {
            try
            {
                Console.WriteLine("error opening log file; trying latest1.log");
                logFile = new FileStream("latest1.log", FileMode.OpenOrCreate);
                logFile.SetLength(0);
                writer = new StreamWriter(logFile);
            }
            catch
            {
                Console.WriteLine("error opening log file; log will not be saved");
            }
        }
    }

    [DebuggerHidden]
    public static void Assert(bool condition, string message = "Assert failed!", [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0, [CallerMemberName] string memberName = "", [CallerArgumentExpression(nameof(condition))] string expr ="")
    {
        if (!condition)
        {
            Error(message + " " + expr);
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
    }

    [Conditional("DEBUG")]
    public static void Message(string message, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0, [CallerMemberName] string memberName = "")
    {
        ConsoleColor prevCol = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
        WriteDirect(FormatMessage(message, Severity.Message, callerFile, callerLine, memberName));
        Console.ForegroundColor = prevCol;
    }

    [Conditional("DEBUG")]
    public static void Error(string message, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0, [CallerMemberName] string memberName = "")
    {
        ConsoleColor prevCol = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        WriteDirect(FormatMessage(message, Severity.Error, callerFile, callerLine, memberName));
        Console.ForegroundColor = prevCol;
    }

    [Conditional("DEBUG")]
    public static void Warning(string message, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0, [CallerMemberName] string memberName = "")
    {
        ConsoleColor prevCol = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteDirect(FormatMessage(message, Severity.Warning, callerFile, callerLine, memberName));
        Console.ForegroundColor = prevCol;
    }

    private static string FormatMessage(string message, Severity severity, string callerFile, int callerLine, string memberName)
    {
        string severityStr = severity switch
        {
            Severity.Error => "ERR",
            Severity.Warning => "WRN",
            _ => "MSG"
        };

        return $"[{DateTime.Now:HH:mm:ss.ffff}] [{severityStr}] ({Path.GetFileName(callerFile)}:{callerLine} {memberName}): {message}\n";
    }

    [Conditional("DEBUG")]
    public static void WriteDirect(string message)
    {
        Console.Write(message);
        writer?.Write(message);
    }

    public static void Uninitialize()
    {
        logFile?.Flush();
        logFile?.Dispose();
    }

    public static void ReportException(Exception exception)
    {
        ConsoleColor prevCol = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        WriteDirect($"[{DateTime.Now:HH:mm:ss.ffff}] [ERR]: {exception}");
        Console.ForegroundColor = prevCol;
    }

    enum Severity
    {
        Message,
        Warning,
        Error,
    }
}
