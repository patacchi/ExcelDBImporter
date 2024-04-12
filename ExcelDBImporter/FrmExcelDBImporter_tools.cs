using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter
{
    partial class FrmExcelImpoerter
    {
        /// <summary>
        /// ExcelDBImporterのアプリ名
        /// </summary>
        public const string CONST_STR_ExcelDBImporterAppName = "ExcelDBImporter";
        /// <summary>
        /// DateTimePickerの範囲内にあるレコードのOutputFlagを落とす
        /// </summary>
        /// <returns>Intで処理件数</returns>
        private int UnsetOutputFlagByTimePickerTime()
        {
            try
            {
                ExcelDbContext dbContext = new();
                //指定範囲の日付で、更にOutputFlagがtrueになっているレコードがあるかどうか
                ShShukka? flugExists = dbContext.ShShukka
                                        .Where(f => f.DateMarshalling >= DtpickStart.Value && f.DateMarshalling <= DtpickEnd.Value
                                         && f.IsAlreadyOutput == true)
                                        .FirstOrDefault();
                if (flugExists == null) 
                {
                    MessageBox.Show("該当データ無し");
                    return 0;
                }
                int IntUpdateCount = dbContext.ShShukka
                    .Where(f => f.DateMarshalling >= DtpickStart.Value && f.DateMarshalling <= DtpickEnd.Value
                     && f.IsAlreadyOutput)
                    .ExecuteUpdate(u => u.SetProperty(f => f.IsAlreadyOutput, false));
                dbContext.Dispose();
                MessageBox.Show(IntUpdateCount.ToString() + "件のフラグを落としました");
                return IntUpdateCount;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        private void AppSettingExistsCheck()
        {
            try
            {
                ExcelDbContext dbContext = new();
                //ExcelDBImpoerter
                AppSetting? appSetting = dbContext.AppSettings
                                        .Where(a => a.StrAppName == CONST_STR_ExcelDBImporterAppName)
                                        .FirstOrDefault();
                //アプリ名見つからなかったら登録する
                if (appSetting == null)
                {
                    dbContext.AppSettings.Add(new AppSetting
                    {
                        StrAppName = CONST_STR_ExcelDBImporterAppName
                    });
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        public static void OpenFolderInExplorer(string folderPath)
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

    }
}
