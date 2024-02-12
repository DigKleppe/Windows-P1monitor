using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace P1monitor
{
        public partial class ChartControl : UserControl
    {

        const int cbWidth = 150;
        const int cbYSpacing = 50;
        List<CheckBox> checkBoxes = new List<CheckBox>();
        List <Series> seriesList = new List <Series>();
                    
            
        
        public ChartControl()
        {
            InitializeComponent();
            init(chartDescrs);
        }

        private void ChartControl_Load(object sender, EventArgs e)
        {
            Width = Parent.Width-100;
            Height = Parent.Height-100;
            // Top = Parent.Top-5;
            // Left = Parent.Left-5;
            BorderStyle = BorderStyle.FixedSingle;
        }

        void init(ChartDescr[] chartDescrs)
        {
            int cbXpos = 0;
            foreach (ChartDescr c in chartDescrs)
            {
                CheckBox cb = new CheckBox();
                cb.Text = c.name;
                cb.Checked = c.enabled;
                cb.Left = 10 + cbXpos;
                cb.Width = cbWidth; 
                cbXpos += cbWidth+10;
                cb.Parent = controlPanel;
                checkBoxes.Add(cb);
                Series series = new Series(c.name);
                seriesList.Add(series);
                if ( c.yAxes == 2)
                    series.YAxisType = AxisType.Secondary;
                series.XValueType = ChartValueType.Time;
                chart1.Series.Add(series);
            }
        }
                
        public void plot(int series, double value)
        {
            seriesList[series].Points.Add(value);
        }
        public void resize ( int width)
        {
            controlPanel.Width = width;
        }

        internal class ChartDescr
        {
            public string name; // name
            public int yAxes; // special function
            public bool enabled;
            public ChartDescr(string name, int yAxes, bool enabled)
            {
                this.name = name;
                this.enabled = enabled;
                this.yAxes = yAxes;
            }
        };

        ChartDescr[] chartDescrs = {
        new ChartDescr ( "Opgenomen vermogen", 1, true),
        new ChartDescr ( "Geleverd vermogen", 1, true),
        new ChartDescr ( "Stroom L1", 2, true),
        new ChartDescr ( "Stroom L2", 2, true),
        new ChartDescr ( "Stroom L3", 2, true),
        };

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void ChartControl_AutoSizeChanged(object sender, EventArgs e)
        {
            controlPanel.Width = Width;
        }

        public void resize(Size size)
        {
            controlPanel.Size = size;
            chart1.Size = size;

        }

        private void ChartControl_Load_1(object sender, EventArgs e)
        {

        }
    }
}
