using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ExcelDBImporter.Context;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter
{
    public partial class FrmExcelImpoerter : Form
    {
        public FrmExcelImpoerter()
        {
            InitializeComponent();
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
                    ExcelDbContext dbContext = new ();
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
    }
}
