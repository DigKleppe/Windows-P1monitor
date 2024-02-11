using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P1monitor
{
    public partial class SerialPortForm : Form
    {
        private List<RadioButton> portRadioButtonList;

        public SerialPortForm(string portName)
        {
            InitializeComponent();
            portRadioButtonList = new List<RadioButton>();
            setPort(portName);
        }

        public void setPort(string portName)
        {
            bool found = false;
            InterfaceForm_Shown(this, null);
            foreach (RadioButton button in portRadioButtonList)
            {
                if (button.Text == portName)
                    button.Checked = true;
                found = true;
            }
            if (!found)
            {
                if (portRadioButtonList.Count > 0)
                    portRadioButtonList[0].Checked = true;
            }
        }

        public string getPort()
        {
            foreach (RadioButton button in portRadioButtonList)
            {
                if (button.Checked)
                    return button.Text;
            }
            return "no comport";
        }

        private void InterfaceForm_Shown(object sender, EventArgs e)
        {
            int item = 0;
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
                statusStrip1. = "Geen serieele poort gevonden!";
            else
            {
                foreach (string name in ports)
                {
                    RadioButton rb = new RadioButton();
                    portRadioButtonList.Add(rb);
                    portRadioButtonList[item].Text = name;
                    portRadioButtonList[item].Parent = portGB;
                    portRadioButtonList[item].Left = 10;
                    portRadioButtonList[item].Top = 12 + (item * 20); // portRadioButtonList[item].Heigth);
                    item++;
                }
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
