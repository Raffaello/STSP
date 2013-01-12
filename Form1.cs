using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Threading;


namespace TSP
{
    public partial class Form1 : Form
    {
        AboutBox1 a;
        Graphics g1, g2;
        SolidBrush brush = new SolidBrush(Color.SeaShell);   // nome nodi
        Pen pen = new Pen(Color.RoyalBlue, 5);
        Pen pen2 = new Pen(Color.Red, 1);                      // penna per archi
        Pen pen3 = new Pen(Color.Yellow, 2);                   //cambio archi
        Pen pen4 = new Pen(Color.White, 1);
        Pen pen5 = new Pen(Color.YellowGreen, 1);
        Font font = new Font("Arial", 10);                     // font
        //Bitmap bmp;
        cTSP tsp;

        bool sort_split = false;
        byte Mixbb_rule;
        ulong numHybrid;

        ulong BranchingLevel;
        bool switched;


        float scale, scale_norm;
        int center_x, center_y;
        int offset_x, offset_y;
        bool ShowNum = false;
        int mx, my;
        bool mousemove = false;
        bool ShowTour = true;
        bool ShowChull = false;
        bool ShowMST = true;
        bool ShowTreeNode = false;

        bool computeAsint = false;
        bool UseALPHA_NN = false;
        bool StrongPruning = false;
        int StrongSplit = 0; //se true >0

        bool _1treecomputed = false;
        Stopwatch time_elapsed;
        string form_name;

        NNThread objnn;
        int best_node0;
        int bb_rule;
        //bool std1treealg = true;

        EDGE_LIST[] lb_best;
        float lb_value_best;
        float ub_best;
        bool std1treealg;

        int d2n; //degree node 2 of lb.
        int subgrad_inc; //incremento di 1 o di 2 se sub grad fast...

        float[] tmp_p; //pesi temporanei del lb.

        List<int>[] cluster;

        SetTextCallBack stcb;
        IncremetProgress s_;
        RefreshTourCallBack refresh_;
        RefreshMSTCallBack refresh_mst;
        Bitmap buffer;
        TreeNodeAdd tnadd;
        SetStatusStripBBText sssbbtxt;


        ulong BranchTreeNodeNumber = 0;
        int e1, e2, e3, e4;
        EDGE_LIST[] edge_new;
        TreeNodeCollection tnodes;

        FormTreeView FTV;

        public Form1()
        {
            InitializeComponent();

            //g1 = panel1.CreateGraphics();
            a = new AboutBox1();
            tsp = new cTSP();
            cluster = null;
            time_elapsed = new Stopwatch();
            stcb = new SetTextCallBack(SetText);
            s_ = new IncremetProgress(IncProgBar);
            refresh_ = new RefreshTourCallBack(RefreshTour);
            refresh_mst = new RefreshMSTCallBack(RefreshMST);
            tnadd = new TreeNodeAdd(TNAdd);
            sssbbtxt = new SetStatusStripBBText(SSSBBTxt);

            buffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            g2 = Graphics.FromImage(buffer);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Visible = false;
            toolStripStatusLabel_d2n.Visible = false;
            toolStripStatusLabel4.Visible = false;
            textBox1.Text = "Progetto di Ottimizzazione Combinatoria.\r\nBranch&Bound.";
            textBox1.AppendText("\r\n");
            toolStripButton2.Enabled = false;
            //g2.Clear(Color.White);
            form_name = this.Text;
            FTV = new FormTreeView(this.Location.X + this.Width, this.Location.Y);
            FTV.treeView1.Nodes.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                showConvexHullToolStripMenuItem.Enabled = false;
                showMSTToolStripMenuItem.Enabled = false;
                solveToolStripMenuItem.Enabled = false;
                showCoToolStripMenuItem.Enabled = false;
                logTourPathToolStripMenuItem.Enabled = false;
                recalculateTourPathToolStripMenuItem.Enabled = false;
                toolStripButton2.Enabled = true;
                FTV.treeView1.Nodes.Clear();
                time_elapsed.Reset();
                time_elapsed.Start();
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel3.Visible = false;
                toolStripStatusLabel2.Visible = false;
                toolStripStatusLabel4.Visible = false;
                toolStripStatusLabel_d2n.Visible = false;
                toolStripButton11.Enabled = false;

                backgroundWorker1.RunWorkerAsync(openFileDialog1.FileName);
                this.Text = form_name + " - " + openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf('\\') + 1);
                //LoadFile(openFileDialog1.FileName);

                //time_elapsed.Stop();
                //Computing_Time_Elapsed();
                DrawGraph();
                this.Refresh();
            }
        }
        delegate void SetStatusStripBBText(string str1, string str2, string str3, string str4);
        private void SSSBBTxt(string str1, string str2, string str3, string str4)
        {
            toolStripStatusLabel4.Text = str1;
            toolStripStatusLabel3.Text = str2;
            toolStripStatusLabel2.Text = str3;
            toolStripStatusLabel_d2n.Text = str4;
        }

        delegate void TreeNodeAdd(TreeNode t);
        private void TNAdd(TreeNode t)
        {
            this.tnodes.Add(t);
        }

        delegate void IncremetProgress(int i);
        private void IncProgBar(int i)
        {
            this.toolStripProgressBar1.Increment(i);
        }

        delegate void SetProgressMaxCB(int max);
        private void SetProgressMax(int max)
        {
            this.toolStripProgressBar1.Maximum = max;
            this.toolStripProgressBar1.Value = 0;
        }


        delegate void SetTextCallBack(string str);
        private void SetText(string str)
        {
            this.textBox1.AppendText(str);
        }

        delegate void RefreshTourCallBack(string str);
        private void RefreshTour(string str)
        {
            toolStripStatusLabel2.Text = str;
            toolStripStatusLabel2.Visible = true;
            DrawGraph();
            g2.DrawLine(pen3, tsp.Get_x(e1) * scale + center_x, tsp.Get_y(e1) * scale + center_y, tsp.Get_x(e3) * scale + center_x, tsp.Get_y(e3) * scale + center_y);
            g2.DrawLine(pen3, tsp.Get_x(e2) * scale + center_x, tsp.Get_y(e2) * scale + center_y, tsp.Get_x(e4) * scale + center_x, tsp.Get_y(e4) * scale + center_y);

            this.Refresh();
        }

        delegate void RefreshMSTCallBack(string str);
        private void RefreshMST(string str)
        {
            toolStripStatusLabel3.Text = str;
            toolStripStatusLabel3.Visible = true;
            toolStripStatusLabel_d2n.Visible = true;
            DrawGraph();

            for (e1--; e1 >= 0; e1--)
            {
                g2.DrawLine(pen3, tsp.Get_x(edge_new[e1].a) * scale + center_x, tsp.Get_y(edge_new[e1].a) * scale + center_y, tsp.Get_x(edge_new[e1].b) * scale + center_x, tsp.Get_y(edge_new[e1].b) * scale + center_y);
                //       g2.DrawLine(pen3, tsp.Get_x(e2) * scale + center_x, tsp.Get_y(e2) * scale + center_y, tsp.Get_x(e4) * scale + center_x, tsp.Get_y(e4) * scale + center_y);
            }
            this.Refresh();
        }


        private void LoadFile(string nf)
        {
            FileStream fin;
            StreamReader fstr_in;
            string s;
            int n, i;
            string[] s3;

            Cursor.Current = Cursors.WaitCursor;
            n = 0;
            //tsp.Reset_Data();

            toolStripStatusLabel1.Text = "Loading data...";
            s = "\r\nReading file: " + nf;
            if (this.textBox1.InvokeRequired)
            {

                this.Invoke(stcb, s);
            }
            else
                textBox1.AppendText(s);
            try
            {
                fin = new FileStream(nf, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException)
            {
                toolStripStatusLabel1.Text = "Error!";
                s = "\r\nFile not found!!";
                if (this.textBox1.InvokeRequired)
                {
                    this.Invoke(stcb, s);
                }
                else
                    textBox1.AppendText(s);

                return;
            }
            fstr_in = new StreamReader(fin);

            //NAME
            do
            {
                s = fstr_in.ReadLine();
                if (s == null)
                    return;
                if (s.StartsWith("NAME"))
                {
                    s = "\r\nProblem name : " + s.Substring(s.IndexOf(':') + 1);
                    if (this.textBox1.InvokeRequired)
                    {
                        this.Invoke(stcb, s);
                    }
                    else
                        textBox1.AppendText(s);

                }
                //TYPE
                //s = fstr_in.ReadLine();
                else if (s.StartsWith("TYPE"))
                {
                    if (!s.Substring(s.IndexOf(':') + 1).TrimStart().StartsWith("TSP"))
                    {
                        textBox1.AppendText("\r\nNOT TSP TYPE!!!");
                        toolStripStatusLabel1.Text = "Error!";
                        fin.Close();
                        return;
                    }
                }
                //DIMENSION
                //s = fstr_in.ReadLine();
                else if (s.StartsWith("DIMENSION"))
                {
                    n = int.Parse(s.Substring(s.IndexOf(':') + 1).Trim());

                    s = "\r\nNode Number : " + n.ToString();
                    if (this.textBox1.InvokeRequired)
                    {
                        this.Invoke(stcb, s);
                    }
                    else
                        textBox1.AppendText(s);
                }

                //EDGE_WEIGHT_TYPE
                //s = fstr_in.ReadLine();
                else if (s.StartsWith("EDGE_WEIGHT_TYPE"))
                {
                    if (!s.Substring(s.IndexOf(':') + 1).Trim().StartsWith("EUC_2D"))
                    {
                        s = "\r\nCoordinates must be euclidean 2D type!!!";
                        if (this.textBox1.InvokeRequired)
                        {
                            this.Invoke(stcb, s);
                        }
                        else
                            textBox1.AppendText(s);

                        toolStripStatusLabel1.Text = "Error!";
                        fin.Close();
                        return;
                    }
                }
                //
                //eventuali commenti nel file o NODE_COORD SECTION
                //s = fstr_in.ReadLine();
                else if (s.StartsWith("COMMENT"))
                {
                    s = "\r\n" + s.Substring(s.IndexOf(':') + 1).Trim();
                    if (this.textBox1.InvokeRequired)
                    {
                        this.Invoke(stcb, s);
                    }
                    else
                        textBox1.AppendText(s);

                }
                else
                    //{
                    //while (!s.StartsWith("NODE_COORD_SECTION"))
                    //{
                    //s = fstr_in.ReadLine();
                    //if (s == null)
                    //    break;
                    //}

                    if (s == null)
                    {
                        s = "\r\nNOT TSP FILE!";
                        if (this.textBox1.InvokeRequired)
                        {
                            this.Invoke(stcb, s);
                        }
                        else
                            textBox1.AppendText(s);
                        return;
                    }
                //}
            }
            while (!s.StartsWith("NODE_COORD_SECTION"));
            //LETTURA COORDINATE
            if (toolStrip1.InvokeRequired)
            {
                SetProgressMaxCB p = new SetProgressMaxCB(SetProgressMax);
                this.Invoke(p, n);
            }
            else
                toolStripProgressBar1.Maximum = n;
            cluster = null;
            tsp = new cTSP(n);
            for (i = 0; i < n; i++)
            {
                s = fstr_in.ReadLine().TrimStart();
                s.TrimEnd();
                if (s == "")
                {
                    s = "\r\nError in TSP file!!!";
                    if (this.textBox1.InvokeRequired)
                    {
                        this.Invoke(stcb, s);
                    }
                    else
                        textBox1.AppendText(s);
                    return;
                }

                s3 = s.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

                float x,y;
                float.TryParse(s3[1], out x);
                float.TryParse(s3[2], out y);
                tsp.setNode(i, x,y);

                if (statusStrip1.InvokeRequired)
                {
                    IncremetProgress d = new IncremetProgress(IncProgBar);
                    this.Invoke(d, 1);
                }
                else
                    toolStripProgressBar1.Increment(1);
            }

            s = "\r\n File Loaded!";

            if (this.textBox1.InvokeRequired)
            {
                this.Invoke(stcb, s);
            }
            else
                textBox1.AppendText(s);

            fin.Close();
            tsp.Loaded_Data();
            s = "\r\n Computing Distances...";
            if (this.textBox1.InvokeRequired)
            {
                this.Invoke(stcb, s);
            }
            else
                textBox1.AppendText(s);

            ComputeDistances();
            s = "OK!\r\n Sorting Distances...";
            if (this.textBox1.InvokeRequired)
            {
                this.Invoke(stcb, s);
            }
            else
                textBox1.AppendText(s);

            tsp.sort_distances();
            //tsp.delta = (int)(Math.Round(Math.Sqrt(tsp.GetN()) + Math.Log10(tsp.GetN())));
            //tsp.ComputeAlphaNN();
            s = "OK!";
            if (this.textBox1.InvokeRequired)
            {
                this.Invoke(stcb, s);
            }
            else
                textBox1.AppendText(s);

            toolStripStatusLabel1.Text = "Ready.";


            scale = (this.ClientSize.Width - textBox1.ClientSize.Width) * 0.9F / (tsp.GetMax_x() - tsp.GetMin_x());

            scale_norm = (this.ClientSize.Height - toolStrip1.ClientSize.Height - statusStrip1.ClientSize.Height - menuStrip1.ClientSize.Height) * 0.9F / (tsp.GetMax_y() - tsp.GetMin_y());
            if (scale < scale_norm)
                scale_norm = scale;
            scale = scale_norm;

            center_x = center_y = 0;
            offset_x = 10;
            offset_y = menuStrip1.ClientSize.Height + toolStrip1.ClientSize.Height + 20;

            offset_x -= Convert.ToInt32(tsp.GetMin_x() * scale);
            offset_y -= Convert.ToInt32(tsp.GetMin_y() * scale);


            //DrawGraph();

            Cursor.Current = Cursors.Arrow;
        }

        private void ComputeDistances()
        {
            int n, i, j;
            float v;

            if (!tsp.isLoaded())
                return;

            n = tsp.GetN();
            //nn = n * n;
            //Computing Distances..

            for (i = 0; i < n; i++)
            {
                tsp.ComputeDistance(i, i, 0);

                for (j = i + 1; j < n; j++)
                {
                    v = tsp.ComputeDistance(i, j);
#if DEBUG
                    if (v < 0)
                        MessageBox.Show("ERROR DISTANCE!!!");
#endif

                    tsp.ComputeDistance(j, i, v);
                }
            }
            tsp.Computed_distance();
        }

        private void DrawGraph()
        {
            int i, n;
            int j1, j2;

            //g1 = panel1.CreateGraphics();

            //g = Graphics.FromImage(bmp);

            g2.Clear(Color.Black);
            if (tsp.isOK())
            {
                g2.ResetTransform();
                g2.TranslateTransform(offset_x, offset_y);

                n = tsp.GetN();
                for (i = 0; i < n; i++)
                {
                    g2.DrawEllipse(pen, tsp.Get_x(i) * scale + center_x - 2, tsp.Get_y(i) * scale + center_y - 2, 4, 4);
                    if (ShowNum)
                        g2.DrawString(i.ToString(), font, brush, tsp.Get_x(i) * scale + center_x, tsp.Get_y(i) * scale - 15 + center_y);
                }

                if (cluster != null)
                {
                    Color c = pen.Color;

                    for (i = 0; i < n; i++)
                    {
                        if (cluster[i].Count == 0)
                            continue;

                        pen.Color = Color.White;
                        byte
                            R = (byte)((128 + (cluster[i].Count * 10))),
                            G = (byte)(128 + (cluster[i].Count * 20)),
                            B = (byte)(128 + (cluster[i].Count * 40));

                        pen.Color = Color.FromArgb(R, G, B);
                        //pen.Width = widthpen;
                        foreach (int l in cluster[i])
                            g2.DrawEllipse(pen, tsp.Get_x(l) * scale + center_x - 2, tsp.Get_y(l) * scale + center_y - 2, 4, 4);

                    }
                    pen.Color = c;
                }

                if ((ShowTour) && (tsp.isToured()))
                {
                    for (i = 1, j1 = tsp.GetTourNode(0); i < n; i++)
                    {
                        j2 = tsp.GetTourNode(i);
                        //textBox1.AppendText("\r\n" + "Node " + j1.ToString() + " to " + j2.ToString());
                        g2.DrawLine(pen2, tsp.Get_x(j1) * scale + center_x, tsp.Get_y(j1) * scale + center_y, tsp.Get_x(j2) * scale + center_x, tsp.Get_y(j2) * scale + center_y);
                        j1 = j2;
                    }
                    j2 = tsp.GetTourNode(0);
                    g2.DrawLine(pen2, tsp.Get_x(j1) * scale + center_x, tsp.Get_y(j1) * scale + center_y, tsp.Get_x(j2) * scale + center_x, tsp.Get_y(j2) * scale + center_y);
                }

                if ((tsp.computed_chull) && (ShowChull))
                {
                    for (i = 1, j1 = tsp.GetChull_Node(0); i < tsp.GetNChull(); i++)
                    {
                        j2 = tsp.GetChull_Node(i);
                        g2.DrawLine(pen4, tsp.Get_x(j1) * scale + center_x, tsp.Get_y(j1) * scale + center_y, tsp.Get_x(j2) * scale + center_x, tsp.Get_y(j2) * scale + center_y);
                        j1 = j2;
                    }
                    j2 = tsp.GetChull_Node(0);
                    g2.DrawLine(pen4, tsp.Get_x(j1) * scale + center_x, tsp.Get_y(j1) * scale + center_y, tsp.Get_x(j2) * scale + center_x, tsp.Get_y(j2) * scale + center_y);

                }

                if ((tsp.computed_lb) && (ShowMST))
                {
                    for (i = 0; i < tsp.GetN(); i++)
                    {
                        g2.DrawLine(pen5, tsp.Get_x(tsp.lb[i].a) * scale + center_x, tsp.Get_y(tsp.lb[i].a) * scale + center_y,
                                          tsp.Get_x(tsp.lb[i].b) * scale + center_x, tsp.Get_y(tsp.lb[i].b) * scale + center_y);
                    }

                }
                //g2.ResetTransform();
                g2.TranslateTransform(center_x, center_y);
            }
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = !statusStrip1.Visible;
        }

        private void toolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = !toolStrip1.Visible;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            a.ShowDialog();
        }

        private void logBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Visible = !textBox1.Visible;
            splitter1.Visible = !splitter1.Visible;
            panel1.Visible = !panel1.Visible;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            g1 = e.Graphics;
            //DrawGraph();
            g1.DrawImage(buffer, 0, 0);

            //g2.Clear(Color.White);
            //g1.DrawLine(pen, 10, 10, 100, 100);

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if ((this.ClientSize.Width > 0) && (this.ClientSize.Height > 0))
            {
                buffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                g2 = Graphics.FromImage(buffer);

                scale = (this.ClientSize.Width - textBox1.ClientSize.Width) * 0.9F / (tsp.GetMax_x() - tsp.GetMin_x());
                scale_norm = (this.ClientSize.Height - toolStrip1.ClientSize.Height - statusStrip1.ClientSize.Height - menuStrip1.ClientSize.Height) * 0.9F / (tsp.GetMax_y() - tsp.GetMin_y());
                if (scale < scale_norm)
                    scale_norm = scale;
                scale = scale_norm;
                //scale_y = (panel1.Height * 0.9F) / (tsp.GetMax_y() - tsp.GetMin_y());
                DrawGraph();
                this.Refresh();
            }
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scale *= 0.9F;
            //scale_y *= 0.9F;
            DrawGraph();
            this.Refresh();
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scale *= 1.1F;
            DrawGraph();
            this.Refresh();
        }

        private void zoomNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scale = scale_norm;
            center_x = center_y = 0;
            DrawGraph();
            this.Refresh();
        }

        private void showNodeNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowNum = !ShowNum;
            if (sender != toolStripButton7)
                toolStripButton7.Checked = !toolStripButton7.Checked;
            else
                showNodeNumberToolStripMenuItem.Checked = !showNodeNumberToolStripMenuItem.Checked;
            DrawGraph();
            this.Refresh();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousemove)
            {
                center_x = e.X - mx;
                center_y = e.Y - my;
                DrawGraph();
                this.Refresh();
                //DrawGraph();
            }

            if (tsp.isLoaded())
                Cursor.Current = Cursors.Hand;

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mx = e.X - center_x;
                my = e.Y - center_y;
                mousemove = true;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousemove = false;

            }
        }

        private void tourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTour = !ShowTour;
            if (sender != toolStripButton6)
                toolStripButton6.Checked = !toolStripButton6.Checked;
            else
                tourToolStripMenuItem.Checked = !tourToolStripMenuItem.Checked;
            DrawGraph();
            this.Refresh();
        }

        private void sequentialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float v;

            tsp.Sequential_tour();
            v = tsp.Calculate_tour_length();
            toolStripStatusLabel2.Visible = true;
            toolStripStatusLabel2.Text = "Tour Length : " + v.ToString();
            DrawGraph();
            this.Refresh();
            logTourPathToolStripMenuItem.Enabled = true;
            recalculateTourPathToolStripMenuItem.Enabled = true;
            textBox1.AppendText("\r\nTour Length = " + v.ToString());
        }

        private void logTourPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i, n;


            for (i = 0, n = tsp.GetN(); i < n; i++)
            {
                textBox1.AppendText("\r\n Node " + i.ToString() + " : " + tsp.GetTourNode(i).ToString());
            }
        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float v;

            tsp.Reset_tour();
            tsp.Random_tour();
            v = tsp.Calculate_tour_length();
            toolStripStatusLabel2.Visible = true;
            toolStripStatusLabel2.Text = "Tour Length : " + v.ToString();
            textBox1.AppendText("\r\nTour Length = " + v.ToString());
            DrawGraph();
            this.Refresh();
            logTourPathToolStripMenuItem.Enabled = true;
            recalculateTourPathToolStripMenuItem.Enabled = true;


        }

        private void recalculateTourPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i, n, j1, j2, t, tmp;
            float v, d;

            toolStripStatusLabel2.Text = "Tour Length : " + tsp.Calculate_tour_length().ToString();

            for (i = 1, v = 0, t = 0, n = tsp.GetN(), j1 = tsp.GetTourNode(0); i < n; i++)
            {
                j2 = tsp.GetTourNode(i);
                d = tsp.distance[j1, j2];
                tmp = (int)(d + 0.5F);
                textBox1.AppendText("\r\n Node " + j1.ToString() + " to " + j2.ToString() +
                                    " = " + d.ToString() + " (" + tmp + ")");
                v += d;

                t += tmp;
                j1 = j2;
            }
            j2 = tsp.GetTourNode(0);
            d = tsp.distance[j1, j2];
            tmp = (int)(d + 0.5F);
            textBox1.AppendText("\r\n Node " + j1.ToString() + " to " + j2.ToString() +
                                " = " + d.ToString() + " (" + ((int)d).ToString() + ")");
            v += d;
            t += tmp;
            textBox1.AppendText("\r\n TOTAL = " + v.ToString() + " (" + t.ToString() + ")");

            if (v != tsp.Calculate_tour_length())
                textBox1.AppendText("\r\nERRORE NEL CODICE! MAH! " + tsp.Calculate_tour_length().ToString());
        }

        private void StartComputation()
        {
            _1treecomputed = false;
            time_elapsed.Reset();
            time_elapsed.Start();
            Cursor.Current = Cursors.WaitCursor;
            solveToolStripMenuItem.Enabled = false;
            fileToolStripMenuItem.Enabled = false;
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = true;
            graphToolStripMenuItem.Enabled = false;
            toolStripButton11.Enabled = false;
            toolStripSplitButton1.Enabled = false;
        }

        private void neirestNeighborurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NNForm nnform = new NNForm(tsp.GetN());

            toolStripStatusLabel1.Text = "Working...";
            if (nnform.ShowDialog() == DialogResult.OK)
            {
                tsp.Reset_tour();
                StartComputation();
                textBox1.AppendText("\r\nComputing Nearest Neighbour...");
                if (nnform.isAll())
                {
                    int node_best;
                    int n;

                    textBox1.AppendText("\r\nfor each node...");
                    n = tsp.GetN();
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Maximum = n;

                    node_best = n;
                    objnn = new NNThread(0, n, nnform.showres);
                    backgroundWorker2.RunWorkerAsync(objnn);
                }
                else
                {
                    float v;

                    textBox1.AppendText("\r\nInitial node : " + nnform.GetNode0());
                    tsp.NeirestN_tour(nnform.GetNode0());
                    textBox1.AppendText("\r\nDone.");
                    time_elapsed.Stop();
                    v = tsp.Calculate_tour_length();
                    textBox1.AppendText("\r\nTour Length = " + v.ToString());
                    toolStripStatusLabel2.Visible = true;
                    tsp.computed_tour = true;
                    toolStripStatusLabel2.Text = "Tour Length : " + v.ToString();
                    DrawGraph();
                    this.Refresh();
                    logTourPathToolStripMenuItem.Enabled = true;
                    recalculateTourPathToolStripMenuItem.Enabled = true;
                    solveToolStripMenuItem.Enabled = true;
                    toolStripButton1.Enabled = true;
                    fileToolStripMenuItem.Enabled = true;
                    toolStripButton2.Enabled = false;
                    graphToolStripMenuItem.Enabled = true;
                    Cursor.Current = Cursors.Arrow;
                    Computing_Time_Elapsed();
                }

            }
            toolStripStatusLabel1.Text = "Ready.";
        }

        //x il threadParam di Nearest neighbour

        private void ComputingNNThread(object param)
        {
            NNThread p = (NNThread)param;
            int i, n;
            float v, best;
            int node_best;

            best = float.MaxValue;
            node_best = 0;

            for (i = p.GetStart(), n = p.GetStop(); (i < n) && (!backgroundWorker2.CancellationPending); i++)
            {
                tsp.Reset_tour();
                tsp.NeirestN_tour(i);
                v = tsp.Calculate_tour_length();
                if (p.show)
                {
                    if (this.textBox1.InvokeRequired)
                    {
                        string str = "\r\nNode " + i.ToString() + " = " + v.ToString();
                        textBox1.Invoke(stcb, str);
                    }
                    else
                        textBox1.AppendText("\r\nNode " + i.ToString() + " = " + v.ToString());
                }
                if (best > v)
                {
                    best = v;
                    node_best = i;
                }
                if (toolStrip1.InvokeRequired)
                {
                    this.Invoke(s_, 1);
                }
                else
                    toolStripProgressBar1.Increment(1);

            }
            if (backgroundWorker2.CancellationPending)
            {
                if (this.textBox1.InvokeRequired)
                {
                    string str = "\r\nAborted computation!";
                    textBox1.Invoke(stcb, str);
                }
                else
                    textBox1.AppendText("\r\nAborted computation!");
            }
            p.SetBest(node_best, best);
        }

        private void Computing_Time_Elapsed()
        {
            TimeSpan elaps;
            string str;

            elaps = time_elapsed.Elapsed;
            str = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                elaps.Hours, elaps.Minutes, elaps.Seconds,
                elaps.Milliseconds / 10);

            textBox1.AppendText("\r\nTime elapsed : " + str);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadFile(e.Argument.ToString());
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            time_elapsed.Stop();
            Computing_Time_Elapsed();
            solveToolStripMenuItem.Enabled = true;
            toolStripButton2.Enabled = false;
            toolStripButton1.Enabled = true;
            toolStripButton11.Enabled = true;
            fileToolStripMenuItem.Enabled = true;
            graphToolStripMenuItem.Enabled = true;
            toolStripSplitButton1.Enabled = true;
            saveToolStripMenuItem.Enabled = true;

            DrawGraph();
            this.Refresh();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            ComputingNNThread(e.Argument);
        }

        private void StopComputation()
        {
            float v;

            time_elapsed.Stop();
            tsp.computed_tour = true;
            if ((toolStrip1.InvokeRequired) && (statusStrip1.InvokeRequired) && (menuStrip1.InvokeRequired))
                return;

            toolStripButton2.Enabled = false;
            toolStripButton1.Enabled = true;
            toolStripButton11.Enabled = true;
            toolStripSplitButton1.Enabled = true;
            toolStripStatusLabel2.Visible = true;
            toolStripStatusLabel_d2n.Visible = true;
            fileToolStripMenuItem.Enabled = true;

            v = tsp.Calculate_tour_length();
            toolStripStatusLabel2.Text = "Tour Length : " + v.ToString();
            toolStripStatusLabel1.Text = "Ready.";
            textBox1.AppendText("\r\nTour Length = " + v.ToString());
            DrawGraph();
            this.Refresh();
            logTourPathToolStripMenuItem.Enabled = true;
            recalculateTourPathToolStripMenuItem.Enabled = true;
            Cursor.Current = Cursors.Arrow;
            solveToolStripMenuItem.Enabled = true;
            graphToolStripMenuItem.Enabled = true;
            UseALPHA_NN = false;
            Computing_Time_Elapsed();
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopComputation();
            tsp.Reset_tour();
            tsp.NeirestN_tour(objnn.GetBestNode());
            textBox1.AppendText("\r\nBest node is " + objnn.GetBestNode().ToString() + " length = " + objnn.GetBestValue().ToString());
            textBox1.AppendText("\r\nDone.");
            toolStripStatusLabel2.Text = "Tour Length : " + objnn.GetBestValue().ToString();
            tsp.computed_tour = true;
            DrawGraph();
            this.Refresh();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((backgroundWorker1.IsBusy) ||
                (backgroundWorker2.IsBusy) ||
                  (backgroundWorker3.IsBusy) ||
                  (backgroundWorker4.IsBusy) ||
                  (backgroundWorker5_lb.IsBusy) ||
                (backgroundWorker6.IsBusy) ||
                (backgroundWorker5.IsBusy))
            {
                e.Cancel = true;
                MessageBox.Show("You must Abort current operation!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            backgroundWorker2.CancelAsync();
            backgroundWorker3.CancelAsync();
            backgroundWorker4.CancelAsync();
            backgroundWorker5_lb.CancelAsync();
            backgroundWorker6.CancelAsync();
            backgroundWorker5.CancelAsync();
        }

        private void cheapestInsertionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float v;

            toolStripStatusLabel1.Text = "Working...";
            textBox1.AppendText("\r\nComputing Cheapest Insertion...");
            tsp.CheapestInsertion();
            v = tsp.Calculate_tour_length();
            toolStripStatusLabel2.Visible = true;
            toolStripStatusLabel2.Text = "Tour Length : " + v.ToString();
            DrawGraph();
            this.Refresh();
            logTourPathToolStripMenuItem.Enabled = true;
            recalculateTourPathToolStripMenuItem.Enabled = true;
            textBox1.AppendText("\r\nTour Length = " + v.ToString());
            toolStripStatusLabel1.Text = "Ready.";
        }

        private void optToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2opt f2opt = new Form2opt(tsp.computed_tour, tsp.GetN());


            if (DialogResult.OK == f2opt.ShowDialog())
            {
                toolStripStatusLabel1.Text = "Working...";

                toolStripProgressBar1.Maximum = tsp.GetN();
                toolStripProgressBar1.Value = 0;
                StartComputation();

                c2optparam obj = new c2optparam(f2opt.GetNode0(), f2opt.GetAlgo(), f2opt.GetStartTour(), f2opt.GetShowProg());
                backgroundWorker3.RunWorkerAsync(obj);
            }

        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            c2optparam obj = e.Argument as c2optparam;

            if ((obj.start_node == -1) && (obj.itour == 3))
            {
                int i;
                float best = float.MaxValue;
                int nodebest = 0;
                float v;

                for (i = 0; ((i < tsp.GetN()) && (!bw.CancellationPending)); i++)
                {
                    obj.start_node = i;
                    Compute2opt(obj, sender);
                    if ((v = tsp.Calculate_tour_length()) < best)
                    {
                        best = v;
                        nodebest = i;
                    }

                    //backgroundWorker3.ReportProgress(1);
                    if (toolStrip1.InvokeRequired)
                    {
                        IncremetProgress d = new IncremetProgress(IncProgBar);
                        toolStrip1.Invoke(d, 1);
                    }
                    else toolStripProgressBar1.Increment(1);
                }

                //Best Tour
                obj.start_node = nodebest;
                if (textBox1.InvokeRequired)
                {
                    this.Invoke(stcb, new object[] { "\r\nBest Node : " + nodebest.ToString() });

                }
                else
                    textBox1.AppendText("\r\nBest Node : " + best_node0.ToString());
                Compute2opt(obj, sender);
            }
            else
                Compute2opt(e.Argument, sender);
        }

        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopComputation();
            DrawGraph();
            if (this.InvokeRequired)
            {
                this.Invoke(refresh_, new object[] { "" });
            }
            else
                this.Refresh();
        }

        private void Compute2opt(object param, object sender)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            float v = 0;
            c2optparam p = (c2optparam)param;
            string s;
            switch (p.itour)
            {
                case 1: tsp.Sequential_tour();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nSequential Tour = " + v.ToString();
                    //                        textBox1.AppendText("\r\nSequential Tour = " + v.ToString());
                    break;
                case 2: tsp.Random_tour();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nRandom Tour = " + v.ToString();
                    //                    textBox1.AppendText("\r\nRandom Tour = " + v.ToString());
                    break;
                case 3: tsp.NeirestN_tour(p.start_node);
                    v = tsp.Calculate_tour_length();
                    s = "\r\nNearest Neighbour node " + p.start_node.ToString();
                    break;
                case 4: v = tsp.Calculate_tour_length();
                    s = "\r\nCurrent Initial tour : " + v.ToString();
                    //                    textBox1.AppendText("\r\nCurrent Initial tour : " + v);
                    break;
                case 5: //CheapestInsertion
                    tsp.CheapestInsertion();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nCheapest Insertion = " + v.ToString();
                    break;
                case 6: //cristofides
                    tsp.Christofides();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nChristofides = " + v.ToString();
                    break;
                default: s = "";
                    break;
            }
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, s);
            else
                textBox1.AppendText(s);

            tsp.computed_tour = true;

            if (p.show)
            {
                e1 = e2 = e3 = e4 = 0;
                s = "Tour length : " + v.ToString();
                if (toolStrip1.InvokeRequired)
                {
                    this.Invoke(refresh_, s);
                }
                else
                {
                    toolStripStatusLabel2.Text = s;
                    toolStripStatusLabel2.Visible = true;
                    DrawGraph();
                    this.Refresh();
                }
            }

            switch (p.algo)
            {
                case 1: s = "\r\nUsing Standard 2-opt routine...";
                    break;
                case 2: s = "\r\nUsing Precision 2-opt routine...";
                    break;
                case 3: s = "\r\nUsing Old 2-opt routine...";
                    break;
                default: s = "";
                    break;
            }
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, s);
            else
                textBox1.AppendText(s);

            bool ret;
            c2opt _2opt = new c2opt(tsp, p.algo, p.show);

            do
            {
                ret = _2opt.do2opt();
                if ((ret) && (p.show))
                {
                    s = "Tour length : " + tsp.UpdateTourLength(_2opt.diff).ToString();
                    if (toolStrip1.InvokeRequired)
                    {
                        e1 = _2opt.e1;
                        e2 = _2opt.e2;
                        e3 = _2opt.e3;
                        e4 = _2opt.e4;
                        this.Invoke(refresh_, s);
                    }
                    else
                    {
                        toolStripStatusLabel2.Text = s;
                        toolStripStatusLabel2.Visible = true;
                        DrawGraph();

                        g2.DrawLine(pen3, tsp.Get_x(_2opt.e1) * scale + center_x, tsp.Get_y(_2opt.e1) * scale + center_y,
                                    tsp.Get_x(_2opt.e3) * scale + center_x, tsp.Get_x(_2opt.e3) * scale + center_y);
                        g2.DrawLine(pen3, tsp.Get_x(_2opt.e2) * scale + center_x, tsp.Get_y(_2opt.e2) * scale + center_y,
                                    tsp.Get_x(_2opt.e4) * scale + center_x, tsp.Get_x(_2opt.e4) * scale + center_y);
                        this.Refresh();


                    }

                    s = "\r\n2opt Swap = (" + _2opt.e1 + "," + _2opt.e2 + ") ---> (" + _2opt.e1 + "," + _2opt.e3 + ") ; (" +
                                       _2opt.e3 + "," + _2opt.e4 + ") ---> (" + _2opt.e2 + "," + _2opt.e4 + ")";
                    if (textBox1.InvokeRequired)
                        textBox1.Invoke(stcb, s);
                    else
                        textBox1.AppendText(s);
                }
            } while ((_2opt.GetResult()) && (!bw.CancellationPending));



        }

        private void Compute3opt(object param, object sender)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
#if DEBUG
            if (bw == null)
                bw = new BackgroundWorker();
#endif
            float v = 0;
            c3optparam p = (c3optparam)param;
            string s;
            switch (p.itour)
            {
                case 1: tsp.Sequential_tour();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nSequential Tour = " + v.ToString();
                    //                        textBox1.AppendText("\r\nSequential Tour = " + v.ToString());
                    break;
                case 2: tsp.Random_tour();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nRandom Tour = " + v.ToString();
                    //                    textBox1.AppendText("\r\nRandom Tour = " + v.ToString());
                    break;
                case 3: tsp.NeirestN_tour(p.start_node);
                    v = tsp.Calculate_tour_length();
                    s = "\r\nNearest Neighbour node " + p.start_node.ToString();
                    break;
                case 4: v = tsp.Calculate_tour_length();
                    s = "\r\nCurrent Initial tour : " + v.ToString();
                    //                    textBox1.AppendText("\r\nCurrent Initial tour : " + v);
                    break;
                case 5: //CheapestInsertion
                    tsp.CheapestInsertion();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nCheapest Insertion = " + v.ToString();
                    break;
                case 6: //cristofides...
                    tsp.Christofides();
                    v = tsp.Calculate_tour_length();
                    s = "\r\nCristofides = " + v.ToString();
                    break;
                default: s = "";
                    break;
            }
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, s);
            else
                textBox1.AppendText(s);

            tsp.computed_tour = true;

            if (p.show)
            {
                e1 = e2 = e3 = e4 = 0;
                s = "Tour length : " + v.ToString();
                if (toolStrip1.InvokeRequired)
                    this.Invoke(refresh_, s);
                else
                {
                    toolStripStatusLabel2.Text = s;
                    toolStripStatusLabel2.Visible = true;
                    DrawGraph();
                    this.Refresh();
                }
            }

            switch (p.algo)
            {
                case 1: s = "\r\nUsing Standard 3-opt routine...";
                    break;
                case 2: s = "\r\nUsing Precision 3-opt routine...";
                    break;
                //case 3: s = "\r\nUsing Old 2-opt routine...";
                //    break;
                default: s = "";
                    break;
            }
            //s += "(Candidate Set = " + p.numNN.ToString() + " Nearest Neighbour)";
            //if (textBox1.InvokeRequired)
            //    textBox1.Invoke(stcb, s );
            //else
            //    textBox1.AppendText(s);

            bool ret;

            c3opt _3opt = new c3opt(tsp, p.algo, p.show, p.numNN, p.fast);

            do
            {
                ret = _3opt.do3opt();
                if ((ret) && (p.show))
                {
                    s = "Tour length : " + tsp.UpdateTourLength(_3opt.diff).ToString();
                }
            } while ((ret) && (!bw.CancellationPending));


            s = "\r\nInteger Solution = " + tsp.Calculate_int_tour_length();
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, s);
            else
                textBox1.AppendText(s);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            zoomInToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            zoomNormalToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            zoomOutToolStripMenuItem_Click(sender, e);
        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            scale = (this.ClientSize.Width - textBox1.ClientSize.Width) * 0.9F / (tsp.GetMax_x() - tsp.GetMin_x());
            scale_norm = (this.ClientSize.Height - toolStrip1.ClientSize.Height - statusStrip1.ClientSize.Height - menuStrip1.ClientSize.Height) * 0.9F / (tsp.GetMax_y() - tsp.GetMin_y());
            if (scale < scale_norm)
                scale_norm = scale;
            scale = scale_norm;
            DrawGraph();
            this.Refresh();

        }

        private void convexHullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int nhull;

            nhull = tsp.ConvexHull();
            showConvexHullToolStripMenuItem.Enabled = true;
            showCoToolStripMenuItem.Enabled = true;
            textBox1.AppendText("\r\nConvex Hull number of nodes : " + nhull.ToString());
            if (ShowChull)
            {
                DrawGraph();
                this.Refresh();
            }
        }

        private void showConvexHullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowChull = !ShowChull;
            if (sender != toolStripButton8)
                toolStripButton8.Checked = !toolStripButton8.Checked;
            else
                showConvexHullToolStripMenuItem.Checked = !showConvexHullToolStripMenuItem.Checked;
            DrawGraph();
            this.Refresh();
        }

        private void showCoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i;

            for (i = 0; i < tsp.GetNChull(); i++)
                textBox1.AppendText("\r\nNode : " + tsp.GetChull_Node(i).ToString());
        }

        private void optToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form3opt f3opt = new Form3opt(tsp.computed_tour, tsp.GetN());

            if (f3opt.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = "Working...";

                toolStripProgressBar1.Maximum = tsp.GetN();
                toolStripProgressBar1.Value = 0;
                StartComputation();

                c3optparam p = new c3optparam(f3opt.GetNode0(), f3opt.GetAlgo(), f3opt.GetStartTour(), f3opt.GetShow(), f3opt.numNN, f3opt.fast);
                backgroundWorker5.RunWorkerAsync(p);

            }
        }

        private void mSTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float v;

            StartComputation();
            v = tsp.Compute_MST();
            time_elapsed.Stop();
            toolStripButton2.Enabled = false;
            fileToolStripMenuItem.Enabled = true;
            toolStripButton1.Enabled = true;
            toolStripStatusLabel1.Text = "Ready.";
            toolStripStatusLabel3.Text = "MST : " + v.ToString();
            toolStripStatusLabel3.Visible = true;
            Cursor.Current = Cursors.Arrow;
            solveToolStripMenuItem.Enabled = true;
            graphToolStripMenuItem.Enabled = true;
            showMSTToolStripMenuItem.Enabled = true;
            Computing_Time_Elapsed();
            textBox1.AppendText("\r\n MST = " + v);
            DrawGraph();
            this.Refresh();
        }

        private void treeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_1tree ft = new Form_1tree(tsp.GetN());

            if (ft.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = "Working...";
                std1treealg = true;
                if (ft.best)
                {
                    StartComputation();
                    toolStripProgressBar1.Maximum = tsp.GetN();
                    toolStripProgressBar1.Value = 0;
                    backgroundWorker4.RunWorkerAsync();
                }
                else
                {
                    float v;

                    time_elapsed.Reset();
                    time_elapsed.Start();
                    Cursor.Current = Cursors.WaitCursor;
                    solveToolStripMenuItem.Enabled = false;
                    fileToolStripMenuItem.Enabled = false;
                    toolStripButton1.Enabled = false;
                    graphToolStripMenuItem.Enabled = false;
                    if (ft.node0 > tsp.GetN())
                        ft.node0 = tsp.GetN() - 1;
                    textBox1.AppendText("\r\nComputing 1-Tree node : " + ft.node0);
                    v = tsp.Compute_1_tree_old(ft.node0, tsp.distance, null);
                    textBox1.AppendText("\r\nLength = " + v.ToString());
                    time_elapsed.Stop();
                    toolStripButton2.Enabled = false;
                    fileToolStripMenuItem.Enabled = true;
                    toolStripButton1.Enabled = true;
                    toolStripStatusLabel1.Text = "Ready.";
                    Cursor.Current = Cursors.Arrow;
                    solveToolStripMenuItem.Enabled = true;
                    graphToolStripMenuItem.Enabled = true;
                    toolStripStatusLabel3.Text = "1-Tree : " + v.ToString();
                    showMSTToolStripMenuItem.Enabled = true;
                    toolStripStatusLabel3.Visible = true;
                    Computing_Time_Elapsed();
                    DrawGraph();
                    this.Refresh();
                }
            }
        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            //1-tree
            float best = float.MaxValue;
            float v;
            int i, nodei;
            string str;

            for (i = nodei = 0; ((i < tsp.GetN()) && (!bw.CancellationPending)); i++)
            {
                if (std1treealg)
                    v = tsp.Compute_1_tree_old(i, tsp.distance, null);
                else
                    v = tsp.Compute_1_tree(i, tsp.distance, null, false);
                str = "\r\n1-Tree Node " + i.ToString() + " = " + v.ToString();
                if (textBox1.InvokeRequired)
                    textBox1.Invoke(stcb, str);
                else
                    textBox1.AppendText(str);
                //backgroundWorker4.ReportProgress(0);
                if (statusStrip1.InvokeRequired)
                {
                    IncremetProgress d = new IncremetProgress(IncProgBar);
                    statusStrip1.Invoke(d, 1);
                }
                else
                    toolStripProgressBar1.Increment(1);

                if (v < best)
                {
                    best = v;
                    nodei = i;
                }
            }
            best_node0 = nodei;
        }

        private void backgroundWorker4_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            float v;

            time_elapsed.Stop();
            toolStripButton2.Enabled = false;
            fileToolStripMenuItem.Enabled = true;
            toolStripButton1.Enabled = true;
            toolStripSplitButton1.Enabled = true;
            toolStripStatusLabel1.Text = "Ready.";
            Cursor.Current = Cursors.Arrow;
            solveToolStripMenuItem.Enabled = true;
            graphToolStripMenuItem.Enabled = true;
            textBox1.AppendText("\r\nComputing 1-Tree Best Initial Node : " + best_node0);
            if (std1treealg)
                v = tsp.Compute_1_tree_old(best_node0, tsp.distance, null);
            else
                v = tsp.Compute_1_tree(best_node0, tsp.distance, null, false);
            textBox1.AppendText("\r\nLength = " + v.ToString());
            toolStripStatusLabel3.Text = "1-Tree : " + v.ToString();
            toolStripStatusLabel3.Visible = true;
            showMSTToolStripMenuItem.Enabled = true;
            Computing_Time_Elapsed();
            DrawGraph();
            this.Refresh();

        }

        private void backgroundWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void showMSTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMST = !ShowMST;
            if (sender != toolStripButton9)
                toolStripButton9.Checked = !toolStripButton9.Checked;
            else
                showMSTToolStripMenuItem.Checked = !showMSTToolStripMenuItem.Checked;
            DrawGraph();
            this.Refresh();
        }

        private void lowerBoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_LBopt flb = new Form_LBopt(tsp.computed_tour, tsp.GetN(), tsp.ALPHA_NN_MAX);
            byte b = 1; ;

            if (flb.ShowDialog() == DialogResult.OK)
            {
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = tsp.GetN();
                textBox1.AppendText("\r\nComputing LB.....");
                //c2lboptparam lb_opt = flb.lb_opt;
                StartComputation();


                if (flb.lb_opt.UBnode0 == -1)
                    b++;
                if (flb.lb_opt.LBnode0 == -1)
                    b++;

                toolStripProgressBar1.Maximum = tsp.GetN() * b;
                toolStripProgressBar1.Value = 0;
                backgroundWorker5_lb.RunWorkerAsync(flb.lb_opt);
            }
        }

        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorker5_lb_DoWork(object sender, DoWorkEventArgs e)
        {
            string str;
            float ub, lb;

            //clboptparam lb_opt = (clboptparam)e.Argument;
            bool tour;
            clboptparam lb_opt = e.Argument as clboptparam;

            tsp.computed_lb = false;
            std1treealg = lb_opt.std1treealg;
            subgrad_inc = 1;

            str = "\r\nComputing Upper Bound...";
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, str);
            else
                textBox1.AppendText(str);

            if (lb_opt.compAsInt)
                tsp.SetIntDistanceMatrix();


            if (lb_opt._3opt)
            {
                //computing 3-opt
                c3optparam obj = new c3optparam(lb_opt.UBnode0, lb_opt.UBalgo, lb_opt.UBstart, lb_opt.UBshowprog, 0, false);
                DoWorkEventArgs d = new DoWorkEventArgs(obj);

                backgroundWorker5_DoWork(sender, d);

                if (lb_opt.UBnode0 == -1)
                    lb_opt.UBnode0 = obj.start_node;
            }
            else
            {
                //computin 2-opt;
                c2optparam obj = new c2optparam(lb_opt.UBnode0, lb_opt.UBalgo, lb_opt.UBstart, lb_opt.UBshowprog);
                DoWorkEventArgs d = new DoWorkEventArgs(obj);

                backgroundWorker3_DoWork(sender, d);
                //backgroundWorker3.RunWorkerAsync(obj);
                //while (backgroundWorker3.IsBusy) ;

                if (lb_opt.UBnode0 == -1)
                    lb_opt.UBnode0 = obj.start_node;

            }
            //calcolo del valore di upper bound appena trovato...
            if (lb_opt.compAsInt)
            {
                computeAsint = true;
                ub = tsp.Calculate_int_tour_length();
            }
            else
            {
                computeAsint = false;
                ub = tsp.Calculate_tour_length();
            }

            str = "\r\nUpper Bound Value = " + ub.ToString();
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, str);
            else
                textBox1.AppendText(str);

            // Computing lower bound from 1-tree...


            if (lb_opt.BestLB)
            {
                int i, nodei;
                float v, best;
                int best_d2n = 0;
                best = float.MinValue;
                float min12_best = float.MaxValue;
                float min12_cur;
                int N=tsp.GetN();
                //1-tree e lb
                for (i = nodei = 0; ((i < N)); i++)
                {
                    //if(std1treealg)
                    v = Compute_lb(i, ub, lb_opt.ShowProg, null, out tour, sender, null);
                    //else
                    //    v = Compute_lb_alt(i, ub, lb_opt.ShowProg, null, out tour, sender, null);
                    min12_cur = tsp.distance[tsp.lb[N - 1].a, tsp.lb[N - 1].b] +
                                tsp.distance[tsp.lb[N - 2].a, tsp.lb[N - 2].b];
                    if (tour) /*(d2n == N)*/ /*&& (best_d2n <= d2n))*/
                    //ottimo trovato (raro)
                    {
                        //se è un tour è la sol ott.... quindi è sufficente tenere questa e finire il tutto...
                        //semplificando oltretutto anche questo algoritmo.
                        
                        if ((best_d2n == d2n) && (v < best))
                        {
                            nodei = i;
                            best = v;
                            best_d2n = d2n;
                            min12_best = min12_cur;
                        }
                        else if (best_d2n < d2n)
                        {
                            nodei = i;
                            best = v;
                            best_d2n = d2n;
                            min12_best = min12_cur;
                        }

                        //si potrebbe interrompere qui... si continua per "illustrare" meglio l'algoritmo (scopo didattico).
                        //break;
                    }
                    else if (d2n > best_d2n)
                    {
                        nodei = i;
                        best = v;
                        best_d2n = d2n;
                        min12_best = min12_cur;
                    }

                    //modificato da == a <= ovvero si da priorità all'arco inferiore se non è maggiore di nodi.
                    
                    //else if ((d2n == best_d2n) /*&& (v > best)*/)
                    //{
                        //bisogna usare le distanze modifcate dai pesi lagrangiani per migliorare questa scelta!!!
                        //per far ciò estrapolare gli ultimi 2 archi dalla soluzione, relative ad i.
                        //gli altri, quelli del best tenerli salvati in una var float.
                        //if (tsp.distance[nodei, tsp.sort_dist[nodei, 1]] + tsp.distance[nodei, tsp.sort_dist[nodei, 2]] > 
                        //         tsp.distance[i, tsp.sort_dist[i, 1]] + tsp.distance[i, tsp.sort_dist[i, 2]])
                                                     //se la diff è maggiore del 0.005% --> 0.005/100=0.00005
                    else if((min12_best > min12_cur) &&((best*0.99995)<v))
                        {
                            nodei = i;
                            best = v;
                            best_d2n = d2n;
                            min12_best = min12_cur;
                        }
                        //else if ((v > best) &&(min12_cur==min12_best))
                        //{
                        //    nodei = i;
                        //    best = v;
                        //    best_d2n = d2n;
                        //    min12_best = min12_cur;
                        //}
                    

                    str = "\r\nLB Node " + i.ToString() + " = " + v.ToString() + " (2°node= " + d2n + ")";
                    if (tour)
                        str += " (tour)";

                    if (textBox1.InvokeRequired)
                        textBox1.Invoke(stcb, str);
                    else
                        textBox1.AppendText(str);

                    if (statusStrip1.InvokeRequired)
                    {
                        IncremetProgress d = new IncremetProgress(IncProgBar);
                        this.Invoke(d, 1);
                    }
                    else
                        toolStripProgressBar1.Increment(1);
                }
                best_node0 = nodei;
                lb_opt.LBnode0 = best_node0;

                //if(std1treealg)
                v = Compute_lb(nodei, ub, lb_opt.ShowProg, null, out tour, sender, null);
                //else
                //    v = Compute_lb_alt(nodei, ub, lb_opt.ShowProg, null, out tour, sender, null);
                str = "\r\nBest LB Node = " + nodei.ToString() + " (2°node= " + d2n + ")";
                lb = v;
                textBox1.Invoke(stcb, str);

            }
            else
            {
                if (lb_opt.LBnode0 == -1)
                {
                    backgroundWorker4_DoWork(sender, null);
                    lb_opt.LBnode0 = best_node0;
                    str = "\r\n Best 1-Tree Node = " + best_node0.ToString();
                    if (textBox1.InvokeRequired)
                        textBox1.Invoke(stcb, str);
                    else
                        textBox1.AppendText(str);

                }
                else if (lb_opt.LBnode0 == -2)
                    lb_opt.LBnode0 = lb_opt.UBnode0;

                str = "\r\nComputing Lower Bound...";
                if (textBox1.InvokeRequired)
                    textBox1.Invoke(stcb, str);
                else
                    textBox1.AppendText(str);
                //if(std1treealg)
                lb = Compute_lb(lb_opt.LBnode0, ub, lb_opt.ShowProg, null, out tour, sender, null);
                //else
                //    lb = Compute_lb_alt(lb_opt.LBnode0, ub, lb_opt.ShowProg, null, out tour, sender, null);
            }
            //lb = tsp.CalculateLBLength();
            //if(lb_opt.compAsInt)
            //    str = "\r\nLower Bound Value = " + lb.ToString() + "(" + tsp.CalculateLBintLength().ToString() + ")\r\n";
            //else
            str = "\r\nLower Bound Value = " + lb.ToString() + "(" + tsp.CalculateLBLength().ToString() + ")";
            toolStripStatusLabel_d2n.Text = "Node 2° : " + d2n.ToString();

            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, str);
            else
                textBox1.AppendText(str);

            if (tsp.computed_lb)
                lb_best = tsp.lb;

            e.Result = lb;

            //if (lb_opt.compAsInt)
            //    tsp.ResetDistanceMatrix();
            //e.Argument = lb_opt;

        }

        public float Compute_lb_alt(int node0, float ub, bool show, EDGE_LIST[] ex_edges, out bool istourok, object sender, float[] p)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            int i, j, m, n1, M = tsp.GetN(); ;
            float lambda;

            float sum_p;
            float sum_degree;
            float lb = 0;
            float LB = float.MinValue;
            int period, period_start;
            bool init = true;

            int[] lastDegreenode = new int[M];
            float[] p_best = new float[M];

            float[,] old_dist = new float[M, M];
            string str;
            EDGE_LIST[] old = new EDGE_LIST[M];

            istourok = false;
            //copio le distanze...
            Array.Copy(tsp.GetDistanceMatrix(), old_dist, old_dist.Length);

            // ciclo per n volte
            sum_p = d2n = 0;
            if (p == null)
                p = new float[M];
            else for (i = 0; i < M; i++)
                    sum_p += p[i];

            n1 = M - 1;

            if (show)
                edge_new = new EDGE_LIST[tsp.GetN()];
            period_start = M / 2;

            //Calcolo il 1-tree lb
            if (UseALPHA_NN)
                lb = tsp.Compute_1_treeAlpha(node0, old_dist, ex_edges);
            else
                lb = tsp.Compute_1_tree(node0, old_dist, ex_edges, false);
            for (period = period_start, lambda = 1; period > 0 && lambda > 0; period /= 2, lambda /= 2)
            {
                //aggiustamento pesi.
                for (m = 0; ((m < period) && (!bw.CancellationPending)); m++)
                {
                    // calcolo il grado dei nodi
                    d2n = tsp.Compute_Degree_Node();
                    if (d2n == M)
                        break;
                    // calcolo la sommatoria del grado dei nodi
                    sum_degree = tsp.SumDegree();

                    //calcolo p 
                    for (i = 0, sum_p = 0; i < M; i++)
                    {
                        if (tsp.degree_node[i] != 2)
                        {
                            p[i] += lambda * (7 * tsp.degree_node[i] + lastDegreenode[i] * 3) / 10;
                            sum_p += p[i];
                        }
                    }
                    lastDegreenode = (int[])tsp.degree_node.Clone();
                    // modifico le distanze in base al grado dei nodi.
                    for (i = 0; i < n1; i++)
                    {
                        for (j = i + 1; j < M; j++)
                        {
                            old_dist[i, j] = tsp.distance[i, j] + (p[i] + p[j]);
                            old_dist[j, i] = old_dist[i, j];
                        }
                    }

                    //Calcolo il 1-tree lb
                    if (UseALPHA_NN)
                        lb = tsp.Compute_1_treeAlpha(node0, old_dist, ex_edges);
                    else
                        lb = tsp.Compute_1_tree(node0, old_dist, ex_edges, false);

                    if (lb > LB)
                    {
                        LB = lb;
                        Array.Copy(p, p_best, M);

                        if ((init) && (lambda * Math.Sqrt(sum_degree) > 0))
                            lambda *= 2;

                        if ((m == period) &&
                           ((period *= 2) > period_start))
                            period = period_start;
                    }
                    else if ((init) &&
                            (m > period / 2))
                    {
                        init = false;
                        m = 0;
                        lambda = 3 * lambda / 4;
                    }

                    //copio se necessario il vecchio lb...
                    if (show)
                        Array.Copy(tsp.lb, old, tsp.lb.Length);


                    // fine ciclo
                    if (show)
                    {
                        int k = 0;

                        for (i = 0; i < M; i++)
                        {
                            for (j = 0; j < M; j++)
                            {
                                if ((old[j].a == tsp.lb[i].a) && (old[j].b == tsp.lb[i].b))
                                    break;
                            }
                            //se non c'è
                            if (j == M)
                                edge_new[k++] = tsp.lb[i];
                        }

                        str = "Lower Bound : " + tsp.CalculateLBLength().ToString();
                        if (this.InvokeRequired)
                        {
                            e1 = k;
                            this.Invoke(refresh_mst, str);
                        }
                        //Stampo Il risultato
                        str = "\r\n" + m.ToString() + ". Lower Bound = " + lb.ToString() + " (" + tsp.CalculateLBLength().ToString() + ")";
                        if (textBox1.InvokeRequired)
                            textBox1.Invoke(stcb, str);
                        else
                            textBox1.AppendText(str);

                    }

                }
            }

            Array.Copy(p_best, p, M);
            if (UseALPHA_NN)
                lb = tsp.Compute_1_treeAlpha(node0, old_dist, ex_edges);
            else
                lb = tsp.Compute_1_tree(node0, old_dist, ex_edges, false);

            d2n = tsp.Compute_Degree_Node();

            //Se ho trovato la soluzione ottima...
            if (d2n == M)
                istourok = true;


            tmp_p = p; //copio in p per il passaggio...

            return lb;
            //return tsp.CalculateLBLength();
        }

        public float Compute_lb(int node0, float ub, bool show, EDGE_LIST[] ex_edges, out bool istourok, object sender, float[] p)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
#if DEBUG
            if (bw == null)
                bw = new BackgroundWorker();
#endif

            int i, j, m, n1, M = tsp.GetN();
            float lambda = 0;
            //int best_d2n;
            float sum_p;
            float sum_degree;
            float lb = 0;
            float[] best_p;

            float[,] old_dist = new float[M, M];
            string str;
            EDGE_LIST[] old = new EDGE_LIST[M];
            //float lambda_old;
            istourok = false;
            //copio le distanze...
            Array.Copy(tsp.GetDistanceMatrix(), old_dist, old_dist.Length);

            // ciclo per n volte
            sum_p = d2n = 0;
            if (p == null)
                p = new float[M];
            else
                for (i = 0; i < M; i++)
                    sum_p += p[i];

            n1 = M - 1;

            if (show)
                edge_new = new EDGE_LIST[M];

            //prima del ciclo costruire le coppie d'archi per kruskal.
            //lo commento perchè è più veloce se costruiti per ogni iterazione. 
            //sarà a causa del caso peggiore del quicksort sugli archi.... (SICURAMENTE)
            //ovvero è molto più lento ad ordinare gli archi se si pre-initializzano.
            //se invece ogni volta si ricostruisce sempre lo stesso set, è più veloce ad ordinarli
            //in conclusione c'è una differenza di un ordine di 10 volte circa facendo fare operazioni in +
            //dovuto al tempo di ordinamento. Causa Quicksort tempo peggiore (caso pessimo). 

            //soluzione: usare quicksort_randomizzato. Ma sperimentalmente si è osservato non essere più veloce
            //rispetto a costruire tutte le volte le coppie di archi e poi ordinarli
            //quindi non si usa il quicksort random...
            //ma si usa qsort normale con initializzazione coppie d'archi ogni volta...

            //if (UseALPHA_NN)
            //    tsp.mst.BuildEdgeSet_Alpha(node0, ex_edges, tsp.ALPHA_NN, tsp.sort_alpha);
            //else
            //    tsp.mst.BuildEdgeSet(node0, ex_edges);
#if DEBUG
            //for (i = 0; i < tsp.mst.N_old; i++)
            //{
            //    str = "\r\n["+tsp.mst.tmp[i].a+","+tsp.mst.tmp[i].b+"] = " + tsp.distance[tsp.mst.tmp[i].a, tsp.mst.tmp[i].b];
            //    if (textBox1.InvokeRequired)
            //        textBox1.Invoke(stcb, str);
            //    else
            //        textBox1.AppendText(str);
            //}

#endif
            best_p = new float[M];
            
            EDGE_LIST[] best_lb_sol = new EDGE_LIST[M];

            //Calcolo il 1-tree lb
            if (_1treecomputed==false)
            {
                if (UseALPHA_NN)
                    if (std1treealg)
                        lb = tsp.Compute_1_treeAlpha_old(node0, old_dist, ex_edges);
                    else
                        lb = tsp.Compute_1_treeAlpha(node0, old_dist, ex_edges);
                else if (std1treealg)
                    lb = tsp.Compute_1_tree_old(node0, old_dist, ex_edges);
                else
                    lb = tsp.Compute_1_tree(node0, old_dist, ex_edges, false);
            }
            else 
                _1treecomputed = false;

            if (lb == float.MinValue)
                return lb;
            float LB = float.MinValue;
            int delta_count = 0;

            float alpha = 2;
            //bisognerebbe calcolarlo una volta solo perché sempre uguale invece qui nel B&B viene ricalcolato sempre.
            //Quante iterazioni far fare al subgradiente? difficile risposta...
            //int M1;
            //if (!root)
            //    M1 = M / 2;
            //else
            //    M1 = M;
            //int M1 = (int)Math.Round(Math.Log(tsp.GetN()) * Math.Pow(tsp.GetN(), 0.62));
            //M1 = M/2;
           
            for (m = 1; ((m < M) && (ub > lb) && (!bw.CancellationPending)); /*m++*/m+=subgrad_inc)
            {
#if DEBUG
                //str = "\r\n";
                //if (textBox1.InvokeRequired)
                //    textBox1.Invoke(stcb, str);
                //else
                //    textBox1.AppendText(str);
                //for (i = 0; i < tsp.GetN(); i++)
                //{
                //    str = "\r\n[" + tsp.lb[i].a + "," + tsp.lb[i].b + "] = " + tsp.distance[tsp.lb[i].a, tsp.lb[i].b];
                //    if (textBox1.InvokeRequired)
                //        textBox1.Invoke(stcb, str);
                //    else
                //        textBox1.AppendText(str);
                //}
#endif

                // calcolo il grado dei nodi
                d2n = tsp.Compute_Degree_Node();
                if (d2n == M)
                    break;
                // calcolo la sommatoria del grado dei nodi
                sum_degree = tsp.SumDegree();

                //Calcolo lambda
                //lambda = (float)2.0 * ((float)(M - m) / (n1));
                lambda = (float)alpha * ((float)(M - m) / (n1));
                //lambda = (float)alpha;

                //parametro t
                lambda *= (ub - lb) / (sum_degree);

                //calcolo p 
                for (i = 0, sum_p = 0; i < M; i++)
                {
                    p[i] += lambda * ((float)tsp.degree_node[i] - 2);
                    sum_p += p[i];
                }

                // modifico le distanze in base al grado dei nodi.
                for (i = 0; i < n1; i++)
                {
                    for (j = i + 1; j < M; j++)
                    {
                        old_dist[i, j] = tsp.distance[i, j] + (p[i] + p[j]);
                        old_dist[j, i] = old_dist[i, j];
                    }
                }

                //Calcolo il 1-tree lb
                if (UseALPHA_NN)
                    if (std1treealg)
                        lb = tsp.Compute_1_treeAlpha_old(node0, old_dist, ex_edges);
                    else
                        lb = tsp.Compute_1_treeAlpha(node0, old_dist, ex_edges);
                else if (std1treealg)
                    lb = tsp.Compute_1_tree_old(node0, old_dist, ex_edges);
                else
                    lb = tsp.Compute_1_tree(node0, old_dist, ex_edges, (m == n1));

                //Aggiusto il peso della soluzione
                lb -= (float)2 * sum_p;

                if (lb > LB)
                {
                    LB = lb;
                    p.CopyTo(best_p,0);
                    //best_d2n = d2n;
                    //tsp.lb.CopyTo(best_lb_sol, 0);
                    
                    //tenere commentato perchè si è notato una convergenza maggiore all'ottimo e minore nodi di branching
                    //delta_count = 0; 
                }
                else
                    delta_count++;

                if (delta_count == tsp.delta)
                {
                    delta_count = 0;
                    alpha /= 2;
                }
                //se negativa esco perchè è inutile!! o errore. o non possibile completare 1-tree (caso alphanearness) 
                if (lb < 0)
                    break;

                //copio se necessario il vecchio lb...
                if (show)
                    Array.Copy(tsp.lb, old, tsp.lb.Length);

                // fine ciclo
                if (show)
                {
                    int k = 0;

                    for (i = 0; i < M; i++)
                    {
                        for (j = 0; j < M; j++)
                        {
                            if ((old[j].a == tsp.lb[i].a) && (old[j].b == tsp.lb[i].b))
                                break;
                        }
                        //se non c'è
                        if (j == M)
                            edge_new[k++] = tsp.lb[i];
                    }

                    str = "Lower Bound : " + tsp.CalculateLBLength().ToString();
                    if (this.InvokeRequired)
                    {
                        e1 = k;
                        this.Invoke(refresh_mst, str);
                    }
                    //Stampo Il risultato
                    str = "\r\n" + m.ToString() + ". Lower Bound = " + lb.ToString() + " (" + tsp.CalculateLBLength().ToString() + ")";
                    if (textBox1.InvokeRequired)
                        textBox1.Invoke(stcb, str);
                    else
                        textBox1.AppendText(str);
                }
            }
           

            

            //#if DEBUG //visualizzo l'arco 1-tree 2°
            //            str = "\r\n Arco 2° 1-tree = " + tsp.lb[n1].a + "," + tsp.lb[n1].b + " d2n = "+d2n + " tour=" + istourok.ToString();
            //            if (ex_edges != null)
            //            {
            //                str += "\r\nEx_edges =";
            //                for (i = 0; i < ex_edges.Length; i++)
            //                {
            //                    if ((tsp.lb[n1].a == ex_edges[i].a) && (tsp.lb[n1].b == ex_edges[i].b))
            //                        str += ex_edges[i].a + "," + ex_edges[i].b + " | ";
            //                }
            //            }
            //            if (textBox1.InvokeRequired)
            //                textBox1.Invoke(stcb, str);
            //            else
            //                textBox1.AppendText(str);
            //#endif

            //tmp_p = p; //copio in p per il passaggio...
            //tmp_p = best_p;

            

            if (LB > float.MinValue)
            {
                lb = LB;
                tmp_p = best_p;
                //best_lb_sol.CopyTo(tsp.lb, 0);
                //best_lb_sol = null;
                //d2n = best_d2n;
            }
            else 
                tmp_p = null;

            //Se ho trovato la soluzione ottima...
            if ((d2n = tsp.Compute_Degree_Node()) == M)
                istourok = true;

            return lb;
            //return tsp.CalculateLBLength();
        }

        private void backgroundWorker5_lb_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopComputation();

            toolStripStatusLabel3.Text = "Lower Bound : " + e.Result.ToString();
            toolStripStatusLabel3.Visible = true;
            showMSTToolStripMenuItem.Enabled = true;
            toolStripStatusLabel4.Visible = true;
            toolStripStatusLabel4.Text = "GAP : " + (String.Format("{0,2:f}", (100 - ((float)e.Result) / tsp.Calculate_tour_length() * 100))) + "%";
            if (computeAsint)
                tsp.ResetDistanceMatrix();
        }

        private void branchBoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_LBopt flb = new Form_LBopt(tsp.computed_tour, tsp.GetN(), tsp.ALPHA_NN_MAX);
            byte b = 1;

            if (flb.ShowDialog() == DialogResult.OK)
            {
                //calcolo il branch & bound!!!
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = tsp.GetN();
                textBox1.AppendText("\r\nComputing LB.....");
                toolStripStatusLabel1.Text = "Working...";
                toolStripStatusLabel4.Text = toolStripStatusLabel3.Text = toolStripStatusLabel2.Text = "";
                toolStripStatusLabel4.Visible = toolStripStatusLabel3.Visible = toolStripStatusLabel2.Visible = true;
                //c2lboptparam lb_opt = flb.lb_opt;
                //FTV.treeView1.Nodes.Clear();

                toolStripStatusLabel_d2n.Visible = true;
                StartComputation();


                if (flb.lb_opt.UBnode0 == -1)
                    b++;
                if (flb.lb_opt.LBnode0 == -1)
                    b++;
                toolStripProgressBar1.Maximum = tsp.GetN() * b;
                toolStripProgressBar1.Value = 0;


                //soluzione radice
                //primo lower bound...
                FTV.treeView1.Nodes.Clear();
                tnodes = FTV.treeView1.Nodes;

                backgroundWorker6.RunWorkerAsync(flb.lb_opt);
                //EDGE_LIST[] edges = new EDGE_LIST[2];
                //edges[0].a = 1; edges[0].b = 3;
                //edges[1].a = 4; edges[1].b = 7;
                //Compute_lb(flb.lb_opt.LBnode0, tsp.Calculate_tour_length(), false, edges);
                //poi splitto il problema sui nodi di grado >=3
                //EDGE_LIST[] edges;
                //edges = tsp.GetEdgeSplit();


                //generando 3 sotto problemi escludendo un arco

                //ricorsivamente su ogni sotto problema ricalcolo lb e splitto!!!


                //Immagazzinare i vari archi in una lista EDGE_LIST in cui ci sono tutti gli archi da omettere nel calcolo del 1-tree!

                //StopComputation();
            }


        }


        private void backgroundWorker6_DoWork(object sender, DoWorkEventArgs e)
        {
            float lb;
            string str;

            clboptparam lb_opt = e.Argument as clboptparam;

            UseALPHA_NN = false;

            backgroundWorker5_lb_DoWork(sender, e);

            if (lb_opt.AlphaNearness)
            {
                tsp.ComputeAlphaNN(lb_opt.LBnode0, lb_opt.alpha_num);
                UseALPHA_NN = true;
                str = "\r\nAlphaNearness Candidate Set N° = " + tsp.ALPHA_NN;
                if (textBox1.InvokeRequired)
                    textBox1.Invoke(stcb, str);
                else
                    textBox1.AppendText(str);
            }
            else
                UseALPHA_NN = false;


            //if (lb_opt.AlphaNearness)
            //    UseALPHA_NN = true;


            sort_split = lb_opt.sort_split;
            if (lb_opt.StrongPruning)
                StrongPruning = true;
            else
                StrongPruning = false;
            
            bb_rule = lb_opt.bb_rule;
            
            std1treealg = lb_opt.std1treealg;

            ub_best = tsp.Calculate_tour_length();
            //lb_value_best = ub_best + 1; //nel caso ub è ottimo!
            lb_value_best = ub_best; //nel caso ub è ottimo!
            //tnodes = treeView1.Nodes;
            //statusStrip1.Invoke(SetProgressMaxCB, tsp.GetN() * tsp.GetN());
            //treeView1.Visible = false;

            BranchTreeNodeNumber = 1;
            BranchingLevel = 0; // level 0 = radice. 1° LB

            lb = (float)e.Result;
            TreeNode T = new TreeNode("Node Start " + lb_opt.LBnode0.ToString() + " ["+lb.ToString()+"]");
            if (FTV.InvokeRequired)
                FTV.Invoke(tnadd, T);
            else
                tnodes.Add(T);

            tnodes = tnodes[tnodes.IndexOf(T)].Nodes;

            if (lb_opt.strong_sort_split)
            {
                sort_split = true;
                StrongSplit = 1;
            }
            else
                StrongSplit = 0;

            if (lb_opt.use_pool_arcs)
                tsp.Build_pool_arcs(lb_opt.pool_arcs_strong);
            else
                tsp.pool_arcs = null;


            string s = "\r\n\r\nB&B rule\t\t: " + bb_rule.ToString() +
                       "\r\nSub Grad fast\t: "+ lb_opt.SubGradFast.ToString() +
                       "\r\nStrong Pruning\t: " + StrongPruning.ToString() +
                       "\r\nSplitSort\t\t: " + sort_split.ToString() +
                       "\r\nStrongSplitSort\t: " + StrongSplit.ToString() +
                       "\r\nArcs Pool\t\t: " + lb_opt.use_pool_arcs.ToString() +
                       "\r\nArcs Pool Strong\t: " + lb_opt.pool_arcs_strong.ToString()
                       + "\r\n\r\nComputing Branch&Bound...";

            
            if (textBox1.InvokeRequired)
                textBox1.Invoke(stcb, s);
            else
                textBox1.AppendText(s);
            //tmp_p = null;

            if (lb_opt.SubGradFast)
                subgrad_inc = 2;

            if (lb_opt.bb_rule == 6)
            {
                Mixbb_rule = 1; //hybrid
                
                /* ***************************************************************************************************************
                 * bisognerebbe fare dei test con vari dimensioni di nodi di grafo etc per trovare il n° "magico" di switching,
                 * attraverso un'analisi statistica del n°.
                 * per ora è fissato a 7 * [n° nodi]
                 * Oppure ragionare in profondità di nodo albero di branching e anche qui almeno considerare
                 * 5-10 livelli di profondità
                 * ------------------------------------ IDEA ulteriore "Deep Hybrid" -------------------------------------------
                 * o addirittura meglio, 
                 * fino a X=[3,5]? di livello di profondità usare regola 3, se più in profondità usare la 5
                 * ****************************************************************************************************************/
                //numHybrid = 700; // è sufficiente 700, 1000 ...... (si potrebbe fare come parametro) (500?)(nodi*7)
                numHybrid = (uint)tsp.GetN() * 7;
                bb_rule = 3;
                switched = false;
            }
            else if (lb_opt.bb_rule == 7)
            {
                Mixbb_rule = 2; //Deep Hybrid
                bb_rule = 3;
                switched = false;
            }
            else 
                Mixbb_rule = 0; //false rule E [1..5]
                
            lb = ComputeBB(null, lb_opt.LBnode0, lb_opt.ShowProg);
            BranchingLevel--;
            
            tsp.pool_arcs = null; //azzero il pool_arcs nel caso in cui l'avessi utilizzato.

            if ((tsp.lb != null) && (lb_best != null))
            {
                //lb_best.CopyTo(tsp.lb, 0);
                e.Result = tsp.CalculateLBLength();
                tsp.computed_lb = true;
            }
            else
            {
                e.Result = 0;
                tsp.computed_lb = false;
            }
        }

        private float ComputeBB(EDGE_LIST[] edges_ex, int node0, bool show)
        {
            float lb = -1;
            EDGE_LIST[] edges;
            EDGE_LIST[] e2;
            int i;
            bool tour;
            //tmp_p = null;

            if (backgroundWorker6.CancellationPending)
                return 0;

            if (edges_ex == null)
            {
                e2 = new EDGE_LIST[1];
                lb_best = new EDGE_LIST[tsp.GetN()];
            }
            else
            {
                e2 = new EDGE_LIST[edges_ex.Length + 1];
                edges_ex.CopyTo(e2, 1);
            }

            BranchingLevel++;
            switch (Mixbb_rule)
            {
                case 1: 
                    if (BranchTreeNodeNumber >= numHybrid)
                    {
                        textBox1.Invoke(stcb, "\r\nHybrid Rule Switching from 3 to 5, Node = " + BranchTreeNodeNumber.ToString());
                        Mixbb_rule = 0;
                        //switched = true;
                        bb_rule = 5;
                    }
                    break;
                case 2 :
                    if (BranchingLevel >= 2) //forse è sufficeinte 2 livello...
                    {
                        if (!switched)
                        {
                            textBox1.Invoke(stcb, "\r\nHybrid Rule Switching from 3 to 5, Node = " + BranchTreeNodeNumber.ToString());
                            bb_rule = 5;
                            switched = true;
                        }
                    }
                    else
                    {
                        if (switched)
                        {
                            textBox1.Invoke(stcb, "\r\nHybrid Rule Switching back from 5 to 3, Node = " + BranchTreeNodeNumber.ToString());
                            bb_rule = 3;
                            switched = false;
                        }
                    }
                    break;
            }

            //if ((Mixbb_rule==1) && (BranchTreeNodeNumber >= numHybrid))
            //{
            //    textBox1.Invoke(stcb, "\r\nHybrid Rule Switching from 3 to 5, Node = " + BranchTreeNodeNumber.ToString());
            //    Mixbb_rule = false;
            //    bb_rule = 5;
            //}

            //if ((BranchingLevel >= 3) && (Mixbb_rule==2))
            //{
            //    if (!switched)
            //    {
            //        textBox1.Invoke(stcb, "\r\nHybrid Rule Switching from 3 to 5, Node = " + BranchTreeNodeNumber.ToString());
            //        bb_rule = 5;
            //        switched = true;
            //    }
            //}
            //else
            //{
            //    if (switched)
            //    {
            //        textBox1.Invoke(stcb, "\r\nHybrid Rule Switching back from 5 to 3, Node = " + BranchTreeNodeNumber.ToString());
            //        bb_rule = 3;
            //        switched = false;
            //    }
            //}

            edges = tsp.GetEdgeSplit(out tour, sort_split, bb_rule/*,tmp_p*/);
            /* in teoria non arriva mai in questo stato, perchè il LB precdente
             * sarebbe già tour, quindi non avvia la ricorsione.
             * si potrebbe commentare.
             * si lascia come "safe check"
             * */
            if (edges == null) 
            {
                if (tour) //in teoria è perforza tour.... però per un if non è un problema (safe check se GetEdgeSplit da errore)
                {
                    lb = tsp.CalculateLBLength();
                    if (/*(lb_value_best > lb) &&*/ (lb < ub_best))
                    {
                        tsp.lb.CopyTo(lb_best, 0);
                        //lb_value_best = lb;
                        //ub_best = tsp.CalculateLBLength();
                        ub_best = lb;
                    }

                    string str = "\r\nTOUR = " + lb.ToString() + "(" + ub_best.ToString() + ")" + " Node = "+BranchTreeNodeNumber.ToString();
                    textBox1.Invoke(stcb, str);
                }

                return lb;
            }

            //meglio eliminarli direttamente invece che skipparli. + efficente.
            if (tsp.pool_arcs != null)
            {
                //bool skip = false;
                bool[] mark = new bool[edges.Length];
                int count = 0;
                int j;
                //if (tsp.pool_arcs != null)
                for (i = 0; i < edges.Length;i++)
                {
                    for (j = 0; j < tsp.pool_arcs.Length; j++)
                    {
                        if ((edges[i].a == tsp.pool_arcs[j].a) && (edges[i].b == tsp.pool_arcs[j].b))
                        {
                            //rimuovo l'edges i
                            mark[i] = true;
                            count++;
                            break;
                        }
                    }
                }
                int tot = edges.Length-count;
                EDGE_LIST[] edtmp = new EDGE_LIST[tot];
                j = 0;
                for (i = 0; i < edges.Length; i++)
                {
                    if (!mark[i])
                        edtmp[j++] = edges[i];
                }
#if DEBUG
                if (j != tot)
                {
                    MessageBox.Show("arcs pool: j!=count");
                }
#endif
                //riassegno il vettore edges..
                //questa sub-funzione potrebbe essere integrata nella classe tsp
                edges = edtmp;
            }

            //float[] tmp_p2 = new float[tsp.GetN()];
            //Array.Copy(tmp_p, tmp_p2, tmp_p2.Length);
            float[] tmp_p2 = null;
            if (tmp_p != null) //lo è sempre... (safe check)
                tmp_p2 = (float[])tmp_p.Clone();

            
            for (i = 0; ((i < edges.Length - StrongSplit) && (!backgroundWorker6.CancellationPending)); i++)
            {
                string str;
                TreeNode T;
                float lb2;
                
                ////******************** Arcs Pool ***************
                //bool skip = false;
                //if (tsp.pool_arcs != null)
                //{
                //    for (int j = 0; j < tsp.pool_arcs.Length; j++)
                //    {
                //        if ((edges[i].a == tsp.pool_arcs[j].a) && (edges[i].b == tsp.pool_arcs[j].b))
                //        {
                //            skip = true;
                //            break;
                //        }
                //    }
                //}
                //if (skip)
                //    continue;
                //*************************************************
                e2[0].a = edges[i].a;
                e2[0].b = edges[i].b;

                //if(tmp_p!=null)
                //if(std1treealg)
                lb = Compute_lb(node0, ub_best, show, e2, out tour, backgroundWorker6, (float[])tmp_p);
                BranchTreeNodeNumber++;
                //else
                //    lb = Compute_lb_alt(node0, ub_best, show, e2, out tour, backgroundWorker6, (float[])tmp_p);
                //else //if(std1treealg)
                //    lb = Compute_lb(node0, ub_best, show, e2, out tour, backgroundWorker6,null);
                //else
                //    lb = Compute_lb_alt(node0, ub_best, show, e2, out tour, backgroundWorker6, null);
                statusStrip1.Invoke(sssbbtxt,
                                    "GAP : " + string.Format("{0:+0.00;-0.00}", ((100 - ((float)lb) / ub_best * 100))) + "%",
                                    "Lower Bound : " + lb.ToString(),
                                    "Upper Bound : " + ub_best.ToString(),
                                    "Node 2° : " + d2n.ToString());

                if (StrongPruning)
                    lb2 = (float)(lb * 1.0005); //aumento del 0,5% (forse troppo, meglio 0,05%)
                else
                    lb2 = lb;

                if (tour)
                {
                    lb = tsp.CalculateLBLength();
                    //<= non è sufficente, basterebbe < ... ma così almeno notifica qualche tour uguale alternativo...
                    if (/*(lb_value_best > lb) && */(lb <= ub_best))//per il tour che sia minimo, se lb è <= allora può essere l'ottimo
                    {
                        tsp.lb.CopyTo(lb_best, 0);
                        ub_best = lb;
                        str = "\r\nFound Tour : " + lb.ToString() + " Node = " + BranchTreeNodeNumber.ToString(); ;
                        textBox1.Invoke(stcb, str);
                    }

                    str = "(" + e2[0].a.ToString() + "," + e2[0].b.ToString() + ")" + " = " + tsp.CalculateLBLength();
                    T = new TreeNode(str);
                    T.NodeFont = new Font(FTV.treeView1.Font, FontStyle.Bold);

                    if (FTV.InvokeRequired)
                        FTV.treeView1.Invoke(tnadd, T);
                    else
                        tnodes.Add(T);
                }
                else if ((lb2 < ub_best) && (lb2 > 0))
                {
                    str = "(" + e2[0].a.ToString() + "," + e2[0].b.ToString() + ")" + " : [" + lb + "]";
                    T = new TreeNode(str);
                    //tnodes.Add(T);
                    if (FTV.InvokeRequired)
                        FTV.treeView1.Invoke(tnadd, T);
                    else
                        tnodes.Add(T);

                    TreeNodeCollection tmp = tnodes;
                    tnodes = tnodes[tnodes.IndexOf(T)].Nodes;

                    lb = ComputeBB(e2, node0, show);
                    tnodes = tmp;
                    BranchingLevel--;
                }
                else //Pruning
                {
                    str = "(" + e2[0].a.ToString() + "," + e2[0].b.ToString() + ") : [Pruning = " + lb + "]";
                    T = new TreeNode(str);
                    T.NodeFont = new Font(FTV.treeView1.Font, FontStyle.Italic);
                    //tnodes.Add(T);
                    if (FTV.InvokeRequired)
                        FTV.treeView1.Invoke(tnadd, T);
                    else
                        tnodes.Add(T);
                }

                tmp_p = tmp_p2;
            }
            return lb;
        }

        private void backgroundWorker6_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            float v;

            //copiare il lower bound se è un tour...
            v = tsp.Calculate_tour_length();
            if (ub_best <= v)
            {
                //copiare il lower bound come tour!!!
                if (tsp.computed_lb)
                    textBox1.AppendText("\r\nOptimal Tour Founded!");
                if (lb_best != null)
                    lb_best.CopyTo(tsp.lb, 0);
                if (tsp.CalculateLBLength() > 0)
                {
                    tsp.BuildTourFromLB();
                    toolStripStatusLabel2.Text = "Upper Bound : " + ub_best;
                }
                DrawGraph();
                Refresh();

            }
            StopComputation();

            if (computeAsint)
                tsp.ResetDistanceMatrix();
            toolStripStatusLabel3.Text = "Lower Bound : " + e.Result.ToString();
            toolStripStatusLabel3.Visible = true;
            showMSTToolStripMenuItem.Enabled = true;
            //treeView1.Visible = ShowTreeNode;
            FTV.treeView1.Update();
            textBox1.AppendText("\r\nBranch Tree Total Node = " + BranchTreeNodeNumber + "\r\n");
        }

        private void viewTreeBranchBoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTreeNode = !ShowTreeNode;
            if (sender != toolStripButton10)
                toolStripButton10.Checked = !toolStripButton10.Checked;
            else
                viewTreeBranchBoundToolStripMenuItem.Checked = !viewTreeBranchBoundToolStripMenuItem.Checked;
            if (ShowTreeNode)
                FTV.Show();
            else
                FTV.Hide();

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if ((!FTV.Visible) && (ShowTreeNode))
            {
                viewTreeBranchBoundToolStripMenuItem_Click(sender, e);
                viewTreeBranchBoundToolStripMenuItem.Checked = ShowTreeNode;
                toolStripButton10.Checked = ShowTreeNode;
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            tourToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            showNodeNumberToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            showConvexHullToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            showMSTToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            viewTreeBranchBoundToolStripMenuItem_Click(sender, e);
        }

        private void generateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormGenerate fgen = new FormGenerate();

            if (fgen.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel4.Visible = toolStripStatusLabel2.Visible = toolStripStatusLabel3.Visible = false;
                textBox1.AppendText("\r\nGenerating problem....");
                if (GenerateTSP(fgen.n, fgen.usquare, fgen.n2, fgen.n3, fgen.n2b, fgen.n3b))
                    textBox1.AppendText("OK!");
            }
        }

        private bool GenerateTSP(int n, bool usquare, int max_x, int max_y, int min_x, int min_y)
        {
            int i, j, poss = 10;
            Random rand = new Random();
            Random r2 = new Random(rand.Next());
            float x, y;

            cluster = null;
            tsp = new cTSP(n);
            FTV.treeView1.Nodes.Clear();
            for (i = 0; i < n; i++)
            {
                if (poss <= 0)
                {
                    textBox1.AppendText("ERRROR!!");
                    tsp.Loaded_Data(false);
                    toolStripSplitButton1.Enabled = saveTSPToolStripMenuItem.Enabled = false;
                    return false;
                }
                if (usquare)
                {
                    x = (float)rand.NextDouble();
                    y = (float)rand.NextDouble();
                }
                else
                {
                    x = (float)rand.Next(min_x, max_x - 1) + (float)r2.Next(max_x) / (float)max_x;
                    y = (float)rand.Next(min_y, max_y - 1) + (float)r2.Next(max_y) / (float)max_y;
                }
                for (j = 0; j < i; j++)
                    if ((x == tsp.Get_x(j)) && (y == tsp.Get_y(j)))
                        break;
                if (j < i)
                {
                    i--;
                    poss--;
                }
                else
                {
                    tsp.setNode(i, x, y);
                    poss = 10;
                }
            }

            tsp.Loaded_Data();
            toolStripSplitButton1.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            ComputeDistances();
            tsp.sort_distances();
            //tsp.ComputeAlphaNN();

            scale = (this.ClientSize.Width - textBox1.ClientSize.Width) * 0.9F / (tsp.GetMax_x() - tsp.GetMin_x());

            scale_norm = (this.ClientSize.Height - toolStrip1.ClientSize.Height - statusStrip1.ClientSize.Height - menuStrip1.ClientSize.Height) * 0.9F / (tsp.GetMax_y() - tsp.GetMin_y());
            if (scale < scale_norm)
                scale_norm = scale;
            scale = scale_norm;

            center_x = center_y = 0;
            offset_x = 10;
            offset_y = menuStrip1.ClientSize.Height + toolStrip1.ClientSize.Height + 20;

            offset_x -= Convert.ToInt32(tsp.GetMin_x() * scale);
            offset_y -= Convert.ToInt32(tsp.GetMin_y() * scale);

            solveToolStripMenuItem.Enabled = true;
            toolStripButton2.Enabled = false;
            toolStripButton1.Enabled = true;
            fileToolStripMenuItem.Enabled = true;
            graphToolStripMenuItem.Enabled = true;

            DrawGraph();
            this.Refresh();
            return true;
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            generateToolStripMenuItem_Click(sender, e);
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            saveTSPToolStripMenuItem_Click(sender, e);
        }

        private void saveTSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.AutoUpgradeEnabled = true;
            saveFileDialog1.DefaultExt = "*.tsp";
            saveFileDialog1.Filter = "TSP (*.tsp)|*.tsp|Text (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Save TSP Problem";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveTSP(saveFileDialog1.FileName);
            }
        }

        private void SaveTSP(string fn)
        {
            FileStream fout;
            StreamWriter fstr_out;
            string s;
            int i;

            Cursor.Current = Cursors.WaitCursor;
            toolStripStatusLabel1.Text = "Saving TSP...";
            s = "\r\nSaving TSP to file : " + fn;

            if (this.textBox1.InvokeRequired)
                this.Invoke(stcb, s);
            else
                textBox1.AppendText(s);

            fout = new FileStream(fn, FileMode.Create, FileAccess.Write);
            fstr_out = new StreamWriter(fout);

            fstr_out.WriteLine("NAME: ");
            fstr_out.WriteLine("TYPE: TSP");
            fstr_out.WriteLine("DIMENSION: " + tsp.GetN().ToString());
            fstr_out.WriteLine("EDGE_WEIGHT_TYPE: EUC_2D");
            fstr_out.WriteLine("NODE_COORD_SECTION");

            for (i = 0; i < tsp.GetN(); i++)
                fstr_out.WriteLine(i.ToString() + " " + tsp.Get_x(i).ToString() + " " + tsp.Get_y(i).ToString());

            s = "\r\nSaved.";
            if (this.textBox1.InvokeRequired)
                this.Invoke(stcb, s);
            else
                textBox1.AppendText(s);
            fout.Flush();
            fstr_out.Flush();
            fout.Close();
            toolStripStatusLabel1.Text = "Ready.";
            Cursor.Current = Cursors.Arrow;
        }

        private void SaveTSPTour(string fn)
        {
            FileStream fout;
            StreamWriter fstr_out;
            string s;
            int i;

            Cursor.Current = Cursors.WaitCursor;
            toolStripStatusLabel1.Text = "Saving TSP...";
            s = "\r\nSaving TSP to file : " + fn;

            if (this.textBox1.InvokeRequired)
                this.Invoke(stcb, s);
            else
                textBox1.AppendText(s);

            fout = new FileStream(fn, FileMode.Create, FileAccess.Write);
            fstr_out = new StreamWriter(fout);

            fstr_out.WriteLine("NAME: ");
            fstr_out.WriteLine("TYPE: TOUR");
            fstr_out.WriteLine("DIMENSION: " + tsp.GetN().ToString());
            fstr_out.WriteLine("TOUR_SECTION");

            for (i = 0; i < tsp.GetN(); i++)
                fstr_out.WriteLine(tsp.GetTourNode(i));

            s = "\r\nSaved.";
            if (this.textBox1.InvokeRequired)
                this.Invoke(stcb, s);
            else
                textBox1.AppendText(s);
            fout.Flush();
            fstr_out.Flush();
            fout.Close();
            toolStripStatusLabel1.Text = "Ready.";
            Cursor.Current = Cursors.Arrow;
        }

        private void saveTourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.AutoUpgradeEnabled = true;
            saveFileDialog1.DefaultExt = "*.tsp";
            saveFileDialog1.Filter = "TSP (*.tsp)|*.tsp|Text (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Save Tour.";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveTSPTour(saveFileDialog1.FileName);
            }

        }

        private void saveMSTLBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.AutoUpgradeEnabled = true;
            saveFileDialog1.DefaultExt = "*.tsp";
            saveFileDialog1.Filter = "TSP (*.tsp)|*.tsp|Text (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Save EDGE LIST";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

            }

        }

        private void saveTourToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveTourToolStripMenuItem_Click(sender, e);
        }

        private void saveMST1TreeLBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveMSTLBToolStripMenuItem_Click(sender, e);
        }

        private void backgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            c3optparam obj = e.Argument as c3optparam;

            if ((obj.start_node == -1) && (obj.itour == 3))
            {
                int i;
                float best = float.MaxValue;
                int nodebest = 0;
                float v;

                for (i = 0; ((i < tsp.GetN()) && (!bw.CancellationPending)); i++)
                {
                    obj.start_node = i;
                    Compute3opt(obj, sender);
                    if ((v = tsp.Calculate_tour_length()) < best)
                    {
                        best = v;
                        nodebest = i;
                    }

                    if (toolStrip1.InvokeRequired)
                    {
                        IncremetProgress d = new IncremetProgress(IncProgBar);
                        toolStrip1.Invoke(d, 1);
                    }
                    else
                        toolStripProgressBar1.Increment(1);
                }
                //cheapest insertion...
                obj.itour = 5;
                Compute3opt(obj, sender);
                if ((v = tsp.Calculate_tour_length()) < best)
                {
                    //nodebest = i;
                    best = v;
                    string str = "Best is Cheapest Insertion : ";
                    if (textBox1.InvokeRequired)
                        this.Invoke(stcb, str);
                    else
                        textBox1.AppendText(str);
                }
                //Cristofides...
                else
                {
                    obj.itour = 6;
                    Compute3opt(obj, sender);
                    if ((v = tsp.Calculate_tour_length()) < best)
                    {
                        //nodebest = i;
                        best = v;
                        string str = "Best is Christofides...";
                        if (textBox1.InvokeRequired)
                            this.Invoke(stcb, str);
                        else
                            textBox1.AppendText(str);
                    }
                    else
                    {
                        obj.itour = 3;
                        //Best Tour
                        obj.start_node = nodebest;
                        if (textBox1.InvokeRequired)
                        {
                            this.Invoke(stcb, "\r\nBest Node : " + nodebest.ToString());
                        }
                        else
                            textBox1.AppendText("\r\nBest Node : " + best_node0.ToString());
                        Compute3opt(obj, sender);
                    }
                }
            }
            else
                Compute3opt(e.Argument, sender);
        }

        private void backgroundWorker5_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopComputation();
            DrawGraph();
            if (this.InvokeRequired)
            {
                this.Invoke(refresh_, new object[] { "" });
            }
            else
                this.Refresh();
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {
            //Prova euristico riduzione nodi....

            /* Logica ed idea di funzionamento in breve:
             * dal grafo corrente si cerca la distanza minore fra 2 nodi
             * e questi 2 si collassano generando un nodo (in pratica si rimuove un nodo ad ogni iterazione)
             * dalle 2 colonne delle distanze dei 2 nodi se ne crea una sola con il valore uguale a (in scelta):
             *  1.MAX della distanza rispetto ad un 3 nodo
             *  2.Si può scegliere di fare una media delle distanze.
             *  3 MIN distanza rispetto ad un 3 nodo
             *  
             *  si memorizza in uno stack l'operazione compiuta, così si può tornare indietro 'spoppando' dallo stack stesso. 
             *  
             *  il costo per ciò è ricalcolarsi le distanze delle 2 distanze. le altre si copiano dalla matrice vecchia.
             *  
             * quante iterazioni fare? ovvero quante nodi collassare?
             * 
             */


            double dist_avg = 0.0;
            float[,] dist = tsp.GetDistanceMatrix();
            int i, j;
            int n = tsp.GetN();
            int n1 = n - 1;

            int total = n * (n1) / 2;

            for (i = 0; i < n1; i++)
            {
                for (j = i + 1; j < n; j++)
                    dist_avg += dist[i, j];
            }

            dist_avg /= total;

            List<EDGE_LIST> nodes = new List<EDGE_LIST>();

            for (i = 0; i < n1; i++)
            {
                for (j = i + 1; j < n; j++)
                {
                    if (dist[i, j] < dist_avg)
                    {
                        //inserisco in lista i 2 nodi
                        EDGE_LIST ed;
                        ed.a = i;
                        ed.b = j;
                        nodes.Add(ed);
                    }
                }
            }

            EDGE_LIST min;
            min.a = min.b = 0;

            float min_dist = float.MaxValue;
            //bisognerebbe suddividere il tsp geometricamente poi dalle regioni, 
            //collassare i nodi o sottorisolvere i problemi

            for (i = 0; i < n1; i++)
            {
                for (j = i + 1; j < n; j++)
                {
                    if (dist[i, j] < min_dist)
                    {
                        //inserisco in lista i 2 nodi
                        min.a = i;
                        min.b = j;
                        min_dist = dist[i, j];
                    }
                }
            }
            textBox1.AppendText("\r\nMin dist = " + min_dist + " (avg = " + dist_avg + ")");
            //trovato il minimo collasso i 2 nodi in 1.
            cluster = null;
            cTSP tsp2 = new cTSP(n1);

            for (i = 0, j = 0; i < n; i++)
            {
                if ((min.a == i) || (min.b == i))
                    continue;

                tsp2.setNode(j, tsp.Get_x(i), tsp.Get_y(i));
                j++;
            }
            //l'ultimo nodo è quello collassato.
            tsp2.setNode(j,
                (tsp.Get_x(min.a) + tsp.Get_x(min.b)) / 2,
                (tsp.Get_y(min.a) + tsp.Get_y(min.b)) / 2);

            textBox1.AppendText("\r\nnodi collassati = " + min.a + " - " + min.b);
            textBox1.AppendText("\r\nnuovo nodo = " + j);
            //per test, brutalmente lo imposto come nuovo
            tsp = tsp2;
            tsp.Loaded_Data();
            ComputeDistances();
            tsp.sort_distances();

            DrawGraph();
            this.Refresh();
        }

        private void viewAlphaNearnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int node = 0;

            tsp.ComputeAlphaNN(node, tsp.GetN());

            for (int i = 0; i < tsp.GetN(); i++)
            {
                textBox1.AppendText("\r\nAlpha [" + node + "," + tsp.sort_alpha[node, i] + "] (" + i + ") = " + tsp.alpha[node, tsp.sort_alpha[node, i]]);
            }
        }

        private void viewSortedDistancessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int node = 0;


            for (int i = 0; i < tsp.GetN(); i++)
            {
                textBox1.AppendText("\r\nSorted [" + node + "," + i + "] = " + tsp.sort_dist[node, i]);
            }

        }

        private void saveGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveTSPToolStripMenuItem_Click(sender, e);
        }

        private void cLusteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //algoritmo di clustering dei nodi....

            /* idea:
             * kd-tree (2d-tree) usare per dividere l'area geomtrica.? 
             * 
             * 1. conta quanti nodi sono vicini rispetto alla media delle distanze
             *    e cerca nel frattempo i 4 nodi negli angoli geometrici  
             * 2. dal risultato precedente rimuove i nodi che sono esattamente la stessa cosa. (facoltativo forse)
             * 3. crea i cluster attraverso il risultato del contatore
             * 
             * alla fine ci sono dei cluster che rimangono fuori e si possono attaccare ad un cluster vicino o
             * sceglierli come cluster da nodo singolo.
             * e presente il codice per attaccarli in un cluster, ma si lasciano singoli.
             * 
             * MANCA creare i cluster per i 4 nodi degli angoli se si fossero trovati, o equivalentemnte associare il cluster 
             * contente quei nodi ai nodi stessi, ma meglio fare calcolare appositamente il cluster per quei nodi
             * 
             * RISULTATO aspettato:
             * ci sara un cluster più grande degli altri, circa in zona centrale, dipende dal problema
             * questo sara il cluster MASTER, e ad intuito non conviene ridurlo, al max risolverlo separatamente
             * gli altri clusters minori, ed eventualmente periferici, si possono collassare in un macro-nodo
             * per ridurre la dimensione del grafo, trovato l'ottimo (attraverso uno stack?) si ripristinano
             * i nodi originali, da cui si fa un dijkstra partendo dai 2 nodi incidenti al macronodo, e risolvendo da
             * quei 2 nodi, il percorso minimo con il grafo ripristinato. 
             *
             * è da dimostrare che sia l'ottimo!
             * quindi si potrebbe fare un branching limitando solo il branching relativo agli archi ripristinati.
             * (ex: se il macronodo comprende {0,1,2} ed è attaccato a 3 e 4 ed il tuor ottimo del macronodo è {0,2,1}
             * il risultato sara {3,0,2,1,4} dimostrare che sia ottimo:
             * il branching, se necessario si fa su {3,0},{3,2},{3,1},{3,4}... ma è impossibile farlo!!:D
             * quindi non si dimostra col branching....
             *
             *
             * 
             */

            bool sparse = false;

            double dist_avg = 0.0;
            float[,] dist = tsp.GetDistanceMatrix();
            int i, j;
            int n = tsp.GetN();
            int n1 = n - 1;
            int nodeTL, nodeTR, nodeBL, nodeBR;
            nodeTL = nodeTR = nodeBL = nodeBR = -1;


            int total = n * (n1) / 2;

            for (i = 0; i < n1; i++)
            {
                for (j = i + 1; j < n; j++)
                    dist_avg += dist[i, j];
            }

            dist_avg /= total;

            //stima se è sparso o no... approx.
            if ((dist_avg < tsp.GetMax_x() - tsp.GetMin_x()) &&
                dist_avg < tsp.GetMax_y() - tsp.GetMin_y())
            {
                textBox1.AppendText("\r\n Grafo sparso!");
                sparse = true;

                //se è sparso prendo i 4 nodi esterni per clusterizzare.
            }


            textBox1.AppendText("\r\n AVG dist = " + dist_avg.ToString());

            Point min = new Point(),
                  max = new Point();

            min.X = min.Y = int.MaxValue;
            max.X = max.Y = int.MinValue;
            int w = -1, h = -1;

            List<int>[] count = new List<int>[n];

            if (sparse)
            {
                float minx = tsp.GetMin_x();
                float maxx = tsp.GetMax_x();
                float miny = tsp.GetMin_y();
                float maxy = tsp.GetMax_y();

                float BL = minx + miny;
                float TL = minx + maxy;
                float BR = maxx + miny;
                float TR = maxx + maxy;



                for (i = 0; i < n; i++)
                {
                    count[i] = new List<int>();

                    float x = tsp.Get_x(i);
                    float y = tsp.Get_y(i);

                    if (x <= minx)
                    {
                        if (y <= miny)
                            nodeBL = i;
                        else if (y >= maxy)
                            nodeTL = i;
                    }
                    else if (x >= maxx)
                    {
                        if (y <= miny)
                            nodeBR = i;
                        else if (y >= maxy)
                            nodeTR = i;

                    }


                    for (j = 0; j < n; j++)
                    {
                        if (j == i)
                            continue;
                        if (dist[i, j] < dist_avg)
                        {
                            count[i].Add(j);
                        }

                    }
                }
            }

            //ora se ho i 5 nodi d'angolo, devo costruire i loro rispettivi cluster...
            //...(da fare)
            bool[] mark = new bool[n];


            if ((nodeBL != -1) &&
                (nodeBR != -1) &&
                (nodeTL != -1) &&
                (nodeTR != -1))
            {
                //usare count
                //ex: count[nodeBL] mi dice i suoi vicini...
                for (i = 0; i < count.Length; i++)
                {
                    if ((i == nodeBL) ||
                        (i == nodeTL) ||
                        (i == nodeBR) ||
                        (i == nodeTR))
                        continue;
                    count[i].Clear();
                }
                //alla fine count.length==4 (su tutti) ovvero gli altri sono 0.
            }
            //else //fino a textbox1..
            {
                /* posso cercare i NN in comune di indice fra i vari nodi. 
                 * ovvero se il nodo 1 ha come 1° nn 3 AND 3 ha come 1° NN 1 
                 * ---> allora sono in cluster!!!
                 */

                for (i = 0; i < n; i++)
                {
                    for (int i2 = 0; i2 < count[i].Count; i2++)
                    {
                        int sort_index = 0;
                        int l = count[i][i2];

                        for (j = 1; j < n; j++)
                        {
                            if (l == tsp.sort_dist[i, j])
                            {
                                sort_index = j;
                                break;
                            }
                        }

                        if (sort_index == 0)
                            continue;

                        //verifico che il nodo l sia il medesimo del nodo j per fare il cluster...
                        if (i == tsp.sort_dist[l, sort_index])
                        {
                            //ok agglomero che si può fare un clusterino...
                        }
                        else // lo rimuovo che così mi rimane il cluster... ;)
                        {
                            count[i].Remove(l);
                            count[l].Remove(i);
                            i2--;
                        }


                    }
                }
            } //else

            textBox1.AppendText("\r\nNodeTL = " + nodeTL +
                                        "\r\nNodeBL = " + nodeBL +
                                        "\r\nNodeTR = " + nodeTR +
                                        "\r\nNodeBR = " + nodeBR);


            //bisogna trovare i cicli nei cluster ed eliminarli...
            List<int>[] cluster = new List<int>[n];
            //mark = new bool[n];
            int[] m = new int[n];
            for (i = 0; i < n; i++)
                m[i] = i;

            for (i = 0; i < n; i++)
            {
                if (cluster[i] == null)
                    cluster[i] = new List<int>();
                //cerco il nodo i in che cluster è presenti...
                for (j = i + 1; j < n; j++)
                {
                    for (int k = 0, l; k < count[j].Count; k++)
                    {
                        l = count[j][k];
                        if (l == m[i])
                        {
                            cluster[m[i]].Add(j);
                            cluster[m[i]].AddRange(count[j]);
                            foreach (int a in count[j])
                                if (a > i)
                                    m[a] = m[i];
                            //else
                            // m[a] = i;
                            m[j] = m[i];
                            break;
                        }
                    }
                }
            }

            for (i = 0; i < n; i++)
            {
                cluster[i].Sort();
                for (int il = 0, l1, l2; il < cluster[i].Count - 1; il++)
                {
                    l1 = cluster[i][il];
                    l2 = cluster[i][il + 1];
                    if (l1 == l2)
                    {
                        cluster[i].Remove(l1);
                        il--;
                    }
                }
            }

            for (i = 0; i < n; i++)
            {
                for (int il = 0; il < cluster[i].Count; il++)
                {
                    int l = cluster[i][il];
                    for (j = i + 1; j < n; j++)
                    {
                        foreach (int l2 in cluster[j])
                        {
                            if (l2 == l)
                            {
                                cluster[i].AddRange(cluster[j]);
                                cluster[j].Clear();
                                break;
                            }
                        }
                    }
                }
            }

            for (i = 0; i < n; i++)
            {
                cluster[i].Sort();
                for (int il = 0, l1, l2; il < cluster[i].Count - 1; il++)
                {
                    l1 = cluster[i][il];
                    l2 = cluster[i][il + 1];
                    if (l1 == l2)
                    {
                        cluster[i].Remove(l1);
                        il--;
                    }
                }
            }

            int c = 0;
            int c1 = 0;
            int c3 = 0;
            for (i = 0; i < n; i++)
            {
                if (count[i].Count == 1)
                {
                    c1++;
                    if ((count[count[i][0]].Count) >= 1)
                        count[i].Clear();

                }
                foreach (int l in count[i])
                {
                    textBox1.AppendText("\r\nNode " + i + " cluster = " + l);
                    c++;
                }
            }

            for (i = 0; i < n; i++)
            {
                if (cluster[i].Count > 0)
                    c3++;
                foreach (int l in cluster[i])
                    textBox1.AppendText("\r\ncluster " + i + " with node = " + l);
            }



            /*i cluster con 0 nodi, sono interessanti... 
             * perchè si possono aggregare ad espandere gli altri, 
             * se loro non risultassero in nessun cluster...
             */
            mark = new bool[n];
            for (i = 0; i < n; i++)
            {
                if (count[i].Count > 0)
                    mark[i] = true;
                foreach (int l in count[i])
                    mark[l] = true;
            }
            int c2 = 0;
            for (i = 0; i < n; i++)
                if (!mark[i])
                {
                    textBox1.AppendText("\r\nNodo " + i + " non appartenente a nessn cluster!");
                    for (j = 1; j < n; j++)
                        if (count[tsp.sort_dist[i, j]].Count >= 1)
                        {
                            textBox1.AppendText("\r\nil suo primo cluster è del nodo:" + tsp.sort_dist[i, j]);
                            break;
                        }
                }
                else
                    c2++;

            textBox1.AppendText("\r\nTOT = " + c);
            textBox1.AppendText("\r\nTot 1 = " + c1);
            textBox1.AppendText("\r\nTot cluster = " + c2);
            textBox1.AppendText("\r\nTot cluster[] = " + c3);

            g2.DrawRectangle(pen, min.X * scale + center_x - 2,
                min.Y * scale + center_y - 2,
                w * scale + center_x - 2,
                h * scale + center_y - 2);

            this.cluster = cluster;
            DrawGraph();
            this.Refresh();

        }

        private void christophidesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartComputation();
            tsp.Christofides();
            StopComputation();
            //logTourPathToolStripMenuItem.Enabled = true;
            //recalculateTourPathToolStripMenuItem.Enabled = true;
            //textBox1.AppendText("\r\nChristofides Tour = " + tsp.Calculate_tour_length());
            DrawGraph();
            Refresh();
        }

        private void tSPHeuRedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!tsp.TSP_GraphReduction())
                MessageBox.Show("error");

            textBox1.AppendText("\r\nNuovi Nodi Totali = " + tsp.GetN().ToString());
            DrawGraph();
            Refresh();
        }

        private void tSPGrapRestoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!tsp.TSP_Graph_Restore(true,true))
                MessageBox.Show("error");

            textBox1.AppendText("\r\nNuovi Nodi Totali = " + tsp.GetN().ToString());
            DrawGraph();
            Refresh();
        }

        private void tSPHeuRedStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!tsp.TSP_GraphReductionStep())
                MessageBox.Show("error");

            textBox1.AppendText("\r\nNuovi Nodi Totali = " + tsp.GetN().ToString());
            DrawGraph();
            Refresh();
        }

        private void tSPGraphRestoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!tsp.TSP_Graph_Restore(true, false))
                MessageBox.Show("error");

            textBox1.AppendText("\r\nNuovi Nodi Totali = " + tsp.GetN().ToString());
            DrawGraph();
            Refresh();
        }

        private void rule1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int c = tsp.pr_Reduction_Rule1();
            textBox1.AppendText("\r\npr reduction rule 1 node candidate for shrink = " + c);

            //cercare 3 nodi u,v,w tali che: se wuv=w,v,u allora shrink u,v
            int u, v, w;
            //int i,j,k;
            
            //u=v=w=-1;
            //la i la uso come w
            for (w = 0; w < tsp.GetN(); w++)
            {
                //la j la uso come v
                for (v = 0; v < tsp.GetN(); v++)
                {
                    if (v == w)
                        continue;
                    //la k la uso come u
                    for (u = v + 1; u < tsp.GetN(); u++)
                    {
                        if ((u == w) || (u == v))
                            continue;

                        //float wuv = distance[i,k] + distance[k,j];
                        //float wvu = distance[i,j] + distance[j,k];
                        float wuv = tsp.distance[w, u];
                        float wvu = tsp.distance[w, v];

                        if (wuv == wvu)
                        {
                            //shrink u v in un unico nodo u';
                            textBox1.AppendText("\r\n Shrink (" + u + "," + v + ")"+" w="+w);
                        }
                    }
                }
            }

        }

        private void tSPBBGRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float v = tsp.TSP_Graph_Restore_1Tree();
            textBox1.AppendText("\r\n1-Tree = " + v);
            clboptparam lbopt = new clboptparam();
            lbopt._3opt=true;
            lbopt.AlphaNearness=false;
            lbopt.bb_rule=3;
            lbopt.BestLB=false;
            lbopt.compAsInt=true;
            lbopt.LBnode0=tsp.lb[tsp.GetN()-1].a;
            lbopt.ShowProg=false;
            lbopt.sort_split=true;
            lbopt.std1treealg=true;
            lbopt.StrongPruning=false;
            lbopt.UBalgo=1;
            lbopt.UBnode0=0;
            lbopt.UBshowprog=false;
            lbopt.UBstart=3;
            //DoWorkEventArgs ev = new DoWorkEventArgs(lbopt);
            
            //backgroundWorker5_lb_DoWork(this, ev);
            FTV.treeView1.Nodes.Clear();
            tnodes = FTV.treeView1.Nodes;
            backgroundWorker6.RunWorkerAsync(lbopt);
            //backgroundWorker6_DoWork(this, ev);
            //this.DrawGraph();
            //this.Refresh();
        }

        private void BB_restore(object sender, EventArgs e)
        {
            

            do
            {
                float v = tsp.TSP_Graph_Restore_1Tree();
                if (v == 0)
                    break;

                //MessageBox.Show("");
                clboptparam lbopt = new clboptparam();
                tsp.Compute_Degree_Node();
                for (int i = 0; i < tsp.GetN(); i++)
                {
                    if (tsp.degree_node[i] == 2)
                    {
                        lbopt.LBnode0 = i;
                        break;
                    }
                }




                lbopt._3opt = true;
                lbopt.AlphaNearness = false;
                lbopt.bb_rule = 3;
                lbopt.BestLB = false;
                lbopt.compAsInt = true;
                lbopt.ShowProg = false;
                lbopt.sort_split = true;
                lbopt.std1treealg = true;
                lbopt.StrongPruning = false;
                lbopt.UBalgo = 1;
                lbopt.UBnode0 = 0;
                lbopt.UBshowprog = false;
                lbopt.UBstart = 3;
                
                this.Invoke(refresh_, "");
                string s = "\r\n1-Tree = " + v + "Node = " + tsp.GetN();
                if(textBox1.InvokeRequired)
                    textBox1.Invoke(stcb, s);
                else
                    textBox1.AppendText(s);

                //DoWorkEventArgs ev = new DoWorkEventArgs(lbopt);

                //backgroundWorker5_lb_DoWork(this, ev);
                FTV.treeView1.Nodes.Clear();
                tnodes = FTV.treeView1.Nodes;
                _1treecomputed = true;
                backgroundWorker6_DoWork(sender, new DoWorkEventArgs(lbopt));
                
                //backgroundWorker6_DoWork(this, ev);
                //this.DrawGraph();
                //this.Refresh();
            } while (true);
        }
        
        private void BB_restore_comp(object sender, RunWorkerCompletedEventArgs e)
        {
            float v;

            //copiare il lower bound se è un tour...
            v = tsp.Calculate_tour_length();
            if (ub_best <= v)
            {
                //copiare il lower bound come tour!!!
                if (tsp.computed_lb)
                    textBox1.AppendText("\r\nOptimal Tour Founded!");
                if (lb_best != null)
                    lb_best.CopyTo(tsp.lb, 0);
                if (tsp.CalculateLBLength() > 0)
                {
                    tsp.BuildTourFromLB();
                    toolStripStatusLabel2.Text = "Upper Bound : " + ub_best;
                }
                DrawGraph();
                Refresh();

            }
            StopComputation();

            if (computeAsint)
                tsp.ResetDistanceMatrix();
            toolStripStatusLabel3.Text = "Lower Bound : " + e.Result.ToString();
            toolStripStatusLabel3.Visible = true;
            showMSTToolStripMenuItem.Enabled = true;
            //treeView1.Visible = ShowTreeNode;
            FTV.treeView1.Update();
            textBox1.AppendText("\r\nBranch Tree Total Node = " + BranchTreeNodeNumber + "\r\n");
        }
        

        private void tSPBBGRToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = tsp.GetN();
            textBox1.AppendText("\r\nComputing LB.....");
            toolStripStatusLabel1.Text = "Working...";
            toolStripStatusLabel4.Text = toolStripStatusLabel3.Text = toolStripStatusLabel2.Text = "";
            toolStripStatusLabel4.Visible = toolStripStatusLabel3.Visible = toolStripStatusLabel2.Visible = true;
            //c2lboptparam lb_opt = flb.lb_opt;
            //FTV.treeView1.Nodes.Clear();

            toolStripStatusLabel_d2n.Visible = true;
            StartComputation();

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork +=new DoWorkEventHandler(BB_restore);
            bw.RunWorkerCompleted+=new RunWorkerCompletedEventHandler(BB_restore_comp);
            bw.RunWorkerAsync();
        }

        private void archiCertiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsp.Build_pool_arcs(false);
            textBox1.AppendText("\r\n TOT = "+tsp.pool_arcs.Length.ToString());
            for (int i = 0; i < tsp.pool_arcs.Length; i++)
            {
                textBox1.AppendText("\r\n(" + tsp.pool_arcs[i].a + "," + tsp.pool_arcs[i].b + ")");
            }
        }

        private void archiCertiRedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsp.Build_pool_arcs(true);
            textBox1.AppendText("\r\n TOT = " + tsp.pool_arcs.Length.ToString());
            for (int i = 0; i < tsp.pool_arcs.Length; i++)
            {
                textBox1.AppendText("\r\n(" + tsp.pool_arcs[i].a + "," + tsp.pool_arcs[i].b + ")");
            }
        }

    }
}
