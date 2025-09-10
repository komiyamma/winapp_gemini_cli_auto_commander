using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GeminiCLIAutoCommander
{
    internal static class GeminiPathResolver
    {
        private static string TryReadStdout(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };
                using (var p = Process.Start(psi))
                {
                    if (p == null) return null;
                    string stdout = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return stdout;
                }
            }
            catch { return null; }
        }

        private static string NormalizeLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            string t = s.Trim();
            int i = t.IndexOf('\n');
            if (i >= 0) t = t.Substring(0, i).Trim();
            if (t.Length >= 2 && t[0] == '"' && t[t.Length - 1] == '"') t = t.Substring(1, t.Length - 2);
            return t;
        }

        public static string FindGeminiCmdPath()
        {
            string prefix = TryReadStdout("cmd.exe", "/c npm config get prefix");
            prefix = NormalizeLine(prefix);
            if (!string.IsNullOrEmpty(prefix))
            {
                string candidate = Path.Combine(prefix, GeminiConstants.GeminiCmdFileName);
                if (File.Exists(candidate)) return candidate;
            }
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fallback = Path.Combine(appdata, "npm", GeminiConstants.GeminiCmdFileName);
            if (File.Exists(fallback)) return fallback;
            return null;
        }
    }
}
