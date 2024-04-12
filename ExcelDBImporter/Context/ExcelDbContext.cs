using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
/// Nuget ClosedXML
using ClosedXML.Excel;
using ExcelDBImporter.Models;

/// Nuget Microsoft.EntityFrameworkCore.Sqlite
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcelDBImporter.Context
{
    public class ExcelDbContext : DbContext
    {
        public DbSet<ShShukka> ShShukka { get; set; }
        public DbSet<TableFieldAliasNameList> TableFieldAliasNameLists { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<TableDBcolumnNameAndExcelFieldName> TableDBcolumnNameAndExcelFieldNames { get; set; }
        public DbSet<TQRinput> TQRinputs { get; set; }
        public DbSet<TTempQRrowData> TTempQRrows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Application.StartupPath, "excel_data.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            optionsBuilder.LogTo(message => Debug.WriteLine(message),LogLevel.Information);
        }
    }
}
