using ExcelDBImporter.Context;
using ExcelDBImporter.Modeles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter
{
    partial class FrmExcelImpoerter
    {
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
    }
}
