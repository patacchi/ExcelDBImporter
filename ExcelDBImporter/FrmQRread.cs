using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            //シリアルポートの初期化
            serialCommunication = new SerialCommunication();
            serialCommunication.DataReceived += SerialCommunication_DataReceived!;
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
        private void SerialCommunication_DataReceived(object sender, string data)
        {
            Invoke(() => TxtBoxQRread.AppendText(data));
        }

        /// <summary>
        /// QRコード入力待機時間満了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 入力待機時間タイムアウト後
            string StrText = TxtBoxQRread.Text ?? string.Empty;
            if (string.IsNullOrEmpty(StrText)) { return; }
            //入力用テキストボックスをReadOnlyに
            if (this.InvokeRequired)
            {
                //Invokeが必要な場合(メイン(UI)スレッドじゃないのが変更しようとした)
                this.Invoke(() => TxtBoxQRread.ReadOnly = true);
                //this.Invoke(new DelegateDisableInput(DisableInput));
            }
            else
            {
                //メインスレッドから呼ばれた場合
                TxtBoxQRread.ReadOnly = true;
            }
            //MessageBox.Show(TxtBoxQRread.Text);
            DecordQRstringToTQRinput(StrText);
        }
        private void TextBoxQRread_TextChanged(object sender, EventArgs e)
        {
            // テキストボックスのテキストが変更されるたびにタイマーを再起動
            timerInputEnd.Stop();
            timerInputEnd.Start();
        }

        /// <summary>
        /// QRコードの文字列を解析してTQRinputテーブルに反映させる
        /// </summary>
        /// <param name="text"></param>
        private void DecordQRstringToTQRinput(string? text)
        {
            if (string.IsNullOrEmpty(text)) { return; }
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
