using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using ExcelDataReader;
using ExcelDataReader.Exceptions;
namespace ExcelDBImporter
{
    /// <summary>
    /// Excelファイルを指定して、xlsファイルの場合はxlsxファイルに変換してファイルパルを返す
    /// </summary>
    internal class ExcelFileComverter
    {
        public string StrOriginFilePath { get; set; } = null!;
        /// <summary>
        /// ファイル変換が必要だった場合に生成されるファイルパス(一時ファイル)
        /// 処理完了後に削除する必要があり
        /// </summary>
        public string? StrConvertedFilePath { get; set; }

        public string ExcelFileComVerter()
        {
            //DBよりLastLoadFromDirを取得、なければString.Empty
            string StrLastLoadDir = string.Empty;
            ExcelDbContext dbLoadDir = new();
            AppSetting? appSetting = dbLoadDir.AppSettings.FirstOrDefault(a => a.StrAppName == FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
            if (appSetting != null)
            {
                //LoadFromDirの設定が存在する場合のみ設定
                StrLastLoadDir = string.IsNullOrEmpty(appSetting.StrLastLoadFromDir) ? string.Empty : appSetting.StrLastLoadFromDir;
            }
            //ファイル選択ダイアログ表示
            OpenFileDialog DialogImportExcelFile = new()
            {
                InitialDirectory = StrLastLoadDir,
                Filter = "Excel files (*.xlsx)|*.xlsx;*.xls|" + "Excel files(old)(*.xls)|*.xls"
            };
            if (DialogImportExcelFile.ShowDialog() == DialogResult.OK)
            {
                StrOriginFilePath = DialogImportExcelFile.FileName;
                //DBのLastLoadFromDirを更新
                if (appSetting != null)
                {
                    appSetting.StrLastLoadFromDir = Path.GetDirectoryName(DialogImportExcelFile.FileName); 
                }
                dbLoadDir.SaveChanges();
                dbLoadDir.Dispose();
                //ダイアログのフィルタを無視して選択された場合の防御(拡張子チェック)
                if (!IsSupportedExcelFile(StrOriginFilePath))
                {
                    MessageBox.Show("Excelファイル(*.xlsx / *.xls)を選択してください。",
                        "ファイル選択エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return string.Empty;
                }
                if (Path.GetExtension(StrOriginFilePath).Equals(".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    //xlsファイルだった場合(変換が必要)
                    if (!File.Exists(StrOriginFilePath))
                    {
                        MessageBox.Show("ファイルが見つかりません");
                        return string.Empty;
                    }
                    try
                    {
                        XlsToXlsx(StrOriginFilePath);
                    }
                    catch (Exception)
                    {
                        //XlsToXlsx 側で種別に応じたエラーダイアログ表示済み。
                        //再スローされた例外が呼び出し側(Frm)で二重表示・未捕捉にならないようここで止める
                        return string.Empty;
                    }
                    return StrConvertedFilePath ?? string.Empty;
                }
                else
                {
                    return StrOriginFilePath;
                }
            }
            MessageBox.Show("ファイル選択がキャンセルされました");
            return string.Empty;
        }
        /// <summary>
        /// ダイアログで許可している拡張子(.xlsx / .xls)かどうか。
        /// フィルタを無視した選択(.csv等)を弾くための防御チェック。
        /// ※テスト可能にするため internal
        /// </summary>
        internal static bool IsSupportedExcelFile(string? StrFilePath)
        {
            if (string.IsNullOrEmpty(StrFilePath)) { return false; }
            string strExt = Path.GetExtension(StrFilePath);
            return strExt.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || strExt.Equals(".xls", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// .xls を .xlsx に変換し、一時ファイルパスを StrConvertedFilePath に設定する。
        /// 失敗時は例外を投げ、種別に応じたエラーダイアログを表示する(UI 層)。
        /// 変換ロジック自体は ConvertXlsToXlsx に分離している(UI 非依存・テスト可能)。
        /// </summary>
        internal void XlsToXlsx(string StrOldExcelFilePath)
        {
            //出力パスの決定(変換元と同じフォルダ)
            string StrXlsfileDir = Path.GetDirectoryName(StrOldExcelFilePath) ?? Path.GetTempPath();
            string StrOutputFileName = Path.Combine(StrXlsfileDir, Path.GetRandomFileName() + ".xlsx");
            try
            {
                ConvertXlsToXlsx(StrOldExcelFilePath, StrOutputFileName);
                StrConvertedFilePath = StrOutputFileName;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"変換元のファイルが見つかりません。\n{ex.FileName}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch (ArgumentException ex)
            {
                //ExcelDataReaderが認識できない形式(.xls偽装ファイル等)
                MessageBox.Show($"Excelファイルとして認識できませんでした。\nファイル形式(.xls)を確認して下さい。\n\n{ex.Message}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw;
            }
            catch (ExcelReaderException ex)
            {
                //ExcelDataReaderがファイルシグネチャを認識できない(.xls偽装ファイル・破損等)
                MessageBox.Show($"Excelファイルとして認識できませんでした。\nファイル形式(.xls)を確認して下さい。\n\n{ex.Message}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw;
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show($"{ex.Message}\nファイルを確認して下さい。",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"ファイルへのアクセスが拒否されました。\n出力先フォルダの書き込み権限を確認して下さい。\n出力先: {StrOutputFileName}\n\n{ex.Message}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch (IOException ex)
            {
                //読み取りロック・ディスクエラー・生成失敗等
                MessageBox.Show($"ファイルの読み書きに失敗しました。\nファイルが他のプログラム(Excel等)で開かれていないか確認して下さい。\n\n{ex.Message}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"xls→xlsx変換に失敗しました。\n\n{ex.Message}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// .xls → .xlsx 変換のコア処理(UI 非依存・テスト可能)。
        /// 失敗時は種別に応じた例外を投げる(FileNotFoundException / ArgumentException /
        /// InvalidDataException / UnauthorizedAccessException / IOException)。
        /// </summary>
        internal static void ConvertXlsToXlsx(string StrOldExcelFilePath, string StrOutputFileName)
        {
            //旧バイナリ形式(.xls)をExcelDataReaderで読み取り、ClosedXMLで.xlsxとして書き出す
            //Excelの起動は行わない(Excel/Office非依存)

            //1.変換元ファイルの実体確認
            if (!File.Exists(StrOldExcelFilePath))
            {
                throw new FileNotFoundException("変換元のファイルが見つかりません", StrOldExcelFilePath);
            }

            using (FileStream fsOrigin = File.Open(StrOldExcelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(fsOrigin))
            using (XLWorkbook xlWorkbook = new())
            {
                //2.シート存在チェック(シートが1つも無いファイルを弾く)
                if (reader.ResultsCount == 0)
                {
                    throw new InvalidDataException("ファイル内にシートが見つかりません");
                }

                int intSheetIndex = 0;
                do
                {
                    //シート名はExcel/ClosedXMLの制約(不正文字・長さ・重複)に合うよう整形してから追加
                    IXLWorksheet xlSheet = xlWorkbook.Worksheets.Add(SanitizeSheetName(reader.Name, intSheetIndex, xlWorkbook));
                    int intRow = 1;
                    while (reader.Read())
                    {
                        for (int intCol = 0; intCol < reader.FieldCount; intCol++)
                        {
                            if (reader.IsDBNull(intCol)) { continue; }
                            object? value = reader.GetValue(intCol);
                            if (value == null) { continue; }
                            IXLCell xlCell = xlSheet.Cell(intRow, intCol + 1);
                            xlCell.Value = ToXlCellValue(value);
                            //表示書式があれば引き継ぐ(値自体はGetValueの型で保持)。
                            //書式の取得・設定失敗は値に影響しないため致命的化しない
                            try
                            {
                                string? StrFormat = reader.GetNumberFormatString(intCol);
                                if (!string.IsNullOrEmpty(StrFormat))
                                {
                                    xlCell.Style.NumberFormat.Format = StrFormat;
                                }
                            }
                            catch
                            {
                                //意図的に無視(表示書式のみの問題)
                            }
                        }
                        intRow++;
                    }
                    intSheetIndex++;
                } while (reader.NextResult());
                xlWorkbook.SaveAs(StrOutputFileName);
            }

            //3.出力ファイルの実体確認
            if (!File.Exists(StrOutputFileName))
            {
                throw new IOException($"xlsxファイルが生成されませんでした。\n出力先: {StrOutputFileName}");
            }
        }

        /// <summary>
        /// シート名をExcel/ClosedXMLの制約に合わせて整形する
        /// (不正文字 : \ / ? * [ ] の置換、31文字制限、空名・重複名の回避)
        /// ※テスト可能にするため internal
        /// </summary>
        internal static string SanitizeSheetName(string? StrRawName, int intSheetIndex, XLWorkbook xlWorkbook)
        {
            string StrName = (StrRawName ?? string.Empty).Trim();
            foreach (char c in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            {
                StrName = StrName.Replace(c, '_');
            }
            if (StrName.Length > 31) { StrName = StrName.Substring(0, 31); }
            if (string.IsNullOrWhiteSpace(StrName)) { StrName = $"Sheet{intSheetIndex + 1}"; }
            //同名シート回避(切り詰め後や元ファイルの重複名対策)
            string StrBase = StrName.Length > 28 ? StrName.Substring(0, 28) : StrName;
            int intSuffix = 1;
            while (xlWorkbook.Worksheets.Any(ws => string.Equals(ws.Name, StrName, StringComparison.OrdinalIgnoreCase)))
            {
                StrName = $"{StrBase}_{intSuffix}";
                intSuffix++;
            }
            return StrName;
        }

        /// <summary>
        /// ExcelDataReaderの GetValue() が返す値(double/int/bool/DateTime/TimeSpan/string)を
        /// ClosedXMLのセル値に変換する
        /// </summary>
        private static XLCellValue ToXlCellValue(object value) => value switch
        {
            bool b => b,
            DateTime dt => dt,
            TimeSpan ts => ts,
            double d => d,
            int i => i,
            string s => s,
            _ => value.ToString() ?? string.Empty,
        };
    }
}
