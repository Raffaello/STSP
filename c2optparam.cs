using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class c2optparam
    {
        public int start_node;
        public byte algo;
        public byte itour;
        public bool show;

        public c2optparam(int _s, byte _a, byte _i, bool sh)
        {
            start_node = _s;
            algo = _a;
            itour = _i;
            show = sh;
        }
    }

    class c3optparam : c2optparam
    {
        public c3optparam(int _s, byte _a, byte _i, bool sh, int _nn, bool _fast)
            : base(_s, _a, _i, sh)
        {
            numNN = _nn;
            fast = _fast;
        }
        public int numNN;
        public bool fast;

    }
}
