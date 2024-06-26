using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelDBImporter.Tool
{
    public class SerialCommunication
    {
        private enum ReceiveState
        {
            [Comment("通常のJSONコード読み取り完了")]
            JSONCodeReadComplete = 1,
            [Comment("制御コマンド(返値あり)読み取り完了、DB登録なし")]
            ContrlReadComplete,
            [Comment("JSONコード読み取り未完了、次データ必要")]
            NeedNextJSONReceive,
            [Comment("データが無かった")]
            NoData,
            [Comment("エラー発生した")]
            Error,
            [Comment("受信完了(タイマー使用)")]
            CompReceive,
        }
        private readonly SerialPort serialPort;
        /// <summary>
        /// 受信データをバッファリングしておくQueue
        /// </summary>
        private ConcurrentQueue<string> cqStrSerialBuffer = new();
        /// <summary>
        /// データ終端検出用受信バッファ(ReadBytes)
        /// </summary>
        private StringBuilder SBreceiveBuff = new();

        private ConcurrentQueue<string> CqStrSerialBuffer { get => cqStrSerialBuffer; set => cqStrSerialBuffer = value; }
        /// <summary>
        /// 無通信自動切断のためのメンバ変数
        /// </summary>
        private DateTime lastDataReceivedTime;
        
        private readonly System.Timers.Timer disconnectTimer;
        private const int disconnectTimeout = 110000; // 110 seconds timeout
        /// <summary>
        /// 読み込み待機計測用タイマー
        /// </summary>
        private readonly System.Timers.Timer readWaitTimer;
        /// <summary>
        /// 読み込み完了までの待ち時間
        /// </summary>
        private const int ReadWaitTimeout = 2 * 1000;
        /// <summary>
        /// 通常 JSON データ受信イベント
        /// </summary>
        public event EventHandler<string>? DataReceived;
        /// <summary>
        /// 制御コマンド(戻り値あり) データ受信イベント
        /// </summary>
        public event EventHandler<string>? ControlMsgReceived;
        /// <summary>
        /// 受信完了イベント(タイマー使用)
        /// </summary>
        public event EventHandler<(string Strresult,int IntErrorCount)>? CompleteReceive;
        /// <summary>
        /// まが行が完了していない続きがあるよ、のイベント
        /// </summary>
        public event EventHandler<string>? NeedNextDataReceived;

        public string PortName => serialPort.PortName;

        public bool IsOpen => serialPort.IsOpen;

        /// <summary>
        /// DMコード内の日付形式を表す正規表現 yyyy/MM/dd HH/mm/ss
        /// </summary>
        private const string Const_Str_Datepattern = @"\b\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}\b";

        public SerialCommunication()
        {
            serialPort = new()
            {
                BaudRate = 9600,
                NewLine = "\r\n",
                DtrEnable = true,
                Handshake = Handshake.RequestToSendXOnXOff,
                RtsEnable = true,
            };
            serialPort.DataReceived += SerialPort_DataReceived;

            lastDataReceivedTime = DateTime.Now;
            disconnectTimer = new()
            {
                Interval = 5000 // Check every 5 second
            };
            disconnectTimer.Elapsed += Timer_Elapsed!;
            readWaitTimer = new()
            {
                //1秒周期でチェック
                Interval = 1100
            };
            readWaitTimer.Elapsed += ReadWait_Timer_Elapsed!;
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (serialPort.IsOpen && (DateTime.Now - lastDataReceivedTime).TotalMilliseconds > disconnectTimeout)
            {
                if (serialPort.IsOpen) 
                {
                    Close();
                }
            }
        }
        /// <summary>
        /// 受信待機時間タイマーイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadWait_Timer_Elapsed(object sender, EventArgs e)
        {
            if (serialPort.IsOpen && (DateTime.Now - lastDataReceivedTime).TotalMilliseconds > ReadWaitTimeout)
            {
                //最終受信時間から待機時間過ぎていたら
                readWaitTimer.Stop();
                //結果QueueにStringBuilderバッファの内容を全て追加
                cqStrSerialBuffer.Enqueue(SBreceiveBuff.ToString());
                //StringBuilderバッファ初期化
                SBreceiveBuff.Clear();
                
                //CompDataReceiveイベント発火
                CompleteReceive?.Invoke(this, ReadAllDatafromQueue());
            }
        }
        public static string[] GetPortNums ()
        {
            return SerialPort.GetPortNames();
        }

        private void UpdateLastDataReceivedTime()
        {
            lastDataReceivedTime = DateTime.Now;
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                ReceiveState receiveState = ReadFromSerialPort(out string StrResult);
                switch (receiveState)
                {
                    case ReceiveState.JSONCodeReadComplete:
                        //通常JSONデータ受信時
                        //クラスStrinbBuilderバッファに今回の結果を追加
                        SBreceiveBuff.Append(StrResult);
                        //受信待機時間タイマースタート
                        readWaitTimer.Stop();
                        Task.Delay(1);
                        readWaitTimer.Start();
                        //DataReveibedイベント発火
                        DataReceived?.Invoke(this, StrResult);
                        break;
                    case ReceiveState.ContrlReadComplete:
                        //制御文字データの場合
                        //結果キューの追加は無し
                        //ControlMsgReceivedイベント発火
                        ControlMsgReceived?.Invoke(this, StrResult);
                        break;
                    case ReceiveState.NeedNextJSONReceive:
                        //受信途中
                        //クラスStringBuilderバッファに今回の内容を追加
                        SBreceiveBuff.Append(StrResult);
                        //受信途中イベント発火
                        NeedNextDataReceived?.Invoke(this, StrResult);
                        break;
                    default:
                        break;
                }
                //最終受信時刻更新
                UpdateLastDataReceivedTime();
            }
        }

        /// <summary>
        /// 受信バッファに存在している全てのデータを返す
        /// </summary>
        /// <returns>通信内容のString</returns>
        public (string StrResult,int IntErrorCount) ReadAllDatafromQueue()
        {
            //呼ばれた時点でデータが無かったら
            if (CqStrSerialBuffer.IsEmpty) { return (string.Empty,0); }
            //結果格納文字列構築用StringBuilder
            System.Text.StringBuilder sb = new();
            //残り要素数、スタートは現時点での総要素数
            int IntCountRemain = cqStrSerialBuffer.Count;
            int IntErrorcount = 0;
            while(IntCountRemain > 0)
            {
                if (cqStrSerialBuffer.TryDequeue(out string? StrResut))
                {
                    //日付データの個数チェック(通信エラーチェック)
                    if (CountDates(StrResut) > 1)
                    {
                        //読み出したデータに日付データが複数あった場合はエラーカウントインクリメントする
                        Debug.WriteLine($"{nameof(ReadAllDatafromQueue)} で 1行に日付複数発見,CommuicationError");
                        IntErrorcount++;
                    }
                    //Dequeue成功したらSBに追加
                    sb.Append(StrResut);
                    IntCountRemain--;
                }
            }
            if (sb.Length > 0) 
            {
                //ループ抜けてSBにデータがあったらStringにして返す
                return (sb.ToString(),IntErrorcount);
            }
            else { return (string.Empty,0); }
    }
        private int CountDates(string Strinput)
        {
            // string pattern = @"\b\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}\b";

            Regex regex = new Regex(Const_Str_Datepattern);
            MatchCollection matches = regex.Matches(Strinput);

            return matches.Count;
        }

        /// <summary>
        /// シリアルポートからデータを読み出し、結果の最後のバイトデータによりReceiveStateEnum値を返す
        /// </summary>
        /// <param name="StrResult">出力用 今回の通信内容をエンコードしたString </param>
        /// <returns>ReceiveStateのEnum値</returns>
        private ReceiveState ReadFromSerialPort(out string StrResult)
        {
            try
            {
                if (serialPort.IsOpen)
                {

                    //データ終端見つけるためにまずはバイト配列で今あるデータを読み込む
                    //受信データ長を取得
                    int IntReadToBytes = serialPort.BytesToRead;
                    byte[] buff;
                    if (IntReadToBytes > 0)
                    {
                        //読み込むべきデータがあった場合
                        //バッファ確保
                        buff = new byte[IntReadToBytes];
                        //シリアルポートから受信データをバッファに読み込む
                        serialPort.Read(buff, 0, IntReadToBytes);
                        //読み込んだ内容をクラスのStringBuilderに追加する
                        StrResult =  Encoding.UTF8.GetString(buff);
                    }
                    else
                    {
                        StrResult = string.Empty;
                        //受信データ無し
                        return ReceiveState.NoData;
                    }

                    //受信データの最終バイトの結果により処理を分岐
                    return buff[IntReadToBytes - 1] switch
                    {
                        0x0A => ReceiveState.JSONCodeReadComplete,//0x0A(\n)の場合
                                                                  //通常JSONデータ受信
                        0x0D => ReceiveState.ContrlReadComplete,//0x0D(\r)の場合
                                                                //制御データ受信
                        _ => ReceiveState.NeedNextJSONReceive,//終了文字が0x09でも0x0Dでも無い場合
                                                              //データ受信途中とみなす
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(nameof(ReadFromSerialPort) + " " + ex.Message);
                StrResult = string.Empty;
                //エラー発生
                return ReceiveState.Error;
            }
            StrResult = string.Empty;
            //ここまで抜けてきちゃったらNoDataとする
            return ReceiveState.NoData;
        }


        public void Open(string portName)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = portName;
                    serialPort.Open();
                    //自動切断タイマー開始
                    disconnectTimer.Stop();
                    disconnectTimer.Start();
                    //自動切断基準時間更新
                    UpdateLastDataReceivedTime ();
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(nameof(Open) +" " + ex.Message);
                throw;
            }
        }

        public void Close()
        {
            try
            {
                disconnectTimer.Stop();
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                disconnectTimer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(nameof(Close) +" " + ex.Message);
                serialPort.Dispose();
            }
        }
    }
}
