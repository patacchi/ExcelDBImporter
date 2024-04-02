using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Models
{
    [Comment("各アプリの設定を格納する。1アプリ１レコード")]
    [Table(name:nameof(AppSetting))]
    public class AppSetting
    {
        [Column(name: nameof(AppSettingID), Order = 0)]
        [Key]
        public int AppSettingID { get; set; }
        [Column(name: nameof(StrAppName), Order = 1)]
        [Comment("アプリ名、必須")]
        [Required]
        public string StrAppName { get; set; } = null!;
        [Column(name: nameof(StrLastLoadFromDir), Order = 2)]
        [Comment("最終読み取りディレクトリ")]
        public string? StrLastLoadFromDir { get; set; }
        [Column(name: nameof(StrLastSaveToDir), Order = 3)]
        [Comment("最終保存ディレクトリ")]
        public string? StrLastSaveToDir { get; set; }

    }
}
