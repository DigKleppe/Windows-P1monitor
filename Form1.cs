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

namespace P1monitor
{
    public partial class Form1 : Form
    {
        static SerialPort serialPort;
        static Parser parser = new Parser();

        public Form1()
        {
            InitializeComponent();
            Height = Properties.Settings.Default.FormHeight;
            Width = Properties.Settings.Default.FormWidth;
            Top = Properties.Settings.Default.FormTop;
            Left = Properties.Settings.Default.FormLeft;
            Parser parser = new Parser();
            if ( OpenSerialPort (Properties.Settings.Default.Comport) == false )
            {
                SerialPortForm serialPortForm = new SerialPortForm(Properties.Settings.Default.Comport);
            }
        }
              
        public bool OpenSerialPort(string port)
        {
            try {
                serialPort = new SerialPort(port);
                if (serialPort != null)
                {
                    serialPort.BaudRate = 115200;
                    serialPort.Parity = Parity.None;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Handshake = Handshake.None;
                    serialPort.ReadTimeout = 500;
                    serialPort.WriteTimeout = 500;
                    serialPort.Open();

                    Thread readThread = new Thread(Read);
                    return true;
                }
            }
            catch { }
            return false;
        }


        public static void Read()
        {
            while (true)
            {
                try
                {
                    string message = serialPort.ReadLine();
                    Console.WriteLine(message);
                    parser.parseP1data(message);

                }
                catch (TimeoutException) { }
            }
        }

        private void poortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SerialPortForm f = new SerialPortForm(Properties.Settings.Default.Comport);
            if (f.ShowDialog() == DialogResult.OK)
            {
                    Properties.Settings.Default.Comport = f.getPort();
            }
            f.Dispose();
        }
    }
}
