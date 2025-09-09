using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GeminiCLIAutoCommander
{
    internal class Program
    {
        private static KillOnCloseJob _jobRef;
        private static Process _procRef;
        private static bool _cleaned;
        private static readonly object _lock = new object();
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

        static void Main(string[] args)
        {
            var executable = GeminiPathResolver.FindGeminiCmdPath();
            if (string.IsNullOrEmpty(executable)) { Console.WriteLine($"{GeminiConstants.GeminiCmdFileName} が見つかりませんでした。"); return; }

            var arguments = $"-m {GeminiConstants.Model} -y -p \"{GeminiConstants.PromptJapanese}\"";
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("[RUN] " + executable + " " + arguments);

            _ctrlHandler = _ => { CleanupAndExit(1); return true; };
            SetConsoleCtrlHandler(_ctrlHandler, true);

            KillOnCloseJob job = null; Process process = null;
            try
            {
                job = new KillOnCloseJob(); _jobRef = job;
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
                process = new Process { StartInfo = psi, EnableRaisingEvents = true }; _procRef = process;
                process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                if (!process.Start()) { Console.WriteLine("プロセスを開始できませんでした。" ); CleanupAndExit(1); return; }
                job.AddProcess(process);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                Console.CancelKeyPress += (s, e) => { e.Cancel = true; CleanupAndExit(1); };
                process.WaitForExit();
                Console.WriteLine("ExitCode: " + process.ExitCode);
                CleanupAndExit(process.ExitCode);
            }
            catch (Exception ex) { Console.WriteLine("実行時エラー: " + ex.Message); CleanupAndExit(1); }
        }
    }
}
