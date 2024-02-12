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
    public partial class TableItem : UserControl
    {
       // private const int height = 40;
        
        public TableItem( string  text, int ypos)
        {
            InitializeComponent();
            Top = ypos * Height;
            Left = 10;
        
            setText(text);
        }
        public void setText ( string text)
        {
            string[] strings = text.Split(new char[] { ';' });
            tablelabel.Text = strings[0] + " :";
            textBox1.Text = strings[1];
        }
    }
}
