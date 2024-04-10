using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelDBImporter.Tool
{
    public class SerialCommunication
    {
        private readonly SerialPort serialPort;

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
        }
        
        public static string[] GetPortNums ()
        {
            return SerialPort.GetPortNames();
        }

        private async void SerialPort_DataReceivedAsync(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                string data = await ReadFromSerialPortAsync();
                DataReceived?.Invoke(this, data);
            }
        }

        private Task<string> ReadFromSerialPortAsync()
        {
            return Task.Run(() =>
            {
                if (serialPort.IsOpen)
                {
                    return serialPort.ReadExisting();
                }
                return string.Empty;
            });
        }

        public void Open(string portName)
        {
            if (!serialPort.IsOpen)
            {
                serialPort.PortName = portName;
                serialPort.Open();
            }
        }

        public void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
    }
}
