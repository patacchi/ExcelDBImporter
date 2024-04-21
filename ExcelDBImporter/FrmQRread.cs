using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDBImporter.Tool;

namespace ExcelDBImporter
{
    public partial class FrmQRread : Form
    {
        /// <summary>
        /// シリアル通信を行うクラスのインスタンス変数
        /// </summary>
        private readonly SerialCommunication serialCommunication;
        /// <summary>
        /// 入力終端(時間ベース)測定用のTimer
        /// </summary>
        private readonly System.Timers.Timer timerInputEnd;
        /// <summary>
        /// QRコード入力インターバル閾値。この時間入力が無かったら終端とみなす
        /// </summary>
        private const int delayMilliseconds = 1000; // 1秒
        /// <summary>
        /// 接続状態更新間隔
        /// </summary>
        private const int ConnectionCheckInMilliseconds = 5000;
        private readonly Stopwatch readTimeStopwatch;
        /// <summary>
        /// 定期実行したいタスク(非同期実行)
        /// </summary>
        private Task? timerTask;
        /// <summary>
        /// タスクキャンセルするためのToken
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        public FrmQRread()
        {
            InitializeComponent();
            //読み込み時間計測用ストップウォッチ初期化
            readTimeStopwatch = new();
            //シリアルポートの初期化
            serialCommunication = new SerialCommunication();
            //データ受信途中のイベント
            serialCommunication.NeedNextDataReceived += SerialCommunication_DataReceived!;
            //通常JSONデータ受信時のイベント
            serialCommunication.DataReceived += SerialCommunication_DataReceived!;
            //受信完了時のイベント
            serialCommunication.CompleteReceive += SerialCommunication_CompReceive!;
            //制御コマンド受信時のイベント
            serialCommunication.ControlMsgReceived += SerialCommunication_ControlReceived!;
            
            //ポート番号コンボボックスの設定
            InitializePotNumCmbBox();
            timerInputEnd = new System.Timers.Timer
            {
                Interval = delayMilliseconds
            };
            timerInputEnd.Elapsed += Timer_Elapsed!;
            timerInputEnd.AutoReset = false; // タイマーが一度経過したら自動的に再開しないように設定
            cancellationTokenSource = new CancellationTokenSource();
        }
        private void InitializePotNumCmbBox()
        {
            string[] portNames = SerialCommunication.GetPortNums();
            if (portNames.Length == 0) { return; }
            foreach (string portName in portNames)
            {
                CmbBoxPortNum.Items.Add(portName);
            }
        }
        /// <summary>
        /// 通常JSONデータ受信時、追加データある可能性あり
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Strdata">今回の受信データ</param>
        private void SerialCommunication_DataReceived(object sender, string Strdata)
        {
            // データ受信する度にタイマー再起動
            timerInputEnd.Stop();
            Task.Delay(1);
            timerInputEnd.Start();
            //ストップウォッチ止まっていたら開始する
            if (!readTimeStopwatch.IsRunning) 
            {
                readTimeStopwatch.Start(); 
            }
            //入力用テキストボックスをReadOnlyに
            if (this.InvokeRequired)
            {
                //Invokeが必要な場合(メイン(UI)スレッドじゃないのが変更しようとした)
                this.Invoke(() => TxtBoxQRread.Text = Strdata);
                this.Invoke(() => TxtBoxQRread.ReadOnly = true);
            }
            else
            {
                //メインスレッドから呼ばれた場合
                //TxtBoxQRread.Text = Strdata;
                TxtBoxQRread.ReadOnly = true;
            }
            //処理時間ラベルの更新のみ行う、デコードはCompReceiveしてから
            Invoke(() => LblElsapedTime.Text = ($"受信処理中 {readTimeStopwatch.ElapsedMilliseconds} ミリ秒経過"));
        }
        /// <summary>
        /// データ受信完了時(タイマー使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Strdata">受信した結果Queueを全て統合したもの</param>
        private void SerialCommunication_CompReceive(object sender, string Strdata)
        {
            // データ受信する度にタイマー再起動
            timerInputEnd.Stop();
            Task.Delay(1);
            timerInputEnd.Start();
            //ストップウォッチ止まっていたら開始する
            if (!readTimeStopwatch.IsRunning)
            {
                readTimeStopwatch.Start();
            }
            //入力用テキストボックスをReadOnlyに
            if (this.InvokeRequired)
            {
                //Invokeが必要な場合(メイン(UI)スレッドじゃないのが変更しようとした)
                this.Invoke(() => TxtBoxQRread.Text = Strdata);
                this.Invoke(() => TxtBoxQRread.ReadOnly = true);
            }
            else
            {
                //メインスレッドから呼ばれた場合
                TxtBoxQRread.Text = Strdata;
                TxtBoxQRread.ReadOnly = true;
            }
            DecordQRstringToTQRinput(Strdata);
            //デコード処理終了後ストップウォッチ停止
            readTimeStopwatch.Stop();
            Invoke(() => LblElsapedTime.Text = ($"{readTimeStopwatch.ElapsedMilliseconds} ミリ秒で処理完了"));
            readTimeStopwatch.Reset();
        }

        /// <summary>
        /// 制御コマンド受信時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="StrData">今回の受信データのみ</param>
        private void SerialCommunication_ControlReceived(object sender,string Strdata)
        {
            //テキストボックスの表示を更新するのみで終了
            //入力用テキストボックスをReadOnlyに
            if (this.InvokeRequired)
            {
                //Invokeが必要な場合(メイン(UI)スレッドじゃないのが変更しようとした)
                this.Invoke(() => TxtBoxQRread.Text = Strdata);
                this.Invoke(() => TxtBoxQRread.ReadOnly = true);
                Invoke(() => LblElsapedTime.Text = ($"{DateTime.Now} に制御データ受信"));
                //this.Invoke(new DelegateDisableInput(DisableInput));
            }
            else
            {
                //メインスレッドから呼ばれた場合
                TxtBoxQRread.Text = Strdata;
                TxtBoxQRread.ReadOnly = true;
            }
        }

        /// <summary>
        /// QRコード入力待機時間満了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 入力待機時間タイムアウト後
            // SeriaCommunicationのバッファから結果を読み取り、テキストボックスに適用
            string StrComnBuffer = serialCommunication.ReadAllDatafromQueue() ?? string.Empty;
            if (string.IsNullOrEmpty(StrComnBuffer)) { return; }
        }
        private void TextBoxQRread_TextChanged(object sender, EventArgs e)
        {
            /*
            // テキストボックスのテキストが変更されるたびにタイマーを再起動
            timerInputEnd.Stop();
            timerInputEnd.Start();
            */
        }

        /// <summary>
        /// QRコードの文字列を解析してTQRinputテーブルに反映させる
        /// </summary>
        /// <param name="text"></param>
        private void DecordQRstringToTQRinput(string? text)
        {
            if (string.IsNullOrEmpty(text)) { return; }
            //デコード、テーブル登録開始
            ParseDMtextToTQRinput parseDMtext = new ParseDMtextToTQRinput(text);
            parseDMtext.ParseDMStrToTempTable();
            //TempテーブルからTQRinputテーブルに登録する作業へ
            MessageBox.Show($"{parseDMtext.RegistToTQRinput()} 件のデータを処理しました");
        }

        /// <summary>
        /// バーコードからの入力値を編集可能にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEditInput_Click(object sender, EventArgs e)
        {
            if (TxtBoxQRread.ReadOnly == false)
            {
                MessageBox.Show("入力値は既に編集可能になっています");
            }
            else
            {
                TxtBoxQRread.ReadOnly = false;
                MessageBox.Show("入力値が編集可能になりました");
            }
            //手動入力した場合は、DB登録ボタンを有効にする
            BtnRegistToTempDB.Enabled = true;
        }

        
        private void BtnRegistToTempDB_Click(object sender,EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtBoxQRread.Text))
            {
                MessageBox.Show("テキストボックスの内容が空でした。処理を中断します");
                return;
            }
            //DBに現在のQRread テキストボックスの内容を登録する
            DecordQRstringToTQRinput(TxtBoxQRread.Text);
            //完了したらまた登録ボタンのEnabledをfalseにする
            BtnRegistToTempDB.Enabled = false;
            TxtBoxQRread.ReadOnly = true;
        }
        private void BtnPortOpen_Click(object sender, EventArgs e)
        {
            if (CmbBoxPortNum.Items.Count == 0)
            {
                MessageBox.Show("接続可能なポートがありませんでした。処理を中断します");
                return;
            }
            //ポート番号が選択されていなかったら一番上のを選択する
            if (CmbBoxPortNum.SelectedIndex == -1) { CmbBoxPortNum.SelectedIndex = 0; }
            if (string.IsNullOrEmpty(CmbBoxPortNum.SelectedItem?.ToString())) { return; }
            string selectedPort = CmbBoxPortNum.SelectedItem?.ToString()!;
            if (!string.IsNullOrEmpty(selectedPort))
            {
                if (serialCommunication.IsOpen)
                {
                    MessageBox.Show("既にポート " + selectedPort + " は開いています");
                    return;
                }
                try
                {
                    serialCommunication.Open(selectedPort);
                    MessageBox.Show($"ポート {selectedPort} が開かれました。\n");
                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void BtnPortClose_Click(object sender, EventArgs e)
        {
            if (!serialCommunication.IsOpen)
            {
                MessageBox.Show("ポートは既に閉じています");
                return;
            }
            try
            {
                serialCommunication.Close();
                MessageBox.Show($"ポート {serialCommunication.PortName} が閉じられました。\n");
                //TxtBoxQRread.AppendText($"ポート {serialCommunication.PortName} が閉じられました。\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// 非同期で定期的に実行する処理
        /// </summary>
        private async Task StartTimer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(ConnectionCheckInMilliseconds,cancellationToken); // 待機秒数は定数で定義

                if (!cancellationToken.IsCancellationRequested ) 
                {
                    // UIスレッドでラベルの更新を行う
                    Invoke(new Action(() =>
                    {
                        if (string.IsNullOrEmpty(serialCommunication.PortName))
                        {
                            LblConnectionStatus.Text = "未接続状態";
                            return;
                        }
                        if (serialCommunication.IsOpen)
                        {
                            LblConnectionStatus.Text = $"ポート {serialCommunication.PortName} に接続中";
                        }
                        else
                        {
                            LblConnectionStatus.Text = $"ポート {serialCommunication.PortName} に接続されていません";
                        }
                    }
                    ));
                }
            }
        }
        private void FrmQRread_Load(object sender, EventArgs e)
        {
            // タイマータスクを開始
            timerTask = StartTimer(cancellationTokenSource.Token);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                readTimeStopwatch.Stop();
                serialCommunication.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // タイマータスクをキャンセル
            cancellationTokenSource.Cancel();
            base.OnFormClosing(e);
        }

    }
}
