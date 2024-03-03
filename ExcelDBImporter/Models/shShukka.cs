using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Modeles
{
    [Comment("発番出荷物件予定表モデルクラス")]
    public class ShShukka
    {
        public int ShShukkaID {  get; set; }
        [Comment("出荷計画")]
        public DateTime DateShukka { get; set; }
        [Comment("製番")]
        public string StrSeiban { get; set; } = null!;
        [Comment("注文主")]
        public string StrOrderFrom { get; set; } = null!;
        [Comment("品名")]
        public string StrHinmei { get; set; } = null!;
        [Comment("発番数量")]
        public int IntAmount { get; set; }
        [Comment("出図日")]
        public DateTime? DateShutuzu { get; set; }
        [Comment("マーシャリング日")]
        public DateTime? DateMarshalling { get; set; }
        [Comment("組立日")]
        public DateTime? DateAssemble { get; set; }
        [Comment("試験日")]
        public DateTime? DateFunctionTest { get; set; }
        [Comment("出荷準備")]
        public DateTime? DatePrepare { get; set; }
        [Comment("出荷検査日")]
        public DateTime? DateShippingTest { get; set; }
    }
}