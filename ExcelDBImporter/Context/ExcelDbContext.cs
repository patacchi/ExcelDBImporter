using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
/// Nuget ClosedXML
using ClosedXML.Excel;
using ExcelDBImporter.Modeles;

/// Nuget Microsoft.EntityFrameworkCore.Sqlite
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Context
{
    public class ExcelDbContext : DbContext
    {
        public DbSet<ShShukka> ExcelData { get; set; }
        public DbSet<TableFieldAliasNameList> TableFieldAliasNameLists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Application.StartupPath, "excel_data.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
