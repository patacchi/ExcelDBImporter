using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelDBImporter.Tool
{
    public class SerialCommunication
    {
        private readonly SerialPort serialPort;
        /// <summary>
        /// 受信データをバッファリングしておくQueue
        /// </summary>
        private ConcurrentQueue<string> cqStrSerialBuffer = new();
        private ConcurrentQueue<string> CqStrSerialBuffer { get => cqStrSerialBuffer; set => cqStrSerialBuffer = value; }
        /// <summary>
        /// 無通信自動切断のためのメンバ変数
        /// </summary>
        private DateTime lastDataReceivedTime;
        
        private readonly System.Timers.Timer disconnectTimer;
        private const int disconnectTimeout = 110000; // 110 seconds timeout

        public event EventHandler<string>? DataReceived;

        public string PortName => serialPort.PortName;

        public bool IsOpen => serialPort.IsOpen;

        public SerialCommunication()
        {
            serialPort = new()
            {
                BaudRate = 38400,
                NewLine = "\r\n"
            };
            serialPort.DataReceived += SerialPort_DataReceivedAsync;

            lastDataReceivedTime = DateTime.Now;
            disconnectTimer = new()
            {
                Interval = 5000 // Check every 5 second
            };
            disconnectTimer.Elapsed += Timer_Elapsed!;
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
        public static string[] GetPortNums ()
        {
            return SerialPort.GetPortNames();
        }

        private void UpdateLastDataReceivedTime()
        {
            lastDataReceivedTime = DateTime.Now;
        }
        private async void SerialPort_DataReceivedAsync(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                string data = await ReadFromSerialPortAsync();
                if (data.Length > 0) 
                {
                    //バッファに現在のデータ追加
                    CqStrSerialBuffer.Enqueue(data);
                }
                DataReceived?.Invoke(this, data);
                //最終受信時刻更新
                UpdateLastDataReceivedTime();
            }
        }

        /// <summary>
        /// 受信バッファに存在している全てのデータを返す
        /// </summary>
        /// <returns>通信内容のString</returns>
        public string? ReadAllDatafromQueue()
        {
            //呼ばれた時点でデータが無かったら
            if (CqStrSerialBuffer.IsEmpty) { return null; }
            //結果格納文字列構築用StringBuilder
            System.Text.StringBuilder sb = new();
            //残り要素数、スタートは現時点での総要素数
            int IntCountRemain = cqStrSerialBuffer.Count;
            while(IntCountRemain > 0)
            {
                if (cqStrSerialBuffer.TryDequeue(out string? StrResut))
                {
                    //Dequeue成功したらSBに追加
                    sb.Append(StrResut);
                    IntCountRemain--;
                }
            }
            if (sb.Length > 0) 
            {
                //ループ抜けてSBにデータがあったらStringにして返す
                return sb.ToString();
            }
            else { return null; }
    }
        private async Task<string> ReadFromSerialPortAsync()
        {
            return await Task.Run(() =>
            {
                try 
                {
                    if (serialPort.IsOpen)
                    {
                        return serialPort.ReadExisting();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(nameof(ReadFromSerialPortAsync) +" " + ex.Message);
                    return string.Empty;
                }
                return string.Empty;
            });
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
                disconnectTimer.Close();
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(nameof(Close) +" " + ex.Message);
                serialPort.Dispose();
            }
        }
    }
}
