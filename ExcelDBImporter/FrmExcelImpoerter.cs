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
using ExcelDBImporter.Models.View;
using ExcelDBImporter.Tool;
using System.Text;


namespace ExcelDBImporter
{
    public partial class FrmExcelImpoerter : Form
    {
        /// <summary>
        /// 出力Excelファイルの横幅
        /// </summary>
        private const Single Const_Outpu_Title_Width = 9;
        private const int Const_DataTable_Header_Row = 4;
        private const int Const_MainTitle_Row = 2;

        public FrmExcelImpoerter()
        {
            try
            {
                InitializeComponent();
                Tool.DatabaseInitializer.DatabaseExlistCheker();
                Text = Text + " Ver " + Tool.AssemblyInfo.AssemblyVersion();
                AppSettingExistsCheck();
                DateTimePickerInitialize();
            }
            catch
            {
                this.Close();
                throw;
            }
        }
        private void BtnImputExcelFile_Click(object sender, EventArgs e)
        {
            //.xls(バイナリ)ファイルだった場合は.xlsx(XML)ファイルに変換する
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
            using ExcelDbContext dbContext = new();
            try
            {
                //TQRinputテーブルより、ViewMarsharingテーブルに集計を行う
                List<ViewMarsharing>? views = dbContext.TQRinputs
                                            .GroupBy(tqr => tqr.DateInputDate.Date)
                                            .Select(g => new ViewMarsharing
                                            {
                                                DatePerDay = g.Key,
                                                IntPrepareReceive =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.PrepareReveiveSet) ? 1 : 0),
                                                IntFreewayData =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.FreewayDataInput) ? 1 : 0),
                                                IntDelivery =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.Delivery) ? 1 : 0),
                                                IntShipping =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.PrepareShpping) ? 1 : 0),
                                                IntMicroWave =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.opMicroWave) ? 1 : 0),
                                                IntCableCut =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.CutCable) ? 1 : 0),
                                                IntMoving =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.Moving) ? 1 : 0),
                                                IntOther =
                                                g.Sum(tqr => tqr.QROPcode.HasFlag(QrOPcode.Other) ? 1 : 0)
                                            }
                                            ).ToList();
                //リストの結果をUpsert
                dbContext.UpsertEntities(views)
                    .Execute();

                /*
                StringBuilder sbLog = new();
                dbContext.BulkMerge(views, options =>
                {
                    options.ColumnPrimaryKeyExpression = c => c.DatePerDay;
                    options.Log = s => sbLog.AppendLine(s);
                    options.IgnoreOnMergeUpdateExpression = ig => new
                    {
                        ig.IsCompiled
                    };
                }
                );
                dbContext.BulkSaveChanges(options =>
                {
                    options.Log = s => sbLog.AppendLine(s);
                });
                Debug.WriteLine(sbLog.ToString());
                */

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{nameof(DateTimePickerInitialize)} で {ex.Message} エラー");
                return;
            }
            //Outputフラグが立っているもので最新のデータを取得

            ViewMarsharing? OutputNewest = dbContext.ViewMarsharings
                                    .Where(s => s.IsCompiled == true)
                                    .OrderByDescending(s => s.DatePerDay)
                                    .FirstOrDefault();

            DateTime dateFirstDayinTargetManth = new(DateTime.Now.AddMonths(IntOffsetMonth).Year, DateTime.Now.AddMonths(IntOffsetMonth).Month, 1);
            //フラグが立っているレコードが無かった場合は現在日時を設定する
            DateTime DateEndDayofMarshalling = (OutputNewest == null
                                  || OutputNewest.DatePerDay == null) ? DateTime.Now : (DateTime)OutputNewest.DatePerDay;
            //フラグ付き最新データと対象月初日の日付が古いほうをスタート日付とする
            DtpickStart.Value = dateFirstDayinTargetManth <= DateEndDayofMarshalling ? dateFirstDayinTargetManth : DateEndDayofMarshalling;
            //終了日は前日 23:59:59.9999
            DtpickEnd.Value = new DateTime(DateTime.Now.Year,
                                            DateTime.Now.Month,
                                            DateTime.Now.Day
                                            ).AddMilliseconds(-1);
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

        private void BtnfrmShowQR_Read_Click(object sender, EventArgs e)
        {
            FrmQRread frmQRread = new();
            frmQRread.ShowDialog();
        }
        private void BtnShowQRForm_Click(object sender, EventArgs e)
        {
            FrmPrintQRCode frmPrintQRCode = new();
            frmPrintQRCode.ShowDialog();
        }

        private void OutputxlsxFilterdByTimePickerTime()
        {
            DateTime dateStart = DtpickStart.Value.Date;
            DateTime dateEnd = DtpickEnd.Value.Date;
            if (dateStart > dateEnd) { dateEnd = dateStart; }
            try
            {
                //開始日と終了日の間でデータが無いものを補完する
                ViewTableEditor vte = new();
                vte.FillEmptyDay(dateStart, dateEnd);
                using ExcelDbContext dbContext = new();
                //dbContext.ViewMarsharings.AddRange(views);
                //日付が範囲内でなおかつ出力済みでは「無い」物を選択
                var views = dbContext.ViewMarsharings
                                    .Where(e => e.DatePerDay >= dateStart && e.DatePerDay <= dateEnd
                                        && e.IsCompiled == false)
                                            .OrderBy(e => e.DatePerDay)
                                            .Select(s => new
                                            {
                                                s.DatePerDay,
                                                s.IntPrepareReceive,
                                                s.IntFreewayData,
                                                s.IntDelivery,
                                                s.IntShipping,
                                                s.IntMicroWave,
                                                s.IntCableCut,
                                                s.IntMoving,
                                                s.IntOther
                                            })
                                            .ToList();
                if (views == null || views.Count == 0)
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
                    FileName = "5D8B4869P002_電磁・マイクロ波資材管理実績集計" + DtpickEnd.Value.Date.Year + "年" + DtpickEnd.Value.Date.Month + "月"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int IntTableHeaderRow = Const_DataTable_Header_Row;
                    int IntTitleRow = Const_MainTitle_Row;
                    double DblTitleFontSize = 13;
                    XLWorkbook wb = new();
                    //デフォルトのフォントとフォントサイズを設定
                    XLWorkbook.DefaultStyle.Font.FontName = "BIZ UDゴシック";
                    wb.Style.Font.FontName = "BIZ UDゴシック";
                    wb.Style.Font.FontSize = 9;
                    IXLWorksheet xlworksheet = wb.AddWorksheet("電磁・マイクロ波資材管理実績集計" + DtpickEnd.Value.Date.Year + "年" + DtpickEnd.Value.Date.Month + "月");
                    //リストをシートに挿入
                    xlworksheet.Cell(IntTableHeaderRow, 1).InsertTable(views);
                    var CellsTitle = xlworksheet.Row(IntTableHeaderRow).CellsUsed();
                    foreach (IXLCell? cell in CellsTitle)
                    {
                        var aliasName = dbContext.TableFieldAliasNameLists
                                                .Include(tableDBColumn => tableDBColumn.DBcolumn)
                                                .Where(table => table.DBcolumn.StrClassName == typeof(ShShukka).Name &&
                                                table.DBcolumn.StrDBColumnName == cell.Value.ToString())
                                                /*.Where(t => t.StrClassName == typeof(ShShukka).Name &&
                                                t.StrColumnName == cell.Value.ToString())*/
                                                .FirstOrDefault();
                        cell.Value = GetAllProperty.GetPropertyComment<ViewMarsharing>
                                    (cell.Value.ToString())
                                    ?? cell.Value;
                        //各セルの横幅を設定
                        xlworksheet.Column(cell.Address.ColumnNumber).Width = Const_Outpu_Title_Width;
                        //上下左右方向中央揃え
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    //上下左右に罫線を引く
                    xlworksheet.Range(xlworksheet.Cell(IntTableHeaderRow, 1), (xlworksheet.LastCellUsed())).Style
                        .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    //とりあえず幅の自動調整はなしで
                    //xlworksheet.ColumnsUsed().AdjustToContents();
                    //foreach (IXLCell cell1 in CellsTitle) { xlworksheet.Column(cell1.Address.ColumnNumber).Width *= 1.30; }
                    //タイトルの入力
                    xlworksheet.Cell(IntTitleRow, 1).Value = DtpickEnd.Value.Date.Year + "年" + DtpickEnd.Value.Date.Month + "月 (MS)  電磁・マイクロ波資材管理実績    5D8B4869P002";
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
                    dbContext.ViewMarsharings
                        .Where(v => v.DatePerDay >= dateStart && v.DatePerDay <= dateEnd)
                        .ExecuteUpdate(u => u.SetProperty(p => p.IsCompiled, true));
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
                return;
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

        /// <summary>
        /// 入出庫履歴CSV取込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInOutCSVInclude_Click(object sender, EventArgs e)
        {
            IncludeInOutCSV();
            //ShInOutをTQRに反映させる
            ShInOutToTQR();

        }
    }
}
