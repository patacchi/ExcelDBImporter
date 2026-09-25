using ExcelDBImporter.Context;
using ExcelDBImporter.Tool;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter.Models.View
{
    /// <summary>
    /// マーシャリング実績表示に必要なビュークラス
    /// </summary>

    [Table(nameof(ViewMarsharing))]
    public class ViewMarsharing : IHaveDefaultPattern<ViewMarsharing>
    {
        [Column(nameof(ViewMarsharingID))]
        public int ViewMarsharingID { get; set; }

        [Column(nameof(DatePerDay))]
        [Comment("日付")]
        public DateTime? DatePerDay { get; set; }
        /// <summary>
        /// PrepareReveiveSet
        /// </summary>
        [Column(nameof(IntPrepareReceive))]
        [Comment("棚入れ前\r\n準備作業")]
        public int? IntPrepareReceive { get; set; }
        /// <summary>
        /// FreewayDataInput
        /// </summary>
        [Column(nameof(IntFreewayData))]
        [Comment("Freeway\r\nデータ処理")]
        public int? IntFreewayData { get; set; }
        /// <summary>
        /// Delivery
        /// </summary>
        [Column(nameof(IntDelivery))]
        [Comment("出庫作業")]
        public int? IntDelivery { get; set; }
        /// <summary>
        /// PrepareShpping
        /// </summary>
        [Column(nameof(IntShipping))]
        [Comment("支給品準備\r\n払出")]
        public int? IntShipping { get; set; }
        /// <summary>
        /// opMicroWave
        /// </summary>
        [Column(nameof(IntMicroWave))]
        [Comment("マイクロ波\r\n払出")]
        public int? IntMicroWave { get; set; }
        /// <summary>
        /// CutCable
        /// </summary>
        [Column(nameof(IntCableCut))]
        [Comment("ケーブル\r\n切断作業")]
        public int? IntCableCut { get; set; }
        /// <summary>
        /// Moving
        /// </summary>
        [Column(nameof(IntMoving))]
        [Comment("運搬回数\r\n倉庫内移動")]
        public int? IntMoving { get; set; }
        /// <summary>
        /// Other
        /// </summary>
        [Column(nameof(IntOther))]
        [Comment("その他作業")]
        public int? IntOther { get; set; }
        [Column(nameof(IsCompiled))]
        [Comment("集計されたかどうかを示す")]
        public bool IsCompiled { get; set; } = false;

        /// <summary>
        /// Upsert時のキーフィールド
        /// </summary>
        Expression<Func<ViewMarsharing, object>> IUpsertKeyPattern<ViewMarsharing>.KeyPattern { get; } = keys => new
        {
            keys.DatePerDay
        };

        /// <summary>
        /// Upsert時の除外フィールド
        /// とりあえずID列、集計フラグ列
        /// </summary>
        Expression<Func<ViewMarsharing, object>>? IHaveDefaultPattern<ViewMarsharing>.DefauldExcludePattern { get; }= exclude => new
        {
            exclude.ViewMarsharingID,
            exclude.IsCompiled
        };

        /// <summary>
        /// 重複してるかチェックして、結果をboolで返すと共に、outに指定された変数にエンティティそのものを返す
        /// </summary>
        /// <param name="dbContext">dbcontextは共用</param>
        /// <param name="viewMarsharing">比較対象のエンティティ</param>
        /// <param name="existing">out 重複しているものがあればそのエンティティが、無ければnull</param>
        /// <returns>重複していればtrue、していなければfalse</returns>
        public static bool IsDupe(in ExcelDbContext dbContext,ViewMarsharing viewMarsharing,out ViewMarsharing? existing)
        {
            existing = dbContext.ViewMarsharings
                                    .FirstOrDefault(v => v.DatePerDay == viewMarsharing.DatePerDay);
            if (existing == null)
            {
                //重複無し
                return false;
            }
            else
            {
                //重複有り
                return true;
            }
        }
    }
}
