using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ExcelDBImporter.Tool;
using ExcelDBImporter.Context;
using System.Linq.Expressions;
namespace ExcelDBImporter.Models
{
    /// <summary>
    /// QRコードにおいて、作業分類を示す定数定義
    /// </summary>
    [Flags]
    public enum QrOPcode : UInt32
    {
        /// <summary>
        /// 未初期化判定用フラグ
        /// </summary>
        None = 0U,
        /// <summary>
        /// 棚入れ前準備作業
        /// </summary>
        [Comment("棚入れ前準備作業")]
        PrepareReceive = 1U << 0,
        /// <summary>
        /// 実際の棚入れを実行
        /// </summary>
        [Comment("実際の棚入れを実行")]
        Receive = 1U <<1,
        /// <summary>
        /// Freewayでのデータ処理作業
        /// </summary>
        [Comment("Freewayでのデータ処理作業")]
        FreewayDataInput = 1U << 2,
        /// <summary>
        /// 出庫作業
        /// </summary>
        [Comment("出庫作業")]
        Delivery = 1U << 3,
        /// <summary>
        /// 支給準備作業
        /// </summary>
        [Comment("支給準備作業")]
        PrepareShpping = 1U << 4,
        /// <summary>
        /// 倉庫内配置変更作業
        /// </summary>
        [Comment("倉庫内配置変更作業")]
        Moving = 1U << 5,
        /// <summary>
        /// ラベル等表示物準備作業
        /// </summary>
        [Comment("ラベル等表示物準備作業")]
        LabelPrepare = 1U << 6,
        /// <summary>
        /// マイクロ波関連作業。行程分離のために存在、実際は複合フラグのMicroWaveDelivaryを使う
        /// </summary>
        opMicroWave = 1U << 7,
        /// <summary>
        /// ケーブル切断作業
        /// </summary>
        [Comment("ケーブル切断作業")]
        CutCable = 1U << 8,
        /// <summary>
        /// その他作業、Enum終端
        /// </summary>
        [Comment("その他")]
        Other = 1U << 31,
        //----------------------------------ここから複合フラグ----------------------
        /// <summary>
        /// 棚入れ準備と棚入れセット
        /// </summary>
        [Comment("棚入れ準備と棚入れセット")]
        PrepareReveiveSet = PrepareReceive | Receive,
        /// <summary>
        /// マイクロ波部品出庫作業
        /// </summary>
        [Comment("マイクロ波部品出庫作業")]
        MicrowaveDelivary = FreewayDataInput| opMicroWave | Delivery,
        /// <summary>
        /// 支給品出庫・支給準備作業セット
        /// </summary>
        [Comment("支給品出庫・支給準備作業セット")]
        ShppingDeliverSet =FreewayDataInput | PrepareShpping | Delivery,
        /// <summary>
        /// 新規部品登録作業(Freeway込み)
        /// </summary>
        [Comment("新規部品登録作業(Freeway込み)")]
        NewItemRegister = PrepareReceive | FreewayDataInput | LabelPrepare,
        /// <summary>
        /// ロケーション変更作業(移動・Freeway込み)
        /// </summary>
        [Comment("ロケーション変更作業(移動・Freeway込み)")]
        ChangeLocation = FreewayDataInput | Moving | LabelPrepare,
        /// <summary>
        /// 戻入、FreewayDataと棚入れセット
        /// </summary>
        [Comment("戻入")]
        Reinyu = FreewayDataInput | Receive
    }
    public enum ScanCode : UInt32
    {

    }
    [Comment("工程管理用 QRコード記録テーブル")]
    [Table(nameof(TQRinput))]
    public class TQRinput() : IHaveDefaultPattern<TQRinput>
    {
        private QrOPcode qROPcode;

        [Key]
        [Column(nameof(TQRinputId))]
        [Comment("QRテーブルのキー")]
        public int TQRinputId { get; set; }
        [Column(nameof(DateInputDate))]
        [Comment("入力日時、作業開始日時として使用")]
        public DateTime DateInputDate { get; set; }
        [Column(nameof(DateToDate))]
        [Comment("終了日時、基本的に自動計算で入力する")]
        public DateTime? DateToDate { get; set; }
        [Column(nameof(QROPcode))]
        [Comment("行程種別のenum。初期値 None")]
        public QrOPcode QROPcode { get => qROPcode; set => qROPcode = value; }
        [Column(nameof(StrOrderNum))]
        [Comment("オーダーNo 主に入庫の時に使う")]
        public string? StrOrderNum { get; set; }
        [Column(nameof(StrTagBarcode))]
        [Comment("TAGの下にあるバーコード。とりあえず読み取った結果そのまま格納する")]
        public string? StrTagBarcode { get; set; }
        [Column(nameof(IsCompiled))]
        [Comment("ViewMarsharingテーブルに登録済みフラグ")]
        public bool IsCompiled { get; set; } = false;

        /// <summary>
        /// Upsert時のキーパターンをラムダ式で指定
        /// </summary>
        Expression<Func<TQRinput, object>> IUpsertKeyPattern<TQRinput>.KeyPattern { get; } = keys => new
        {
            keys.DateInputDate,
            keys.QROPcode
        };
        /// <summary>
        /// Upsert時の除外フィールドをラムダ式で指定
        /// 現時点では無し(オートインクリメントフィールド自動検出テスト)
        /// </summary>
        Expression<Func<TQRinput, object>>? IHaveDefaultPattern<TQRinput>.DefauldExcludePattern { get; } = null;

        /// <summary>
        /// 重複データがあるかどうかチェック。重複条件は、InputDateとQROpcodeが一致するものとする
        /// もしくは、StrOrderNumかStrTagBarcodeに値が入っていて、この二つと、OPcode、InputDateのYear,Month,Dayまで同じ物が有れば重複とする
        /// (同じタグ、現品票の物を同じOPCodeで同日に複数回読んだ)
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="tqrinput">比較するTQRinputインスタンス</param>
        /// <returns>重複有りの場合はtrue、無しの場合はfalse</returns>
        public static bool IsDupe(in ExcelDbContext dbContext,TQRinput tqrinput)
        {
            if (!(string.IsNullOrEmpty(tqrinput.StrOrderNum)
                &&string.IsNullOrEmpty(tqrinput.StrTagBarcode)))
            {
                //付加情報どちらかに値が入っている場合
                //付加情報、OPcode、DateInputのYear、Month,Dayまで同じデータが有れば重複とみなす
                TQRinput? ExistStrAndSameDay = dbContext.TQRinputs
                                                .FirstOrDefault(tqr => 
                                                tqr.QROPcode == tqrinput.QROPcode 
                                                && tqr.StrOrderNum == tqrinput.StrOrderNum
                                                && tqr.StrTagBarcode == tqrinput.StrTagBarcode
                                                && tqr.DateInputDate.Year == tqrinput.DateInputDate.Year
                                                && tqr.DateInputDate.Month == tqrinput.DateInputDate.Month
                                                && tqr.DateInputDate.Day == tqrinput.DateInputDate.Day);
                if (ExistStrAndSameDay != null) 
                {
                    //同日に同じタグ、現品票を同じOPコードで読んだ場合
                    //重複とみなす
                    return true;
                }
            }
            //残りは、DateInputとOPCodeが一致すれば重複とみなす
            TQRinput? NoStrSameDayCode = dbContext.TQRinputs.FirstOrDefault(t => t.DateInputDate == tqrinput.DateInputDate
                                                                    && t.QROPcode == tqrinput.QROPcode);
            if (NoStrSameDayCode == null) 
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
    }
}
