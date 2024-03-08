using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
