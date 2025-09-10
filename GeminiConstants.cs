using System;
using System.Configuration;

namespace GeminiCLIAutoCommander
{
    internal static class GeminiConstants
    {
        public static readonly string GeminiCmdFileName = "gemini.cmd";

        // 実際に使用される設定値 (App.config の appSettings から読み込み)
        public static readonly string Model = ConfigurationManager.AppSettings["GeminiModel"] ?? string.Empty;
        public static readonly string PromptJapanese = ConfigurationManager.AppSettings["GeminiPrompt"] ?? string.Empty;
    }
}
