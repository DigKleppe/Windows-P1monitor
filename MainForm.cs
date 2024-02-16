using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace P1monitor
{
    public partial class MainForm : Form
    {

        private static readonly bool simulate = false;
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
        private static int lastHourTimeStamp = 0;
        static bool newData = false;
        static List<TableItem> tableItems = new List<TableItem>();   
        ChartControl hourChartControl = new ChartControl(3600); // 1 hour at 1 sec interval
        ChartControl dayChartControl = new ChartControl(60*24);// 1 day at 1 min interval
        simulation simulation = new simulation();

        public MainForm()
        {
            InitializeComponent();
            Height = Properties.Settings.Default.FormHeight;
            Width = Properties.Settings.Default.FormWidth;
            Top = Properties.Settings.Default.FormTop;
            Left = Properties.Settings.Default.FormLeft;
            Parser parser = new Parser();
            tabPage3.Controls.Add(hourChartControl);
            tabPage4.Controls.Add(dayChartControl);
            int n = 0;
            foreach (string str in bigTableItemNames)
            {
                BigTableItem bigTableItem = new BigTableItem(str + "; -- ", n++);
                bigTableItem.Parent = bigTablePanel;
                bigTableItems.Add(bigTableItem);
            }
            if (!simulate)
            {
                if (OpenSerialPort(Properties.Settings.Default.Comport) == false)
                {
                    poortToolStripMenuItem_Click(null, null);
                }
                toolStripStatusLabel1.Text = "Serieele poort: " + Properties.Settings.Default.Comport;
            }
            else
                toolStripStatusLabel1.Text = "Simulatie";

            MainForm_ResizeEnd(null, null); // resize chart
          
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
                   list = parser.ParseP1data(message);
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
                OpenSerialPort(Properties.Settings.Default.Comport);
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

                if (simulate)
                {
                    string[] items = simulation._3PhaseSimData[0].Split(new char[] { '\n' });

                    foreach (string s in items) {

                        list = parser.ParseP1data(s);
                    }
                }
                else
                {
                    string message = serialPort.ReadLine();
                    //  Console.WriteLine(message);
                    list = parser.ParseP1data(message);
                }
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
                          
                log_t lastLogValue = parser.GetlastLogValue();
                hourChartControl.plot(0, lastLogValue.power);
                hourChartControl.plot(1, lastLogValue.deliveredPower);
                hourChartControl.plot(2, lastLogValue.currentL1);
                hourChartControl.plot(3, lastLogValue.currentL2);
                hourChartControl.plot(4, lastLogValue.currentL3);

                bigTableItems[0].setValue(lastLogValue.power.ToString("0"));
                bigTableItems[1].setValue(lastLogValue.deliveredPower.ToString("0"));
                bigTableItems[2].setValue(lastLogValue.currentL1.ToString("0.0"));
                bigTableItems[3].setValue(lastLogValue.currentL2.ToString("0.0"));
                bigTableItems[4].setValue(lastLogValue.currentL3.ToString("0.0"));

                lastLogValue = parser.GetlastHourLogValue();
                if (lastLogValue.timeStamp != lastHourTimeStamp)
                {   dayChartControl.plot(0, lastLogValue.power);
                    dayChartControl.plot(1, lastLogValue.deliveredPower);
                    dayChartControl.plot(2, lastLogValue.currentL1);
                    dayChartControl.plot(3, lastLogValue.currentL2);
                    dayChartControl.plot(4, lastLogValue.currentL3);
                    lastHourTimeStamp = lastLogValue.timeStamp;
                }
               
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
           hourChartControl.resize(size);
        }
    }
}
