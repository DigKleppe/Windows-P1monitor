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
        static bool newData = false;
        static List<TableItem> tableItems = new List<TableItem>();   
        ChartControl chartControl1 = new ChartControl();
       // TabPage tabPageChart = new TabPage();

       
       

        // create a series for each line
        Series series1 = new Series("Group A");
        Series series2 = new Series("Group B");

        double[] ys1;
        double[] ys2;
        double[] ys3;
        double[] ys4;
        private double x1Value = 1;
        private int x2Value = 1;

        private void buildChart ()
        {
           // series1.Points.DataBindY( ys1);
            series1.ChartType = SeriesChartType.FastLine;

            //  series2.Points.DataBindY(ys2);
            series2.ChartType = SeriesChartType.FastLine;

            // add each series to the chart
            chart1.Series.Clear();
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);

            // additional styling
            chart1.ResetAutoValues();
            chart1.Titles.Clear();
            chart1.Titles.Add($"Column Chart");
            chart1.ChartAreas[0].AxisX.Title = "Horizontal Axis Label";
            chart1.ChartAreas[0].AxisY.Title = "Vertical Axis Label";
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
        }

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
            buildChart();
            MainForm_ResizeEnd(null, null); // resize chart

         //   chartControl1.Parent = panel1;
            tabPage3.Controls.Add(chartControl1);

         //   tabControl1.Controls.Add(tabPageChart);
            

        }
        private void plot ( int series , double value)
        {   DateTime dateTime = DateTime.Now;
            
            switch (series)
            {
                case 0:
                    series1.Points.AddXY(dateTime.ToShortTimeString(), value);
                    break;
                case 1:
                    series2.Points.AddXY(x2Value++, value);
                    break;
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

        private void MainForm_Load(object sender, EventArgs e)
        {

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
                plot(0, lastLogValue.power);
                chartControl1.plot(0, lastLogValue.power);
                chartControl1.plot(1, lastLogValue.deliveredPower);
                chartControl1.plot(2, lastLogValue.currentL1);
                chartControl1.plot(3, lastLogValue.currentL2);
                chartControl1.plot(4, lastLogValue.currentL3);


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
            //        tableItem.setyPos(item);
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
