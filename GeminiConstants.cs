using System;
using System.Configuration;
using System.IO;

namespace GeminiCLIAutoCommander
{
    internal static class GeminiConstants
    {
        public static readonly string GeminiCmdFileName = "gemini.cmd";

        // 設定はローカルの "GeminiCLIAutoCommander.config" から読み込み
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeminiCLIAutoCommander.config");
        private static readonly Configuration ExternalConfig = LoadExternalConfig(ConfigFilePath);

        public static readonly string Model = GetAppSetting("GeminiModel");
        public static readonly string PromptJapanese = GetAppSetting("GeminiPrompt");

        private static Configuration LoadExternalConfig(string path)
        {
            try
            {
                var map = new ExeConfigurationFileMap { ExeConfigFilename = path };
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
            catch
            {
                return null;
            }
        }

        private static string GetAppSetting(string key)
        {
            try
            {
                if (ExternalConfig != null)
                {
                    var setting = ExternalConfig.AppSettings.Settings[key];
                    if (setting != null) return setting.Value ?? string.Empty;
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
