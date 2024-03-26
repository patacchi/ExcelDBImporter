using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Tool
{
    public class DatabaseInitializer
    {
        public static void DatabaseExlistCheker() 
        {
            try
            {
                using ExcelDbContext dbContext = new();
                //適用するマイグレーションあれば実行
                dbContext.Database.Migrate();
                //TableDBcolumnAndExcelFieldNameテーブルにshShukkaモデルクラスのプロパティ名が登録されているかチェック
                CheckDBcolumnNameExists(dbContext);
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
        private static void CheckDBcolumnNameExists(ExcelDbContext dbContext)
        {
            GetAllProperty getAllProperty = new();
            //shShukkaモデルクラスのプロパティ一覧を取得
            List<ClassAndProperty> listProperty = getAllProperty.GetPropertyByClassNameAndNamespace(typeof(ShShukka).Namespace, nameof(ShShukka));
            if (listProperty.Count == 0)
            {
                MessageBox.Show("ShShukkaモデルクラスが見つかりませんでした。");
                return;
            }
            UpsertProperty(dbContext, listProperty);
            //DBに変更適用
            dbContext.SaveChanges();
        }

        /// <summary>
        /// ClassAndProperty型の引数を取り、クラス名とプロパティ名(DBカラム名)の組み合わせが無ければ追加する
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="listProperty"></param>
        private static void UpsertProperty(ExcelDbContext dbContext, List<ClassAndProperty> listProperty)
        {
            foreach (ClassAndProperty property in listProperty)
            {
                //クラス内各プロパティに対してDBに存在するかチェック(無ければ追記)
                TableDBcolumnNameAndExcelFieldName? existit = dbContext.tableDBcolumnNameAndExcelFieldNames.FirstOrDefault(
                                                        d => d.StrClassName == property.ClassName && d.StrDBColumnName == property.PropertyName);
                if (existit == null)
                {
                    //クラス名とプロパティ名(DB カラム名)が存在しないときは追加
                    dbContext.tableDBcolumnNameAndExcelFieldNames.Add(new TableDBcolumnNameAndExcelFieldName
                    {
                        StrClassName = property.ClassName,
                        StrDBColumnName = property.PropertyName,
                    });
                }
            }
        }
    }
}
