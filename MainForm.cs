using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace P1monitor
{
    public partial class MainForm : Form
    {
        static SerialPort serialPort;
        static Parser parser = new Parser();
        Thread readThread;
        static List<string> p1Data = new List<string>();
        static List<BigTableItem> bigTableItems = new List<BigTableItem>();
        private static readonly string[]  bigTableItemNames = {
            "Opgenomen vermogen (W)",
            "Geleverd vermogen (W)",
            "Stroom L1 (A)",
            "Stroom L2 (A)",
            "Stroom L3 (A)",
        };

        static bool newData = false;
        static List<TableItem> tableItems = new List<TableItem>();   
        ChartControl chartControl1 = new ChartControl();
                

        public MainForm()
        {
            InitializeComponent();
            Height = Properties.Settings.Default.FormHeight;
            Width = Properties.Settings.Default.FormWidth;
            Top = Properties.Settings.Default.FormTop;
            Left = Properties.Settings.Default.FormLeft;
            Parser parser = new Parser();
            if (OpenSerialPort(Properties.Settings.Default.Comport) == false)
            {
                poortToolStripMenuItem_Click(null, null);
            }
            toolStripStatusLabel1.Text = "Serieele poort: " + Properties.Settings.Default.Comport;
           
            MainForm_ResizeEnd(null, null); // resize chart
            tabPage3.Controls.Add(chartControl1);
            int n = 0;
            foreach (string str in bigTableItemNames)  
            {
                BigTableItem bigTableItem = new BigTableItem(str + "; -- ", n++);
                bigTableItem.Parent = bigTablePanel;
                bigTableItems.Add(bigTableItem);
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
                    serialPort.ReadTimeout = 1500;
                    serialPort.WriteTimeout = 1500;
                    serialPort.Open();

                   // readThread = new Thread(Read);
                   // readThread.Start();
                    textBox1.AppendText ("Comport open");
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
                   List<string> list = new List<string>();
                   string message = serialPort.ReadLine();
                  //  Console.WriteLine(message);
                   list = parser.parseP1data(message);
                   if (list != null)
                   {
                        p1Data = list;
                        newData = true;
                   } 
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.FormHeight = Height;
            Properties.Settings.Default.FormWidth = Width;
            Properties.Settings.Default.FormTop = Top;
            Properties.Settings.Default.FormLeft = Left;
            //       Properties.Settings.Default.recentFilesList = string.Join(";", toolStripMruList1.RecentFileList);
            Properties.Settings.Default.Save();
        //    readThread.Join();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
           {
                List<string> list = new List<string>();
                string message = serialPort.ReadLine();
                //  Console.WriteLine(message);
                list = parser.parseP1data(message);
                if (list != null)
                {
                    p1Data = list;
                    newData = true;
                }
           }
           catch {             
           }

           if ( newData)
            {
                exportTable(p1Data);
               
                log_t lastLogValue = parser.getlastLogValue();
                chartControl1.plot(0, lastLogValue.power);
                chartControl1.plot(1, lastLogValue.deliveredPower);
                chartControl1.plot(2, lastLogValue.currentL1);
                chartControl1.plot(3, lastLogValue.currentL2);
                chartControl1.plot(4, lastLogValue.currentL3);
               
                bigTableItems[0].setValue(lastLogValue.power.ToString("0"));
                bigTableItems[1].setValue(lastLogValue.deliveredPower.ToString("0"));
                bigTableItems[2].setValue(lastLogValue.currentL1.ToString("0.0"));
                bigTableItems[3].setValue(lastLogValue.currentL2.ToString("0.0"));
                bigTableItems[3].setValue(lastLogValue.currentL3.ToString("0.0"));

                if (p1Data != null)
                {
                    foreach (string str in p1Data)
                        textBox1.AppendText(str + "\r\n");
                }
                newData = false;
            }
        }

        private void exportTable( List<string> p1Data)
        {
            int item = 0;

            if( p1Data.Count != tableItems.Count )
            {
                tableItems.Clear();
                foreach( string str in p1Data)
                {
                    TableItem tableItem = new TableItem(str, item++);
                    tableItem.Parent = tablePanel1;
                    tableItems.Add(tableItem);
                }
            }
            else
            {
                foreach (string str in p1Data)
                {
                    tableItems[item++].setText(str);
                }
            }
        }

        private void tablePanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
           Size size = new Size(tabControl1.Width, tabControl1.Height);
           chartControl1.resize(size);
        }
    }
}
