﻿using CsvHelper.Configuration;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using ExcelDBImporter.Models.View;
using ExcelDBImporter.Tool;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtfUnknown;
using CsvHelper;
using System.Configuration;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

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
                ViewMarsharing? flugExists = dbContext.ViewMarsharings
                                        .Where(f => f.DatePerDay >= DtpickStart.Value && f.DatePerDay <= DtpickEnd.Value
                                         && f.IsCompiled == true)
                                        .FirstOrDefault();
                if (flugExists == null)
                {
                    MessageBox.Show("該当データ無し");
                    return 0;
                }
                int IntUpdateCount = dbContext.ViewMarsharings
                    .Where(f => f.DatePerDay >= DtpickStart.Value && f.DatePerDay <= DtpickEnd.Value
                     && f.IsCompiled)
                    .ExecuteUpdate(u => u.SetProperty(f => f.IsCompiled, false));
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
        public void IncludeInOutCSV()
        {
            //DBよりLastLoadFromDirを取得、なければString.Empty
            string StrLastLoadDir = string.Empty;
            using ExcelDbContext dbcontext = new();
            AppSetting? appSetting = dbcontext.AppSettings.FirstOrDefault(a => a.StrAppName == FrmExcelImpoerter.CONST_STR_ExcelDBImporterAppName);
            if (appSetting != null)
            {
                //LoadFromDirの設定が存在する場合のみ設定
                StrLastLoadDir = string.IsNullOrEmpty(appSetting.StrLastLoadFromDir) ? string.Empty : appSetting.StrLastLoadFromDir;
            }
            //ファイル選択ダイアログ表示
            OpenFileDialog DialogImportExcelFile = new()
            {
                InitialDirectory = StrLastLoadDir,
                Filter = "CSV files (*.csv)|*.csv"
            };
            if (DialogImportExcelFile.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show($"{nameof(IncludeInOutCSV)} ファイル選択がキャンセルされました");
                return;
            }

            string StrInOutFilePath = DialogImportExcelFile.FileName;
            //DBのLastLoadFromDirを更新
            if (appSetting != null)
            {
                appSetting.StrLastLoadFromDir =  System.IO.Path.GetDirectoryName(DialogImportExcelFile.FileName);
            }
            dbcontext.SaveChanges();
            //CSVファイルを読み込み、ヘッダーのListを得る
            var header = GetCsvHeaders(StrInOutFilePath);
            //CSVファイルを読み込み、モデルクラスとマッピングする
            List<ShInOut> listModeles = ReadCsvFile<ShInOut>(StrInOutFilePath, header);
            //DBにUPSertする
            using ExcelDbContext context = new();
            context.BulkMerge(listModeles,
                options => options.ColumnPrimaryKeyExpression = ShInOut => new
                {
                    ShInOut.DateInOut,
                    ShInOut.StrOrderOrSeiban,
                    ShInOut.DblInputNum,
                    ShInOut.DblDeliverNum,
                    ShInOut.StrTehaiCode
                }
            );
            context.BulkSaveChanges();
        }
        private List<string> GetCsvHeaders(string StrfilePath)
        {
            try
            {
                using FileStream straem = File.OpenRead(StrfilePath);
                DetectionResult CharSet = CharsetDetector.DetectFromStream(straem);
                using (StreamReader reader = new StreamReader(StrfilePath,CharSet.Detected.Encoding))
                {
                    
                    List<string> headers = reader.ReadLine()!.Split(',').ToList();
                    return headers;
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"{nameof(GetCsvHeaders)} で {ex.Message} エラーです");
                return new List<string>();
            }
        }
        private List<T> ReadCsvFile<T>(string StrfilePath, List<string> Listheaders) where T : class
        {
            List<T> models = new();
            using FileStream stream = File.OpenRead(StrfilePath);
            DetectionResult CharCode = CharsetDetector.DetectFromStream(stream);
            using (var reader = new StreamReader(StrfilePath,CharCode.Detected.Encoding))
            {
                stream.Position = 0;
                string StrCsvAll = reader.ReadToEnd();
                //ダブルクォーテーション取っちゃえ・・・
                string StrNoEq = StrCsvAll.Replace("=","");
                string StrNoDQ = StrCsvAll.Replace("\"", "");
                stream.Dispose();
                Encoding encoding = Encoding.UTF8;
                //Stringの内容を MemoryStareamに読み込み
                try
                {
                    using MemoryStream ms = new MemoryStream(encoding.GetBytes(StrNoDQ));
                    var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                    {
                        BadDataFound = x =>
                        {
                            Debug.WriteLine(x);
                        },
                        NewLine = Environment.NewLine,
                        ExceptionMessagesContainRawData = false,
                        ReadingExceptionOccurred = x =>
                        {
                            Debug.WriteLine(x);
                            return true;
                        }

                    };
                    CsvConfiguration configulation = CsvConfiguration.FromAttributes<T>();
                    using StreamReader readerMS = new StreamReader(ms);
                    ms.Position = 0;
                    using var csv = new CsvReader(readerMS, configulation);
                    //csv.Read();
                    //csv.ReadHeader();
                    List<T> list = csv.GetRecords<T>().ToList();
                    /*
                    List<T> list = new List<T>();
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<T>();
                        list.Add(record);
                    }
                    */
                    return list;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}");
                }
                return new List<T>() ;
                
                // ヘッダー行を読み飛ばす

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) {  continue; }
                    var values = line.Split(',');

                    // ヘッダーを使用して各フィールドの値を取得し、プロパティにマッピングする
                    var model = Activator.CreateInstance<T>();
                    var properties = typeof(T).GetProperties();
                    //DBとファイルFieldのDictionaryを得る
                    GetAllProperty getprop = new();
                    Dictionary<string, string> DicDBandField = getprop.GetDBandFileFieldNamePair(typeof(ShInOut).Namespace!,
                                                                                                 nameof(ShInOut),
                                                                                                 nameof(TableDBcolumnNameAndExcelFieldName.StrShInOutFieldName));

                    for (int i = 0; i < Listheaders.Count; i++)
                    {
                        var property = properties.FirstOrDefault(p =>DicDBandField[p.Name].Equals(Listheaders[i], StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            property.SetValue(model, Convert.ChangeType(values[i], property.PropertyType));
                        }
                    }
                    models.Add(model);
                }
            }

            return models;
        }
    }
}
