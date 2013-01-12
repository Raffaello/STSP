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
    public partial class Form2opt : Form
    {
        bool currenttour;
        int n;

        bool showprog; 
        byte algo;              //1=std; 2=prec; 3=old
        byte start;             //1=seq; 2=ran;  3=nn; 4=cur; 5= chepeast Inertion

        int node0;

        //public Form2opt()
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
        }

        public Form2opt(bool c, int _n)
        {
            InitializeComponent();
            Init(c, _n);
        }

        private void Form2opt_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(radioButton5, "Fast and standard quality tour.");
            toolTip2.SetToolTip(radioButton6, "Slow but better quality tour.");
            toolTip3.SetToolTip(radioButton7, "Old routine for adifferent result, slow.");
            toolTip4.SetToolTip(checkBox1, "Showing and updating the graph during the computation");

            numericUpDown1.Maximum = n;

            if (!currenttour)
            {
                radioButton4.Enabled = false;
                radioButton4.Checked = false;
                radioButton3.Checked = true;
                numericUpDown1.Enabled = true;
                checkBox2.Enabled = true;
                start = 3;
            }
            else
            {
                checkBox2.Enabled = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                numericUpDown1.Enabled = true;
                start = 3;
                checkBox2.Enabled = true;
            }
            else
            {
                checkBox2.Enabled = false;
                numericUpDown1.Enabled = false;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            showprog = !showprog;
            label2.Visible = showprog;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            algo = 2;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            algo = 3;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            start = 2;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            start = 1;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            start = 4;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            node0 = Convert.ToInt32(numericUpDown1.Value);            
        }

        public byte GetStartTour()
        {
            return start;
        }

        public int GetNode0()
        {
            return node0;
        }

        public bool GetShowProg()
        {
            return showprog;
        }

        public byte GetAlgo()
        {
            return algo;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            start = 5;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                numericUpDown1.Enabled = false;
                node0 = -1;
            }
            else
                numericUpDown1.Enabled = true;
        }

        private void radioButtonCrist_CheckedChanged(object sender, EventArgs e)
        {
            start = 6;
        }
    }
}
