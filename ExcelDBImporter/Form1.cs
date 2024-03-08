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
                    //ExcelDbContext dbContext = new();
                    MessageBox.Show("DB��荞�݊���");
                }
                catch (ArgumentException arg)
                {
                    if (arg.Message.Contains("There isn't a worksheet associated with that position."))
                    {
                        MessageBox.Show("�w�肳�ꂽ�ʒu�ɃV�[�g��������܂���ł����B\n���̓t�@�C�����m�F���ĉ������B");
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
        /// ���t�t�B���^�̏����ݒ���s���܂�
        /// </summary>
        public void DateTimePickerInitialize()
        {
            //1�����O�̏��������߂�
            //�����ɕύX�ɂȂ����E�E�E
            int IntOffsetMonth = 0;
            //Output�t���O�������Ă�����̂ōŐV�̃f�[�^���擾
            ExcelDbContext dbContext = new();
            ShShukka? OutputNewest = dbContext.ShShukka
                                    .Where(s => s.IsAlreadyOutput == true)
                                    .OrderByDescending(s => s.DateMarshalling)
                                    .FirstOrDefault();

            DateTime dateFirstDayinTargetManth = new(DateTime.Now.AddMonths(IntOffsetMonth).Year, DateTime.Now.AddMonths(IntOffsetMonth).Month, 1);
            //�t���O�������Ă��郌�R�[�h�����������ꍇ�͌��ݓ�����ݒ肷��
            DateTime DateOlder = (OutputNewest == null
                                  || OutputNewest.DateMarshalling == null) ? DateTime.Now : (DateTime)OutputNewest.DateMarshalling;
            //�t���O�t���ŐV�f�[�^�ƑΏی������̓��t���Â��ق����X�^�[�g���t�Ƃ���
            DtpickStart.Value = dateFirstDayinTargetManth <= DateOlder ? dateFirstDayinTargetManth : DateOlder;
            //1�����O�̍ŏI����23:59:59
            DtpickEnd.Value = new DateTime(dateFirstDayinTargetManth.Year,
                                             dateFirstDayinTargetManth.Month,
                                             DateTime.DaysInMonth(dateFirstDayinTargetManth.Year, dateFirstDayinTargetManth.Month)
                                             ).AddDays(1).AddSeconds(-1);
            //�\���`���ύX
            DtpickStart.CustomFormat = "yyyy�NMM��dd�� HH��mm��ss�b";
            DtpickEnd.CustomFormat = "yyyy�NMM��dd�� HH��mm��ss�b";
            dbContext.Dispose();
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
                //���t���͈͓��łȂ����o�͍ς݂ł́u�����v����I��
                var listFilterdData = dbContext.ShShukka
                                                .Where(e => e.DateMarshalling >= dateStart && e.DateMarshalling <= dateEnd
                                                        && e.IsAlreadyOutput == false)
                                                .OrderBy(e => e.DateMarshalling)
                                                .ThenBy(e => e.StrSeiban)
                                                .Select(e => new
                                                {
                                                    //���o�����̑I��
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
                        "�Y������f�[�^������܂���ł����B���o�������m�F���ĉ�����\n" +
                        "�J�n�����F" + DtpickStart.Value.ToString() + "\n" +
                        "�I�������F" + DtpickEnd.Value.ToString() + "\n"
                        );
                    return;
                }

                //DB���ۑ��f�B���N�g���̐ݒ肪���邩�`�F�b�N�A�Ȃ����String.Empty
                AppSetting? appExists = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == CONST_STR_ExcelDBImporterAppName);
                string StrDBSaveDir = string.Empty;
                if (appExists != null)
                {

                    //LastSaveDir�ɐݒ�l�����݂���ꍇ�̂ݐݒ�
                    StrDBSaveDir = string.IsNullOrEmpty(appExists.StrLastSaveToDir) ? string.Empty : appExists.StrLastSaveToDir;
                }
                //�ۑ��t�@�C�����I��
                SaveFileDialog saveFileDialog = new()
                {
                    InitialDirectory = StrDBSaveDir,
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = "5D8B4869P002_�}�[�V�������O���яW�v" + DtpickEnd.Value.Date.Year + "�N" + DtpickEnd.Value.Date.Month + "��"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int IntTableHeaderRow = 4;
                    int IntTitleRow = 2;
                    double DblTitleFontSize = 13;
                    XLWorkbook wb = new();
                    //�f�t�H���g�̃t�H���g�ƃt�H���g�T�C�Y��ݒ�
                    XLWorkbook.DefaultStyle.Font.FontName = "BIZ UD�S�V�b�N";
                    wb.Style.Font.FontName = "BIZ UD�S�V�b�N";
                    wb.Style.Font.FontSize = 9;
                    IXLWorksheet xlworksheet = wb.AddWorksheet("�}�[�V�������O���яW�v" + DtpickEnd.Value.Date.Year + "�N" + DtpickEnd.Value.Date.Month + "��");
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
                    //�^�C�g���̓���
                    xlworksheet.Cell(IntTitleRow, 1).Value = DtpickEnd.Value.Date.Year + "�N" + DtpickEnd.Value.Date.Month + "��  �}�[�V�������O����    5D8B4869P002";
                    xlworksheet.Cell(IntTitleRow, 1).Style.Font.FontSize = DblTitleFontSize;
                    //�I��͈͂Œ���(��肭�������ȁH)
                    xlworksheet.Range(IntTitleRow, 1, IntTitleRow, xlworksheet.Row(IntTableHeaderRow).LastCellUsed().Address.ColumnNumber)
                        .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.CenterContinuous;
                    //�^�C�g���s�̐ݒ�
                    xlworksheet.PageSetup.SetRowsToRepeatAtTop(1, IntTableHeaderRow);
                    //����͈͂̐ݒ�
                    xlworksheet.PageSetup.PrintAreas.Add(xlworksheet.Cell(1, 1).Address, xlworksheet.LastCellUsed().Address);
                    wb.SaveAs(saveFileDialog.FileName);
                    wb.Dispose();
                    //�o�͍ς݃t���O���Z�b�g
                    dbContext.ShShukka
                        .Where(e => e.DateMarshalling >= dateStart && e.DateMarshalling <= dateEnd)
                        .ExecuteUpdate(u => u.SetProperty(p => p.IsAlreadyOutput, true));
                    //�o�̓f�B���N�g�����X�V
                    AppSetting? appSetting = dbContext.AppSettings.FirstOrDefault(a => a.StrAppName == CONST_STR_ExcelDBImporterAppName);
                    if (appSetting == null) 
                    {
                        //�A�v���ݒ肻�̂��̂�������Ȃ�����
                        MessageBox.Show("�A�v���ݒ肪������܂���ł����B�����𒆒f���܂�\n" + CONST_STR_ExcelDBImporterAppName);
                    }
                    else
                    {
                        //�o�̓f�B���N�g���X�V
                        appSetting.StrLastSaveToDir = Path.GetDirectoryName(saveFileDialog.FileName);
                        dbContext.SaveChanges();
                    }
                    dbContext.Dispose();
                    //�o�̓t�@�C�������e�L�X�g�{�b�N�X�ɓK�p
                    TextBoxOutputFileName.Text = saveFileDialog.FileName;
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

        /// <summary>
        /// �o�̓t�@�C�����Ŏ������f�B���N�g�������݂���΁A�\���{�^����L���ɂ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxOutputFileName_TextChanged(object sender, EventArgs e)
        {
            //�e�L�X�g�{�b�N�X����ł͂Ȃ��ꍇ�̂ݏ���
            if (!string.IsNullOrEmpty(TextBoxOutputFileName.Text) && Directory.Exists(Path.GetDirectoryName(TextBoxOutputFileName.Text)))
            {
                //�X�Ƀe�L�X�g�{�b�N�X�̃f�B���N�g�������݂��鎞�̃t�H���_�\���{�^���L����
                BtnOpenOutputDir.Enabled = true;
            }
            //�e�L�X�g�{�b�N�X����̏ꍇ�̓t�H���_�\���{�^������
            else
            {
                BtnOpenOutputDir.Enabled = false;
            }
        }

        /// <summary>
        /// �o�̓f�B���N�g�����G�N�X�v���[���[�ŊJ���{�^��
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
                // �t�H���_�����݂��邩�ǂ����m�F
                if (Directory.Exists(folderPath))
                {
                    // �w��̃p�X�̃t�H���_���G�N�X�v���[���[�ŊJ��
                    Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    MessageBox.Show("�w��̃t�H���_�����݂��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�G�N�X�v���[���[���J���ۂɃG���[���������܂���: {ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmExcelImpoerter_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //ctrl�L�[��������Ă���Ԃ����t���O�����{�^����L���ɂ���
                case Keys.ControlKey:
                    {
                        BtnUnsetOutputFlag.Enabled = true;
                        break;
                    }
                //����ȊO�͓��ɉ������Ȃ�
                default: { return; }
            }
        }

        private void FrmExcelImpoerter_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //ctrl�L�[�����ꂽ��t���O�����{�^���𖳌��ɂ���
                case Keys.ControlKey:
                    {
                        BtnUnsetOutputFlag.Enabled = false;
                        break;
                    }
                //����ȊO�͓��ɉ������Ȃ�
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
