#:package ZXing.Net@0.16.9

// ============================================================================
// QRコード -> SVG 出力ツール (ファイルベースアプリ / .NET 10)
//
// 使い方:
//   dotnet run --file Tools/qr.cs "<QR化文字列>" [出力SVGパス]
// 例:
//   dotnet run --file Tools/qr.cs "テスト文字列ABC" out.svg
//
// 参考: ExcelDBImporter/Tool/QRcodeCreate.cs (DMコード版の実装をQRコード用に調整)
// ============================================================================

using System.Text;
using ZXing;
using ZXing.QrCode;
using ZXing.Rendering;

// 引数: [0]=QR化文字列(必須) [1]=出力SVGパス(省略時 qr.svg)
if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("Usage: dotnet run --file qr.cs \"<text>\" [output.svg]");
    return 1;
}

string text = args[0];
string outputPath = args.Length == 2 ? args[1] : "qr.svg";

try
{
    // QRコードのフォーマットを指定 (参考: QRcodeCreate.DMWriterformat())
    BarcodeWriterSvg qrWriter = new()
    {
        // 種類はQRコード
        Format = BarcodeFormat.QR_CODE,
        Options = new QrCodeEncodingOptions
        {
            // 文字列エンコード
            CharacterSet = "UTF-8",
            Width = 300,
            Height = 300,
            Margin = 5,
        },
    };

    SvgRenderer.SvgImage svgImage = qrWriter.Write(text);

    // UTF-8(BOMなし)でSVGを保存
    File.WriteAllText(outputPath, svgImage.Content, new UTF8Encoding(false));

    Console.WriteLine(Path.GetFullPath(outputPath));
    return 0;
}
catch (Exception ex)
{
    // 文字列長超過などによるQR生成失敗やファイル書き込みエラー
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
