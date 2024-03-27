using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;

namespace ExcelDBImporter.Tool
{
    public class RegistAllClassAndPropertys
    {
        /// <summary>
        /// 特定の名前空間以下の全てのクラスのプロパティをTableDBcolumnAndExcelFieldテーブル登録する
        /// </summary>
        /// <param name="StrTargetNameSpace"></param>
        public RegistAllClassAndPropertys(string StrTargetNameSpace)
        {
            if (string.IsNullOrEmpty(StrTargetNameSpace)) { return; }
            GetAllProperty getAllProperty = new();
            List<ClassAndProperty> listPropertys = getAllProperty.GetClassnameAndPropertyPairBasedNameSpace(StrTargetNameSpace);
            RegistToDB(listPropertys);
        }
        /// <summary>
        /// 指定のクラスのプロパティのみ登録する
        /// </summary>
        /// <param name="StrNameSpace"></param>
        /// <param name="StrClassName"></param>
        public RegistAllClassAndPropertys(string StrNameSpace,string StrClassName)
        {
            if (string.IsNullOrEmpty(StrNameSpace) || string.IsNullOrEmpty(StrClassName)) { return; }
            GetAllProperty getAllProperty = new();
            List<ClassAndProperty> listPropertys = getAllProperty.GetPropertyByClassNameAndNamespace(StrNameSpace,StrClassName);
            RegistToDB(listPropertys);
        }
        internal void RegistToDB(List<ClassAndProperty> listProperty)
        {
            if (listProperty == null) { return; }
            using ExcelDbContext dbContext = new();
            try
            {
                foreach (ClassAndProperty property in listProperty) 
                {
                    TableDBcolumnNameAndExcelFieldName? existing = dbContext.tableDBcolumnNameAndExcelFieldNames.FirstOrDefault(
                        e => e.StrClassName == property.ClassName && e.StrDBColumnName == property.PropertyName);
                    if (existing == null)
                    {
                        dbContext.tableDBcolumnNameAndExcelFieldNames.Add(new TableDBcolumnNameAndExcelFieldName
                        {
                            StrClassName = property.ClassName,
                            StrDBColumnName = property.PropertyName
                        });
                    }
                }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                dbContext.Dispose();
            }
        }
    }
}
