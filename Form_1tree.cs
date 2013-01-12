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
    public partial class Form_1tree : Form
    {
        int n;
        public bool best;
        public int node0;

        public Form_1tree(int _n)
        {
            InitializeComponent();
            n = _n;
            best = false;
            node0 = 0;
            numericUpDown1.Maximum = n;
        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value >= n)
                numericUpDown1.Value = n;
            if (numericUpDown1.Value < 0)
                numericUpDown1.Value = 0;

            node0 = (int)numericUpDown1.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = !numericUpDown1.Enabled;
            best = !best;
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
    }
}
