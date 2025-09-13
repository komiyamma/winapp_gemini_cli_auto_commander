# GeminiCLIAutoCommander v1.0.0

## 概要

これは、`gemini.cmd`（Google Gemini のコマンドラインインターフェース）を自動で実行するための C# 製ラッパーアプリケーションです。
設定ファイルに基づいて `gemini.cmd` を特定のプロンプトとパラメータで起動し、その実行を管理します。

## 主な機能

- **`gemini.cmd` の自動検索**: 環境変数 `PATH` から `gemini.cmd` を自動的に見つけ出します。
- **設定に基づいた実行**: `GeminiCLIAutoCommander.config` ファイルで、使用するモデルやプロンプトを柔軟に設定できます。
- **タイムアウト機能**: 設定した時間を超えてプロセスが実行された場合、自動的に終了させることができます。
- **堅牢なプロセス管理**: アプリケーションが予期せず終了した場合でも、子プロセス (`gemini.cmd`) が残らないように設計されています。

## 使い方

1.  `GeminiCLIAutoCommander.config` ファイルに必要な設定を記述します。
2.  `GeminiCLIAutoCommander.exe` を実行します。
3.  `gemini.cmd` が設定に基づいて実行され、その出力がコンソールに表示されます。

## 設定

設定は `GeminiCLIAutoCommander.config` ファイルで行います。このファイルは `GeminiCLIAutoCommander.exe` と同じディレクトリに配置してください。

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <!-- 使用するGeminiのモデル -->
    <add key="GeminiModel" value="gemini-2.5-flash" />

    <!-- Geminiに渡すプロンプト -->
    <add key="GeminiPrompt" value="@my_command.md を実行してください。..." />

    <!-- タイムアウト時間（分単位）。0を指定すると無効になります。 -->
    <add key="LimitMinutes" value="30" />
  </appSettings>
</configuration>
```

### 設定項目

- `GeminiModel`: `gemini.cmd` で使用するモデル名を指定します。
- `GeminiPrompt`: `gemini.cmd` に `-p` オプションとして渡されるプロンプトの文字列です。
- `LimitMinutes`: プロセスの最大実行時間を分単位で指定します。この時間を超えると、プロセスは強制終了されます。`0` を設定すると、タイムアウトは無効になります。
