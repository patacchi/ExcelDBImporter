using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Models
{
    [Comment("各アプリの設定を格納する。1アプリ１レコード")]
    public class AppSetting
    {
        [Key]
        public int AppSettingID { get; set; }
        [Comment("アプリ名、必須")]
        [Required]
        public string StrAppName { get; set; } = null!;
        [Comment("最終読み取りディレクトリ")]
        public string? StrLastLoadFromDir { get; set; }
        [Comment("最終保存ディレクトリ")]
        public string? StrLastSaveToDir { get; set; }

    }
}
