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
            //�󂾂����瑽���L�����Z���������Ȃ̂ł��̂܂ܐÂ��ɔ�����
            if (string.IsNullOrEmpty(ImportExcelFilePath)) { return; }
            if (!File.Exists(ImportExcelFilePath))
            {
                MessageBox.Show("�w�肳�ꂽ�t�@�C����������܂���ł���(ImportFile)");
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
                    MessageBox.Show("DB��荞�݊���");
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
        /// ���t�t�B���^�̏����ݒ���s���܂�
        /// </summary>
        private void DateTimePickerInitialize()
        {
            //1�����O�̏��������߂�
            DateTime datePrevMonth = new(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
            DtpickStart.Value = datePrevMonth;
            //1�����O�̍ŏI����23:59:59
            DtpickEnd.Value = new DateTime(datePrevMonth.Year,
                                             datePrevMonth.Month,
                                             DateTime.DaysInMonth(datePrevMonth.Year, datePrevMonth.Month)
                                             ).AddDays(1).AddSeconds(-1);
            //�\���`���ύX
            DtpickStart.CustomFormat = "yyyy�NMM��dd�� HH��mm��ss�b";
            DtpickEnd.CustomFormat = "yyyy�NMM��dd�� HH��mm��ss�b";
        }

        private void BtnFieldNamAlias_Click(object sender, EventArgs e)
        {
            FrmDataViewer FrmDataViewer = new();
            FrmDataViewer.ShowDialog();
        }
    }
}
