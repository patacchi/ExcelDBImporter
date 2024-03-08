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
        public RegistAllClassAndPropertys(string StrTargetNameSpace)
        {
            if (string.IsNullOrEmpty(StrTargetNameSpace)) { return; }
            GetAllProperty getAllProperty = new();
            List<ClassAndProperty> listPropertys = getAllProperty.GetClassnameAndPropertyPairBasedNameSpace(StrTargetNameSpace);
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
                    TableFieldAliasNameList? existing = dbContext.TableFieldAliasNameLists.FirstOrDefault(
                        e => e.StrClassName == property.ClassName && e.StrColumnName == property.PropertyName);
                    if (existing == null)
                    {
                        dbContext.TableFieldAliasNameLists.Add(new TableFieldAliasNameList
                        {
                            StrClassName = property.ClassName,
                            StrColumnName = property.PropertyName
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
