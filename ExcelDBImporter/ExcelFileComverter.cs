using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DocumentFormat.OpenXml.Office2016.Presentation.Command;
using Microsoft.Office.Interop.Excel;
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
            //ファイル選択ダイアログ表示
            OpenFileDialog DialogImportExcelFile = new()
            {
                Filter = "Excel files (*.xlsx)|*.xlsx;*.xls|" + "Excel files(old)(*.xls)|*.xls"
            };
            if (DialogImportExcelFile.ShowDialog() == DialogResult.OK)
            {
                StrOriginFilePath = DialogImportExcelFile.FileName;
                if (Path.GetExtension(StrOriginFilePath).Equals(".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    //xlsファイルだった場合(変換が必要)
                    if (!File.Exists(StrOriginFilePath)) { MessageBox.Show("ファイルが見つかりません");}
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
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook?  workbook = null; 
            try
            {
                string StrXlsfileDir = Path.GetDirectoryName(StrOldExcelFilePath) ?? Path.GetTempPath();
                string StrOutputFileName = Path.Combine(StrXlsfileDir,Path.GetRandomFileName() + ".xlsx");
                workbook = excelApp.Workbooks.Open(StrOldExcelFilePath,0,true);
                excelApp.DisplayAlerts = false;
                workbook.SaveAs(StrOutputFileName, XlFileFormat.xlOpenXMLWorkbook);
                excelApp.DisplayAlerts = true;
                StrConvertedFilePath = StrOutputFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    excelApp.Quit();
                    //COMオブジェクトの解放
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }

                //excelAppの解放
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }
        }
    }
}
