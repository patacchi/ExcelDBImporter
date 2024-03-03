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
                    ExcelDbContext dbContext = new();
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

        /// <summary>
        /// �w�肳�ꂽ���t���������xlsx�t�@�C�����o�͂��܂�
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
                                                    //���o�����̑I��
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
                        "�Y������f�[�^������܂���ł����B���o�������m�F���ĉ�����\n" +
                        "�J�n�����F" + DtpickStart.Value.ToString() + "\n" +
                        "�I�������F" + DtpickEnd.Value.ToString() + "\n"
                        );
                    return;
                }
                //�ۑ��t�@�C�����I��
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = "5P8D______�}�[�V�������O���яW�v" + DtpickStart.Value.Date.Year + "�N" + DtpickStart.Value.Date.Month + "��"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    XLWorkbook wb = new();
                    IXLWorksheet xlworksheet = wb.AddWorksheet("�}�[�V�������O���яW�v" + DtpickStart.Value.Date.Year + "�N" + DtpickStart.Value.Date.Month + "��");
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
                    MessageBox.Show("xlsx�t�@�C���o�͊������܂����B");
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
