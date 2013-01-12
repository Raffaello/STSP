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
    public partial class NNForm : Form
    {
        int n;
        int node0;
        bool all;
        public bool showres;

        public NNForm(int _n)
        {
            this.
            InitializeComponent();
            n = _n;
            all = false;
            node0 = 0;
            numericUpDown1.Value = 0;
            numericUpDown1.Maximum = n;
            showres = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; 
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = !label1.Visible;
            numericUpDown1.Enabled = !numericUpDown1.Enabled;
            checkBox2.Enabled = label1.Visible;
            all = !all;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if ((numericUpDown1.Value > 0) && (numericUpDown1.Value < n))
            {
                node0 = (int)numericUpDown1.Value;
            }
            else
                numericUpDown1.Value = node0;
        }

        public bool isAll()
        {
            return all;
        }
        public int GetNode0()
        {
            return node0;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            showres = !showres;
        }
    }
}

