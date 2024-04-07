﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ExcelDBImporter.Tool;
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
        MicrowaveDelivary = opMicroWave | Delivery,
        /// <summary>
        /// 支給品出庫・支給準備作業セット
        /// </summary>
        [Comment("支給品出庫・支給準備作業セット")]
        ShppingDeliverSet = PrepareShpping | Delivery,
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
    }
    [Comment("工程管理用 QRコード記録テーブル")]
    [Table(nameof(TQRinput))]
    public class TQRinput()
    {
        private QrOPcode qROPcode;

        [Key]
        [Column(nameof(TQRinputId))]
        [Comment("QRテーブルのキー")]
        public int TQRinputId { get; set; }
        [Column(nameof(DateInputDate))]
        [Comment("入力日時、作業開始日時として使用")]
        public DateTime? DateInputDate { get; set; }
        [Column(nameof(DateToDate))]
        [Comment("終了日時、基本的に自動計算で入力する")]
        public DateTime? DateToDate { get; set; }
        [Column(nameof(QROPcode))]
        [Comment("行程種別のenum。初期値 None")]
        public QrOPcode QROPcode { get => qROPcode; set => qROPcode = value; }
    }
}
