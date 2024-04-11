using ExcelDBImporter;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Svg;
using System.Drawing.Imaging;
using PdfSharp.Fonts;

class PdfCreator
{
    private const double MaerginRate = 0.75;

    public void CreatePdf(Dictionary<string, MemoryStream> dicSvgStream, int imagesPerPage)
    {
        using ExcelDbContext dbContext = new();
        //DBより保存ディレクトリの設定があるかチェック、なければString.Empty
        AppSetting? appExists = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
        string StrDBSaveDir = string.Empty;
        if (appExists != null)
        {

            //LastSaveDirに設定値が存在する場合のみ設定
            StrDBSaveDir = string.IsNullOrEmpty(appExists.StrLastSaveToDir) ? string.Empty : appExists.StrLastSaveToDir;
        }
        //保存ファイル名選択
        SaveFileDialog saveFileDialog = new()
        {
            InitialDirectory = StrDBSaveDir,
            Filter = "PDF files (*.pdf)|*.pdf",
            FileName = "入力用QRコード" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")
        };
        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
            MessageBox.Show("キャンセルされました");
            return;
        }
        if (GlobalFontSettings.FontResolver is null) 
        {
            // カスタムフォントリゾルバーを作成
            CustomFontResolver fontResolver = new CustomFontResolver();
            // PdfSharp のフォントリゾルバーを設定
            GlobalFontSettings.FontResolver = fontResolver; 
        }
        // PDF ドキュメントの作成
        PdfDocument document = new();
        // ページサイズの設定 (A4)
        XSize pageSize = PageSizeConverter.ToSize(PdfSharp.PageSize.A4);

        // イメージの数を取得
        int imageCount = dicSvgStream.Count;

        // ページ数を計算
        int pageCount = (imageCount + imagesPerPage - 1) / imagesPerPage;

        // 各ページに画像を配置
        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            // 新しいページを追加
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // イメージの配置領域を計算
            double availableWidth = pageSize.Width - 20; // ページの幅から余白を引く
            double availableHeight = pageSize.Height - 20; // ページの高さから余白を引く

            // イメージのサイズを計算
            double imageSideLength = CalculateSquareSize(imagesPerPage, availableWidth, availableHeight); // イメージの辺の長さ
            double x = 10; // イメージの x 座標
            double y = 10; // イメージの y 座標

            // １ページに画像を配置
            for (int i = 0; i < imagesPerPage; i++)
            {
                int imageIndex = pageIndex * imagesPerPage + i;
                if (imageIndex >= imageCount)
                    break;

                // 画像を取得
                KeyValuePair<string, MemoryStream> imageEntry = dicSvgStream.ElementAt(imageIndex);
                MemoryStream svgStream = imageEntry.Value;
                svgStream.Seek(0, SeekOrigin.Begin);
                //SVGイメージをビットマップに変換
                SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(svgStream);
                // 画像を描画
                MemoryStream msPNG = new();
                //PNG形式のMemoryStreamに保存(透過必要なため)
                svgDocument.Draw((int)(imageSideLength * MaerginRate), (int)(imageSideLength* MaerginRate)).Save(msPNG, ImageFormat.Png);
                msPNG.Seek(0, SeekOrigin.Begin);
                XImage image = XImage.FromStream(msPNG);
                gfx.DrawImage(image, x, y, imageSideLength * MaerginRate, imageSideLength * MaerginRate);

                // 画像の下にタイトルを表示
                string title = imageEntry.Key;
                XFont font = new XFont("源真ゴシック Medium", 10); // フォント設定
                XSize titleSize = gfx.MeasureString(title, font); // タイトルのサイズを計測
                double titleX = x + ((imageSideLength * MaerginRate) - titleSize.Width) / 2; // タイトルの x 座標
                double titleY = y + (imageSideLength * MaerginRate)  + 15; // タイトルの y 座標
                gfx.DrawString(title, font, XBrushes.Black, titleX, titleY);

                // 次のイメージの座標を計算
                x += imageSideLength; // 次の列に移動
                if (x + imageSideLength > pageSize.Width) // ページの右端を超えたら
                {
                    x = 10; // 次の行の先頭に移動
                    y += imageSideLength + titleSize.Height; // 次の行に移動
                }
            }
        }
        // PDF ファイルに保存
        document.Save(saveFileDialog.FileName);
        //出力ディレクトリを更新
        AppSetting? appSetting = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
        if (appSetting == null)
        {
            //アプリ設定そのものが見つからなかった
            MessageBox.Show("アプリ設定が見つかりませんでした。処理を中断します\n" + FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
        }
        else
        {
            //出力ディレクトを更新
            appSetting.StrLastSaveToDir = Path.GetDirectoryName(saveFileDialog.FileName);
            dbContext.SaveChanges();
        }
        dbContext.Dispose();
    }
    static double CalculateSquareSize(int IntnperPages, double maxWidth, double maxHeight)
    {
        // 各辺に描画する正方形の数
        int numPerRow = (int)Math.Floor(Math.Sqrt(IntnperPages));
        int numPerCol = (int)Math.Ceiling((double)IntnperPages / numPerRow);

        // 正方形のサイズを計算
        double width = maxWidth / numPerRow;
        double height = maxHeight / numPerCol;

        // より小さい辺に合わせる
        return Math.Min(width, height);
    }
}
