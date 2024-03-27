using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Models
{
    [Comment("テーブル列名の別名(表示名等)格納テーブル")]
    [PrimaryKey(nameof(TableFieldAliasNameListId))]
    
    public class TableFieldAliasNameList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order =0)]
        public int TableFieldAliasNameListId { get; set; }
        [Column(Order =1)]
        [Comment("DB columnnameテーブルの外部キー")]
        public int TableDBcolumnNameAndExcelFieldNameID { get; set; }
        [Column(Order = 2)]
        [Comment("列の表示用別名")]
        public string? StrColnmnAliasName { get; set; }

        /*
        [Column(Order = 3)]
        public string StrClassName { get; set; } = null!;
        [Column(Order = 4)]
        public string StrColumnName { get; set; } = null!;
        */

        [Comment("ナビゲーションプロパティ")]
        public required TableDBcolumnNameAndExcelFieldName DBcolumn { get; set; }

    }
}
