using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Models
{
    [Comment("テーブル列名の別名(表示名等)格納テーブル")]
    [PrimaryKey(nameof(TableFieldAliasNameListId))]
    [Table(name:nameof(TableFieldAliasNameList))]
    
    public class TableFieldAliasNameList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name:nameof(TableFieldAliasNameListId), Order =0)]
        public int TableFieldAliasNameListId { get; set; }
        [Column(name:nameof(TableDBcolumnNameAndExcelFieldNameID), Order =1)]
        [Comment("DB columnnameテーブルの外部キー")]
        public int TableDBcolumnNameAndExcelFieldNameID { get; set; }
        [Column(name:nameof(StrColnmnAliasName), Order = 2)]
        [Comment("列の表示用別名")]
        public string? StrColnmnAliasName { get; set; }

        [Column(name: nameof(StrClassName), Order = 3)]
        [NotMapped]
        public string StrClassName { get; set; } = null!;
        
        [Column(name:nameof(StrDBColumnName), Order = 4)]
        [NotMapped]
        public string StrDBColumnName { get; set; } = null!;
        

        [Comment("ナビゲーションプロパティ")]
        public TableDBcolumnNameAndExcelFieldName DBcolumn { get; set; } = null!;

    }
}
