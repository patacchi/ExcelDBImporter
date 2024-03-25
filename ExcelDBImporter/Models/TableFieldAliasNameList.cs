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
        [Column(Order = 1)]
        public string StrClassName { get; set; } = null!;
        [Column(Order = 2)]
        public string StrColumnName { get; set; } = null!;
        public string? StrColnmnAliasName {  get; set; }
    }
}
