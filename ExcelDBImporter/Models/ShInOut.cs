using ExcelDBImporter.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using ExcelDBImporter.Tool;
namespace ExcelDBImporter.Models
{
    [Table(nameof(ShInOut))]
    [Microsoft.EntityFrameworkCore.Comment("入出庫履歴を格納するテーブル")]
    [Delimiter(",")]
    [CultureInfo("ja-JP")]
    [LineBreakInQuotedFieldIsBadData(true)]
    [CsvHelper.Configuration.Attributes.ExceptionMessagesContainRawData(true)]
    public class ShInOut : IHaveDefaultPattern<ShInOut>
    {
        [Column(nameof(ShInOutID))]
        [Ignore]
        public int ShInOutID { get; set; }
        [Column(nameof(DateInOut))]
        [CsvHelper.Configuration.Attributes.Index(0)]
        [Microsoft.EntityFrameworkCore.Comment("入出庫日")]
        public DateTime? DateInOut { get; set; }
        [Column(nameof(StrOrderOrSeiban))]
        [CsvHelper.Configuration.Attributes.Index(1)]
        [Microsoft.EntityFrameworkCore.Comment("オーダーか製番、役に立たなそう")]
        public string? StrOrderOrSeiban { get; set; }
        [Column(nameof(StrKanriKa))]
        [Name("管理課記号")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        [Microsoft.EntityFrameworkCore.Comment("管理課")]
        public string? StrKanriKa { get; set; }
        [Column(nameof(StrTehaiCode))]
        [Microsoft.EntityFrameworkCore.Comment("手配コード")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string StrTehaiCode { get; set; } = null!;
        [Column(nameof(DblInputNum))]
        [Microsoft.EntityFrameworkCore.Comment("入庫個数")]
        [CsvHelper.Configuration.Attributes.Index(5)]
        public double? DblInputNum { get; set; }
        [Column(nameof(DblDeliverNum))]
        [Microsoft.EntityFrameworkCore.Comment("出庫個数")]
        [CsvHelper.Configuration.Attributes.Index(6)]
        public double? DblDeliverNum { get; set; }
        [Column(nameof(StrKishu))]
        [Microsoft.EntityFrameworkCore.Comment("手配機種")]
        [CsvHelper.Configuration.Attributes.Index(9)]
        public string? StrKishu { get; set; }
        [Column(nameof(StrStockCode))]
        [Microsoft.EntityFrameworkCore.Comment("貯蔵記号")]
        [CsvHelper.Configuration.Attributes.Index(14)]
        public string? StrStockCode { get; set; }
        [Column(nameof(DblRemainAmount))]

        [Microsoft.EntityFrameworkCore.Comment("在庫数量")]
        [CsvHelper.Configuration.Attributes.Index(18)]
        public double? DblRemainAmount { get; set; } = 0;
        [NotMapped]
        [Optional]
        public string? StrDummy { get; set; }

        public static bool IsDupe(ExcelDbContext dbContext,ShInOut shInOut,out ShInOut? outExisting)
        {
            outExisting = dbContext.ShInOuts
                                    .FirstOrDefault(s =>
                                    s.DateInOut == shInOut.DateInOut
                                    && s.StrOrderOrSeiban == shInOut.StrOrderOrSeiban
                                    && s.StrTehaiCode == shInOut.StrTehaiCode
                                    && s.DblInputNum == shInOut.DblInputNum
                                    && s.DblDeliverNum == shInOut.DblDeliverNum
                                    );
            if (outExisting == null ) 
            {
                //重複無し
                return false;
            }
            else
            {
                //重複あり
                return true;
            }
        }

        Func<ShInOut, object>[] IHaveDefaultPattern<ShInOut>.DefaultExcludedFieldsPattern()
        {
            return
            [
                e =>
                {
                    return new
                    {
                        e.StrStockCode
                    };
                }
            ];
        }

        Func<ShInOut, object>[] IHaveDefaultPattern<ShInOut>.DefaultKeyPattern()
        {
            return
            [
                key =>
                {
                    return new
                    {
                        key.ShInOutID/*,
                        key.DateInOut,
                        key.StrOrderOrSeiban,
                        key.DblInputNum,
                        key.DblDeliverNum,
                        key.StrTehaiCode*/
                   };
                }
            ];
        }
    }
}
