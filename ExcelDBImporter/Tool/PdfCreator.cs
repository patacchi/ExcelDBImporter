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
using DocumentFormat.OpenXml.Drawing.Charts;

class PdfCreator
{
    private const double MaerginRate = 0.75;
    //PDF出力時の左右の余白の合計
    private const int Const_PDF_Width_Margin_Sum = 60;
    //PDF出力時の上下の余白の合計
    private const int Const_PDF_Heigt_Margin_Sum = 40;
    private const int Const_Image_To_TItle_Vertical_Maergin = 15;

    public void CreatePdf(Dictionary<string, MemoryStream> dicSvgStream, int imagesPerPage)
    {
        using ExcelDbContext dbContext = new();
        //DBより保存ディレクトリ(PDF)の設定があるかチェック、なければString.Empty
        //アプリ設定そのものがあるかチェック
        AppSetting? appExists = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
        string StrDBSaveDir = string.Empty;
        if (appExists != null)
        {
            //LastPDFSaveDirに設定値が存在する場合のみ設定
            StrDBSaveDir = string.IsNullOrEmpty(appExists.StrLastPDFSaveToDir) ? string.Empty : appExists.StrLastPDFSaveToDir;
        }
        //保存ファイル名選択
        SaveFileDialog saveFileDialog = new()
        {
            InitialDirectory = StrDBSaveDir,
            Filter = "PDF files (*.pdf)|*.pdf",
            FileName = "入力用DMコード" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")
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
            double availableWidth = pageSize.Width - Const_PDF_Width_Margin_Sum; // ページの幅から余白を引く
            double availableHeight = pageSize.Height - Const_PDF_Heigt_Margin_Sum; // ページの高さから余白を引く

            //タイトルの行の高さを計測
            XFont fontTest = new XFont("源真ゴシック Medium", 10); // フォント設定
            XSize titleSizeTest = gfx.MeasureString("高さ計測用文字列", fontTest);
            // イメージのサイズを計算
            double imageSideLength = CalculateSquareSize(imagesPerPage, availableWidth, availableHeight,titleSizeTest.Height); // イメージの辺の長さ
            double x = Const_PDF_Width_Margin_Sum/2; // イメージの x 座標  余白合計の半分
            double y = Const_PDF_Heigt_Margin_Sum/2; // イメージの y 座標 余白合計の半分

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
                double titleX = x + ((imageSideLength * MaerginRate) - titleSize.Width) / 2; // タイトルの x 座標 センターに配置するため /2がある
                double titleY = y + (imageSideLength * MaerginRate)  + Const_Image_To_TItle_Vertical_Maergin; // タイトルの y 座標
                gfx.DrawString(title, font, XBrushes.Black, titleX, titleY);
                //gfx.DrawString(title, font, XBrushes.Black, new XRect (titleX,titleY,titleSize.Width,titleSize.Height),XStringFormats.TopLeft);
                // 次のイメージの座標を計算
                //次のイメージまでの間隔を計算
                (double Xnext, double Ynext) Spacing = CalculateSquareSpacing(availableWidth,
                                                                              availableHeight,
                                                                              imageSideLength,
                                                                              titleSize.Height);
                x += Spacing.Xnext; // 次の列に移動
                if (x + imageSideLength > pageSize.Width) // ページの右端を超えたら
                {
                    x = Const_PDF_Width_Margin_Sum/2; // 次の行の先頭に移動
                    y += Spacing.Ynext + titleSize.Height; // 次の行に移動
                }
            }
        }
        // PDF ファイルに保存
        document.Save(saveFileDialog.FileName);
        //出力ディレクトリ(PDF)を更新
        AppSetting? appSetting = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
        if (appSetting == null)
        {
            //アプリ設定そのものが見つからなかった
            MessageBox.Show("アプリ設定が見つかりませんでした。処理を中断します\n" + FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
        }
        else
        {
            //出力ディレクトリ(PDF)を更新
            appSetting.StrLastPDFSaveToDir = Path.GetDirectoryName(saveFileDialog.FileName);
            dbContext.SaveChanges();
        }
        //ディレクトリを開くかどうか聞く
        DialogResult dialogResult = MessageBox.Show("PDFファイル作成完了しました。出力フォルダを開きますか？", "PDF作成完了 フォルダ開きますか？", MessageBoxButtons.YesNo);
        if (dialogResult == DialogResult.Yes)
        {
            //出力ディレクトリを開く
            FrmExcelImpoerter.OpenFolderInExplorer(Path.GetDirectoryName(saveFileDialog.FileName) ?? string.Empty);
        }
        dbContext.Dispose();
    }
    static double CalculateSquareSize(int IntimagesInPages, double maxWidth, double maxHeight,double DblTitleHeight)
    {
        // 各辺に描画する正方形の数
        int numPerRow = (int)Math.Floor(Math.Sqrt(IntimagesInPages));
        int numPerCol = (int)Math.Ceiling((double)IntimagesInPages / numPerRow);

        // 正方形のサイズを計算
        double width = maxWidth / numPerRow;
        //高さ方向で、タイトルの高さを考慮
        double height = (maxHeight - (DblTitleHeight*numPerCol)) / numPerCol;

        // より小さい辺に合わせる
        return Math.Min(width, height);
    }
    static (double horizontalSpacing, double verticalSpacing) CalculateSquareSpacing(double DblAvailableWidth,
                                                                                     double DblAvailableHeight,
                                                                                     double imageSideLength,
                                                                                     double heigtmargin)
    {
        //縦方向の余白(タイトル等)を追加した実際の使用領域を計算
        double DblactualimageHeight = imageSideLength + heigtmargin;
        //1行あたりの数を計算
        int squaresPerRow = (int)(DblAvailableWidth / imageSideLength);
        //行数を計算
        int totalRows = (int)(DblAvailableHeight / DblactualimageHeight);
        // 余白を計算
        double totalMarginX = DblAvailableWidth - (imageSideLength * squaresPerRow);
        double totalMarginY = DblAvailableHeight - (DblactualimageHeight * totalRows);

        // 各方向の余白を均等に分配
        double marginX = totalMarginX / (squaresPerRow - 1); // 横方向の余白
        double marginY = totalMarginY / (totalRows - 1); // 縦方向の余白

        // 横方向の間隔を計算
        double horizontalSpacing = imageSideLength + marginX;

        // 縦方向の間隔を計算
        double verticalSpacing = imageSideLength + marginY;

        return (horizontalSpacing, verticalSpacing);
    }
}
