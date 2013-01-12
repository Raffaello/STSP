using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TSP
{
    class c3opt
    {
        private struct TSWAP
        {
            public int a, b, c, d, e, f;
            public byte swap;
        }

        private byte algo;
        private int i;
        private bool showprog, ret;
        private cTSP tsp;
        private int n, nn;
        private float min;
        private TSWAP t;
        private TSWAP best_swap;

        private const float EPSILON = (float)(-1.0e-4); //approximazione di 0 meno

        private bool fast;

        cTSP_LIST list_tour;
        cTSP_LIST_NODE l1, l2, l3, l1b, l2b, l3b;

        float gain;

        public float diff { get { return gain; } }

        public bool GetResult()
        {
            return ret;
        }

        public c3opt(cTSP _tsp, byte _algo, bool _show, int _nn, bool _fast)
        {
            tsp = _tsp;
            n = tsp.GetN();
            algo = _algo;
            showprog = _show;
            nn = _nn;
            fast = _fast;

            list_tour = new cTSP_LIST(_tsp);
            l1 = list_tour.First;
        }

        public bool do3opt()
        {
            bool res = false;

            switch (algo)
            {
                case 1: res = std3opt();
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }

            if (res == false)
                list_tour.BuildToTour();

            return res;
        }

        private byte Best3optSwap()
        {
            t.swap = 0;

            if (t.a != t.f)
            {
                gain = tsp.distance[t.a,t.c] + tsp.distance[t.b, t.e] + tsp.distance[t.d, t.f];
                //t2
                if (gain < min)
                {
                    t.swap = 1;
                    min = gain;
                }
                //t3
                gain = tsp.distance[t.a, t.d] + tsp.distance[t.e, t.c] + tsp.distance[t.b, t.f];
                if (gain < min)
                {
                    t.swap = 2;
                    min = gain;
                }
                //t8
                gain = tsp.distance[t.a, t.c] + tsp.distance[t.b, t.d] + tsp.distance[t.e, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 7;
                }
                //t4
                gain = tsp.distance[t.a, t.e] + tsp.distance[t.d, t.b] + tsp.distance[t.c, t.f];
                if (gain < min)
                {
                    t.swap = 3;
                    min = gain;
                }
                //t5
                gain = tsp.distance[t.a, t.d] + tsp.distance[t.e, t.b] + tsp.distance[t.c, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 4;
                }
                //t6
                gain = tsp.distance[t.a, t.b] + tsp.distance[t.c, t.e] + tsp.distance[t.d, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 5;
                }
                //t7
                gain = tsp.distance[t.a, t.e] + tsp.distance[t.d, t.c] + tsp.distance[t.b, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 6;
                }
            }
            //-------------------- FINE case 1 (i 4 casi 3opt con 6 nodi differenti)
            else //---- t.a == t.f
            {
                //t9
                gain = tsp.distance[t.a, t.d] + tsp.distance[t.e, t.b] + tsp.distance[t.c, t.f];
                if (gain < min)
                {
                    t.swap = 8;
                    min = gain;
                }
                //t 10
                gain = tsp.distance[t.a, t.b] + tsp.distance[t.c, t.e] + tsp.distance[t.d, t.f];
                if (gain < min)
                {
                    t.swap = 9;
                    min = gain;
                }
                //t11
                gain = tsp.distance[t.a, t.c] + tsp.distance[t.b, t.d] + tsp.distance[t.e, t.f];
                if (gain < min)
                {
                    t.swap = 10;
                    min = gain;
                }
            }

            return t.swap;
        }

        //rimosso il parametro per errori di arrotondamento dei float (looppava)
        private byte Best3optSwap2(/*float min*/)
        {
            //float m=min; //rimosso per errori arrotondamento
            //di consequenza tutte le volte ricalcolo come con gain... 2 somme in + ogni volta che chiama la funzione, ma almeno + preciso.
            float m = tsp.distance[t.a, t.b] + tsp.distance[t.c, t.d] + tsp.distance[t.e, t.f];
            float min = m;
            t.swap = 0;

            {
                //t2 
                gain = tsp.distance[t.a, t.c] + tsp.distance[t.b, t.e] + tsp.distance[t.d, t.f];
                if (gain < min)
                {
                    t.swap = 1;
                    min = gain;
                }
                //t3
                gain = tsp.distance[t.a, t.d] + tsp.distance[t.e, t.c] + tsp.distance[t.b, t.f];
                if (gain < min)
                {
                    t.swap = 2;
                    min = gain;
                }
                //t8 (se t.c==t.b no swap)
                gain = tsp.distance[t.a, t.c] + tsp.distance[t.b, t.d] + tsp.distance[t.e, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 7;
                }
                //t4
                gain = tsp.distance[t.a, t.e] + tsp.distance[t.d, t.b] + tsp.distance[t.c, t.f];
                if (gain < min)
                {
                    t.swap = 3;
                    min = gain;
                }
                //t5 
                gain = tsp.distance[t.a, t.d] + tsp.distance[t.e, t.b] + tsp.distance[t.c, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 4;
                }
                //t6 (se t.e==t.d no swap)
                gain = tsp.distance[t.a, t.b] + tsp.distance[t.c, t.e] + tsp.distance[t.d, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 5;
                }
                //t7 (se t.e==t.b no swap ma impossibile per i loop dei 3 for)
                gain = tsp.distance[t.a, t.e] + tsp.distance[t.d, t.c] + tsp.distance[t.b, t.f];
                if (gain < min)
                {
                    min = gain;
                    t.swap = 6;
                }
            }
            gain = min - m;
            return t.swap;
        }

        private void swapcase1()
        {
            int etmp;
            cTSP_LIST_NODE b, d, f, a, c, e;

            a = l1;
            c = l2;
            e = l3;
            b = a.Next;
            d = c.Next;
            f = e.Next;

            switch (t.swap)
            {
                case 1: //t2
                    list_tour.Reverse(a, d);
                    t.b = t.c;
                    t.c = b.Value;
                    list_tour.Reverse(d.Prev, f);
                    t.d = t.e;
                    t.e = d.Value;
                    l2 = b;
                    l3 = d;
                    break;
                case 2: //t3
                    list_tour.Reverse(a, d);
                    a.Next = d; d.Prev = a;
                    e.Next = c; c.Prev = e;
                    b.Next = f; f.Prev = b;
                    etmp = t.b;
                    t.b = t.d;
                    t.d = t.c;
                    t.c = t.e;
                    l2 = e;
                    l3 = b;
                    break;
                case 3: // t4
                    list_tour.Reverse(c, f);
                    a.Next = e; e.Prev = a;
                    d.Next = b; b.Prev = d;
                    c.Next = f; f.Prev = c;
                    etmp = t.b;
                    t.b = t.e;
                    t.c = t.d;
                    t.d = etmp;
                    l2 = d;
                    l3 = c;
                    break;
                case 4: // t5
                    a.Next = d; d.Prev = a;
                    e.Next = b; b.Prev = e;
                    c.Next = f; f.Prev = c;
                    etmp = t.b;
                    t.b = t.d;
                    t.d = etmp;
                    t.c = t.e;
                    l2 = b.Prev;
                    l3 = f.Prev;
                    break;
                case 5: //t6
                    list_tour.Reverse(c, f);
                    c.Next = e; e.Prev = c;
                    d.Next = f; f.Prev = d;
                    t.d = t.e;
                    l3 = d;
                    break;
                case 6: //t7
                    list_tour.Reverse(a, f);
                    etmp = t.c;
                    t.c = t.d;
                    t.d = etmp;
                    t.b = t.e;
                    l2 = d;
                    l3 = b;
                    break;
                case 7: //t8
                    list_tour.Reverse(a, d);
                    a.Next = c; c.Prev = a;
                    b.Next = d; d.Prev = b;
                    etmp = t.b;
                    t.b = t.c;
                    t.c = etmp;
                    l2 = b;
                    break;
                case 8: // t9
                    a.Next = d; d.Prev = a;
                    e.Next = b; b.Prev = e;
                    c.Next = f; f.Prev = c;
                    etmp = t.b;
                    t.b = t.d;
                    t.d = etmp;
                    t.c = t.e;
                    l2 = e;
                    l3 = c;
                    break;
                case 9: //t10
                    list_tour.Reverse(c, f);
                    c.Next = e; e.Prev = c;
                    d.Next = f; f.Prev = d;
                    t.d = t.e;
                    break;
                case 10: //t11
                    list_tour.Reverse(a, d);
                    a.Next = c; c.Prev = a;
                    b.Next = d; d.Prev = b;
                    etmp = t.b;
                    t.b = t.c;
                    t.c = etmp;
                    l2 = b;
                    break;
            }

        }

        private void swapcase2(TSWAP t, cTSP_LIST_NODE l1, cTSP_LIST_NODE l2, cTSP_LIST_NODE l3)
        {
            int etmp;
            cTSP_LIST_NODE b, d, f, a, c, e;

            a = l1;
            c = l2;
            e = l3;
            b = a.Next;
            d = c.Next;
            f = e.Next;

            switch (t.swap)
            {
                case 1: //t2
                    list_tour.Reverse(a, d);
                    t.b = t.c;
                    t.c = b.Value;
                    list_tour.Reverse(d.Prev, f);
                    t.d = t.e;
                    break;
                case 2: //t3
                    list_tour.Reverse(a, d);
                    a.Next = d; d.Prev = a;
                    e.Next = c; c.Prev = e;
                    b.Next = f; f.Prev = b;
                    t.b = t.d;
                    t.d = t.c;
                    t.c = t.e;
                    break;
                case 3: // t4
                    list_tour.Reverse(c, f);
                    a.Next = e; e.Prev = a;
                    d.Next = b; b.Prev = d;
                    c.Next = f; f.Prev = c;
                    etmp = t.b;
                    t.b = t.e;
                    t.c = t.d;
                    t.d = etmp;
                    break;
                case 4: // t5
                    a.Next = d; d.Prev = a;
                    e.Next = b; b.Prev = e;
                    c.Next = f; f.Prev = c;
                    etmp = t.b;
                    t.b = t.d;
                    t.d = etmp;
                    t.c = t.e;
                    break;
                case 5: //t6
                    list_tour.Reverse(c, f);
                    c.Next = e; e.Prev = c;
                    d.Next = f; f.Prev = d;
                    t.d = t.e;
                    break;
                case 6: //t7
                    list_tour.Reverse(a, f);
                    etmp = t.c;
                    t.c = t.d;
                    t.d = etmp;
                    t.b = t.e;
                    break;
                case 7: //t8
                    list_tour.Reverse(a, d);
                    a.Next = c; c.Prev = a;
                    b.Next = d; d.Prev = b;
                    etmp = t.b;
                    t.b = t.c;
                    t.c = etmp;
                    break;
            }

        }

        bool std3optCandidateSet()
        {
            ret = false;

            l1 = list_tour.First;
            l1b = l1.Prev; 

            for (; !l1.Equals(l1b); l1 = l1.Next)
            {
                t.a = l1.Value;
                t.b = l1.Next.Value;
                l2b = l1b.Prev.Prev;

                for (i = 0; i < nn; i++)
                {
                    l2 = list_tour.Find(tsp.GetNN_Node(l1.Value, i));
                    if (l2 == null)
                        throw new NotSupportedException("nodo non trovato");
                    if (l2.Equals(l1))
                        continue;
                    if ((l2.Equals(l1.Next)) || (l2.Next.Equals(l1)))
                        continue;

                    t.c = l2.Value;
                    t.d = l2.Next.Value;

                    //da mettere come opzione!!!
                    //Fast
                    if ((fast) && (tsp.distance[t.a, t.b] <= tsp.distance[t.b, t.c]))
                        continue;


                    l3 = l2.Next.Next;
                    min = tsp.distance[t.a, t.b] + tsp.distance[t.c, t.d];

                    for (; !l3.Equals(l1); l3 = l3.Next)
                    {
                        if ((l3.Equals(l1.Next)) ||
                        (l3.Equals(l2)) || (l3.Equals(l2.Next)) ||
                        (l3.Next.Equals(l2)))
                            continue;

                        t.e = l3.Value;
                        t.f = l3.Next.Value;

                        if ((t.swap = Best3optSwap()) > 0)
                        {
                            ret = true;

                            swapcase1();

                            if ((t.swap != 5) && (i != 9))
                                i = 0;
                            min = tsp.distance[t.a, t.b] + tsp.distance[t.c, t.d];

                        }

                    }//for

                }//for

            }//for
            return ret;
        }

        bool std3opt()
        {
            //do
           // {
                ret = false;
                float best = float.MaxValue;

                cTSP_LIST_NODE b1 = null, b2 = null, b3 = null;

                l1 = list_tour.First;
                l1b = l1.Prev.Prev; //n-1
                for (; !l1.Equals(l1b); l1 = l1.Next)
                {
                    t.a = l1.Value;
                    t.b = l1.Next.Value;
                    l2b = list_tour.First.Prev; //n-1
                    l3b = list_tour.First;
                    
                    for (l2 = l1.Next; !l2.Equals(l2b); l2 = l2.Next)
                    {
                        t.c = l2.Value;
                        t.d = l2.Next.Value;
                        if (t.d == t.a)
                            break;
 
                        for (l3 = l2.Next; !l3.Equals(l3b); l3 = l3.Next)
                        {
                            t.e = l3.Value;
                            t.f = l3.Next.Value;

                            if ((t.swap = Best3optSwap2(/*min+tsp.distance[t.e,t.f]*/)) > 0.0)
                            {
                                if (best > gain)
                                {
                                    best = gain;
                                    best_swap = t;
                                    b1 = l1;
                                    b2 = l2;
                                    b3 = l3;

                                }
                            }

                        }//for

                    }//for

                }//for


                if (best < EPSILON)
                {
                    ret = true;
                    swapcase2(best_swap, b1, b2, b3);
                }
            //} while (ret);

            return ret;
        }//std3opt

    }//class
}
