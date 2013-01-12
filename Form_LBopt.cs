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
    public partial class Form_LBopt : Form
    {
        int n;
        //public bool _3opt = true;
        bool computedTour;
        Form2opt f2opt;
        Form3opt f3opt;
        Form_1tree f1tree;
        bool compasint = true;
        //public bool UBshowprog;
        //public byte UBalgo;              //1=std; 2=prec; 3=old
        //public byte UBstart;             //1=seq; 2=ran;  3=nn; 4=cur; 5= chepeast Inertion
        //public int UBnode0;
        public clboptparam lb_opt;
        public int alpha_num=0;
        bool UBok = false;
        bool MSTok = false;
        bool LBok = true;
        bool BestLB = false;
        bool Alpha = false;
        bool bestlbok = false;

        public Form_LBopt(bool c, int _n, int alphaNN_MAX)
        {
            InitializeComponent();
            
            n = _n;
            computedTour = c;
            lb_opt = new clboptparam();
            numericUpDown1.Maximum = alphaNN_MAX;
        }

        private void Form_LBopt_Load(object sender, EventArgs e)
        {
            //radioButton2.Checked = _3opt;
            f3opt = new Form3opt(computedTour, n);
            f2opt = new Form2opt(computedTour, n);
            f1tree = new Form_1tree(n);
            SetLabel1Text();
            SetLabel2Text();
            lb_opt._3opt = true;
            button1.Enabled = false;
            label4.Text = "";
            checkBox1.Enabled = false;
            lb_opt.StrongPruning = false;
            lb_opt.std1treealg = true;
            lb_opt.use_pool_arcs = false;
            lb_opt.pool_arcs_strong = false;
            lb_opt.SubGradFast = false;
            
            lb_opt.compAsInt = compasint;
            lb_opt.sort_split = true;
            lb_opt.bb_rule = 3;
            label5_BB_Rules.Text = "BB Rule = " + lb_opt.bb_rule.ToString();
        }

        private void CheckOK()
        {
            if ((UBok) && ((MSTok)||(bestlbok)) && (LBok))
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (checkBox1.Checked)
            {
                lb_opt.LBnode0 = -2;
            }
            lb_opt.alpha_num = (int)numericUpDown1.Value;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt._3opt = !lb_opt._3opt;
            if (radioButton2.Checked)
                lb_opt._3opt = true;

            //if (radioButton2.Checked)
            //{
            //    lb_opt._3opt = true;
            //    if (f3opt.ShowDialog() == DialogResult.OK)
            //    {
            //        UBok = true;
            //        CheckOK();
            //    }
            //    else
            //    {
            //        radioButton2.Checked = false;
            //        lb_opt.UBstart = 0;
            //        button1.Enabled = false;
            //        UBok = false;
            //    }
            //}
            //SetLabel1Text();
            //SetLabel2Text();
        }

        private void SetLabel1Text()
        {
//1=seq; 2=ran;  3=nn; 4=cur; 5= chepeast Inertion
            checkBox1.Enabled = false;
            switch (lb_opt.UBstart)
            {
                case 0: label1.Text = "None.";
                    break;
                case 1: label1.Text = "Sequential.";
                    break;
                case 2: label1.Text = "Random.";
                    break;
                case 3: label1.Text = "Nearest Neighborur Node " + lb_opt.UBnode0.ToString() + ".";
                    checkBox1.Enabled = true;
                    break;
                case 4: label1.Text = "Current Tour.";
                    break;
                case 5: label1.Text = "Cheapest Insertion.";
                    break;
            }
            if (lb_opt.UBshowprog)
                label3.Text = "Showing Progress.";
            else
                label3.Text = "";
        }

        private void SetLabel2Text()
        {
            switch (lb_opt.UBalgo)
            {
                //1=std; 2=prec; 3=old
                case 0: label2.Text = "None.";
                    break;
                case 1: label2.Text = "Standard.";
                    break;
                case 2: label2.Text = "Precision.";
                    break;
                case 3: label2.Text = "Old.";
                    break;
            }
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt._3opt = !lb_opt._3opt;
            if (radioButton1.Checked)
                lb_opt._3opt = false;
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            if (f2opt.ShowDialog() == DialogResult.OK)
            {
                lb_opt.UBalgo = f2opt.GetAlgo();
                lb_opt.UBshowprog = f2opt.GetShowProg();
                lb_opt.UBstart = f2opt.GetStartTour();
                lb_opt.UBnode0 = f2opt.GetNode0();
                UBok = true;
                CheckOK();
            }
            else
            {
                radioButton1.Checked = false;
                lb_opt.UBstart = 0;
                button1.Enabled = false;
                UBok = false;
            }
            SetLabel1Text();
            SetLabel2Text();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (f1tree.ShowDialog() == DialogResult.OK)
            {
                MSTok = true;
                CheckOK();
                if (f1tree.best)
                    lb_opt.LBnode0 = -1;
                else
                    lb_opt.LBnode0 = f1tree.node0;
                label4.Text = "1-Tree Node " + lb_opt.LBnode0;
            }
            else
            {
                label4.Text = "";
                button1.Enabled = false;
                MSTok = false;
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                MSTok = true;
                label4.Enabled = false;
            }
            else
            {
                label4.Enabled = true;
                if (label4.Equals(""))
                    MSTok = false;
                else
                    MSTok = true;
            }
            CheckOK();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                lb_opt.ShowProg = true;
            else
                lb_opt.ShowProg = false;

        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            if (f3opt.ShowDialog() == DialogResult.OK)
            {
                lb_opt.UBalgo = f3opt.GetAlgo();
                lb_opt.UBshowprog = false;//f3opt.GetShowProg();
                lb_opt.UBstart = f3opt.GetStartTour();
                lb_opt.UBnode0 = f3opt.GetNode0();
                UBok = true;
                CheckOK();
            }
            else
            {
                radioButton1.Checked = false;
                lb_opt.UBstart = 0;
                button1.Enabled = false;
                UBok = false;
            }
            SetLabel1Text();
            SetLabel2Text();
        }

        private void checkBoxCompAsInt_CheckedChanged(object sender, EventArgs e)
        {
            compasint = !compasint;
            lb_opt.compAsInt = compasint;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            BestLB = !BestLB;
            lb_opt.BestLB = BestLB;
            if (BestLB == true)
                bestlbok = true;
            else
                bestlbok = false;
            CheckOK();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Alpha = !Alpha;
            lb_opt.AlphaNearness = Alpha;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.StrongPruning = !lb_opt.StrongPruning;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.std1treealg = !lb_opt.std1treealg;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.sort_split = !lb_opt.sort_split;
            if (lb_opt.sort_split == false)
            {
                checkBox7.Checked = false;
                lb_opt.strong_sort_split = false;
            }
        }

        private void button4_BB_rules_Click(object sender, EventArgs e)
        {
            int r = lb_opt.bb_rule;
            BB_Rules bbr = new BB_Rules(r);
            if (bbr.ShowDialog() == DialogResult.OK)
            {
                lb_opt.bb_rule = bbr.rule;
                label5_BB_Rules.Text = "BB Rule = " + bbr.rule.ToString();
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.strong_sort_split = !lb_opt.strong_sort_split;
            if (lb_opt.strong_sort_split)
            {
                //lb_opt.sort_split = true;
                checkBox8.Checked = true;
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.use_pool_arcs = !lb_opt.use_pool_arcs;
            checkBox10.Enabled = lb_opt.use_pool_arcs;
            
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.pool_arcs_strong = !lb_opt.pool_arcs_strong;
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            lb_opt.SubGradFast = !lb_opt.SubGradFast;
        }
    }
}
