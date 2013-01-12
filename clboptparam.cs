using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    public class clboptparam
    {
        public bool UBshowprog;
        public byte UBalgo;              //1=std; 2=prec; 3=old
        public byte UBstart;             //1=seq; 2=ran;  3=nn; 4=cur; 5= chepeast Inertion
        public int UBnode0;
        public bool _3opt;
        public int LBnode0;
        public bool ShowProg;
        public bool compAsInt;
        public bool BestLB;
        public bool AlphaNearness;
        public bool StrongPruning;
        public int alpha_num;
        public bool std1treealg;
        public bool sort_split;
        public bool strong_sort_split;
        public int bb_rule;

        public bool use_pool_arcs;
        public bool pool_arcs_strong;

        public bool SubGradFast;
    }
}
