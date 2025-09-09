using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GeminiCLIAutoCommander
{
    internal static class GeminiConstants
    {
        public static readonly string GeminiCmdFileName = "gemini.cmd";

        // é¿ç€Ç…égópÇ≥ÇÍÇÈê›íËíl (GeminiCLIAutoCommander.config ÇÃ GeminiModel / GeminiPrompt Ç©ÇÁì«Ç›çûÇ›)
        public static readonly string Model;
        public static readonly string PromptJapanese;

        static GeminiConstants()
        {
            string model = string.Empty;
            string prompt = string.Empty;
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(baseDir, "GeminiCLIAutoCommander.config");
                if (File.Exists(configPath))
                {
                    string xml = File.ReadAllText(configPath);
                    model = GetAppSetting(xml, "GeminiModel");
                    prompt = GetAppSetting(xml, "GeminiPrompt");
                }
            }
            catch { }

            Model = model ?? string.Empty;
            PromptJapanese = prompt ?? string.Empty;
        }

        private static string GetAppSetting(string xml, string key)
        {
            if (string.IsNullOrEmpty(xml)) return null;
            try
            {
                string pattern = "<add\\s+key=\"" + Regex.Escape(key) + "\"\\s+value=\"(.*?)\"";
                var m = Regex.Match(xml, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (m.Success)
                {
                    string val = m.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(val)) return System.Net.WebUtility.HtmlDecode(val);
                }
            }
            catch { }
            return null;
        }
    }
}
