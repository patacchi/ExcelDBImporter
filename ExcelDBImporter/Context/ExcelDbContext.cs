using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
/// Nuget ClosedXML
using ClosedXML.Excel;
using ExcelDBImporter.Models;
using ExcelDBImporter.Models.View;

/// Nuget Microsoft.EntityFrameworkCore.Sqlite
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcelDBImporter.Context
{
    public class ExcelDbContext : DbContext
    {
        /// <summary>
        /// 既定コンストラクタ(本番: OnConfiguring で Application.StartupPath\excel_data.db を使用)
        /// </summary>
        public ExcelDbContext()
        {
        }

        /// <summary>
        /// 外部から Options を注入されるコンストラクタ(テスト/DI 用)。
        /// Phase 3a の IDbContextFactory 化でもこの形を維持する。
        /// </summary>
        public ExcelDbContext(DbContextOptions<ExcelDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShShukka> ShShukka { get; set; }
        public DbSet<TableFieldAliasNameList> TableFieldAliasNameLists { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<TableDBcolumnNameAndExcelFieldName> TableDBcolumnNameAndExcelFieldNames { get; set; }
        public DbSet<TQRinput> TQRinputs { get; set; }
        public DbSet<TTempQRrowData> TTempQRrows { get; set; }
        public DbSet<ViewMarsharing> ViewMarsharings { get; set; }
        public DbSet<ShInOut> ShInOuts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //外部(テスト/DI)から Options が注入済みの場合は何もしない
            if (optionsBuilder.IsConfigured) { return; }
            string dbPath = Path.Combine(Application.StartupPath, "excel_data.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.LogTo(message => Debug.WriteLine(message),LogLevel.Information);
        }
    }
}
