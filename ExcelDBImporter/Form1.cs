using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDBImporter.Context;
using ExcelDBImporter.Modeles;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter
{
    public partial class FrmExcelImpoerter : Form
    {
        public FrmExcelImpoerter()
        {
            InitializeComponent();
            DateTimePickerInitialize();
            Tool.DatabaseInitializer.DatabaseExlistCheker();
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
                    ExcelDbContext dbContext = new();
                    MessageBox.Show("DB取り込み完了");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
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
        private void DateTimePickerInitialize()
        {
            //1か月前の初日を求める
            DateTime datePrevMonth = new(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
            DtpickStart.Value = datePrevMonth;
            //1か月前の最終日の23:59:59
            DtpickEnd.Value = new DateTime(datePrevMonth.Year,
                                             datePrevMonth.Month,
                                             DateTime.DaysInMonth(datePrevMonth.Year, datePrevMonth.Month)
                                             ).AddDays(1).AddSeconds(-1);
            //表示形式変更
            DtpickStart.CustomFormat = "yyyy年MM月dd日 HH時mm分ss秒";
            DtpickEnd.CustomFormat = "yyyy年MM月dd日 HH時mm分ss秒";
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
            DateTime dateStart = DtpickStart.Value.Date;
            DateTime dateEnd = DtpickEnd.Value.Date;
            if (dateStart > dateEnd) { dateEnd = dateStart; }
            try
            {
                using ExcelDbContext dbContext = new();
                var listFilterdData = dbContext.ExcelData
                                                .Where(e => e.DateMarshalling >= dateStart && e.DateMarshalling <= dateEnd)
                                                .OrderBy(e => e.DateMarshalling)
                                                .ThenBy(e => e.StrSeiban)
                                                .Select(e => new
                                                {
                                                    //抽出する列の選択
                                                    e.StrSeiban,
                                                    e.StrOrderFrom,
                                                    e.StrHinmei,
                                                    e.IntAmount,
                                                    e.DateMarshalling
                                                }
                                                ) .ToList();
                if (listFilterdData == null || listFilterdData.Count == 0)
                {
                    MessageBox.Show(
                        "該当するデータがありませんでした。抽出条件を確認して下さい\n" +
                        "開始日時：" + DtpickStart.Value.ToString() + "\n" +
                        "終了日時：" + DtpickEnd.Value.ToString() + "\n"
                        );
                    return;
                }
                //保存ファイル名選択
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = "5P8D______マーシャリング実績集計" + DtpickStart.Value.Date.Year + "年" + DtpickStart.Value.Date.Month + "月"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    XLWorkbook wb = new();
                    IXLWorksheet xlworksheet = wb.AddWorksheet("マーシャリング実績集計" + DtpickStart.Value.Date.Year + "年" + DtpickStart.Value.Date.Month + "月");
                    xlworksheet.Cell(10, 1).InsertTable(listFilterdData);
                    var CellsTitle = xlworksheet.Row(10).CellsUsed();
                    foreach ( var cell in CellsTitle )
                    {
                        var aliasName = dbContext.TableFieldAliasNameLists
                                                .Where(t => t.StrClassName == typeof(ShShukka).Name &&
                                                t.StrColumnName == cell.Value.ToString())
                                                .FirstOrDefault();
                        if (aliasName != null)
                        {
                            cell.Value = aliasName.StrColnmnAliasName == null ? cell.Value : aliasName.StrColnmnAliasName; 
                        }
                    }
                    xlworksheet.ColumnsUsed().AdjustToContents();
                    wb.SaveAs(saveFileDialog.FileName);
                    wb.Dispose();
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
    }
}
