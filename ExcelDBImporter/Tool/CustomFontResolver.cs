using PdfSharp.Fonts;
using System.IO;

public class CustomFontResolver : IFontResolver
{
    public byte[]? GetFont(string faceName)
    {
        // 指定されたフォント名に応じてフォントファイルを読み込み、バイト配列として返す
        if (faceName == "源真ゴシック Medium")
        {
            //string StrBizUDfontFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "BIZ-UDGothicR.ttc");
            return File.ReadAllBytes(new Uri(@"Fonts/GenShinGothic-Medium.ttf", UriKind.Relative).ToString());
            // フォントファイルを読み込み、byte[] として返す
            // 例:
            // string fontFilePath = "path/to/BIZ-UDPGothic-Regular.ttf";
            // return File.ReadAllBytes(fontFilePath);
        }

        // 指定されたフォントが見つからない場合は null を返す
        return null;
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // フォントのファミリー名に応じて、フォントファイルの情報を提供する
        // ここでは簡略化のため、常に null を返す
        return new FontResolverInfo(familyName);
        //return null;
    }
}