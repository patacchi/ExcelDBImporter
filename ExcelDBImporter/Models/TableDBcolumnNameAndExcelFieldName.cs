using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Models
{
    [Comment("DBとExcelファイルのフィールド名の対応格納テーブル。対応Excelファイルが増えると列が増えていく")]

    public class TableDBcolumnNameAndExcelFieldName
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public int TableDBcolumnNameAndExcelFieldNameID { get; set; }
        [Column(Order = 1)]
        [Comment("格納されているモデルクラス名")]
        public string StrClassName { get; set; } = null!;
        [Column(Order = 2)]
        [Comment("モデルクラスのオリジナルDBColumn名")]
        public string StrDBColumnName { get; set; } = null!;
        [Column(Order = 3)]
        [Comment("出荷物件予定表Excelファイルのフィールド名、4行目と5行目の文字列を連結する")]
        public string? StrshShukkaFieldName { get; set; }
        [Column(Order = 4)]
        [Comment("工程管理システムからの出力Excelファイルのフィールド名")]
        public string? StrshProcessManagementFieldName { get; set; }

        public TableFieldAliasNameList? Alias { get; set; }
    }
}
