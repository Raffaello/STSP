using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class cTSP_LIST_NODE
    {
        public cTSP_LIST_NODE Prev;
        public cTSP_LIST_NODE Next;
        public int Value;
    }

    class cTSP_LIST
    {
        cTSP_LIST_NODE list,first,last;
        cTSP tsp;

        public cTSP_LIST(cTSP _tsp)
        {
            tsp = _tsp;
            BuildFromTour();
        }

        private void BuildFromTour()
        {
            int i;
            cTSP_LIST_NODE l;

            l = list = first = new cTSP_LIST_NODE();
            for(i=0;i<tsp.GetN()-1;i++,l = l.Next)
            {
                l.Value = tsp.GetTourNode(i);
                l.Next = new cTSP_LIST_NODE();
                l.Next.Prev = l;
            }
            l.Value = tsp.GetTourNode(i);
            l.Next = first;
            first.Prev = l;
            last = l;
        }

        public void BuildToTour()
        {
            int i;
            cTSP_LIST_NODE l;
            l = first;

            for (i = 0; i < tsp.GetN(); i++,l=l.Next)
                tsp.setTourNode(i, l.Value);
        }

        public cTSP_LIST_NODE First { get { return first; } }

        public void Reverse(cTSP_LIST_NODE from, cTSP_LIST_NODE to)
        {
            cTSP_LIST_NODE l1, l2, l3, l4, l5, l6;

            l1 = from;
            l2 = to;
            l3 = from.Next;
            l4 = to.Prev;

            while (((l3.Prev != l4) && (l4.Next != l3)) && (l1 != l2))
            {
                //assign
                l1.Next = l4;
                l2.Prev = l3;
                l5 = l4.Prev;
                l4.Prev = l1;
                l6 = l3.Next;
                l3.Next = l2;
                //shift
                l1 = l4;
                l2 = l3;
                l4 = l5;
                l3 = l6;
            }
        }

        public cTSP_LIST_NODE Find(int v)
        {
            cTSP_LIST_NODE l1,l2;

            l2=l1=first;

            do
            {
                l2 = l2.Prev;
                if (l1.Value == v)
                    return l1;
                l1 = l1.Next;
                if (l2.Value == v)
                    return l2;
            } while ((l1 != l2) && (l1.Prev != l2));

            return null;
        }
    }
}
