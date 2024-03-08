using System.Diagnostics;
using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter
{
    public partial class FrmExcelImpoerter : Form
    {
        public FrmExcelImpoerter()
        {
            InitializeComponent();
            Tool.DatabaseInitializer.DatabaseExlistCheker();
            AppSettingExistsCheck();
            DateTimePickerInitialize();
        }
        private void BtnImputExcelFile_Click(object sender, EventArgs e)
        {
            ExcelFileComverter ImportExcelFileConverter = new();
            string ImportExcelFilePath = ImportExcelFileConverter.ExcelFileComVerter();
            //空だったら多分キャンセルか何かなのでそのまま静かに抜ける
            if (string.IsNullOrEmpty(ImportExcelFilePath)) { return; }
            if (!File.Exists(ImportExcelFilePath))
            {
                MessageBox.Show("指定されたファイルが見つかりませんでした(ImportFile)");
                return;
            }
            textBoxInputFilename.Text = ImportExcelFileConverter.StrOriginFilePath;
            using (XLWorkbook xlImputWorkbook = new(ImportExcelFilePath))
            {
                try
                {
                    var xlWorkSheet = xlImputWorkbook.Worksheet(2);
                    IXLRange? rangeInport = xlWorkSheet.RangeUsed();
                    if (rangeInport == null)
                    {
                        return;
                    }
                    ShShukkaUpsert shShukkaUpsert = new(rangeInport);
                    shShukkaUpsert.DoUpsert();
                    //ExcelDbContext dbContext = new();
                    MessageBox.Show("DB取り込み完了");
                }
                catch (ArgumentException arg)
                {
                    if (arg.Message.Contains("There isn't a worksheet associated with that position."))
                    {
                        MessageBox.Show("指定された位置にシートが見つかりませんでした。\n入力ファイルを確認して下さい。");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                    throw;
                }
                finally
                {
                    if (xlImputWorkbook != null) { xlImputWorkbook?.Dispose(); }
                }
            }
            if (!string.IsNullOrEmpty(ImportExcelFileConverter.StrConvertedFilePath))
            {
                if (File.Exists(ImportExcelFileConverter.StrConvertedFilePath))
                {
                    File.Delete(ImportExcelFileConverter.StrConvertedFilePath);
                }
            }
        }

        /// <summary>
        /// 日付フィルタの初期設定を行います
        /// </summary>
        public void DateTimePickerInitialize()
        {
            //1か月前の初日を求める
            //当月に変更になった・・・
            int IntOffsetMonth = 0;
            //Outputフラグが立っているもので最新のデータを取得
            ExcelDbContext dbContext = new();
            ShShukka? OutputNewest = dbContext.ShShukka
                                    .Where(s => s.IsAlreadyOutput == true)
                                    .OrderByDescending(s => s.DateMarshalling)
                                    .FirstOrDefault();

            DateTime dateFirstDayinTargetManth = new(DateTime.Now.AddMonths(IntOffsetMonth).Year, DateTime.Now.AddMonths(IntOffsetMonth).Month, 1);
            //フラグが立っているレコードが無かった場合は現在日時を設定する
            DateTime DateOlder = (OutputNewest == null
                                  || OutputNewest.DateMarshalling == null) ? DateTime.Now : (DateTime)OutputNewest.DateMarshalling;
            //フラグ付き最新データと対象月初日の日付が古いほうをスタート日付とする
            DtpickStart.Value = dateFirstDayinTargetManth <= DateOlder ? dateFirstDayinTargetManth : DateOlder;
            //1か月前の最終日の23:59:59
            DtpickEnd.Value = new DateTime(dateFirstDayinTargetManth.Year,
                                             dateFirstDayinTargetManth.Month,
                                             DateTime.DaysInMonth(dateFirstDayinTargetManth.Year, dateFirstDayinTargetManth.Month)
                                             ).AddDays(1).AddSeconds(-1);
            //表示形式変更
            DtpickStart.CustomFormat = "yyyy年MM月dd日 HH時mm分ss秒";
            DtpickEnd.CustomFormat = "yyyy年MM月dd日 HH時mm分ss秒";
            dbContext.Dispose();
        }

        private void BtnFieldNamAlias_Click(object sender, EventArgs e)
        {
            FrmDataViewer FrmDataViewer = new();
            FrmDataViewer.ShowDialog();
        }

        /// <summary>
        /// 指定された日付条件を基にxlsxファイルを出力します
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOutputToxlsx_Click(object sender, EventArgs e)
        {
            OutputxlsxFilterdByTimePickerTime();

        }

        private void OutputxlsxFilterdByTimePickerTime()
        {
            DateTime dateStart = DtpickStart.Value.Date;
            DateTime dateEnd = DtpickEnd.Value.Date;
            if (dateStart > dateEnd) { dateEnd = dateStart; }
            try
            {
                using ExcelDbContext dbContext = new();
                //日付が範囲内でなおかつ出力済みでは「無い」物を選択
                var listFilterdData = dbContext.ShShukka
                                                .Where(e => e.DateMarshalling >= dateStart && e.DateMarshalling <= dateEnd
                                                        && e.IsAlreadyOutput == false)
                                                .OrderBy(e => e.DateMarshalling)
                                                .ThenBy(e => e.StrSeiban)
                                                .Select(e => new
                                                {
                                                    //抽出する列の選択
                                                    e.StrSeiban,
                                                    //e.StrOrderFrom,
                                                    e.StrKishu,
                                                    e.StrHinmei,
                                                    e.IntAmount,
                                                    e.DateMarshalling
                                                }
                                                ).ToList();
                if (listFilterdData == null || listFilterdData.Count == 0)
                {
                    MessageBox.Show(
                        "該当するデータがありませんでした。抽出条件を確認して下さい\n" +
                        "開始日時：" + DtpickStart.Value.ToString() + "\n" +
                        "終了日時：" + DtpickEnd.Value.ToString() + "\n"
                        );
                    return;
                }

                //DBより保存ディレクトリの設定があるかチェック、なければString.Empty
                AppSetting? appExists = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == CONST_STR_ExcelDBImporterAppName);
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
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = "5D8B4869P002_マーシャリング実績集計" + DtpickEnd.Value.Date.Year + "年" + DtpickEnd.Value.Date.Month + "月"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int IntTableHeaderRow = 4;
                    int IntTitleRow = 2;
                    double DblTitleFontSize = 13;
                    XLWorkbook wb = new();
                    //デフォルトのフォントとフォントサイズを設定
                    XLWorkbook.DefaultStyle.Font.FontName = "BIZ UDゴシック";
                    wb.Style.Font.FontName = "BIZ UDゴシック";
                    wb.Style.Font.FontSize = 9;
                    IXLWorksheet xlworksheet = wb.AddWorksheet("マーシャリング実績集計" + DtpickEnd.Value.Date.Year + "年" + DtpickEnd.Value.Date.Month + "月");
                    xlworksheet.Cell(IntTableHeaderRow, 1).InsertTable(listFilterdData);
                    var CellsTitle = xlworksheet.Row(IntTableHeaderRow).CellsUsed();
                    foreach (IXLCell? cell in CellsTitle)
                    {
                        var aliasName = dbContext.TableFieldAliasNameLists
                                                .Where(t => t.StrClassName == typeof(ShShukka).Name &&
                                                t.StrColumnName == cell.Value.ToString())
                                                .FirstOrDefault();
                        if (aliasName != null)
                        {
                            cell.Value = aliasName.StrColnmnAliasName ?? cell.Value;
                        }
                    }
                    xlworksheet.ColumnsUsed().AdjustToContents();
                    foreach (IXLCell cell1 in CellsTitle) { xlworksheet.Column(cell1.Address.ColumnNumber).Width *= 1.30; }
                    //タイトルの入力
                    xlworksheet.Cell(IntTitleRow, 1).Value = DtpickEnd.Value.Date.Year + "年" + DtpickEnd.Value.Date.Month + "月  マーシャリング実績    5D8B4869P002";
                    xlworksheet.Cell(IntTitleRow, 1).Style.Font.FontSize = DblTitleFontSize;
                    //選択範囲で中央(上手くいくかな？)
                    xlworksheet.Range(IntTitleRow, 1, IntTitleRow, xlworksheet.Row(IntTableHeaderRow).LastCellUsed().Address.ColumnNumber)
                        .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.CenterContinuous;
                    //タイトル行の設定
                    xlworksheet.PageSetup.SetRowsToRepeatAtTop(1, IntTableHeaderRow);
                    //印刷範囲の設定
                    xlworksheet.PageSetup.PrintAreas.Add(xlworksheet.Cell(1, 1).Address, xlworksheet.LastCellUsed().Address);
                    wb.SaveAs(saveFileDialog.FileName);
                    wb.Dispose();
                    //出力済みフラグをセット
                    dbContext.ShShukka
                        .Where(e => e.DateMarshalling >= dateStart && e.DateMarshalling <= dateEnd)
                        .ExecuteUpdate(u => u.SetProperty(p => p.IsAlreadyOutput, true));
                    //出力ディレクトリを更新
                    AppSetting? appSetting = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == CONST_STR_ExcelDBImporterAppName);
                    if (appSetting == null) 
                    {
                        //アプリ設定そのものが見つからなかった
                        MessageBox.Show("アプリ設定が見つかりませんでした。処理を中断します\n" + CONST_STR_ExcelDBImporterAppName);
                    }
                    else
                    {
                        //出力ディレクトを更新
                        appSetting.StrLastSaveToDir = Path.GetDirectoryName(saveFileDialog.FileName);
                        dbContext.SaveChanges();
                    }
                    dbContext.Dispose();
                    //出力ファイル名をテキストボックスに適用
                    TextBoxOutputFileName.Text = saveFileDialog.FileName;
                    MessageBox.Show("xlsxファイル出力完了しました。");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            finally
            {
            }
        }

        /// <summary>
        /// 出力ファイル名で示されるディレクトリが存在すれば、表示ボタンを有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxOutputFileName_TextChanged(object sender, EventArgs e)
        {
            //テキストボックスが空ではない場合のみ処理
            if (!string.IsNullOrEmpty(TextBoxOutputFileName.Text) && Directory.Exists(Path.GetDirectoryName(TextBoxOutputFileName.Text)))
            {
                //更にテキストボックスのディレクトリが存在する時のフォルダ表示ボタン有効に
                BtnOpenOutputDir.Enabled = true;
            }
            //テキストボックスが空の場合はフォルダ表示ボタン無効
            else
            {
                BtnOpenOutputDir.Enabled = false;
            }
        }

        /// <summary>
        /// 出力ディレクトリをエクスプローラーで開くボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpenOutputDir_Click(object sender, EventArgs e)
        {
            string Strfolderpath = Path.GetDirectoryName(TextBoxOutputFileName.Text) ?? string.Empty;
            OpenFolderInExplorer(Strfolderpath);
        }

        private static void OpenFolderInExplorer(string folderPath)
        {
            try
            {
                // フォルダが存在するかどうか確認
                if (Directory.Exists(folderPath))
                {
                    // 指定のパスのフォルダをエクスプローラーで開く
                    Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    MessageBox.Show("指定のフォルダが存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エクスプローラーを開く際にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmExcelImpoerter_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //ctrlキーが押されている間だけフラグ解除ボタンを有効にする
                case Keys.ControlKey:
                    {
                        BtnUnsetOutputFlag.Enabled = true;
                        break;
                    }
                //それ以外は特に何もしない
                default: { return; }
            }
        }

        private void FrmExcelImpoerter_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //ctrlキーが離れたらフラグ解除ボタンを無効にする
                case Keys.ControlKey:
                    {
                        BtnUnsetOutputFlag.Enabled = false;
                        break;
                    }
                //それ以外は特に何もしない
                default: { return; }
            }
        }

        private void BtnUnsetOutputFlag_Click(object sender, EventArgs e)
        {
            _ = UnsetOutputFlagByTimePickerTime();
            DateTimePickerInitialize();
            BtnUnsetOutputFlag.Enabled = false;
            return;
        }
    }
}
