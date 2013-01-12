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
    public partial class Form3opt : Form
    {
        bool currenttour;
        int n;

        bool showprog;
        byte algo;              //1=std; 2=prec; 3=old
        byte start;             //1=seq; 2=ran;  3=nn; 4=cur; 5= chepeast Inertion
        public bool fast;

        int node0;
        public int numNN;

        //public Form3opt()
        //{
        //    InitializeComponent();
        //}

        protected void Init(bool c, int _n)
        {
            currenttour = c;
            n = _n;
            showprog = false;
            algo = 1;
            start = 4;
            node0 = 0;
            numericUpDown2.Maximum = n;
            numericUpDown1.Maximum = n;
            fast = false;
            numNN = (int)numericUpDown2.Value;
        }

        public Form3opt(bool c, int _n)
        {
            InitializeComponent();
            Init(c, _n);

            if (radioButton3.Checked)
            {
                start = 3;
                numericUpDown1.Enabled = true;
                checkBox3.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = false;
                checkBox3.Enabled = false;
            }
        }

        public int GetNode0()
        {
            return node0;
        }

        public byte GetAlgo()
        {
            return algo;
        }

        public byte GetStartTour()
        {
            return start;
        }

        public bool GetShow()
        {
            return showprog;
        }

        private void Form3opt_Load(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = n;
            start = 3;

            if (!currenttour)
            {
                radioButton4.Enabled = false;
                radioButton4.Checked = false;
                radioButton3.Checked = true;
                numericUpDown1.Enabled = true;
                checkBox3.Enabled = true;
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            start = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            start = 2;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                start = 3;
                numericUpDown1.Enabled = true;
                checkBox3.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = false;
                checkBox3.Enabled = false;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            start = 4;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            node0 = Convert.ToInt32(numericUpDown1.Value);
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            algo = 1;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            fast = !fast;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            algo = 2;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            showprog = !showprog;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numNN = (int)numericUpDown2.Value;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                node0 = -1;
                numericUpDown1.Enabled = false;
            }
            else
            {
                numericUpDown1.Enabled = true;
                node0 = (int)numericUpDown1.Value;
            }
        }

        private void radioButtonCrist_CheckedChanged(object sender, EventArgs e)
        {
            start = 6;
        }
    }
}
