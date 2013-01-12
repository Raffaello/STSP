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
    public partial class FormGenerate : Form
    {
        public bool usquare;
        public int n;
        public int n2, n3,n2b,n3b;

        public FormGenerate()
        {
            InitializeComponent();
        }

        private void FormGenerate_Load(object sender, EventArgs e)
        {
            n2 = (int)numericUpDown2.Value;
            n3 = (int)numericUpDown3.Value;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this.DialogResult= DialogResult.OK;
            usquare = checkBox1.Checked;
            n = (int)numericUpDown1.Value;
            n2 = (int)numericUpDown2.Value;
            n3 = (int)numericUpDown3.Value;
            n2b = (int)numericUpDown4.Value;
            n3b = (int)numericUpDown5.Value;
            this.Close();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value == 0) 
                numericUpDown2.Value = 1;
            n2 = (int)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown3.Value == 0)
                numericUpDown3.Value = 1;
            n3 = (int)numericUpDown3.Value;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            usquare = checkBox1.Checked;
            groupBox1.Enabled = !usquare;
        }
    }
}
