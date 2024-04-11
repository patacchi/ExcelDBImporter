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

class PdfCreator
{
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
        // PDF ドキュメントの作成
        PdfDocument document = new PdfDocument();

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
            double imageAreaWidth = availableWidth / imagesPerPage; // イメージの配置領域の幅
            double imageAreaHeight = availableHeight / 2; // イメージの配置領域の高さ (2 行)

            // イメージのサイズを計算
            double imageWidth = imageAreaWidth - 10; // イメージの幅
            double imageHeight = imageAreaHeight - 10; // イメージの高さ
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
                //SVGイメージをビットマップに変換
                SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(imageEntry.Value);

                // 画像を描画
                var bitmap = svgDocument.Draw((int)imageWidth, (int)imageHeight);
                XImage image = XImage.FromStream(svgStream);
                gfx.DrawImage(image, x, y, imageWidth, imageHeight);

                // 次のイメージの座標を計算
                x += imageAreaWidth; // 次の列に移動
                if (x + imageAreaWidth > pageSize.Width) // ページの右端を超えたら
                {
                    x = 10; // 次の行の先頭に移動
                    y += imageAreaHeight; // 次の行に移動
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
}
