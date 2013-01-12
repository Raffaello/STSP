using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class c2opt
    {
        private int i;
        private int j;
        private bool ret;
        private byte algo;
        private bool showprog;
        private cTSP tsp;
        private int n2;
        private int n1;
        public int e1, e2, e3, e4, _j;
        public float diff;

        public c2opt(cTSP _tsp, byte _algo, bool _show)
        {
            tsp = _tsp;
            algo = _algo;
            showprog = _show;
            i = j = 0;
            ret = false;
            n2 = tsp.GetN() - 2;
            n1 = n2 + 1;
            diff = 0;
            _j = 0;
        }

        public bool GetResult()
        {
            return ret;
        }

        public bool do2opt()
        {
            bool res = false;

            switch (algo)
            {
                case 1 : res = std2opt();
                         break;
                case 2 : res = pre2opt();
                         break;
                case 3 : res = old2opt();
                         break;
            }

            return res;
        }

        private bool std2opt()
        {
            int imod, nj;

            if (i == n2)
            {
                ret = false;
                i = 0;
            }

            for (; i < n2; i++)
            {
                if (i == 0)
                    nj = n1;
                else
                    nj = tsp.GetN();

                e1 = tsp.GetTourNode(i);
                e2 = tsp.GetTourNode((imod = i + 1));
                if (_j > 0)
                {
                    j = _j;
                    _j = 0;
                }
                else j = i + 2;

                for (; j < nj; j++)
                {
                    e3 = tsp.GetTourNode(j);
                    e4 = tsp.GetTourNode((j + 1) % tsp.GetN());

                    diff = (tsp.GetDistance(e2, e4) + tsp.GetDistance(e1, e3)) - (tsp.GetDistance(e1, e2) + tsp.GetDistance(e3, e4));
                    if (diff < 0)
                    {
                        int ii, jj;
                        int tmp;

                        tsp.setTourNode(imod, e3);
                        tsp.setTourNode(j, e2);

                        for (ii = imod + 1, jj = j - 1; ((jj > ii) || (ii < jj)); jj--, ii++)
                        {
                            tmp = tsp.GetTourNode(ii);
                            tsp.setTourNode(ii, tsp.GetTourNode(jj));
                            tsp.setTourNode(jj, tmp);
                        }

                        if (showprog)
                        {
                            //tsp.UpdateTourLength(diff);
                            _j = j;
                            return ret = true;
                        }
                        else
                        {
                            ret = true;
                            e2 = tsp.GetTourNode(i + 1);
                        }
                        //return ret = true;
                    }
                }
            }

            return false;
        }

        public bool pre2opt()
        {
            int imod, nj;
            int b2, b3,b1,b4;
            float best;
            int ii, jj;

            ii = jj= 0;
            b1 = b2 = b3 = b4 = 0;

            best = 0;
            ret = false;
            for (i=0; i < n2; i++)
            {
                if (i == 0)
                    nj = n1;
                else
                    nj = tsp.GetN();

                e1 = tsp.GetTourNode(i);
                e2 = tsp.GetTourNode((imod = i + 1));

                for (j=i+2; j < nj; j++)
                {
                    e3 = tsp.GetTourNode(j);
                    e4 = tsp.GetTourNode((j + 1) % tsp.GetN());

                    diff = (tsp.GetDistance(e2, e4) + tsp.GetDistance(e1, e3)) - (tsp.GetDistance(e1, e2) + tsp.GetDistance(e3, e4));
                    if (diff < best)
                    {
                        best = diff;
                        b2 = e2;
                        b3 = e3;
                        b1 = e1;
                        b4 = e4;
                        ii = imod;
                        jj = j;
                    }
                }
            }

            if (best < 0)
            {
                int tmp;

                tsp.setTourNode(ii, b3);
                tsp.setTourNode(jj, b2);

                for (ii++, jj--; ((jj > ii) || (ii < jj)); jj--, ii++)
                {
                    tmp = tsp.GetTourNode(ii);
                    tsp.setTourNode(ii, tsp.GetTourNode(jj));
                    tsp.setTourNode(jj, tmp);
                }

                ret = true;
                if (showprog)
                {
                    e2 = b2;
                    e3 = b3;
                    e1 = b1 ;
                    e4 = b4;
                }
            }

            
            return ret;
        }

        public bool old2opt()
        {
            int imod, nj;

            ret = false;

            for (i=0; i < n2; i++)
            {
                if (i == 0)
                    nj = n1;
                else
                    nj = tsp.GetN();

                e1 = tsp.GetTourNode(i);
                e2 = tsp.GetTourNode((imod = i + 1));

                for (j=i+2; j < nj; j++)
                {
                    e3 = tsp.GetTourNode(j);
                    e4 = tsp.GetTourNode((j + 1) % tsp.GetN());

                    diff = (tsp.GetDistance(e2, e4) + tsp.GetDistance(e1, e3)) - (tsp.GetDistance(e1, e2) + tsp.GetDistance(e3, e4));
                    if (diff < 0)
                    {
                        int ii, jj;
                        int tmp;

                        tsp.setTourNode(imod, e3);
                        tsp.setTourNode(j, e2);

                        for (ii = imod + 1, jj = j - 1; ((jj > ii) || (ii < jj)); jj--, ii++)
                        {
                            tmp = tsp.GetTourNode(ii);
                            tsp.setTourNode(ii, tsp.GetTourNode(jj));
                            tsp.setTourNode(jj, tmp);
                        }

                        return ret = true;
                    }
                }
            }
            return ret = false;
        }
    }
}
