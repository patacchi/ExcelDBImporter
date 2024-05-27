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
        /// �o��Excel�t�@�C���̉���
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
            //.xls(�o�C�i��)�t�@�C���������ꍇ��.xlsx(XML)�t�@�C���ɕϊ�����
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
            using ExcelDbContext dbContext = new();
            try
            {
                //TQRinput�e�[�u�����AViewMarsharing�e�[�u���ɏW�v���s��
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
                //���X�g�̌��ʂ�Upsert
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
                MessageBox.Show($"{nameof(DateTimePickerInitialize)} �� {ex.Message} �G���[");
                return;
            }
            //Output�t���O�������Ă�����̂ōŐV�̃f�[�^���擾

            ViewMarsharing? OutputNewest = dbContext.ViewMarsharings
                                    .Where(s => s.IsCompiled == true)
                                    .OrderByDescending(s => s.DatePerDay)
                                    .FirstOrDefault();

            DateTime dateFirstDayinTargetManth = new(DateTime.Now.AddMonths(IntOffsetMonth).Year, DateTime.Now.AddMonths(IntOffsetMonth).Month, 1);
            //�t���O�������Ă��郌�R�[�h�����������ꍇ�͌��ݓ�����ݒ肷��
            DateTime DateEndDayofMarshalling = (OutputNewest == null
                                  || OutputNewest.DatePerDay == null) ? DateTime.Now : (DateTime)OutputNewest.DatePerDay;
            //�t���O�t���ŐV�f�[�^�ƑΏی������̓��t���Â��ق����X�^�[�g���t�Ƃ���
            DtpickStart.Value = dateFirstDayinTargetManth <= DateEndDayofMarshalling ? dateFirstDayinTargetManth : DateEndDayofMarshalling;
            //�I�����͑O�� 23:59:59.9999
            DtpickEnd.Value = new DateTime(DateTime.Now.Year,
                                            DateTime.Now.Month,
                                            DateTime.Now.Day
                                            ).AddMilliseconds(-1);
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
                //�J�n���ƏI�����̊ԂŃf�[�^���������̂�⊮����
                ViewTableEditor vte = new();
                vte.FillEmptyDay(dateStart, dateEnd);
                using ExcelDbContext dbContext = new();
                //dbContext.ViewMarsharings.AddRange(views);
                //���t���͈͓��łȂ����o�͍ς݂ł́u�����v����I��
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
                    FileName = "5D8B4869P002_�d���E�}�C�N���g���ފǗ����яW�v" + DtpickEnd.Value.Date.Year + "�N" + DtpickEnd.Value.Date.Month + "��"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int IntTableHeaderRow = Const_DataTable_Header_Row;
                    int IntTitleRow = Const_MainTitle_Row;
                    double DblTitleFontSize = 13;
                    XLWorkbook wb = new();
                    //�f�t�H���g�̃t�H���g�ƃt�H���g�T�C�Y��ݒ�
                    XLWorkbook.DefaultStyle.Font.FontName = "BIZ UD�S�V�b�N";
                    wb.Style.Font.FontName = "BIZ UD�S�V�b�N";
                    wb.Style.Font.FontSize = 9;
                    IXLWorksheet xlworksheet = wb.AddWorksheet("�d���E�}�C�N���g���ފǗ����яW�v" + DtpickEnd.Value.Date.Year + "�N" + DtpickEnd.Value.Date.Month + "��");
                    //���X�g���V�[�g�ɑ}��
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
                        //�e�Z���̉�����ݒ�
                        xlworksheet.Column(cell.Address.ColumnNumber).Width = Const_Outpu_Title_Width;
                        //�㉺���E������������
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    //�㉺���E�Ɍr��������
                    xlworksheet.Range(xlworksheet.Cell(IntTableHeaderRow, 1), (xlworksheet.LastCellUsed())).Style
                        .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    //�Ƃ肠�������̎��������͂Ȃ���
                    //xlworksheet.ColumnsUsed().AdjustToContents();
                    //foreach (IXLCell cell1 in CellsTitle) { xlworksheet.Column(cell1.Address.ColumnNumber).Width *= 1.30; }
                    //�^�C�g���̓���
                    xlworksheet.Cell(IntTitleRow, 1).Value = DtpickEnd.Value.Date.Year + "�N" + DtpickEnd.Value.Date.Month + "�� (MS)  �d���E�}�C�N���g���ފǗ�����    5D8B4869P002";
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
                    dbContext.ViewMarsharings
                        .Where(v => v.DatePerDay >= dateStart && v.DatePerDay <= dateEnd)
                        .ExecuteUpdate(u => u.SetProperty(p => p.IsCompiled, true));
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
                return;
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

        /// <summary>
        /// ���o�ɗ���CSV�捞
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInOutCSVInclude_Click(object sender, EventArgs e)
        {
            IncludeInOutCSV();
            //ShInOut��TQR�ɔ��f������
            ShInOutToTQR();

        }
    }
}
