using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class NNThread
    {
        private int node_start;
        private int node_stop;
        private int best_node;
        private float best_value;
        public bool show;

        public NNThread(int nstart, int nstop, bool s)
        {
            node_start = nstart;
            node_stop = nstop;
            show = s;
        }

        public int GetBestNode()
        {
            return best_node;
        }

        public void SetBest(int node, float v)
        {
            best_node = node;
            best_value = v;
        }

        public float GetBestValue()
        {
            return best_value;
        }

        public int GetStart()
        {
            return node_start;
        }

        public int GetStop()
        {
            return node_stop;
        }

    }
}
