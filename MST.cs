using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TSP
{
    public struct EDGE_LIST
    {
        public int a;
        public int b;
    }

    //class EdgeComparer : Comparer<EDGE_LIST>
    //{
    //    float[,] dist;
        
    //    public EdgeComparer(float[,] _dist)
    //    {
    //        dist = _dist;
    //    }

    //    public override int Compare(EDGE_LIST x, EDGE_LIST y)
    //    {
    //        //throw new NotImplementedException();
    //        return dist[x.a, x.b].CompareTo(dist[y.a, y.b]);
    //    }
    //}

    

    class MST
    {
        int n,n1,n2;
        public int N { get; private set; }
        public int N_old { get; private set; }
        EDGE_LIST[] lb;
        public EDGE_LIST[] tmp;  // lista di vertici lower bound MST 1-tree
        DISJOINT_SET[] set;
        int ALPHA_NA;

        float min;
        //Random rand;

        public MST(int _n)
        {
            n = _n;
            n1 = n - 1;
            n2 = n - 2;
            N_old = ((n1) * (n2)) >> 1;
            N = ((n1) * (n)) >> 1;
            tmp = new EDGE_LIST[N];
            lb = new EDGE_LIST[n];
            set = new DISJOINT_SET[n];
            ALPHA_NA = 0;
            //rand = new Random();

        }


        struct DISJOINT_SET
        {
            int rank;    //rango
            int parent;  //


            internal void Make_Set(DISJOINT_SET[] set, int x)
            {
                set[x].parent = x;
                set[x].rank = 0;
            }

            public void Union(DISJOINT_SET[] set, int x, int y)
            {
                Link(set, Find_Set(set, x), Find_Set(set, y));
            }

            public void Link(DISJOINT_SET[] set, int x, int y)
            {
                if (set[x].rank > set[y].rank)
                    set[y].parent = x;
                else
                {
                    set[x].parent = y;
                    if (set[x].rank == set[y].rank)
                        set[y].rank++;
                }
            }

            public int Find_Set(DISJOINT_SET[] set, int x)
            {
                if (x != set[x].parent)
                    set[x].parent = Find_Set(set, set[x].parent);

                return set[x].parent;
            }
        }

        public EDGE_LIST[] Get_lb()
        {
            return lb;
        }

//        int PartitionR_Edges(EDGE_LIST[] edges, float[,] d, int p, int r)
//        {
//            int i = rand.Next(p, r+1);
//#if DEBUG
//            if ((i < p) || (i > r))
//                System.Windows.Forms.MessageBox.Show("p=" + p + " r=" + r + " i=" + i);
//#endif
//            EDGE_LIST e_tmp = edges[r];
//            edges[r] = edges[i];
//            edges[i] = e_tmp;
//            return Partition_Edges(edges, d, p, r);
//        }

        int Partition_Edges(EDGE_LIST[] edges, float[,] d, int p, int r)
        {
            int i, j;
            float w;
            EDGE_LIST e_tmp;
            w = d[edges[r].a,edges[r].b];
            i = p;
           
            for (j = p; j < r; j++)
            {
                if (d[edges[j].a,edges[j].b] <= w)
                {
                    //i = i + 1;
                    e_tmp = edges[i];
                    edges[i++] = edges[j];
                    edges[j] = e_tmp;
                }
            }
            e_tmp = edges[i];
            edges[i] = edges[r];
            edges[r] = e_tmp;
            return i;
        }
       
        
        public void QuickSort_Edges(EDGE_LIST[] edges, float[,] d, int p, int r)
        {
            int q;
            if (p<r)
            {
                q = Partition_Edges(edges, d, p, r);
                QuickSort_Edges(edges, d, p, q - 1);
                QuickSort_Edges(edges, d, q + 1, r);
            }
            
        }

        //void QuickSortR_Edges(EDGE_LIST[] edges, float[,] d, int p, int r)
        //{
        //    int q;
        //    if (p < r)
        //    {
        //        q = PartitionR_Edges(edges, d, p, r);
        //        QuickSortR_Edges(edges, d, p, q - 1);
        //        QuickSortR_Edges(edges, d, q + 1, r);
        //    }
        //}

        public float Kruskal_MST(float[,] dist, int[,] sort)
        {
            int j, n1;
            int k, i;   //(n)*(n+1)/2
            //EDGE_LIST[] tmp;  // lista di vertici lower bound MST 1-tree
            //DISJOINT_SET[] set;  //foresta di insieme disjunti.
            //int[] sort;

            //alloco la memoria necessaria.
            //tmp = new EDGE_LIST[N];
            //lb = new EDGE_LIST[n];
            //set = new DISJOINT_SET[n];
            //sort = new int[n];

            //Initializzo il set e il vettore degli indici delle distanze ordinate
            for (i = 0; i < n; i++)
            {
                set[i].Make_Set(set, i);
            }

            //costruisco le coppie di archi.
            n1 = n - 1;
            for (i = 0, k = 0; i < n1; i++)
            {
                for (j = i + 1; j < n; j++)
                {
                    tmp[k].a = i;
                    tmp[k++].b = j;
                }
            }

            //Ordino le coppie di archi
            QuickSort_Edges(tmp, dist, 0, N - 1);
            //Ordino le distanze dal primo nodo con tutti gli altri
            //Quick_Sort_dist(sort,dist[0,0],0,n1);


            k = 0;
            min = 0;
            for (i = 0; i < N; i++)
            {
                if ((set[0].Find_Set(set, tmp[i].a) != set[0].Find_Set(set, tmp[i].b)))
                {
                    lb[k++] = tmp[i];
                    min += dist[tmp[i].a, tmp[i].b];
                    set[0].Union(set, tmp[i].a, tmp[i].b);
                }
            }

            //inserisco gli ultimi 2 archi.
            //lb[k].a = node0;
            //lb[k++].b = sort[node0,1];
            //min += dist[0, sort[node0,1]];
            //lb[k].a = node0;
            //lb[k++].b = sort[node0,2];
            //min += dist[0, sort[node0,2]];

            //return lb;
            return min;
        }

        private int BuildEdges_old(EDGE_LIST[] exluded_edge, int node0)
        {
            int i, j, i2, j2,k;
            //int n1 = n - 1;
            bool a;

            for (i = 0, k = 0; i < n1; i++)
            {
                if (i == node0)
                    continue;
                
                for (i2 = 0,a=false; i2 < exluded_edge.Length; i2++)
                {
                    if ((i == exluded_edge[i2].a))
                    {
                        a=true;
                        break;
                    }
                }

                for (j = i + 1; j < n; j++)
                {
                    if (j == node0)
                        continue;
                    

                    if(a)
                    {
                        for(j2=0;j2<exluded_edge.Length;j2++)
                        {
                            if(((i==exluded_edge[j2].a))&&(j==exluded_edge[j2].b))
                                break;
                        }
                        if (j2 == exluded_edge.Length)
                        {
                            tmp[k].a = i;
                            tmp[k++].b = j;
                        }

                    }
                    else
                    {
                        tmp[k].a = i;
                        tmp[k++].b = j;
                    }
                }
            }

            return k;


        }

        private int BuildEdges(EDGE_LIST[] exluded_edge)
        {
            int i, j, i2, j2, k;
            //int n1 = n - 1;
            bool a;

            for (i = 0, k = 0; i < n1; i++)
            {
                //if (i == node0)
                //    continue;

                for (i2 = 0, a = false; i2 < exluded_edge.Length; i2++)
                {
                    if ((i == exluded_edge[i2].a))
                    {
                        a = true;
                        break;
                    }
                }

                for (j = i + 1; j < n; j++)
                {
                    //if (j == node0)
                    //    continue;
                    if (a)
                    {
                        for (j2 = 0; j2 < exluded_edge.Length; j2++)
                        {
                            if (((i == exluded_edge[j2].a)) && (j == exluded_edge[j2].b))
                                break;
                        }
                        if (j2 == exluded_edge.Length)
                        {
                            tmp[k].a = i;
                            tmp[k++].b = j;
                        }

                    }
                    else
                    {
                        tmp[k].a = i;
                        tmp[k++].b = j;
                    }
                }
            }

            return k;


        }

        private int BuildEdgesAlpha(EDGE_LIST[] exluded_edge, int node0, int[,] sort, int ALPHA_NN)
        {
            int i, i2, j2, k, jnn;
            //int n1 = n - 1;
            bool a;

            for (i = 0, k = 0; i < n1; i++)
            {
                //if (i == node0)
                //    continue;

                for (i2 = 0, a = false; i2 < exluded_edge.Length; i2++)
                {
                    if ((i == exluded_edge[i2].a))
                    {
                        a = true;
                        break;
                    }
                }

                for (jnn = 0; (jnn < ALPHA_NN); jnn++)
                {
                    if (/*(sort[i,jnn] == node0)||*/(sort[i, jnn] <= i))
                        continue;
                    if (a)
                    {
                        for (j2 = 0; j2 < exluded_edge.Length; j2++)
                        {
                            if (((i == exluded_edge[j2].a)) && (sort[i, jnn] == exluded_edge[j2].b))
                                break;
                        }
                        if (j2 == exluded_edge.Length)
                        {
                            if (i > sort[i, jnn])
                            {
                                tmp[k].a = sort[i, jnn];
                                tmp[k++].b = i;
                            }
                            else
                            {
                                tmp[k].a = i;
                                tmp[k++].b = sort[i, jnn];
                            }
                        }

                    }
                    else
                    {
                        if (i > sort[i, jnn])
                        {
                            tmp[k].a = sort[i, jnn];
                            tmp[k++].b = i;
                        }
                        else
                        {
                            tmp[k].a = i;
                            tmp[k++].b = sort[i, jnn];
                        }
                    }
                }
            }

            return k;


        }

        private int BuildEdgesAlpha_old(EDGE_LIST[] exluded_edge, int node0, int[,] sort, int ALPHA_NN)
        {
            int i, i2, j2, k, jnn;
            //int n1 = n - 1;
            bool a;

            for (i = 0, k = 0; i < n; i++)
            {
                if (i == node0)
                    continue;

                for (i2 = 0, a = false; i2 < exluded_edge.Length; i2++)
                {
                    if ((i == exluded_edge[i2].a))
                    {
                        a = true;
                        break;
                    }
                }

                for (jnn = 0; (jnn < ALPHA_NN); jnn++)
                {
                    if ((sort[i, jnn] == node0) || (sort[i, jnn] <= i))
                        continue;
                    if (a)
                    {
                        for (j2 = 0; j2 < exluded_edge.Length; j2++)
                        {
                            if (((i == exluded_edge[j2].a)) && (sort[i, jnn] == exluded_edge[j2].b))
                                break;
                        }
                        if (j2 == exluded_edge.Length)
                        {
                            if (i > sort[i, jnn])
                            {
                                tmp[k].a = sort[i, jnn];
                                tmp[k++].b = i;
                            }
                            else
                            {
                                tmp[k].a = i;
                                tmp[k++].b = sort[i, jnn];
                            }
                        }

                    }
                    else
                    {
                        if (i > sort[i, jnn])
                        {
                            tmp[k].a = sort[i, jnn];
                            tmp[k++].b = i;
                        }
                        else
                        {
                            tmp[k].a = i;
                            tmp[k++].b = sort[i, jnn];
                        }
                    }
                }
            }

            return k;


        }

        public void BuildEdgeSet(int node0,EDGE_LIST[] ex_edges)
        {
            int i,k,j;

            //costruisco le coppie di archi.
            //n1 = n - 1;
            if (ex_edges == null)
            {
                for (i = 0, k = 0; i < n1; i++)
                {
                    if (i == node0)
                        continue;
                    for (j = i + 1; j < n; j++)
                    {
                        if (j == node0)
                            continue;

                        tmp[k].a = i;
                        tmp[k++].b = j;
                    }
                }
            }
            else
                k = BuildEdges_old(ex_edges, node0);
        }

        public void BuildEdgeSet_Alpha(int node0, EDGE_LIST[] ex_edges, int ALPHA_NN, int [,] sort)
        {
            int i, j2, k;
            //costruisco le coppie di archi.
            //n1 = n - 1;
            if (ex_edges == null)
            {
                for (i = 0, k = 0; i < n; i++)
                {
                    if (i == node0)
                        continue;

                    for (j2 = 0; (j2 < ALPHA_NN); j2++)
                    {
                        //trovare i nodi alpha 
                        if ((sort[i, j2] == node0) || (sort[i, j2] <= i))
                            continue;
                        tmp[k].a = i;
                        tmp[k++].b = sort[i, j2];
                    }
                }
            }
            else
                k = BuildEdgesAlpha_old(ex_edges, node0, sort, ALPHA_NN);

            ALPHA_NA = k;
        }

        
        public float Kruskal_1_tree_lb_old(float[,] dist, int[,] sort, int node0, EDGE_LIST[] ex_edges)
        {
            int j;
            int k, i;   //(n)*(n+1)/2

            //costruisco le coppie di archi.
            //n1 = n - 1;
            if (ex_edges == null)
            {
                for (i = 0, k = 0; i < n1; i++)
                {
                    if (i == node0)
                        continue;
                    for (j = i + 1; j < n; j++)
                    {
                        if (j == node0)
                            continue;

                        tmp[k].a = i;
                        tmp[k++].b = j;
                    }
                }
            }
            else
                k = BuildEdges_old(ex_edges, node0);


            //Initializzo il set e il vettore degli indici delle distanze ordinate
            for (i = 0; i < n; i++)
                set[i].Make_Set(set, i);

            //rand = new Random();
            //Ordino le coppie di archi
            QuickSort_Edges(tmp, dist, 0, N_old - 1);
            
            k = 0;
            min = 0;
            for (i = 0; i < N_old; i++)
            {
                if ((set[0].Find_Set(set, tmp[i].a) != set[0].Find_Set(set, tmp[i].b)))
                {
                    lb[k++] = tmp[i];
                    min += dist[tmp[i].a, tmp[i].b];
                    set[0].Union(set, tmp[i].a, tmp[i].b);
                }
            }

            //inserisco gli ultimi 2 archi.
            float min1=float.MaxValue,
                min2=float.MaxValue;
            int imin1=-1, 
                imin2=-1;
            bool skip;
            int count = 0;

            for (i = 0; i < n; i++)
            {
                if (i == node0)
                    continue;

                skip = false;
                if (ex_edges != null)
                {
                    for (j = 0; j < ex_edges.Length; j++)
                    {
                        if (((node0 == ex_edges[j].a) && (i == ex_edges[j].b)) ||
                           ((node0 == ex_edges[j].b) && (i == ex_edges[j].a)))
                        {
                            count++;
                            skip = true;
                            break;
                        }
                    }
                    if (count == n2)
                        return float.MinValue;

                    if (skip == true)
                        continue;
                }

                if (min1 > dist[node0, i])
                {
                    imin2 = imin1;
                    min2 = min1;

                    imin1 = i;
                    min1 = dist[node0, i];
                }
                else if (min2 > dist[node0, i])
                {
                    min2 = dist[node0, i];
                    imin2 = i;
                }
            }
#if DEBUG
                if ((imin1 == -1) || (imin2 == -1))
                    System.Windows.Forms.MessageBox.Show("imin1,imin2: errore calcolo 1-tree");
#endif


            lb[k].a = node0;
            lb[k++].b = imin1;
            
            lb[k].a = node0;
            lb[k].b = imin2;

            min += dist[node0, imin1] + dist[node0, imin2];
 
            return min;
        }

        public float Kruskal_1_tree_lb(float[,] dist, EDGE_LIST[] ex_edges)
        {
            int k, i,j;   

            //Initializzo il set e il vettore degli indici delle distanze ordinate
            for (i = 0; i < n; i++)
                set[i].Make_Set(set, i);

            if (ex_edges == null)
            {
                for (i = 0, k = 0; i < n1; i++)
                {
                    for (j = i + 1; j < n; j++)
                    {
                        tmp[k].a = i;
                        tmp[k++].b = j;
                    }
                }
            }
            else
                k = BuildEdges(ex_edges);

            //Ordino le coppie di archi
            QuickSort_Edges(tmp, dist, 0, N - 1);


            k = 0;
            min = 0;
            for (i = 0; i < N; i++)
            {
                if ((set[0].Find_Set(set, tmp[i].a) != set[0].Find_Set(set, tmp[i].b)))
                {
                    lb[k++] = tmp[i];
                    min += dist[tmp[i].a, tmp[i].b];
                    set[0].Union(set, tmp[i].a, tmp[i].b);
                }
            }
            return min;
        }

        public float Kruskal_1_tree_lb_alpha_old(int ALPHA_NN, float[,] dist, int[,] sort, int node0, EDGE_LIST[] ex_edges)
        {
            int j;
            int k, i,j2;   //(n)*(n+1)/2

            //Initializzo il set e il vettore degli indici delle distanze ordinate
            for (i = 0; i < n; i++)
                set[i].Make_Set(set, i);

            //costruisco le coppie di archi.
            if (ex_edges == null)
            {
                for (i = 0, k = 0; i < n; i++)
                {
                    if (i == node0)
                        continue;

                    for (j2 = 0; (j2 < ALPHA_NN); j2++)
                    {
                        //trovare i nodi alpha 
                        if ((sort[i, j2] == node0) || (sort[i, j2] <= i))
                            continue;
                        tmp[k].a = i;
                        tmp[k++].b = sort[i, j2];
                    }
                }
            }
            else
                k = BuildEdgesAlpha_old(ex_edges, node0, sort, ALPHA_NN);

            ALPHA_NA = k;

            //Ordino le coppie di archi
            QuickSort_Edges(tmp, dist, 0, ALPHA_NA - 1);
            

            k = 0;
            min = 0;

            for (i = 0; i < ALPHA_NA; i++)
            {
                if ((set[0].Find_Set(set, tmp[i].a) != set[0].Find_Set(set, tmp[i].b)))
                {
                    lb[k++] = tmp[i];
                    min += dist[tmp[i].a, tmp[i].b];
                    set[0].Union(set, tmp[i].a, tmp[i].b);
                }
            }

            //inserisco gli ultimi 2 archi.
            float min1 = float.MaxValue,
                min2 = float.MaxValue;
            int imin1 = -1,
                imin2 = -1;
            bool skip;
            int count = 0;

            for (i = 1; i < ALPHA_NN; i++)
            {
                //if (i == node0)
                //    continue;

                skip = false;
                if (ex_edges != null)
                {
                    for (j = 0; j < ex_edges.Length; j++)
                    {
                        if (((sort[node0,i] == ex_edges[j].a) && (node0 == ex_edges[j].b)) ||
                            ((node0 == ex_edges[j].a) && (sort[node0,i] == ex_edges[j].b)))
                        {
                            skip = true;
                            count++;
                            break;
                        }
                    }

                    //if (count == n2)
                    //    return float.MinValue;

                    if (skip == true)
                        continue;
                }

                if (min1 > dist[node0, sort[node0,i]])
                {
                    imin2 = imin1;
                    min2 = min1;

                    imin1 = sort[node0,i];
                    min1 = dist[node0, imin1];
                }
                else if (min2 > dist[node0, sort[node0,i]])
                {
                    imin2 = sort[node0,i];
                    min2 = dist[node0, imin2];
                    
                }
            }

#if DEBUG
            if ((imin1 == -1) || (imin2 == -1))
                System.Windows.Forms.MessageBox.Show("imin1,imin2: errore calcolo 1-tree");
#endif
            if ((imin1 == -1) || (imin2 == -1))
                return float.MinValue;

            lb[k].a = node0;
            lb[k++].b = imin1;
            lb[k].a = node0;
            lb[k].b = imin2;

            min += min1 + min2;


            return min;
        }

        public float Kruskal_1_tree_lb_alpha(int ALPHA_NN, float[,] dist, int[,] sort, int node0, EDGE_LIST[] ex_edges)
        {
            int k, i;   //(n)*(n+1)/2

            //Initializzo il set e il vettore degli indici delle distanze ordinate
            for (i = 0; i < n; i++)
                set[i].Make_Set(set, i);

            //Ordino le coppie di archi
            QuickSort_Edges(tmp, dist, 0, ALPHA_NA - 1);

            k = 0;
            min = 0;

            for (i = 0; i < ALPHA_NA; i++)
            {
                if ((set[0].Find_Set(set, tmp[i].a) != set[0].Find_Set(set, tmp[i].b)))
                {
#if DEBUG
                    if (k == n - 2)
                    {
                        System.Windows.Forms.MessageBox.Show("ERRROR!!! 1-tree_alpha");
                        break;
                    }
                        
#endif
                    lb[k++] = tmp[i];
                    min += dist[tmp[i].a, tmp[i].b];
                    set[0].Union(set, tmp[i].a, tmp[i].b);
                }
            }

            return min;
        }
    }
}
