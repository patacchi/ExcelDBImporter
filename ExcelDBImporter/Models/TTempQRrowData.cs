﻿using ExcelDBImporter.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter.Models
{
    [Table(name: nameof(TTempQRrowData))]
    [Comment("QRコードを読み取った生のデータを一時保存しておくテーブル。入力日時をキーとする。")]
    [Index(nameof(DateInputDate))]
    public class TTempQRrowData
    {
        [Column(name:nameof(TTempQRrowDataId))]
        [Key]
        public int TTempQRrowDataId { get; set; }
        [Column(name:nameof(DateInputDate))]
        [Comment("入力日時")]
        public DateTime DateInputDate { get; set; }
        [Column(name: nameof(StrRowQRcodeData))]
        [Comment("QR(バーコード)の読み取り結果の生データをそのまま格納する")]
        public string StrRowQRcodeData { get; set; } = string.Empty;

        /// <summary>
        /// TTempQRrowDataテーブルで、重複するレコードかどうかチェックする。重複条件は、日時とQRコード内容が一致する事。
        /// </summary>
        /// <param name="context">DbContextの読み取り専用参照</param>
        /// <param name="tempQRrow"></param>
        /// <returns></returns>
        public static bool IsDupe(in ExcelDbContext context,TTempQRrowData tempQRrow)
        {
            TTempQRrowData? existing = context.TTempQRrows.FirstOrDefault(t => t.DateInputDate == tempQRrow.DateInputDate
                                                                               && t.StrRowQRcodeData == tempQRrow.StrRowQRcodeData);
            if (existing == null) 
            { 
                //同じレコードが存在しない
                return false;
            }
            else
            {
                //同じレコードが存在する
                return true;
            }
        }
    }
}