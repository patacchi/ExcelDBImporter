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
                if (Path.GetExtension(StrOriginFilePath).Equals(".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    //xlsファイルだった場合(変換が必要)
                    if (!File.Exists(StrOriginFilePath))
                    {
                        MessageBox.Show("ファイルが見つかりません");
                        return string.Empty;
                    }
                    XlsToXlsx(StrOriginFilePath);
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
        private void XlsToXlsx(string StrOldExcelFilePath)
        {
            //旧バイナリ形式(.xls)をExcelDataReaderで読み取り、ClosedXMLで.xlsxとして書き出す
            //Excelの起動は行わない(Excel/Office非依存)

            //1.変換元ファイルの実体確認
            if (!File.Exists(StrOldExcelFilePath))
            {
                MessageBox.Show($"変換元のファイルが見つかりません。\n{StrOldExcelFilePath}",
                    "xls→xlsx変換エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new FileNotFoundException("変換元のファイルが見つかりません", StrOldExcelFilePath);
            }

            //2.出力パスの決定(変換元と同じフォルダ)
            string StrXlsfileDir = Path.GetDirectoryName(StrOldExcelFilePath) ?? Path.GetTempPath();
            string StrOutputFileName = Path.Combine(StrXlsfileDir, Path.GetRandomFileName() + ".xlsx");

            try
            {
                using (FileStream fsOrigin = File.Open(StrOldExcelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(fsOrigin))
                using (XLWorkbook xlWorkbook = new())
                {
                    //3.シート存在チェック(シートが1つも無いファイルを弾く)
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

                //4.出力ファイルの実体確認
                if (!File.Exists(StrOutputFileName))
                {
                    throw new IOException($"xlsxファイルが生成されませんでした。\n出力先: {StrOutputFileName}");
                }
                StrConvertedFilePath = StrOutputFileName;
            }
            catch (ArgumentException ex)
            {
                //ExcelDataReaderが認識できない形式(.xls偽装ファイル等)
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
        /// シート名をExcel/ClosedXMLの制約に合わせて整形する
        /// (不正文字 : \ / ? * [ ] の置換、31文字制限、空名・重複名の回避)
        /// </summary>
        private static string SanitizeSheetName(string? StrRawName, int intSheetIndex, XLWorkbook xlWorkbook)
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
