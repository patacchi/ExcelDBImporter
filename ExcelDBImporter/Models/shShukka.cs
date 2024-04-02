using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelDBImporter.Models
{
    [Table(name: nameof(ShShukka))]
    [Comment("発番出荷物件予定表モデルクラス")]
    public class ShShukka
    {
        [Column(Order =0)]
        public int ShShukkaID {  get; set; }
        [Comment("出荷計画")]
        [Column(Order =1)]
        public DateTime DateShukka { get; set; }
        [Comment("機種")]
        [Column(Order =2)]
        public string? StrKishu { get; set; }
        [Comment("製番")]
        [Column(Order =3)]
        public string StrSeiban { get; set; } = null!;
        [Comment("注文主")]
        [Column(Order =4)]
        public string StrOrderFrom { get; set; } = null!;
        [Comment("品名")]
        [Column(Order =5)]
        public string StrHinmei { get; set; } = null!;
        [Comment("発番数量")]
        [Column(Order =6)]
        public int IntAmount { get; set; }
        [Comment("出図日")]
        [Column (Order =7)]
        public DateTime? DateShutuzu { get; set; }
        [Comment("マーシャリング日")]
        [Column(Order =8)]
        public DateTime? DateMarshalling { get; set; }
        [Comment("組立日")]
        [Column(Order =9)]
        public DateTime? DateAssemble { get; set; }
        [Comment("試験日")]
        [Column(Order =10)]
        public DateTime? DateFunctionTest { get; set; }
        [Comment("出荷準備")]
        [Column(Order =11)]
        public DateTime? DatePrepare { get; set; }
        [Comment("出荷検査日")]
        [Column(Order =12)]
        public DateTime? DateShippingTest { get; set; }
        [Comment("データ出力済みフラグ")]
        [Column(Order =13)]
        public bool IsAlreadyOutput { get; set; } = false;
    }
}