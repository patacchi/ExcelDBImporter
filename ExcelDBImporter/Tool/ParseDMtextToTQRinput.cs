using DocumentFormat.OpenXml.Drawing.Charts;
using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ExcelDBImporter.Models.ToInput;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace ExcelDBImporter.Tool
{
    /// <summary>
    /// DMコードの内容を最終的にTQRinputテーブルに格納するための
    /// </summary>
    internal class ParseDMtextToTQRinput
    {
        /// <summary>
        /// タグNoの文字列の最大長、これ以上は読み取りミスを疑う
        /// </summary>
        private const int Int_MAX_TagNo_Length = 10;
        /// <summary>
        /// タグNo文字列長の最小値
        /// </summary>
        private const int Int_MIN_TagNo_Length = 4;
        /// <summary>
        /// データの種類を表すEnum
        /// </summary>
        private enum EnumDataKind
        {
            /// <summary>
            /// TAGの下のバーコード
            /// </summary>
            [Comment("TAGNumber")]
            TAGNumber = 1,
            /// <summary>
            /// 現品票のオーダーNo
            /// </summary>
            [Comment("オーダーNo")]
            OrderNumber = 2,
            /// <summary>
            /// 入庫トランザクションテーブルのID
            /// </summary>
            [Comment("入庫トランザクションテーブルのID")]
            TrTableID = 3,
            /// <summary>
            /// TQRinputクラス(旧式)JSON
            /// </summary>
            [Comment("TQRinput JSON")]
            JSON_TQR = 4,
            /// <summary>
            /// DMInputOPcode JSON
            /// </summary>
            [Comment("DMInputOPcode JSON")]
            JSON_DM_OPcode = 5,
            /// <summary>
            /// DMInputScanCode JSON
            /// </summary>
            [Comment("DMInputScanCode JSON")]
            JSON_DM_ScanCode = 6,
            /// <summary>
            /// 手配コード
            /// </summary>
            [Comment("手配コード")]
            TehaiCode = 7,
            /// <summary>
            /// 数量
            /// </summary>
            [Comment("数量")]
            Amount = 8,
            /// <summary>
            /// 不明データ
            /// </summary>
            [Comment("不明データ")]
            UnKnownData = int.MaxValue
        }

        private ConcurrentQueue<string> CqDMString { get; set; } = new();
        private readonly string StrInput=null!;
        internal static readonly string[] newLineSeparator = ["\n", "\r", "\r\n"];
        internal static readonly string[] spaceSeparator = [" ","　"];

        public static bool IsValidJson(string StrJson)
        {
            try
            {
                JsonDocument.Parse(StrJson);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
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
            using ExcelDbContext dbContext = new();
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
#pragma warning disable IDE0018 // インライン変数宣言
                        DateTime result;
#pragma warning restore IDE0018 // インライン変数宣言
                        if (!DateTime.TryParse(string.Join("T", StrDateSplit, 0, 2), out result))
                        {
                            continue;
                        }
                        tempQRrow.DateInputDate = result;
                        //最初の要素以降をJoinして、中身のStringとする
                        //この時、元のデータに空白があった場合、Splitする際に除去されてしまっているので付加する
                        //Index2から開始して、総要素数 - スタートIndexの2
                        tempQRrow.StrRowQRcodeData = string.Join(" ",StrDateSplit,2,StrDateSplit.Length - 2);
                    }
                    //ここまで来て中身が空だったら何もせずに次のループへ
                    if (string.IsNullOrEmpty(tempQRrow.StrRowQRcodeData)) { continue; }
                    //ここまでの結果をdbContextに追加
                    //重複データは登録しない
                    //ToDo Upsertメソッドに置き換える
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
            dbContext.Dispose();
        }
        /// <summary>
        /// コードの中身の種別を判別する
        /// </summary>
        /// <param name="StrCheckData">判別したい内容をStringで</param>
        /// <returns>EnumKind型のEnum値</returns>
        private EnumDataKind GetDataKind(string StrCheckData)
        {
            switch (StrCheckData[..1])
            {
                case "{":
                    //{ から始まってる場合(JSONの可能性が高い)
                    if (!IsValidJson(StrCheckData))
                    {
                        //{から始まってるのにJSONに変換できない時は UnKnownにする
                        return EnumDataKind.UnKnownData;
                    }
                    //JSONに変換できる場合
                    JsonDocument jsonDocument = JsonDocument.Parse(StrCheckData);
                    JsonElement root = jsonDocument.RootElement;
                    if (!root.TryGetProperty(nameof(DMInputOPcode.StrClassName), out JsonElement element))
                    {
                        //JsonにStrClassName要素が無い場合はTQRinputとする
                        return EnumDataKind.JSON_TQR;
                    }
                    else
                    {
                        //StrClassName要素の値によって処理を分岐
                        return element.GetString() switch
                        {
                            nameof(DMInputOPcode) => EnumDataKind.JSON_DM_OPcode,//OPCode
                            nameof(DMInputScanCode) => EnumDataKind.JSON_DM_ScanCode,//ScanCode
                            _ => EnumDataKind.UnKnownData,//未定義の場合 UnKnownを返す
                        };
                    }
                default:
                    //始まりが"{"以外、付加データになる
                    if (Int32.TryParse(StrCheckData, out _))
                    {
                        //Int32に変換できるときは、トランザクションテーブルのIDとみなす
                        return EnumDataKind.TrTableID;
                    }
                    if (StrCheckData[..3].Equals("3n3", StringComparison.OrdinalIgnoreCase))
                    {
                        //3n3で始まる時はオーダーNo
                        return EnumDataKind.OrderNumber;
                    }
                    if (StrCheckData[..3].Equals("3n4", StringComparison.OrdinalIgnoreCase))
                    {
                        //3n4で始まるのは手配コード
                        return EnumDataKind.TehaiCode;
                    }
                    if (StrCheckData[..3].Equals("3n5",StringComparison.OrdinalIgnoreCase))
                    {
                        //3n5で始まるのは数量、ただし無いのも多い
                        return EnumDataKind.Amount;
                    }
                    if (StrCheckData.Length <= Int_MAX_TagNo_Length && StrCheckData.Length >= Int_MIN_TagNo_Length)
                    {
                        //TagNoの文字列長が範囲内の場合はTagNoとする
                        return EnumDataKind.TAGNumber;
                    }
                    //ここまで来たら未定義なので、UnKnownとする
                    return EnumDataKind.UnKnownData;
                    
            }
        }
        /// <summary>
        /// TTmpQRrowテーブルのデータをパースして、TQRinputテーブルに登録する
        /// </summary>
        /// <returns>処理件数をInt</returns>
        public int RegistToTQRinput()
        {
            using ExcelDbContext dbContext = new();
            //まずは日付順にソートしたTTempデータを得る
            List<TTempQRrowData> ttempList = dbContext.TTempQRrows
                                        .OrderBy(temp => temp.DateInputDate)
                                        .ToList();
            //ttempの消去候補の行のIDを格納するQueue
            Queue<int> QueIntDeleteID = new();
            //処理件数
            int IntRegistCount = 0;
            foreach (TTempQRrowData ttemp in ttempList)
            {
                try 
                {
                    switch (GetDataKind(ttemp.StrRowQRcodeData))
                    {
                        case EnumDataKind.JSON_TQR:
                            //TQRinputだった場合
                            //デシリアライズ
                            TQRinput? tQRinput = JsonSerializer.Deserialize<TQRinput>(ttemp.StrRowQRcodeData);
                            if (tQRinput == null) { continue; }
                            //デシリアライズ成功したら、消去候補Queueに残っているttemp行は不要なので削除マーク
                            DeleteTtempByIDQueue(dbContext, QueIntDeleteID);
                            //入力日時をTtempテーブルのものにする
                            tQRinput.DateInputDate = ttemp.DateInputDate;
                            //重複チェック
                            if (!TQRinput.IsDupe(dbContext,tQRinput))
                            {
                                //重複無しの場合のみ追加する
                                //DBに登録
                                dbContext.TQRinputs.Add(tQRinput);
                                dbContext.SaveChanges();
                                //処理件数インクリメント
                                IntRegistCount++;
                            }
                            //消去候補IDに現在のttempのIDを追加
                            QueIntDeleteID.Enqueue(ttemp.TTempQRrowDataId);
                            continue;
                        case EnumDataKind.JSON_DM_OPcode:
                            //DMOPcodeだった場合
                            //デシリアライズ
                            DMInputOPcode? dmOPcode = JsonSerializer.Deserialize<DMInputOPcode>(ttemp.StrRowQRcodeData);
                            if (dmOPcode == null) { continue; }
                            //デシリアライズ成功したら、消去候補Queueに残っているttemp行は不要なので削除マーク
                            DeleteTtempByIDQueue(dbContext, QueIntDeleteID);
                            //DBに登録するTQRinputクラスのインスタンス
                            TQRinput tQRinputOp = new();
                            //入力日時をttempの物に
                            tQRinputOp.DateInputDate = ttemp.DateInputDate;
                            //OPコードをdmOPcodeより
                            tQRinputOp.QROPcode = dmOPcode.QROPcode;
                            //重複チェック
                            if (!TQRinput.IsDupe(dbContext,tQRinputOp))
                            {
                                //重複無しの場合のみDB追加
                                //DBに登録
                                dbContext.TQRinputs.Add(tQRinputOp);
                                dbContext.SaveChanges();
                                //処理件数インクリメント
                                IntRegistCount++;
                            }
                            //消去候補IDに現在のttempのIDを追加
                            QueIntDeleteID.Enqueue(ttemp.TTempQRrowDataId);
                            continue;
                        case EnumDataKind.OrderNumber:
                            //オーダーNo追加するのに、直前に登録したTQRエンティティを取得・・出来るのかな？
                            //まずは消去候補Queueの先頭がTQRinputに登録した行になってるはずなので、その日付を取得
                            TTempQRrowData? ttempFirst = dbContext.TTempQRrows
                                                        .FirstOrDefault(x => x.TTempQRrowDataId == QueIntDeleteID.Peek());
                            if (ttempFirst == null) { continue; }   //指定のIDの物はありませんでした
                            DateTime DateTarget = ttempFirst.DateInputDate;
                            //次にその日付のデータをTQRinputより取得を試みる
                            TQRinput? tqrTarget = dbContext.TQRinputs
                                                .FirstOrDefault(t => t.DateInputDate == ttempFirst.DateInputDate);
                            if (tqrTarget == null) { continue; }
                            //取得したTQRinputエンティティのオーダーNoを更新
                            tqrTarget.StrOrderNum = ttemp.StrRowQRcodeData[3..11].ToUpper();
                            //消去候補IDに現在のttempのIDを追加
                            QueIntDeleteID.Enqueue(ttemp.TTempQRrowDataId);
                            //処理件数インクリメント
                            IntRegistCount++;
                            continue;
                        case EnumDataKind.TAGNumber:
                            //直前に登録したTQRエンティティを取得
                            //まずは消去候補Queueの先頭がTQRinputに登録した行になってるはずなので、その日付を取得
                            TTempQRrowData? ttempFirstTag = dbContext.TTempQRrows
                                                        .FirstOrDefault(x => x.TTempQRrowDataId == QueIntDeleteID.Peek());
                            if (ttempFirstTag == null) { continue; }   //指定のIDの物はありませんでした
                            DateTime DateTargetTag = ttempFirstTag.DateInputDate;
                            //次にその日付のデータをTQRinputより取得を試みる
                            TQRinput? tqrTargetTag = dbContext.TQRinputs
                                                .FirstOrDefault(t => t.DateInputDate == ttempFirstTag.DateInputDate);
                            if (tqrTargetTag == null) { continue; }
                            //取得したTQRinputのタグNoを更新
                            tqrTargetTag.StrTagBarcode = ttemp.StrRowQRcodeData;
                            //消去候補IDに現在のttempのIDを追加
                            QueIntDeleteID.Enqueue(ttemp.TTempQRrowDataId);
                            //処理件数インクリメント
                            IntRegistCount++;
                            continue;
                        case EnumDataKind.UnKnownData:
                            //未定義データだった場合
                            continue;
                        default:
                            //それ以外の値が返ってきたら
                            continue;
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show($"{nameof(RegistToTQRinput)} で {ex.Message} エラー ");
                    continue;
                }
            }
            dbContext.SaveChanges();
            return IntRegistCount;
        }
        private void DeleteTtempByIDQueue(ExcelDbContext dbContext ,Queue<int> QueueIntDeleteID)
        {
            //削除候補が無かったら抜ける
            if (QueueIntDeleteID.Count == 0) { return; }
            while (QueueIntDeleteID.Count > 0)
            {
                TTempQRrowData? ttempDelete = dbContext.TTempQRrows
                                                .FirstOrDefault(t => t.TTempQRrowDataId == QueueIntDeleteID.Dequeue());
                if (ttempDelete == null) { continue; } //指定したIDが見つからなかった
                dbContext.TTempQRrows.Remove(ttempDelete);
            }
            //ループ抜けたらsavechange
            dbContext.SaveChanges();
        }
    }
}
