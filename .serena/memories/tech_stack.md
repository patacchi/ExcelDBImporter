# Tech stack

- C# / .NET 8（`net8.0-windows`、WinForms、Nullable enable、ImplicitUsings enable）
- EF Core 9.0.0-preview.3 + Sqlite（Design/Tools 含む preview 固定。安定版へ上げない）
- Office COM 参照: Microsoft.Office.Interop.Excel / Office.Core（tlbimp、EmbedInteropTypes）→ Windows + Office インストール環境と VS 2022 の MSBuild が必要
- 主要 NuGet: ClosedXML 0.104-preview2、CsvHelper 31、PDFsharp 6.1-preview、ZXing.Net 0.16.9、Svg 3.4.7、System.IO.Ports 9-preview、UTF.Unknown 2.5.1
- シリアル通信（QR リーダー）: System.IO.Ports を `Tool/SerialCommunication.cs` で使用
- PDF フォント: `Fonts/GenShinGothic-Medium.ttf` を Content としてコピー、`Tool/CustomFontResolver.cs` で解決