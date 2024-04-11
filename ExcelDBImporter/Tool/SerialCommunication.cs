using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelDBImporter.Tool
{
    public class SerialCommunication
    {
        private readonly SerialPort serialPort;

        //無通信自動切断のためのメンバ変数
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
                BaudRate = 9600,
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
                DataReceived?.Invoke(this, data);
                //最終受信時刻更新
                UpdateLastDataReceivedTime();
            }
        }

        private Task<string> ReadFromSerialPortAsync()
        {
            return Task.Run(() =>
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
