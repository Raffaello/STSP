using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class cTSP_Tree
    {
        public class Node
        {
            public int city;
            public Node Child;
            public Node Next;
            public Node dad;
            public int city_mask; //per permutazione nodi

            public Node()
            {
                //this.city = 1;
                this.city_mask = 1;
                this.dad=null;
                this.Next=null;
                this.Child=null;
            }

            public Node(int _city)
            {
                this.city = _city;
                this.city_mask = 0;
                this.dad=null;
                this.Next=null;
                this.Child=null;
            }
        }

        //private List<Node> Tree;
        private Node Root;

        public cTSP_Tree(int city)
        {
            
            Root = new Node();
            Root.city = city;
            Root.city_mask = 1;
        }

        public Node GetNode(Node node, int city)
        {
            Node c, tmp;

            if (node.city == city)
                return node;

            c = node.Child;
            while (c != null)
            {
                if (c.city == city)
                    return c;
                tmp = GetNode(c, city);
                if (tmp != null)
                    return tmp;
                c = c.Next;
            }

            return null;
        }

        public bool Insert_Node(int city, Node node, ref int mask)
        {
            Node p;
            if (Root == null)
            {
                Root = node;
                
            //    p = Root;
            //    //Root.city_mask = city;
            //    node = Root;
               
            }
            else
            {
                p = GetNode(Root, city);
                if (p == null)
                    return false;

                node.Next = p.Child;
                p.Child = node;
                node.dad = p;

                if (node.dad!=null)
                {
                    if (node.dad.city_mask < mask)
                        node.city_mask = mask;
                    else
                        mask = node.dad.city_mask + 1;
                }
            }
            
            return true;
        }

        public Node GetNodeMask(Node node, int mask)
        {
            Node c, tmp;

            if (node.city_mask == mask)
                return node;

            c = node.Child;
            while (c != null)
            {
                if (c.city_mask == mask)
                    return c;
                tmp = GetNodeMask(c, mask);
                if (tmp != null)
                    return tmp;
                c = c.Next;
            }

            return null;
        }

        public int GetParent(int city)
        {
            //return GetNode(Root, city).dad.city;
            Node ret = GetNode(Root, city);
            if (ret == null)
                return -1; //dovrebbe essere il nodo0, quindi esclusa da beta.
            else if (ret.dad == null)
                if(ret==Root)
                    return 0;
                else
                    return -2;
            else
                return ret.dad.city;
        }

        public int GetParentMask(int mask)
        {
            Node ret = GetNodeMask(Root, mask);
            if (ret == null)
                return -1;
            else if (ret.dad == null)
                if(ret==Root)
                    return 0;
                else
                    return -2;
            else
                return ret.dad.city_mask;
        }
        
        public int GetMask(int city)
        {
            //return GetNode(Root, city).city_mask;
            Node ret = GetNode(Root, city);
            if (ret == null)
                return -1;
            else
                return ret.city_mask;

        }
        public int GetCity(int mask)
        {
            //return GetNodeMask(Root, mask).city;
            Node ret = GetNodeMask(Root, mask);
            if (ret == null)
                return -1;
            else
                return ret.city;
        }
    }
}
