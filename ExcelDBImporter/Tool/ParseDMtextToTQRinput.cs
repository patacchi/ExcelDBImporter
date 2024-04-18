using DocumentFormat.OpenXml.Drawing.Charts;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter.Tool
{
    /// <summary>
    /// DMコードの内容を最終的にTQRinputテーブルに格納するための
    /// </summary>
    internal class ParseDMtextToTQRinput
    {
        private ConcurrentQueue<string> CqDMString { get; set; } = new();
        private readonly string StrInput=null!;
        internal static readonly string[] newLineSeparator = ["\n", "\r", "\r\n"];
        internal static readonly string[] spaceSeparator = [" ","　"];

        public ParseDMtextToTQRinput(string strInput)
        {
            StrInput = strInput ?? throw new ArgumentNullException(nameof(strInput));
            //入力されたStringを行ごとにQueueに入れる
            string[] Strlines = StrInput.Split(newLineSeparator,StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in Strlines) 
            {
                //すべての行をQueueに入れる
                CqDMString.Enqueue(line);
            }
        }
        /// <summary>
        /// DMStringの中身をTtempQRrowDataに入れる
        /// </summary>
        public void ParseDMStrToTempTable()
        {
            //最初から要素数0だったら抜ける
            if (CqDMString.IsEmpty) { return; }
            ExcelDbContext dbContext = new();
            while(!CqDMString.IsEmpty)
            {
                //Queueの中身がある間はループ
#pragma warning disable IDE0018 // インライン変数宣言
                string? StrChunk;
#pragma warning restore IDE0018 // インライン変数宣言
                               //先頭要素をTryDequeueする
                if (CqDMString.TryDequeue(out StrChunk)) 
                {
                    //Dequeue成功した時のみ実行
                    TTempQRrowData tempQRrow = new();
                    //行を空白で区切り、最初の要素が日付にキャスト出来るかチェック
                    //要素が1個だったり、キャスト出来なかったら現在時刻をセット
                    //最初に空白区切りで、年月日 時間 DMコードの中身と入っている
                    string[] StrDateSplit = StrChunk.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (StrDateSplit.Length == 0)
                    {
                        //要素無かったら抜ける
                        continue;
                    }
                    else if (StrDateSplit.Length < 3)
                    {
                        //要素が2個以下しか無かった場合
                        //バッテリー残量とかのコード内容以外の物を判別する方法がわかるまではスルーする
                        /*
                        tempQRrow.DateInputDate = DateTime.Now;
                        tempQRrow.StrRowQRcodeData = string.Join("",StrDateSplit,1,StrDateSplit.Length - 1);
                        */
                        continue;
                    }
                    else
                    {
                        //要素が複数あった場合
                        //まず、最初の要素と次の要素を結合してDateTimeにキャスト出来るかトライして、出来なかったら中断して次のループへ
                        DateTime result;
                        if (!DateTime.TryParse(string.Join("T", StrDateSplit, 0, 2), out result))
                        {
                            continue;
                        }
                        tempQRrow.DateInputDate = result;
                        //最初の要素以降をJoinして、中身のStringとする
                        //Index2から開始して、総要素数 - スタートIndexの2
                        tempQRrow.StrRowQRcodeData = string.Join("",StrDateSplit,2,StrDateSplit.Length - 2);
                    }
                    //ここまで来て中身が空だったら何もせずに次のループへ
                    if (string.IsNullOrEmpty(tempQRrow.StrRowQRcodeData)) { continue; }
                    //ここまでの結果をdbContextに追加
                    //重複データは登録しない
                    if (TTempQRrowData.IsDupe(dbContext, tempQRrow)) 
                    {
                        //重複データだった場合
                        //とりあえず何もしないでループの次へ
                        continue;
                    }
                    else
                    {
                        //重複データ無かった場合はテーブルに追加
                        dbContext.TTempQRrows.Add(tempQRrow);
                    }
                }
            }
            //ループを抜けたらSaveChange
            dbContext.SaveChanges();
        }
    }
}
