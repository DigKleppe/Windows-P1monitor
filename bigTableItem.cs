using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P1monitor
{
    public partial class BigTableItem : UserControl 
    {
        private const int maxLines = 12;
        public BigTableItem(string text, int idx) 
        {
            InitializeComponent();
            if (idx < maxLines)
            {
                Top = idx * Height;
                Left = 10;
            }
            else
            {
                Top = idx - maxLines;  //  2 collumns
                Left = Width + 20;
            }
            setText(text);
        }
        public void setText(string text)
        {
            string[] strings = text.Split(new char[] { ';' });
            tablelabel.Text = strings[0] + " :";
            textBox1.Text = strings[1];
        }
        public void setValue ( string value)
        {
            textBox1.Text = value;
        }


    }
}
