using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TSP
{
    public partial class BB_Rules : Form
    {
        public int rule { get; private set; }

        public BB_Rules()
        {
            InitializeComponent();
        }
        public BB_Rules(int r)
        {
            rule = r;
            InitializeComponent();
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            rule = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            rule = 2;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            rule = 3;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            rule = 4;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            rule = 5;
        }

        private void BB_Rules_Load(object sender, EventArgs e)
        {
            //int r = rule;
            //radioButton1.Checked = radioButton2.Checked = radioButton3.Checked = radioButton4.Checked = radioButton5.Checked = false;
            switch(rule)
            {
                case 1: radioButton1.Checked = true;
                    break;
                case 2: radioButton2.Checked = true;
                    break;
                case 3: radioButton3.Checked = true;
                    break;
                case 4: radioButton4.Checked = true;
                    break;
                case 5: radioButton5.Checked = true;
                    break;
                case 6: radioButton6.Checked = true;
                    break;
                case 7: radioButton7.Checked = true;
                    break;

            }

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            rule = 6;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            rule = 7;
        }
    }
}
