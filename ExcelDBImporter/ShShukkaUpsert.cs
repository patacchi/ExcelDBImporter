using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using ExcelDBImporter.Context;
using ExcelDBImporter.Modeles;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter
{
    internal class ShShukkaUpsert(IXLRange RangeToUpsert)
    {
        public void DoUpsert()
        {
            if (RangeToUpsert == null)
            {
                MessageBox.Show("更新元のRangeがnullでした");
                return;
            }
            //タイトル行の行数取得
            IXLRangeRow RangeTitleRow = RangeToUpsert.Row(RangeToUpsert.Search("注文主").First().Address.RowNumber);
            //計画と完了で4行目が同じカラムがあるので、5行目と文字連結する
            foreach (var cell in RangeTitleRow.Cells())
            {
                cell.Value = cell.Value.ToString() + cell.CellBelow(1).Value.ToString();
            }
            ShShukka ShShukkaClass = new();
            //列番号取得(移動に対応するため)
            //全体でSerchすると先頭の「発番出荷物件予定表」が悪さをする・・・
            Dictionary<string, int> keyValuePairs = new()
            {
                //出荷計画
                {
                    nameof(ShShukkaClass.DateShukka),
                    RangeToUpsert.Search("出荷計画").First().Address.ColumnNumber
                },
                //製番
                {
                    nameof(ShShukkaClass.StrSeiban),
                    RangeToUpsert.Search("製番").First().Address.ColumnNumber
                },
                //注文主
                {
                    nameof(ShShukkaClass.StrOrderFrom),
                    RangeToUpsert.Search("注文主").First().Address.ColumnNumber
                },
                //品名
                {
                    nameof(ShShukkaClass.StrHinmei),
                    RangeToUpsert.Search("品名").First().Address.ColumnNumber
                },
                //発番数量
                {
                    nameof(ShShukkaClass.IntAmount),
                    RangeToUpsert.Search("発番数量").First().Address.ColumnNumber
                },
                //出図
                {
                    nameof(ShShukkaClass.DateShutuzu),
                    RangeToUpsert.Search("出図完了").First().Address.ColumnNumber
                },
                //マーシャリング
                {
                    nameof(ShShukkaClass.DateMarshalling),
                    RangeToUpsert.Search("ﾏｰｼｬﾘﾝｸﾞ完了").First().Address.ColumnNumber
                },
                //組立
                {
                    nameof(ShShukkaClass.DateAssemble),
                    RangeToUpsert.Search("組立完了").First().Address.ColumnNumber
                },
                //試験
                {
                    nameof(ShShukkaClass.DateFunctionTest),
                    RangeToUpsert.Search("試験完了").First().Address.ColumnNumber
                },
                //出荷準備
                {
                    nameof(ShShukkaClass.DatePrepare),
                    RangeToUpsert.Search("出荷準備完了").First().Address.ColumnNumber
                },
                //出荷検査
                {
                    nameof(ShShukkaClass.DateShippingTest),
                    RangeToUpsert.Search("出荷検査完了").First().Address.ColumnNumber
                }
            };

            using ExcelDbContext dbContextorigin = new();
            //テーブルが無かったら作成を試みる
            //EnsureCreate使うとMigrationの時にタイヘン・・・
            //dbContextorigin.Database.EnsureCreated();
            foreach (IXLRangeRow? row in RangeToUpsert.RowsUsed().Skip(4)) //先頭行スキップ
            {
                if (row != null)
                {
                    try
                    {
                        JudgeUpdateInsert(keyValuePairs, row, dbContextorigin);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
            dbContextorigin.SaveChanges();
            dbContextorigin.Dispose();
        }
        private static void JudgeUpdateInsert(Dictionary<string, int> keyValuePairs, IXLRangeRow row, ExcelDbContext dbContext)
        {
            //シートから各データを取得する
            //データが日付の物は日付データ以外はDatetime.Minvalueをセット
            //データがテキストの物は、テキスト以外はString.Emptyをセット
            //データが数字の物は、数字以外は0をセット
            ShShukka ShShukkaClass = new();
            //出荷計画
            DateTime DateShukka = row.Cell(keyValuePairs[nameof(ShShukkaClass.DateShukka)]).Value.IsDateTime ?
                                row.Cell(keyValuePairs[nameof(ShShukkaClass.DateShukka)]).Value.GetDateTime()
                                : DateTime.MinValue;
            //製番
            string StrSeiban = row.Cell(keyValuePairs[nameof(ShShukkaClass.StrSeiban)]).Value.IsText ?
                                row.Cell(keyValuePairs[nameof(ShShukkaClass.StrSeiban)]).Value.GetText()
                                : string.Empty;
            //注文主
            string StrOrderFrom = row.Cell(keyValuePairs[nameof(ShShukkaClass.StrOrderFrom)]).Value.IsText ?
                                row.Cell(keyValuePairs[nameof(ShShukkaClass.StrOrderFrom)]).Value.GetText() 
                                : string.Empty;
            //品名
            string StrHinmei = row.Cell(keyValuePairs[nameof(ShShukkaClass.StrHinmei)]).Value.IsText ?
                                row.Cell(keyValuePairs[nameof(ShShukkaClass.StrHinmei)]).Value.GetText()
                                : string.Empty;
            //発番数量
            int IntAmount = row.Cell(keyValuePairs[nameof(ShShukkaClass.IntAmount)]).Value.IsNumber ?
                                (int)row.Cell(keyValuePairs[nameof(ShShukkaClass.IntAmount)]).Value.GetNumber() 
                                : 0;
            //出図
            DateTime DateShutuzu = row.Cell(keyValuePairs[nameof(ShShukkaClass.DateShutuzu)]).Value.IsDateTime ?
                                    row.Cell(keyValuePairs[nameof(ShShukkaClass.DateShutuzu)]).Value.GetDateTime() 
                                    : DateTime.MinValue;
            //マーシャリング
            DateTime DateMarshalling = row.Cell(keyValuePairs[nameof(ShShukkaClass.DateMarshalling)]).Value.IsDateTime ?
                                    row.Cell(keyValuePairs[nameof(ShShukkaClass.DateMarshalling)]).Value.GetDateTime()
                                    : DateTime.MinValue;
            //組立
            DateTime DateAssemble = row.Cell(keyValuePairs[nameof(ShShukkaClass.DateAssemble)]).Value.IsDateTime ?
                                    row.Cell(keyValuePairs[nameof(ShShukkaClass.DateAssemble)]).Value.GetDateTime()
                                    :DateTime.MinValue;
            //試験
            DateTime DateFunctionTest = row.Cell(keyValuePairs[nameof(ShShukkaClass.DateFunctionTest)]).Value.IsDateTime ?
                                    row.Cell(keyValuePairs[nameof(ShShukkaClass.DateFunctionTest)]).Value.GetDateTime()
                                    : DateTime.MinValue;
            //出荷準備
            DateTime DatePrepare = row.Cell(keyValuePairs[nameof(ShShukkaClass.DatePrepare)]).Value.IsDateTime ?
                                    row.Cell(keyValuePairs[nameof(ShShukkaClass.DatePrepare)]).Value.GetDateTime() 
                                    : DateTime.MinValue;
            //出荷検査
            DateTime DateShippingTest = row.Cell(keyValuePairs[nameof(ShShukkaClass.DateShippingTest)]).Value.IsDateTime ?
                                    row.Cell(keyValuePairs[nameof(ShShukkaClass.DateShippingTest)]).Value.GetDateTime()
                                    : DateTime.MinValue;
            try
            {
                //既存データか判別する
                ShShukka? existingdata = dbContext.ExcelData.FirstOrDefault(e => e.StrSeiban == StrSeiban);
                if (existingdata == null)
                {
                    //既存データが無い→新規の場合
                    dbContext.ExcelData.Add(new ShShukka
                    {
                        //出荷計画
                        DateShukka = DateShukka,
                        //製番
                        StrSeiban = StrSeiban,
                        //注文主
                        StrOrderFrom = StrOrderFrom,
                        //品名
                        StrHinmei = StrHinmei,
                        //発番数量
                        IntAmount = IntAmount,
                        //出図
                        DateShutuzu = DateShutuzu == DateTime.MinValue ? null : DateShutuzu,
                        //マーシャリング
                        DateMarshalling = DateMarshalling == DateTime.MinValue ? null : DateMarshalling,
                        //組立
                        DateAssemble = DateAssemble == DateTime.MinValue ? null : DateAssemble,
                        //試験
                        DateFunctionTest = DateFunctionTest == DateTime.MinValue ? null : DateFunctionTest,
                        //出荷準備
                        DatePrepare = DatePrepare == DateTime.MinValue ? null : DatePrepare,
                        //出荷検査
                        DateShippingTest = DateShippingTest == DateTime.MinValue ? null : DateShippingTest
                    });
                }
                else
                {
                    //更新の場合
                    //出荷計画
                    existingdata.DateShukka = DateShukka;
                    //製番はキーなので更新無し
                    //注文主
                    existingdata.StrOrderFrom = StrOrderFrom;
                    //品名
                    existingdata.StrHinmei = StrHinmei;
                    //発番数量
                    existingdata.IntAmount = IntAmount;
                    //出図
                    existingdata.DateShutuzu = DateShutuzu == DateTime.MinValue ? null : DateShutuzu;
                    //マーシャリング
                    existingdata.DateMarshalling = DateMarshalling == DateTime.MinValue ? null : DateMarshalling;
                    //組立
                    existingdata.DateAssemble = DateAssemble == DateTime.MinValue ? null : DateAssemble;
                    //試験
                    existingdata.DateFunctionTest = DateFunctionTest == DateTime.MinValue ? null : DateFunctionTest;
                    //出荷準備
                    existingdata.DatePrepare = DatePrepare == DateTime.MinValue ? null : DatePrepare;
                    //出荷検査
                    existingdata.DateShippingTest = DateShippingTest == DateTime.MinValue ? null : DateShippingTest;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (MessageBox.Show(ex.Message,"エラー発生",MessageBoxButtons.CancelTryContinue) == DialogResult.Cancel)
                {
                    dbContext.Dispose();
                    throw;
                };
                //return;
                //throw;
            }
            //1行処理終了
            //出荷計画
            //製番
            //注文主
            //品名
            //発番数量
            //出図
            //マーシャリング
            //組立
            //試験
            //出荷準備
            //出荷検査
        }
    }
}
