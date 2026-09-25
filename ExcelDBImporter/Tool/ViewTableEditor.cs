using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDBImporter.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDBImporter.Models;
using ExcelDBImporter.Models.View;

namespace ExcelDBImporter.Tool
{
    class ViewTableEditor
    {
        /// <summary>
        /// 指定の日付の間で、ViewMarsharingテーブルに該当日付が無い場合追加する
        /// </summary>
        /// <param name="dateStart">開始日</param>
        /// <param name="dateEnd">終了日</param>
        public void FillEmptyDay(DateTime dateStart, DateTime dateEnd)
        {
            //終了日の方が開始日より前だった場合は、開始日を終了日をして扱う(どっちも同じにする)
            DateTime DateEnd = dateStart >= dateEnd ? dateStart : dateEnd;
            try
            {
                //開始日と終了日の間のList<DateTime>を得る
                List<DateTime> datesInRange = Enumerable.Range(0, 1 + DateEnd.Subtract(dateStart).Days)
                                            .Select(offset => dateStart.AddDays(offset))
                                            .ToList();
                using ExcelDbContext dbContext = new();
                //開始日と終了日の間で、DBに既にデータが存在するDatetimeのListを得る
                List<DateTime?> existingInDB = dbContext.ViewMarsharings
                                            .Where(view => view.DatePerDay >= dateStart && view.DatePerDay <= DateEnd)
                                            .Select(view => view.DatePerDay)
                                            .ToList();
                //開始日と終了日のListから、既存のデータを除外した、バルク挿入用のエンティティのリストを得る
                List<ViewMarsharing> entitiesToInsert = datesInRange.Where(date => !existingInDB.Contains(date))
                                                                    .Select(date => new ViewMarsharing 
                                                                    {
                                                                        DatePerDay = date ,
                                                                        IntCableCut = 0,
                                                                        IntDelivery = 0,
                                                                        IntFreewayData = 0,
                                                                        IntMicroWave = 0,
                                                                        IntMoving = 0,
                                                                        IntOther = 0,
                                                                        IntPrepareReceive = 0,
                                                                        IntShipping = 0,
                                                                        IsCompiled = false,
                                                                    })
                                                                    .ToList();
                //バルク挿入
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    dbContext.ViewMarsharings.AddRange(entitiesToInsert);
                    dbContext.SaveChanges();
                    transaction.Commit();
                }
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{nameof(FillEmptyDay)} :  {ex.Message} エラー");
                throw;
            }
        }
    }
}
