using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GeminiCLIAutoCommander;

internal class Program
{
    private static KillOnCloseJob _jobRef;
    private static Process _procRef;
    private static bool _cleaned;
    private static readonly object _lock = new();
    private static ConsoleCtrlDelegate _ctrlHandler;

    private delegate bool ConsoleCtrlDelegate(int ctrlType);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

    private static void CleanupAndExit(int code)
    {
        lock (_lock)
        {
            if (_cleaned) return;
            _cleaned = true;
            try
            {
                if (_procRef != null)
                {
                    try { if (!_procRef.HasExited) _procRef.Kill(); } catch { }
                    _procRef.Dispose();
                    _procRef = null;
                }
            }
            catch { }
            try
            {
                if (_jobRef != null)
                {
                    _jobRef.Dispose();
                    _jobRef = null;
                }
            }
            catch { }
            try { Console.Out.Flush(); } catch { }
            try { Console.Error.Flush(); } catch { }
        }
        Environment.Exit(code);
    }

    private static void HookCtrlHandler()
    {
        _ctrlHandler = _ => { CleanupAndExit(1); return true; };
        SetConsoleCtrlHandler(_ctrlHandler, true);
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; CleanupAndExit(1); };
    }

    private static Timer? StartLimitTimer(int minutes)
    {
        if (minutes <= 0) return null;
        var due = TimeSpan.FromMinutes(minutes);
        return new Timer(_ =>
        {
            try { Console.WriteLine($"制限時間({minutes}分)に達したため終了します。"); } catch { }
            CleanupAndExit(1);
        }, null, due, Timeout.InfiniteTimeSpan);
    }

    private static Process CreateProcess(string executable, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        return new() { StartInfo = psi, EnableRaisingEvents = true };
    }

    static void Main(string[] args)
    {
        var executable = GeminiPathResolver.FindGeminiCmdPath();
        if (string.IsNullOrEmpty(executable)) { Console.WriteLine($"{GeminiConstants.GeminiCmdFileName} が見つかりませんでした。"); return; }

        var arguments = $"-m {GeminiConstants.Model} -y -p \"{GeminiConstants.PromptJapanese}\"";
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine($"[RUN] {executable} {arguments}");

        HookCtrlHandler();

        KillOnCloseJob job = null; Process process = null; Timer timeoutTimer = null;
        try
        {
            job = new KillOnCloseJob(); _jobRef = job;
            process = CreateProcess(executable, arguments); _procRef = process;
            process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            if (!process.Start()) { Console.WriteLine("プロセスを開始できませんでした。"); CleanupAndExit(1); return; }

            job.AddProcess(process);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            timeoutTimer = StartLimitTimer(GeminiConstants.LimitMinutes);

            process.WaitForExit();
            Console.WriteLine($"ExitCode: {process.ExitCode}");
            timeoutTimer?.Dispose();
            CleanupAndExit(process.ExitCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"実行時エラー: {ex.Message}");
            CleanupAndExit(1);
        }
    }
}
