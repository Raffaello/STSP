using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class Graph
    {

        class Node
        {
            float x;
            float y;
            //List<Node> Neighborur;
            //int degree;
            int num;

            public Node(float _x, float _y, int _num)
            {
                x = _x;
                y = _y;
                num = _num;
                //degree = 0;
                //Neighborur = null;
            }

            public float GetX() { return x; }
            public float GetY() { return y; }
        }

        List<Node> graph;
    
        public Graph(cTSP tsp)
        {
            int i;
            Node n;

            graph = new List<Node>();
            for (i = 0; i < tsp.GetN(); i++)
            {
                n = new Node(tsp.Get_x(i), tsp.Get_y(i), i);
                graph.Add(n);
            }
        }

        //public List<Node> GetNode(int index)
        //{
        //    return graph[index];
        //}

        public float GetNodeX(int index) { return graph[index].GetX(); }

        public float GetNodeY(int index) { return graph[index].GetY(); } 

    }
}
