using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class cTSP
    {
        int num_citta,n1;
        public float[,] distance;
        float[,] distcopy;  //per usarle in intere se necessario.
        public int[,] sort_dist;
        float[] x;
        float[] y;
        float tour_length;
        int[] tour_path;
        float max_x, max_y, min_x, min_y;
        int[] chull;
        int nchull;
        public EDGE_LIST[] lb;
        public int delta;

        public EDGE_LIST[] pool_arcs; //significa che ho gli archi certi...
                                      //da cui nodi posso eliminare tutti gli altri archi...
                                      //ridurrei di molto l'insieme degli archi e velocizzerei...

        public float[,] alpha;
        public int[,] sort_alpha;

        public int ALPHA_NN; //n° nodi di alpha nearest
        public int ALPHA_NA; //n° archi
        const int ALPHA_NN_MIN = 5;
        public  int ALPHA_NN_MAX = 50;
        public MST mst;

        bool computed_distance;
        bool loaded_data;
        public bool computed_tour;
        public bool sorted_dist;
        public bool computed_chull;
        public bool computed_lb;   //0 none 1=mst 2=1-tree 3=lb
        public int[] degree_node;

        public struct Node
        {
            public float x;
            public float y;
            public int city;
        }

        Stack<Node> stacknodes;
        public Node Last_Popped;

        public cTSP()
        {
            n1 = num_citta = nchull = 0;
            tour_length=0;
            computed_distance=false;
            loaded_data=false;
            computed_tour = false;
            max_x = max_y = float.MinValue;
            min_x = min_y = float.MaxValue;
            sorted_dist = false;
            computed_chull = false;
            computed_lb = false;
            mst = null;
            degree_node = new int[num_citta];
            //int delta = (int)(Math.Round(Math.Sqrt(num_citta) /*+ Math.Log10(num_citta)*/));
            delta = 0;
        }


        public void Build_pool_arcs(bool strong)
        {
            int i, j,n;
            n=num_citta;
            bool[] mark = new bool[n];
            List<EDGE_LIST> arcs = new List<EDGE_LIST>();
            EDGE_LIST ed;
            int A, B, C, D;

            for (i = 0; i < n; i++)
            {
                if (mark[i])
                    continue;

                A = i;
                B = sort_dist[A, 1];
                C = sort_dist[A, 2]; //2° + vic di A
                D = sort_dist[B, 2]; //2° + vic di B
                //E = sort_dist[B, 1];

             
                if (D == A) //allora b dinveta a
                {
                    mark[i] = true;
                    if (mark[B])
                        continue;
                    B = sort_dist[A, 1];
                    C = sort_dist[A, 2]; //2° + vic di A
                    D = sort_dist[B, 2];
                    i = 0;
                }

                //if ((mark[A]) && (mark[B]))
                //    continue;
                //nienete else perchè dal caso sopra mi ritroverò in uno dei 2 seguenti...
  
                if (D == C) //allora AB tour ottimo.
                {
                    if (strong)
                    {
                        if (A < B)
                        {
                            ed.a = A;
                            ed.b = B;
                        }
                        else
                        {
                            ed.b = A;
                            ed.a = B;
                        }
                        arcs.Add(ed);
                    }
                    //altrimenti non lo inserisco per non rischiare troppo!!! 
                }
                else // caso in cui a,b,c,d 4 nodi diversi
                {
                    float rAC = distance[A, C];
                    float rBD = distance[B, D];                    

                    if ((rAC > distance[A, B]) &&
                        (rBD > distance[A, B]))
                    {
                        if (A < B)
                        {
                            ed.a = A;
                            ed.b = B;
                        }
                        else
                        {
                            ed.b = A;
                            ed.a = B;
                        }

                        arcs.Add(ed);
                    }
                    //prova
                    else
                    {
                        if ((rAC < distance[A, D])&&
                            (distance[B,C]>rBD))
                        {
                            if (A < C)
                            {
                                ed.a = A;
                                ed.b = C;
                                
                            }
                            else
                            {
                                ed.b = A;
                                ed.a = C;
                            }

                            arcs.Add(ed);

                            //è più stringente con questo arco sotto in +
                            if (strong)
                            {
                                if (B < D)
                                {
                                    ed.a = B;
                                    ed.b = D;
                                }
                                else
                                {
                                    ed.a = D;
                                    ed.b = B;
                                }

                                arcs.Add(ed);
                            }
                        }
                    }

                }

                mark[A] = true;
                //mark[B] = true;
            }
            //rimuovo archi doppi...
            for (i = 0; i < arcs.Count; i++)
            {
                ed = arcs[i];
                for (j = i + 1; j < arcs.Count; j++)
                {
                    if ((ed.a == arcs[j].a)&&(ed.b==arcs[j].b))
                    {
                        arcs.RemoveAt(j);
                        j--;
                        i = 0;
                    }
                }
            }
                
            pool_arcs = arcs.ToArray();
            //in modo ridondante posso escludere gli archi dei nodi dei pool...
        }

        //da pool_arcs riduzione grafo dei nodi degli archi certi...
        //da mettere a posto quest'idea e l'algoritmo...
        public void Graph_reduction(bool strong)
        {
            Build_pool_arcs(strong);
            Node n0 = new Node();
            Node n1 = new Node();
            
            stacknodes = new Stack<Node>();
            for (int i = 0; i<pool_arcs.Length; i++)
            {
                n0.city = pool_arcs[i].a;
                n1.city = pool_arcs[i].b;
                n0.x = x[n0.city];
                n0.y = y[n0.city];
                n1.x = x[n1.city];
                n1.y = y[n1.city];

                
                stacknodes.Push(n0);
                stacknodes.Push(n1);
                Collapse1Node(n0, n1, true);
                for (int j = i + 1; j < pool_arcs.Length; j++)
                {
                    //sistemo l'array per il dopo collasso..
                    if(pool_arcs[j].a >= n1.city)
                        pool_arcs[j].a--;
                    if(pool_arcs[j].b >= n1.city)
                        pool_arcs[j].b--;

                }
            }
            
            //ricalcolare le sort_distance..
            this.n1 = num_citta - 1;
            sort_distances();
        }

        public int GetNN_Node(int node, int nn)
        {
            return sort_dist[node, nn];
        }
        
        public cTSP(int n)
        {
            num_citta=n;
            n1 = n - 1;
            distance = new float[n,n];
            x = new float[n];
            y = new float[n];
            tour_path = new int[n];
            tour_length = 0;
            computed_distance = false;
            loaded_data = false;
            max_x = max_y = float.MinValue;
            min_x = min_y = float.MaxValue;
            computed_tour = false;
            sort_dist = new int[n, n];
            sorted_dist = false;
            computed_lb = false;

            degree_node = new int[num_citta];
            mst = new MST(n);
            //è lo stesso numero che usa Alphanearness in automatico come stima di nodi vicini.
            //perchè mi sembra sensato che sia in proporzione al numero di nodi che statico e fissato sperimentalmente.
            delta = (int)(Math.Round(Math.Sqrt(num_citta) + Math.Log10(num_citta)));
        }

        public float[,] GetDistanceMatrix()
        {
            return distance;
        }

        public void SetIntDistanceMatrix()
        {
            int i, j;
            distcopy = new float[num_citta, num_citta];

            for (i = 0; i < num_citta; i++)
            {
                distcopy[i, i] = 0;
                for (j = i + 1; j < num_citta; j++)
                {
                    distcopy[i, j] = distcopy[j, i] = distance[i, j];

                    distance[i, j] = distance[j, i] = (float)Math.Round(distance[i, j]);
                }
            }
        }

        public void ResetDistanceMatrix()
        {
            if (distcopy == null) 
                return;

            int i, j;
            
            for (i = 0; i < num_citta; i++)
            {
                //distcopy[i, i] = 0;
                for (j = i + 1; j < num_citta; j++)
                {
                    distance[i, j] = distance[j, i] = distcopy[i, j];
                }
            }

            distcopy = null;

        }
        public void Reset_Data()
        {
            loaded_data = false;
            min_x = min_y = float.MaxValue;
            max_x = max_y = float.MinValue;
            distance = null;
            sort_dist = null;
            alpha = null;
            sort_alpha = null;
            sorted_dist = false;
            distcopy = null; 
            tour_path = null;
            num_citta=n1=0;
            x=y=null;
            tour_length=0;
            chull=null;
            nchull=0;
            lb=null;
            mst=null;
            computed_distance=false;
            computed_tour=computed_chull=computed_lb=false;
            degree_node = null; ;

        }
        public void Loaded_Data()
        {
            loaded_data=true;
            //trovare il valore max e min per la visualizzazione su schermo
        }
        public void Loaded_Data(bool b)
        {
            loaded_data = b;
            //trovare il valore max e min per la visualizzazione su schermo
        }
        public void setNode(int i, float _x, float _y)
        {
            x[i] = _x;
            y[i] = _y;
            if (_x > max_x)
                max_x = _x;
            if (_y > max_y)
                max_y = _y;

            if (_x < min_x)
                min_x = _x;
            if (_y < min_y)
                min_y = _y;
        }

        public float ComputeDistance(int i, int j)
        {
            float dx, dy;
            
            dx = x[i] - x[j];
            dy = y[i] - y[j];

            return distance[i, j] = Convert.ToSingle(Math.Sqrt(dx * dx + dy * dy));
        }

        public void ComputeDistance(int i, int j, float value)
        {
            distance[i, j] = value;
        }

        public void Computed_distance()
        {
            computed_distance = true;
        }

        public float GetDistance(int i, int j)
        {
            return distance[i, j];
        }

        public bool isLoaded()
        {
            if (loaded_data)
                return true;
            else
                return false;
        }

        public bool isOK()
        {
            if ((loaded_data) && (computed_distance))
                return true;
            else
                return false;
        }

        public int GetN()
        {
            return num_citta;
        }

        public float Get_x(int i)
        {
            return x[i];
        }

        public float Get_y(int i)
        {
            return y[i];
        }

        public void RecalcultateCoordMinMaxXY()
        {
            max_y = max_x = float.MinValue;
            min_y = min_x = float.MaxValue;

            for (int i = 0; i < num_citta; i++)
            {
                if (x[i] > max_x)
                    max_x = x[i];
                if (x[i] < min_x)
                    min_x = x[i];

                if (y[i] > max_y)
                    max_y = y[i];
                if (y[i] < min_y)
                    min_y = y[i];
            }
            
        }
        public float GetMax_x()
        {
            return max_x;
        }

        public float GetMax_y()
        {
            return max_y;
        }

        public float GetMin_x()
        {
            return min_x;
        }

        public float GetMin_y()
        {
            return min_y;
        }
    
        public void InsertNode(float _x, float _y)
        {

        }

        public float Calculate_tour_length()
        {
            int i;
            if (!computed_tour)
                return 0;
            tour_length = 0;
            for (i = 0; i < num_citta - 1; i++)
                tour_length += distance[tour_path[i], tour_path[i + 1]];
            tour_length+=distance[tour_path[i],tour_path[0]];

            return tour_length;
       }

        public int Calculate_int_tour_length()
        {
            int i;

            int tour_length = 0;
            for (i = 0; i < num_citta - 1; i++)
                tour_length += (int) Math.Round(distance[tour_path[i], tour_path[i + 1]]);
            tour_length += (int) Math.Round(distance[tour_path[i], tour_path[0]]);

            return tour_length;
        }

        public int GetTourNode(int i)
        {
            return tour_path[i];
        }

        public void Reset_tour()
        {
            int i;
            if (!loaded_data)
                return;

            for (i = 0; i < num_citta; i++)
                tour_path[i] = num_citta;
            tour_length = 0;
            computed_tour = false;
        }

        public void Sequential_tour()
        {
            int i;

            for (i = 0; i < num_citta; i++)
                tour_path[i] = i;
            computed_tour = true;
        }

        public void Random_tour()
        {
            int i,j,k;

            Random rand = new Random();
            
            for(i=0;i<num_citta;i++)
                tour_path[i]=-1;

            for (i = 0; i < n1; )
            {
                k=rand.Next(num_citta );
                for (j = 0; j < n1; j++)
                {
                    if (k == tour_path[j])
                        break;
                }
                if (j == n1)
                {
                    tour_path[i] = k;
                    i++;
                }
            }

            for (j = 0; j < num_citta; j++)
            {
                for (i = 0; i < num_citta; i++)
                {
                    if (j == tour_path[i])
                        break;
                }
                if (i == num_citta) //ho trovato il valore.
                {
                    tour_path[num_citta - 1] = j;
                    j = num_citta;
                    break;
                }
            }
            computed_tour = true;
        }
        private void Quick_Sort_pi(int[] ipi, float[] pi, int p, int r, int p0)
        {
            int q;

            if (p < r)
            {
                q = Quick_Sort_pi_Partition(ipi, pi, p, r,p0);
                Quick_Sort_pi(ipi, pi, p, q,p0);
                Quick_Sort_pi(ipi, pi , (q + 1), r,p0);
            }
        }

        private int Quick_Sort_pi_Partition(int[] ipi, float[] pi, int p, int r, int p0)
        {
            float x;
            int i, j;

            x = pi[ipi[p]];
            i = p - 1;
            j = r + 1;

            while (true)
            {
                do
                {
                    j--;
                } while (pi[ipi[j]] > x);

                do
                {
                    i++;
                } while (pi[ipi[i]] < x);

                if (i < j)
                {
                    if (pi[ipi[i]] == pi[ipi[j]])
                    {
                        if (distance[ipi[i], p0] < distance[ipi[j], p0])
                        {
                            p = ipi[i];
                            ipi[i] = ipi[j];
                            ipi[j] = p;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        p = ipi[i];
                        ipi[i] = ipi[j];
                        ipi[j] = p;
                    }
                }
                else return j;
            }
        }

       private void Quick_Sort_dist(int idist, int p, int r)
        {
            int q;

            if (p < r)
            {
                q = Quick_Sort_dist_Partition(idist, p, r);
                Quick_Sort_dist(idist,  p, q);
                Quick_Sort_dist(idist, (q + 1), r);
            }
        }

        private int Quick_Sort_dist_Partition(int idist, int p, int r)
        {
            float x;
            int i, j;

            x = distance[idist,sort_dist[idist,p]];
            i = p - 1;
            j = r + 1;

            while (true)
            {
                do
                {
                    j--;
                } while (distance[idist,sort_dist[idist,j]] > x);

                do
                {
                    i++;
                } while (distance[idist,sort_dist[idist,i]] < x);

                if (i < j)
                {
                    p = sort_dist[idist,i];
                    sort_dist[idist,i] = sort_dist[idist,j];
                    sort_dist[idist,j] = p;
                }
                else return j;
            }
        }

        private void Quick_Sort_dist_Alpha(int idist, int p, int r)
        {
            int q;

            if (p < r)
            {
                q = Quick_Sort_dist_Partition_Alpha(idist, p, r);
                Quick_Sort_dist_Alpha(idist, p, q);
                Quick_Sort_dist_Alpha(idist, (q + 1), r);
            }
        }

        private int Quick_Sort_dist_Partition_Alpha(int idist, int p, int r)
        {
            float x;
            int i, j;

            x = distance[idist, sort_alpha[idist, p]];
            i = p - 1;
            j = r + 1;

            while (true)
            {
                do
                {
                    j--;
                } while (distance[idist, sort_alpha[idist, j]] > x);

                do
                {
                    i++;
                } while (distance[idist, sort_alpha[idist, i]] < x);

                if (i < j)
                {
                    p = sort_alpha[idist, i];
                    sort_alpha[idist, i] = sort_alpha[idist, j];
                    sort_alpha[idist, j] = p;
                }
                else return j;
            }
        }

        private bool Check_TOUR(int n)
        {
            int i, j, k;
            bool ret = true;
            //check
            for (i = 0; i < n; i++)
            {
                for (j = 0, k = 0; j < n; j++)
                {
                    if (i == tour_path[j])
                        k++;
                }
                if ((k > 1) || (k == 0))
                {
                    computed_tour = false;
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        public void sort_distances()
        {
            int i,j;

            for (i = 0; i < num_citta; i++)
            {
                for (j = 0; j < num_citta; j++)
                    sort_dist[i, j] = j;

                Quick_Sort_dist(i, 0, n1);
            }
            sorted_dist = true;
        }

        public void NeirestN_tour(int node_start)
        {
            int i,j,k,nn;
            //int[] sort = new int[num_citta];
            Reset_tour();
            tour_path[0] = node_start;
            nn = 1;

            while(nn<num_citta)
            {
                //for (j = 0; j < num_citta; j++)
                  //  sort[j] = j;
                //Quick_Sort_dist(sort, tour_path[i], 0, n1);
                //controllo il primo da mettere...
                i = tour_path[nn - 1];
                for(k=0;k<num_citta;k++)
                {
                    for (j = 0; j < nn; j++)
                    {
                        if (tour_path[j] == sort_dist[i,k])
                            break;
                    }
                    if (j == nn)
                    {
                        tour_path[nn] = sort_dist[i,k];
                        nn++;
                        break;
                    }
                }
            }
        }

        public bool isToured()
        {
            return computed_tour;
        }

        public float UpdateTourLength(float d)
        {
            return tour_length += d;
        }

        public void setTourNode(int i, int node)
        {
            tour_path[i]=node;
        }

        private float CCW(int i1, int i2, int i3)
        {
            return (x[i2] - x[i1]) * (y[i3] - y[i1]) -
                   (y[i2] - y[i1]) * (x[i3] - x[i1]);
        }

        private int[] AngularySort(int p0)
        {
            int[] sorted;
            float[] pi;
            int i;

            pi = new float[num_citta];
            sorted = new int[num_citta];
                
            for (i = 0; i < num_citta; i++)
            {
                sorted[i] = i;
                pi[i] = ((x[p0] - x[i]) / distance[p0, i]);
            }
        


            pi[p0] = -1;
            Quick_Sort_pi(sorted, pi, 0, n1,p0);

            return sorted;
        }

        private int DownRight()
        {
            int i, p0;

            for (i = 1, p0 = 0; i < num_citta; i++)
            {
                if (y[p0] > y[i])
                    p0 = i;
                else
                    if ((y[p0] == y[i]) && (x[p0] < x[i]))
                        p0 = i;
            }
            return p0;
        }

        public int ConvexHull()
        {
            int p0;
            int[] sorted;
            int i, j;
            float cp;

            p0 = DownRight();

            sorted = AngularySort(p0);

            chull = new int[num_citta];

            chull[0] = sorted[0]; chull[1] = sorted[1];

            for (i = 2, j = 1; i < num_citta; i++)
            {
                cp = CCW(sorted[i], chull[j], chull[j - 1]);
                if (cp <= 0)
                {
                    if (cp==0)
                    {
                        if (distance[chull[j - 1], chull[j]] > distance[chull[j - 1], sorted[i]])
                        {
                            chull[j + 1] = chull[j];
                            chull[j] = sorted[i];
                            j++;
                        }
                        else if(distance[sorted[i],sorted[i-1]]==0) // è come se fosse lo stesso punto...
                        {
                            continue;
                        }
                        else
                            chull[++j] = sorted[i];
                    }
                    else 
                        chull[++j] = sorted[i];
                }
                else
                {
                    while ((cp > 0) && (j > 1))
                    {
                        j--;
                        //chull[--j2] = chull[--j];
                        cp = CCW(sorted[i], chull[j], chull[j - 1]);
                    }
                    chull[++j] = sorted[i];
                }
            }

            computed_chull = true;
            return nchull=j+1;
        }

        public int GetNChull()
        {
            return nchull;
        }

        public int GetChull_Node(int i)
        {
            return chull[i];
        }


        public void CheapestInsertion()
        {
            int n_ch,n_ch1; // numero città convex hull;
            int i,i2,j,j2;     //contatori
            float d1,d2;    //numeri per le distanze
            int     bestj,besti,bestj2; // indici 
            float[] min_dist; //vettore di indici delle distanze minori.
            int[]   min_d_i;  //indice dell'arco relativo alle dist.


            //Calcolo del convex hull
            //if(!computed_chull)
            Reset_tour();
                ConvexHull();
            n_ch=nchull;
            //alloco la matrice. delle distanze minime
            n_ch1=nchull-1;
            min_d_i  = new int[num_citta];
            min_dist = new float[num_citta];

            //OverHead
            for (i = 0; i < nchull; i++)
                tour_path[i] = chull[i];
            for (i = 0,j2=nchull; ((i < num_citta)&&(j2<num_citta)); i++)
            {
                for (j = 0; j < num_citta; j++)
                {
                    if (i == tour_path[j])
                        break;
                }
                if (j == num_citta)
                    tour_path[j2++] = i;
            }
            //calcolo le distanze minime di ogni punto con ogni arco.
            for(i=nchull;i<num_citta;i++)
            {
                min_dist[i]=float.MaxValue;
                
                for(j=0,i2=1;j<n_ch1;j++,i2++)
                {
                    d1 = distance[tour_path[j],tour_path[i2]];
                    d2 = distance[tour_path[i],tour_path[j]] +
                            distance[tour_path[i],tour_path[i2]];
                    d2 -= d1;
                    if(d2<min_dist[i])
                    {
                        min_dist[i]=d2;
                        min_d_i[i]=j;
                    }
                }
                //l'ltimo col primo
                d2 = (distance[tour_path[i],tour_path[j]] +
                        distance[tour_path[i],tour_path[0]])-
                        distance[tour_path[j],tour_path[0]];
                if(d2<min_dist[i])
                {
                    min_dist[i]=d2;
                    min_d_i[i]=j;
                }
            }


            //ora cerco dai punti non inseriti l'arco migliore.
            while(n_ch<num_citta)
            {
                d1=float.MaxValue;
                for(i=n_ch,besti=n_ch;i<num_citta;i++)
                {
                    if(min_dist[i]<d1)
                    {
                        d1 = min_dist[i];
                        besti=i;
                    }
                }
                //inserisco il punto besti tra bestj e bestj+1
                j=tour_path[besti];
        
                bestj=min_d_i[besti]+1;
                for(i2=besti;i2>bestj;i2--)
                        tour_path[i2] = tour_path[i2-1];
                tour_path[bestj]=j;
                //controllo che il vettore delle distanze non sia shiftato.
                if(besti>n_ch)
                {
                    for(i2=besti,j=besti-1;i2>n_ch;i2--,j--)
                    {
                        min_dist[i2]=min_dist[j];
                        min_d_i[i2]=min_d_i[j];
                    }
                }
                //Aggiorno le distanze minime fra i nodi e gli archi.
                n_ch1=n_ch++;
                j=bestj-1;
                if(bestj<n_ch1)
                    bestj2=bestj+1;
                else
                    bestj2=0;

                for(i=n_ch;i<num_citta;i++)
                {
                    d2 = (distance[tour_path[i],tour_path[j]] +
                    distance[tour_path[i],tour_path[bestj]])-
                    distance[tour_path[j],tour_path[bestj]];

                    d1 = (distance[tour_path[i],tour_path[bestj2]] +
                    distance[tour_path[i],tour_path[bestj]])-
                    distance[tour_path[bestj],tour_path[bestj2]];
                    //caso normale
                    if((min_dist[i]>d2)||(min_dist[i]>d1))
                    {
                        if(d2<d1)
                        {
                            min_dist[i]=d2;
                            min_d_i[i]=j;
                        }
                        else
                        {
                            min_dist[i]=d1;
                            min_d_i[i]=bestj;
                        }
                    }
                    else if((min_d_i[i]==j))
                    {
                        //l'arco è uguale, ricerco la distanza minima
                        min_dist[i]=float.MaxValue;
                        for(i2=0,j2=1;i2<n_ch1;i2++,j2++)
                        {
                            d1 = (distance[tour_path[i],tour_path[i2]] +
                            distance[tour_path[i],tour_path[j2]])   -
                            distance[tour_path[i2],tour_path[j2]];
                            if(d1<min_dist[i])
                            {
                                min_dist[i]=d1;
                                min_d_i[i]=i2;
                            }
                        }
                        //l'ltimo col primo
                        d1  =(distance[tour_path[i],tour_path[i2]] +
                                        distance[tour_path[i],tour_path[0]])   -
                                        distance[tour_path[i2],tour_path[0]];
                        if(d1<min_dist[i])
                        {
                            min_dist[i]=d1;
                            min_d_i[i]=i2;
                        }
                    }
                    //l'arco minimo è mantenuto, ma visto che ho inserito un nodo
                    //aggiungo uno all'indice.
                    else if(min_d_i[i]>j)
                        min_d_i[i]++;
                }
            }//end while
            computed_tour = true;
            //Check_TOUR();
        }

        public float Compute_MST()
        {
            float length;
            
            length = mst.Kruskal_1_tree_lb(distance,null);
            lb = mst.Get_lb();
            computed_lb = true;
            
            return length;
        }

        public float Compute_1_treeAlpha_old(int node0, float[,] distance, EDGE_LIST[] ex_edges)
        {
            float length;
            //if(mst==null)
                //mst = new MST(num_citta);

            length = mst.Kruskal_1_tree_lb_alpha_old(ALPHA_NN, distance, sort_alpha, node0, ex_edges);
            lb = mst.Get_lb();
            computed_lb = true;

            return length;

        }

        public float Compute_1_treeAlpha(int node0, float[,] distance, EDGE_LIST[] ex_edges)
        {
            float length;
            float min = float.MaxValue;
            //if(mst==null)
            //mst = new MST(num_citta);

            length = mst.Kruskal_1_tree_lb_alpha(ALPHA_NN, distance, sort_alpha, node0, ex_edges);
            lb = mst.Get_lb();
            int d2n = Compute_Degree_Node_1();
            
            //aggiungo l'arco per 1-tree
            if (d2n != num_citta)
            {
                //int n1 = num_citta - 1;
                // bool skip;
                EDGE_LIST best,best2;
                best2.a = best2.b = best.a = best.b = -1;
                int j;
                bool skip;
                float min1, min2;
                min1 = min2 = float.MaxValue;

                for (int i = 0; i < ALPHA_NA; i++)
                {
                    if (degree_node[i] == 1)
                    {
                        //cerco la sua 2° distanza minima...
                        for (j = 0; j < ALPHA_NA; j++)
                        {
                            if (i == j)
                                continue;

                            if (ex_edges != null)
                            {
                                skip = false;
                                for (int j2 = 0; j2 < ex_edges.Length; j2++)
                                {
                                    if (((i == ex_edges[j2].a) && (j == ex_edges[j2].b)) ||
                                        ((j == ex_edges[j2].a) && (i == ex_edges[j2].b)))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                                if (skip == true)
                                    continue;
                            }

                            if (distance[i, j] < min1)
                            {
                                min2 = min1;
                                best2 = best;
                                best.a = i;
                                best.b = j;
                                min1 = distance[i, j];
                            }
                            else if (distance[i, j] < min2)
                            {
                                min2 = distance[i, j];
                                best2.a = i;
                                best2.b = j;
                            }
                        }


                    }
                }

                //aggiungo l'arco..
                if (best2.a > -1)
                {
                    lb[lb.Length - 1] = best2;
                    length += min2;
                    degree_node[best2.a]++;
                    degree_node[best2.b]++;
                }
                else
                {
                    computed_lb = false;
                    lb = null;
                    return float.MinValue;
                }

            }	            
            
            computed_lb = true;
            return length+min;

        }

        //algoritmo classico di 1-tree//
        public float Compute_1_tree_old(int node0, float[,] distance, EDGE_LIST[] ex_edges)
        {
            float length;
            
            length = mst.Kruskal_1_tree_lb_old(distance, sort_dist, node0, ex_edges);
            if (length > float.MinValue)
            {
                lb = mst.Get_lb();
                computed_lb = true;
            }
            else
            {
                lb = null;
                computed_lb = false;
            }
            return length;

        }

        public float Compute_1_tree(int node0, float[,] distance, EDGE_LIST[] ex_edges, bool addarc)
        {
            float length;
            
            length = mst.Kruskal_1_tree_lb(distance,ex_edges);
            lb = mst.Get_lb();
            int d2n=Compute_Degree_Node_1();

            //aggiungo l'arco per 1-tree
            if ((d2n != num_citta)&&(addarc==true))
            {
                //int n1 = num_citta - 1;
               // bool skip;
                EDGE_LIST best,best2;
                best2.a = best2.b = best.a = best.b = -1;
                int j;
                bool skip;
                float min1,min2;
                min2=min1 = float.MaxValue;

                //cerco un arco minimo che va da nodo grado 1 a nodo grado 1...altrimenti
                //2° arco minimo di un nodo di grado 1° (il minore fra tutti)
                for (int i = 0; i < num_citta; i++)
                {
                    if (degree_node[i] == 1)
                    {
                        for (j = 0; j < num_citta; j++)
                        {
                            if (i == j)
                                continue;
                            if (degree_node[j] != 1)
                                continue;

                            if (ex_edges != null)
                            {
                                skip = false;
                                for (int j2 = 0; j2 < ex_edges.Length; j2++)
                                {
                                    if (((i == ex_edges[j2].a) && (j == ex_edges[j2].b)) ||
                                        ((j == ex_edges[j2].a) && (i == ex_edges[j2].b)))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                                if (skip == true)
                                    continue;
                            }

                            if (distance[i, j] < min1)
                            {
                                min2 = min1;
                                best2 = best;
                                best.a = i;
                                best.b = j;
                                min1 = distance[i, j];
                            }
                            else if (distance[i, j] < min2)
                            {
                                min2 = distance[i, j];
                                best2.a = i;
                                best2.b = j;
                            }
                        }
                    }
                }
                if (best2.a == -1)
                {
                    for (int i = 0; i < num_citta; i++)
                    {
                        if (degree_node[i] == 1)
                        {
                            //cerco la sua 2° distanza minima...
                            for (j = 0; j < num_citta; j++)
                            {
                                if (i == j)
                                    continue;

                                if (ex_edges != null)
                                {
                                    skip = false;
                                    for (int j2 = 0; j2 < ex_edges.Length; j2++)
                                    {
                                        if (((i == ex_edges[j2].a) && (j == ex_edges[j2].b)) ||
                                            ((j == ex_edges[j2].a) && (i == ex_edges[j2].b)))
                                        {
                                            skip = true;
                                            break;
                                        }
                                    }
                                    if (skip == true)
                                        continue;
                                }

                                if (distance[i, j] < min1)
                                {
                                    min2 = min1;
                                    best2 = best;
                                    best.a = i;
                                    best.b = j;
                                    min1 = distance[i, j];
                                }
                                else if (distance[i, j] < min2)
                                {
                                    min2 = distance[i, j];
                                    best2.a = i;
                                    best2.b = j;
                                }
                            }
                        }
                    }
                }
                //aggiungo l'arco..
                if (best2.a > -1)
                {
                    lb[lb.Length - 1] = best2;
                    length += min2;
                    degree_node[best2.a]++;
                    degree_node[best2.b]++;
                }
                else
                {
                    computed_lb = false;
                    lb = null;
                    return float.MinValue;
                }


            }	
            computed_lb = true;

            return length;
        }

        public int Compute_Degree_Node_1()
        {
            int i, j, count;

            //degree_node[0]=2;
            for (i = 0, count = 0; i < num_citta; i++)
            {
                degree_node[i] = 0;
                for (j = 0; j < n1; j++) //non ce l'ho l'ultimo arco
                {
                    if ((lb[j].a == i) || (lb[j].b == i))
                        degree_node[i]++;
                }
                if (degree_node[i] == 2)
                    count++;
            }

            return count;
        }

        public int Compute_Degree_Node()
        {
            int i,j,count;

            //degree_node[0]=2;
            for(i=0,count=0;i<num_citta;i++)
            {
                degree_node[i]=0;
                for(j=0;j<num_citta;j++)
                {
                    if((lb[j].a == i)||(lb[j].b == i))
                        degree_node[i]++;
                }
                if(degree_node[i]==2)
                    count++;
            }

            return count;
        }

        public int Compute_Degree_Node_MST()
        {
            int i, j, count;

            //degree_node[0]=2;
            for (i = 0, count = 0; i < num_citta; i++)
            {
                degree_node[i] = 0;
                for (j = 0; j < n1; j++)
                {
                    if ((lb[j].a == i) || (lb[j].b == i))
                        degree_node[i]++;
                }
                if (degree_node[i] %2 != 0)
                    count++;
            }

            return count;
        }


        public int Compute_Degree_Node2()
        {
            int i, j, count;

            degree_node[0] = 2;
            for (i = 1, count = 1; i < num_citta; i++)
            {
                degree_node[i] = 0;
                for (j = 0; j < num_citta; j++)
                {
                    if ((lb[j].a == i) )
                        degree_node[i]++;
                }
                if (degree_node[i] == 2)
                    count++;
            }

            return count;
        }      
  
        public float SumDegree()
        {
            float sum_degree;
            int i;

            for(i=0,sum_degree=0;i<num_citta;i++)
                sum_degree += (float)((int)degree_node[i] - 2)*((int)degree_node[i] - 2);

            return sum_degree;

        }

        //public int[] GetDegreeNode()
        //{
        //    return degree_node;
        //}

        public float CalculateLBLength()
        {
            int i;
            float v;
            if (!computed_lb)
                return float.MinValue;
            for (i = 0, v = 0; i < num_citta; i++)
                v += distance[lb[i].a, lb[i].b];

            return v;
        }

        public int CalculateLBintLength()
        {
            int i;
            int v;

            for (i = 0, v = 0; i < num_citta; i++)
                v += (int)Math.Round(distance[lb[i].a, lb[i].b]);

            return v;
        }

        private int GetFirstThirdDegreeNodeIndex()
        {
            int i;

            for (i = 0; i < num_citta; i++)
            {
                if (degree_node[i] == 3)
                    return i;
            }

            //se non c'è prende il primo con grado > 3 (per sicurezza ed evitare un eventuale bug)
            for (i = 0; i < num_citta; i++)
            {
                if (degree_node[i] > 3)
                    return i;
            }

            return -1; //ritorna errore e fa crashare il programma.. :)

        }

        private int GetMaxCostThirdDegreeNodeIndex()
        {
            int i,besti;
            float best;
            

            for (i = 0, best = -1, besti = -1; i < num_citta; i++)
            {
                if (degree_node[i] == 3)
                {
                    int n2=n1-1;
                    for (int j = 0; j < n2; j++)
                    {
                        if ((lb[j].a == i)&&
                            (distance[i,lb[j].b]>best))
                        {
                            best = lb[j].b;
                            besti = i;
                        }
                        else if ((lb[j].b == i) &&
                            (distance[i, lb[j].a] > best))
                        {
                            best = lb[j].a;
                            besti = i;
                        }
                    }
                    //return i;
                }
            }
            if (besti != -1)
                return besti;

            //se non c'è prende il primo con grado > 3 (per sicurezza ed evitare un eventuale bug)
            for (i = 0; i < num_citta; i++)
            {
                if (degree_node[i] > 3)
                {
                    int n2=n1-1;
                    for (int j = 0; j < n2; j++)
                    {
                        if ((lb[j].a == i) &&
                            (distance[i, lb[j].b] > best))
                        {
                            best = lb[j].b;
                            besti = i;
                        }
                        else if ((lb[j].b == i) &&
                            (distance[i, lb[j].a] > best))
                        {
                            best = lb[j].a;
                            besti = i;
                        }
                    }
                        
                }
                    //return i;
            }

            if (besti != -1)
                return besti;

            return -1; 
        }

        private int GetMaxCostSumThirdDegreeNodeIndex()
        {
            int i, besti;
            float best;
            float sum;

            for (i = 0, best = -1, sum = 0, besti = -1; i < num_citta; i++)
            {
                if (degree_node[i] == 3)
                {
                    int n2 = n1 - 1;
                    sum = 0;
                    for (int j = 0; j < n2; j++)
                    {
                        if (lb[j].a == i)
                            sum += distance[i, lb[j].b];
                        else if (lb[j].b == i)
                            sum += distance[i, lb[j].a];

                    }
                    //return i;
                }
                if (sum > best)
                {
                    best = sum;
                    besti = i;
                }
            }
            if (besti != -1)
                return besti;

            //se non c'è prende il primo con grado > 3 (per sicurezza ed evitare un eventuale bug)
            for (i = 0; i < num_citta; i++)
            {
                if (degree_node[i] > 3)
                {
                    int n2 = n1 - 1;
                    sum = 0;
                    for (int j = 0; j < n2; j++)
                    {
                        if (lb[j].a == i)
                            sum += distance[i, lb[j].b];
                        else if (lb[j].b == i)
                            sum += distance[i, lb[j].a];

                    }
                    //return i;
                }
                if (sum > best)
                {
                    best = sum;
                    besti = i;
                }
                //return i;
            }

            if (besti != -1)
                return besti;

            return -1;
        }
        private List<int> GetAlldDegreeNodeIndex(out int count)
        {
            int i;
            count = 0;
            List<int> ret = new List<int>();
            for (i = 0; i < num_citta; i++)
            {
                if (degree_node[i] > 2)
                { 
                    ret.Add(i);
                    count += degree_node[i];
                }
            }

            return ret;
        }

        private int GetFirstMaxDegreeNodeIndex()
        {
            int i,max,imax;

            for (i = 0,max=2,imax=-1; i < num_citta; i++)
            {
                if (max < degree_node[i])
                {
                    max = degree_node[i];
                    imax = i;
                }
            }

            return imax;
        }

        private int GetFirst3MajDegreeNodeIndex()
        {
            int i;
            
            for (i = 0; i < num_citta; i++)
            {
                if (degree_node[i] >= 3)
                    return i;
            }

            return -1;
        }

        int Partition_Edges_rev(EDGE_LIST[] edges, float[,] d, int p, int r)
        {
            int i, j;
            double w;
            EDGE_LIST e_tmp;
            w = d[edges[r].a, edges[r].b];
            i = p - 1;
            for (j = p; j < r; j++)
            {
                if (d[edges[j].a, edges[j].b] >= w)
                {
                    i = i + 1;
                    e_tmp = edges[i];
                    edges[i] = edges[j];
                    edges[j] = e_tmp;
                }
            }
            e_tmp = edges[i + 1];
            edges[i + 1] = edges[r];
            edges[r] = e_tmp;
            return i + 1;
        }

        void QuickSort_Edges_rev(EDGE_LIST[] edges, float[,] d, int p, int r)
        {
            int q;
            if (p < r)
            {
                q = Partition_Edges_rev(edges, d, p, r);
                QuickSort_Edges_rev(edges, d, p, q - 1);
                QuickSort_Edges_rev(edges, d, q + 1, r);
            }
        }

        public EDGE_LIST[] GetEdgeSplit(out bool tour, bool sort_split, int rule)
        {
            EDGE_LIST[] e=null;
            int node,i,k;

            tour = false;
            if (!computed_lb)
                return null;

            i = Compute_Degree_Node();
            if (i == num_citta) //optimal sol in teoria, (tour trovato)
            {
                tour = true;
                return null;
            }

            //1° regola il primo nodo trovato >= 3
            //node= GetFirst3MajDegreeNodeIndex();
            
            //2° regola prende il nodo con grado max esistente e ritorna fra essi il primo trovato.
            //node = GetFirstMaxDegreeNodeIndex();
            
            //3° regola prende il primo nodo di grado 3, se non c'è il primo con grado > 3 (sembra la migliore)
            //node = GetFirstThirdDegreeNodeIndex();

            //5° regola prende il nodo con l'arco più grande,
            //node = GetMaxCostThirdDegreeNodeIndex();
            // 5°.b prende il nodo con la somma degli archi + grande.
            //node = GetMaxCostSumThirdDegreeNodeIndex();

            switch(rule)
            {
                case 1: node = GetFirst3MajDegreeNodeIndex();
                    break;
                case 2: node = GetFirstMaxDegreeNodeIndex();
                    break;
                case 3: node = GetFirstThirdDegreeNodeIndex();
                    break;
                case 4: node = GetMaxCostThirdDegreeNodeIndex();
                    break;
                case 5: node = GetMaxCostSumThirdDegreeNodeIndex();
                    break;
                default: node = -1;
                    break;

            }

            //se la regola non trava nulla ritorna null!! (1°,2°,3°)
            if (node == -1)
                return null;

            //if ((branchnode0))
            //    e = new EDGE_LIST[degree_node[node] + 2];
            //else
                e = new EDGE_LIST[degree_node[node]];
            k = 0;
            for (i = 0; i < num_citta; i++)
            {
                if (lb[i].a == node)
                {
                    e[k].a = node;
                    e[k++].b = lb[i].b;
                }
                else if (lb[i].b == node)
                {
                    e[k].a = lb[i].a;
                    e[k++].b = node;
                }

            }

            //4° regola prende tutti i nodi di grado >2
            //int count;
            //List<int> nodes = GetAlldDegreeNodeIndex(out count);
            //if (nodes == null)
            //    return null;
            //    e = new EDGE_LIST[count];
            //k = 0;
            //foreach (int n in nodes)
            //{
            //    for (i = 0; i < num_citta; i++)
            //    {
            //        if (i == n)
            //            continue;
            //        if (lb[i].a == n)
            //        {
            //            e[k].a = n;
            //            e[k++].b = lb[i].b;
            //        }
            //        //else if (lb[i].b == n)
            //        //{
            //        //    e[k].a = lb[i].a;
            //        //    e[k++].b = n;
            //        //}

            //    }
            //}

            
            //if ((branchnode0))
            //{
            //    e[k++] = lb[n1 - 1];
            //    e[k] = lb[n1];
            //}

            if(sort_split)
                QuickSort_Edges_rev(e, distance, 0, e.Length - 1);
       
            return e;
        }

        //aggiunge anche i nodi del 1-tree da branchare, quelli del nodo0, ovvero quelli che sono sempre di grado 2 dall'inizio
        //alla fine.
        public EDGE_LIST[] GetEdgeSplit_old(out bool tour)
        {
            EDGE_LIST[] e = null;
            int node, i, k;
            tour=false;
            if (!computed_lb)
                return null;

            i = Compute_Degree_Node();
            if (i == num_citta) //optimal sol in teoria, (tour trovato)
            {
                tour=true;
                return null;
            }

            //1° regola il primo nodo trovato >= 3
            //node= GetFirst3MajDegreeNodeIndex();

            //2° regola prende il nodo con grado max esistente e ritorna fra essi il primo trovato.
            //node = GetFirstMaxDegreeNodeIndex();

            //3° regola prende il primo nodo di grado 3, se non c'è il primo con grado > 3
            node = GetFirstThirdDegreeNodeIndex();

            //se la regola non trava nulla ritorna null!! (1°,2°,3°)
            if (node == -1)
                return null;

            e = new EDGE_LIST[degree_node[node]+2]; //2 sono gli archi del nodo0
            k = 0;
            int c = degree_node[node];
            for (i = 0; (i < num_citta) && (c>0); i++)
            {
                if (lb[i].a == node)
                {
                    e[k].a = node;
                    e[k++].b = lb[i].b;
                    c--;
                }
                else if (lb[i].b == node)
                {
                    e[k].a = lb[i].a;
                    e[k++].b = node;
                    c--;
                }
            }
            e[k++] = lb[n1-1];
            e[k] = lb[n1];

            QuickSort_Edges_rev(e, distance, 0, e.Length - 1);

            //4° regola prende tutti i nodi di grado >2
            //int count;
            //List<int> nodes = GetAlldDegreeNodeIndex(out count);
            //if (nodes == null)
            //    return null;
            //e = new EDGE_LIST[count];
            //k = 0;
            //foreach (int n in nodes)
            //{
            //    for (i = 0; i < num_citta; i++)
            //    {
            //        if (lb[i].a == n)
            //        {
            //            e[k].a = n;
            //            e[k++].b = lb[i].b;
            //        }
            //        else if (lb[i].b == n)
            //        {
            //            e[k].a = lb[i].a;
            //            e[k++].b = n;
            //        }

            //    }
            //}

            return e;
        }
        public float BuildLBFromTour(bool _1tree=false)
        {
            int i, j;
            if (!computed_tour)
                return 0;

            //Array.Clear(degree_node, 0, num_citta);

            int a,b,n;
            float v = 0;
            if (_1tree)
            {
                n = n1-1;
                {
                    b = sort_dist[Last_Popped.city, 1];
                    if (b > n1)
                    {
                        lb[n1].b = b;
                        lb[n1].a = Last_Popped.city;
                    }
                    else
                    {
                        lb[n1].a = b;
                        lb[n1].b = Last_Popped.city;
                    }
                    v += distance[b, n1];
                    //degree_node[b]++;
                    //degree_node[Last_Popped.city]++;
                }
            }
            else
                n = num_citta;

            for (i=0,j=0;i<n;i++)
            {
                a=tour_path[i];
                b=tour_path[i+1];
                if(a>b)
                {
                    lb[j].a=b;
                    lb[j++].b=a;
                }
                else
                {
                    lb[j].a=a;
                    lb[j++].b=b;
                }
                //degree_node[a]++;
                //degree_node[b]++;
                
                v += distance[a, b];
            }
            
            a = tour_path[i];
            b = tour_path[0];
            if (a > b)
            {
                lb[j].a = b;
                lb[j++].b = a;
            }
            else
            {
                lb[j].a = a;
                lb[j++].b = b;
            }   
        
 
            computed_lb = true;
#if DEBUG
            num_citta--;
            n1--;
            int[] tour_tmp = (int[])tour_path.Clone();
            BuildTourFromLB();
            num_citta++;
            n1++;
            for (i = 0; i < n1; i++)
            {
                if (tour_tmp[i] != tour_path[i])
                {
                    tour_path = tour_tmp;
                    System.Windows.Forms.MessageBox.Show("error BuildLBFromTour");
                    break;
                }
            }
#endif
                return v;
            
        }

        public void BuildTourFromLB()
        {
            int i, j;

            if (!computed_lb)
                return;

            bool[] visited = new bool[num_citta];
            //Reset_tour();

            tour_path[0] = lb[n1].a;
            tour_path[1] = lb[n1].b;
            //tour_path[num_citta - 1] = lb[num_citta - 1].b;

            visited[n1] = /*visited[num_citta - 2] =*/ true;


            for (i = 1; i < num_citta; i++)
            {
                for (j = 0; j < n1; j++)
                {
                    if (visited[j])
                        continue;
                    if (lb[j].a == tour_path[i - 1])
                    {
                        //allora il b è da copiare!
                        tour_path[i] = lb[j].b;
                        visited[j] = true;
                        break;
                    }
                    else if (lb[j].b == tour_path[i - 1])
                    {
                        //copio a...
                        tour_path[i] = lb[j].a;
                        visited[j] = true;
                        break;
                    }
                }
            }
        }

        public void BuildTourFromLB_old()
        {
            int i,j;

            if (!computed_lb)
                return;
            
            bool[] visited = new bool[num_citta];
            //Reset_tour();
            
            tour_path[0] = lb[num_citta - 2].a;
            tour_path[1] = lb[num_citta - 2].b;
            tour_path[num_citta - 1] = lb[num_citta - 1].b;
            
            visited[num_citta - 1] = visited[num_citta - 2] = true;
            

            for (i = 2; i < num_citta-1; i++)
            {
                for (j = 0; j < num_citta-2; j++)
                {
                    if (visited[j])
                        continue;
                    if(lb[j].a==tour_path[i-1])
                    {
                        //allora il b è da copiare!
                        tour_path[i] = lb[j].b;
                        visited[j] = true;
                        break;
                    }
                    else if (lb[j].b == tour_path[i - 1])
                    {
                        //copio a...
                        tour_path[i] = lb[j].a;
                        visited[j] = true;
                        break;
                    }
                }
            }
        }

        public void ComputeAlpha(int node0, int n, float[,] dist, int[,] sort, EDGE_LIST[] lb)
        {
            int j, k;
            alpha = new float[n, n];
            float arc,arc2;

            //caso 2 nodo 1° come estremo
            k = n - 2;
            j = n - 1;

            arc = dist[lb[j].a, lb[j].b];
            arc2 = dist[lb[k].a, lb[k].b];
            //tengo il piu lungo in arc e l'altro solo l'indice in k.
            if (arc < arc2)
            {
                arc = arc2;
                k = lb[j].b;
            }
            else 
                k=lb[k].b;
            


            //calcolo alpha di 0 (caso del nodo 1)
            //for (j = 0; j < n; j++)
            //{
            //    if (j == node0)
            //        alpha[j, j] = 0;
            //    else
            //        alpha[j, node0] = alpha[node0, j] = Math.Abs((dist[node0, j] - dist[lb[k].a, lb[k].b]));
            //}

            //caso 1. l'arco è presente nel 1-tree ---> quindi il relativo alpha = 0. (in automatico con la matrice a zero)

            //caso 2. l'arco ha un estremita nel node0
            for (j = 0; j < n; j++)
            {
                if ((j == node0)||(j==k)) // continue perchè sono i 2 archi e quindi caso 1 , alpha=0
                    continue;
                alpha[node0, j] = alpha[j, node0] = dist[node0, j] - arc;
            }

            //caso 3, 
            //Calcolo i rimanenti nodi...
            if (!ComputeBeta(node0, n, dist, lb, alpha))
                alpha = null;

            //distanze pre ordinate...
            sort_distances_alpha();


        }

        public void sort_distances_alpha()
        {
            int i, j;
            
            sort_alpha = new int[num_citta, num_citta];

            for (i = 0; i < num_citta; i++)
            {
                for (j = 0; j < num_citta; j++)
                    sort_alpha[i, j] = j;

                Quick_Sort_dist_Alpha(i, 0, n1);
            }
        }


        private bool ComputeBeta(int node0, int n, float[,] dist, EDGE_LIST[] lb, float[,] alpha)
        {
            int i,j,k,i2,j2,k2; //contatori
            int n2;             // n-2
            float[] beta;        // alpha in lineare
            int[]   mark;        // usato per beta
            int[]   dad;         // usato per beta
            int[]   mask;        // vettore per memorizzare la permutazione delle città
            int[]   dadmask;      // padre della permutazione...
            cTSP_Tree Tree;

            n2  = n-2;

            //alloco la mem. x beta
            beta        = new float[n];
            dad         = new int[n];
            mark        = new int[n];
            mask        = new int[n];
            dadmask     = new int[n];

            //Costruisco L'albero sulla lista di archi...
            Tree = new cTSP_Tree(lb[0].a);
            
            mark[1] = 1;
            
            for(k=1,j2=2;k<n;k++)
            {
                j=Tree.GetCity(k);
                
                for(i=0,i2=2;i<n2;i++,i2++)
                {
                    if((lb[i].a==j)&&(mark[i2]==0))
                    {
                        Tree.Insert_Node(lb[i].a,new cTSP_Tree.Node(lb[i].b),ref j2);
                        j2++;
                        mark[i2]=1;
                    }
                    if((lb[i].b==j)&&(mark[i2]==0))
                    {
                        Tree.Insert_Node(lb[i].b,new cTSP_Tree.Node(lb[i].a),ref j2);
                        j2++;
                        mark[i2]=1;
                    }
                }
    
            }
            
            mask[0] = node0;
            mask[1] = lb[0].a;
            for(i=2;i<n;i++)
            {
                dadmask[i] = Tree.GetParentMask(i);  //padre delle città permutate
               

                dad[i]     = Tree.GetParent(i);       //padre originale
                mask[i]    = Tree.GetCity(i);         //città permutata in...
                
#if DEBUG
                if ((i!=node0)&& ((dadmask[i] < 0)||
                    (dad[i] < 0)||
                    (mask[i] < 0)))
                    System.Diagnostics.Debug.Fail("ERROR TREE dad, dadmask, mask < 0");
                if(dadmask[i]>=i)
                    System.Diagnostics.Debug.Fail("dadmask[i]>=i");
#endif
            }

#if DEBUG
    
        System.IO.StreamWriter sw = new System.IO.StreamWriter("mask.log");
        for(i=0;i<n;i++)
            sw.WriteLine(String.Format("mask["+i+"]="+mask[i]));
        sw.Close();

        sw = new System.IO.StreamWriter("dad.log");
         for(i=0;i<n;i++)
            sw.WriteLine(String.Format("dad["+i+"]="+dad[i]));
        sw.Close();

        sw = new System.IO.StreamWriter("dadmask.log");
         for(i=0;i<n;i++)
            sw.WriteLine(String.Format("dadmask["+i+"]="+dadmask[i]));
        sw.Close();
        
    
#endif


        //calcolo Beta
        Array.Clear(mark, 0, n);

        for(i=1;i<n;i++)
        {
            i2      = mask[i];
            beta[i] = float.MinValue;
            for(k=i,k2=i2;k!=1/*2*/;k=j,k2=j2)
            {
                j2 = dad[k2];
                j  = dadmask[k];
                if (j == node0)
                    continue;
                beta[j] = Math.Max(beta[k], dist[k2,j2]);
                mark[j]=i;
            }

            for(j=1;j<n;j++)
            {
                if (j == node0)
                    continue;
                j2 = mask[j];
                if(j!=i)
                {
                    if(mark[j]!=i)
                    {
                        beta[j] = Math.Max(beta[dad[j]], dist[j2,dad[j2]]);
                        alpha[i2,j2] = alpha[j2,i2] = (dist[i2,j2] - beta[j]);
                    }
                }
            }
        }


        return true;
        }

        public bool ComputeAlphaNN(int node0, int alpha_num)
        {
            float w;

            if (alpha_num == 0)
                ALPHA_NN = (int)(Math.Round(Math.Sqrt(num_citta) +Math.Log(num_citta))+2) ;
            else
                ALPHA_NN = alpha_num;

            if (ALPHA_NN < ALPHA_NN_MIN)
                ALPHA_NN = ALPHA_NN_MIN;
            if (ALPHA_NN > ALPHA_NN_MAX)
                ALPHA_NN = ALPHA_NN_MAX;
           
            if (lb == null)
                w = Compute_1_tree_old(0, distance, null);
            
            
            ComputeAlpha(node0,num_citta, distance, sort_dist, lb);
            if (alpha == null)
                return false;

            //controllo che ci siano almeno NN+2 nodi per usare le candidate set di 20 nodi
            if (num_citta < ALPHA_NN /*+ 2 */)
            {
                //ALPHA_NN = (n1) / 2;
                ALPHA_NN = num_citta - 2;
            }
            //else
            //{
            //    //ricalcolo il n° totale di archi
            //    ALPHA_NA = (ALPHA_NN) * (num_citta - 2);
            //}
            
            ALPHA_NA = (ALPHA_NN-1) * (num_citta - 2)/2; ;
            return true;
        }


        public void Christofides()
        {
            int i, j,k;
            List<int> X0 = new List<int>();
            List<EDGE_LIST> M0;
            float min;
            float mst = Compute_MST();
            Compute_Degree_Node_MST();

            //calcolo X0
            for (i = 0; i < num_citta; i++)
            {
                //se è dispari
                if (degree_node[i] % 2 == 1)
                    X0.Add(i);
            }

            int ii=(X0.Count - 1) * (X0.Count)/2 ;
            EDGE_LIST[] tmp = new EDGE_LIST[ii];
            for (i = 0,k=0; i < X0.Count-1; i++)
            {
                for(j=i+1;j<X0.Count;j++)
                {
                    tmp[k].a = X0[i];
                    tmp[k++].b = X0[j];
                }
            }
            this.mst.QuickSort_Edges(tmp, distance, 0, ii - 1);
            //ora inserisco gli archi a costo minimo in M0
            M0 = new List<EDGE_LIST>();
            bool[] mark2 = new bool[num_citta];
            int count = X0.Count;
            
            while (count > 0)
            {
                for (i = 0; i < ii; i++)
                {
                    if((mark2[tmp[i].a])||(mark2[tmp[i].b]))
                        continue;
                    else
                    {
                        //essendo già ordinato è il minore disponibile.
                        M0.Add(tmp[i]);
                        mark2[tmp[i].a]=mark2[tmp[i].b]=true;
                        count-=2;
                        break;
                    }
                }
#if DEBUG
                if (i == ii) //bug
                    System.Windows.Forms.MessageBox.Show("i=ii");
#endif
         
            } 
            //ora basta unire MST con gli archi M0...
            //e creaere un percorso euleriano
            int ng = n1 + M0.Count;
            EDGE_LIST[] G = new EDGE_LIST[ng];
            lb.CopyTo(G, 0);
            M0.ToArray().CopyTo(G, n1);
#if DEBUG
            //cvontrollo che sia effettivamente di grado pari tutto
            for (i = 0; i < ng; i++)
            {
                int d = 0;
                for (j = 0; j < ng; j++)
                {
                    if ((i == G[j].a) || (i == G[j].b))
                        d++;
                }

                if (d % 2 != 0)
                    System.Windows.Forms.MessageBox.Show("grafo non euleriano. Cristofides ERRORR!");
            }
#endif
            //in G c'è il grafo mst + min.matching
            //ora bisogna cercare un percorso euleriano che da un nodo0 visiti tutti gli altri e torni al nodo0
            bool[] mark = new bool[ng];
            mark2 = new bool[num_citta];
            List<int> path = new List<int>();
            
            path.Add(G[0].a);
            mark2[G[0].a] = true;
            int imin=-1;
            int imin2 = -1;
            bool child = false;

            for (i = 1; i < num_citta; i++)
            {
                int node = path.Last();
                min = float.MaxValue;
                child = false;
                imin = -1;
                //trovo arco costo minimo.
                for (j = 0; j < ng; j++)
                {
                    if (mark[j])
                        continue;
                    if((G[j].a!=node)&&(G[j].b!=node))
                        continue;
                    if ((min > distance[G[j].a, G[j].b]))
                    {
                        if ((G[j].a == node))
                        {
                            if(mark2[G[j].b])
                            {
                                int node1 = G[j].b;

                                for (k = 0; k < num_citta; k++)
                                {
                                    if (mark2[k])
                                        continue;
                                    if (min > distance[node, k])
                                    {
                                        min = distance[node, k];
                                        imin2 = k;
                                        imin = j;
                                        child = true;
                                    }
                                }
                            }
                            else
                            {
                                min = distance[G[j].a, G[j].b];
                                imin = j;
                                child = false;
                            }
                            
                        }
                        else if ((G[j].b == node))
                        {
                            if (mark2[G[j].a])
                            {
                                //se il nodo che dovrei inserire è gia marcato..
                                //cerco un arco non inserito appartenente al nodo marcato...
                                int node1 = G[j].a;

                                for (k = 0; k < num_citta; k++)
                                {
                                    if (mark2[k])
                                        continue;
                                    if (min > distance[node, k])
                                    {
                                        min = distance[node, k];
                                        imin2 = k;
                                        imin = j;
                                        child = true;
                                    }
                                }
                            }
                            else
                            {
                                min = distance[G[j].a, G[j].b];
                                imin = j;
                                child = false;
                            }
                        }
                    }
                }
                //aggingo il nodo
                if (child)
                {
                    mark2[imin2] = true;
                    path.Add(imin2);
                }
                else if (G[imin].a == node)
                {
                    mark2[G[imin].b]=true;
                    path.Add(G[imin].b);
                    mark[imin] = true;
                }
                else
                {
                    mark2[G[imin].a] = true;
                    path.Add(G[imin].a);
                    mark[imin] = true;
                }
            }
            //l'ultimo nodo del percorso si collega al primo. percorso euleriano
#if DEBUG
            if (path.Count != num_citta)
                System.Windows.Forms.MessageBox.Show("eul path count = " + path.Count);
#endif
            path.ToArray().CopyTo(tour_path,0);
            computed_tour = true;
        }
        

        private List<int>[] GetCandidateNodesForCollapse(out int nodeBL, out int nodeBR, out int nodeTL, out int nodeTR)
        {
            bool sparse = false;
            double dist_avg = 0.0;
            //float[,] distance = distance;
            int i, j;
            //int num_citta;
            //int n1 = n - 1;
            //int nodeTL, nodeTR, nodeBL, nodeBR;
            nodeTL = nodeTR = nodeBL = nodeBR = -1;
            
            if (num_citta <= 3)
                return null;
            int total = num_citta * (n1) / 2;

            for (i = 0; i < n1; i++)
            {
                for (j = i + 1; j < num_citta; j++)
                    dist_avg += distance[i, j];
            }

            dist_avg /= total;

            //stima se è sparso o no... approx.
            if ((dist_avg < GetMax_x() - GetMin_x()) &&
                dist_avg < GetMax_y() - GetMin_y())
                sparse = true;


            Node min = new Node(),
                 max = new Node();

            min.x = min.y = int.MaxValue;
            max.x = max.y = int.MinValue;
            //int w = -1, h = -1;

            List<int>[] count = null; ;

            if (sparse)
            {
                float minx = GetMin_x();
                float maxx = GetMax_x();
                float miny = GetMin_y();
                float maxy = GetMax_y();

                float BL = minx + miny;
                float TL = minx + maxy;
                float BR = maxx + miny;
                float TR = maxx + maxy;

                count = new List<int>[num_citta];

                for (i = 0; i < num_citta; i++)
                {
                    count[i] = new List<int>();

                    float x = Get_x(i);
                    float y = Get_y(i);

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


                    for (j = 0; j < num_citta; j++)
                    {
                        if (j == i)
                            continue;
                        if (distance[i, j] < dist_avg)
                        {
                            count[i].Add(j);
                        }

                    }
                }


                //se ho i 4 nodi d'angolo... tengo solo quelli.
                if ((nodeBL != -1) ||
                    (nodeBR != -1) ||
                    (nodeTL != -1) ||
                    (nodeTR != -1))
                {
                    //non so quale angolo trovato.. parto da br e almeno 1 su 4 è valido
                    if (nodeBL == -1)
                    {
                        if (nodeBR != -1)
                            nodeBL = nodeBR;
                        else if (nodeTL != -1)
                            nodeBL = nodeTL;
                        else if (nodeTR != -1)
                            nodeBL = nodeTR;

                    }
                    //sicuramente c'è BL...
                    if (nodeBR == -1)
                    {
                        //if (nodeBL != -1)
                        nodeBR = nodeBL;
                        //else if (nodeTR != -1)
                        //    nodeBR = nodeTR;
                    }

                    //sicuramente c'è BL e BR
                    if (nodeTL == -1)
                    {
                        if (nodeTR != -1)
                            nodeTL = nodeTR;
                        else //if (nodeBL != -1)
                            nodeTL = nodeBL;

                    }
                    //gli altri 3 ci sono...
                    if (nodeTR == -1)
                    {
                        //if (nodeTL != -1)
                        nodeTR = nodeTL;
                        //else if (nodeBR != -1)
                        //    nodeTR = nodeBR;
                    }

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


                /* posso cercare i NN in comune di indice fra i vari nodi. 
                 * ovvero se il nodo 1 ha come 1° nn 3 AND 3 ha come 1° NN 1 
                 * ---> allora sono in cluster!!!
                */

                for (i = 0; i < num_citta; i++)
                {
                    for (int i2 = 0; i2 < count[i].Count; i2++)
                    {
                        int sort_index = 0;
                        int l = count[i][i2];

                        for (j = 1; j < num_citta; j++)
                        {
                            if (l == sort_dist[i, j])
                            {
                                sort_index = j;
                                break;
                            }
                        }

                        if (sort_index == 0)
                            continue;

                        //verifico che il nodo l sia il medesimo del nodo j per fare il cluster...
                        if (i == sort_dist[l, sort_index])
                        {
                            //ok agglomero che si può fare un clusterino...
                        }
                        else if (count[l].Count > 0)// lo rimuovo che così mi rimane il cluster... ;)
                        {
                            count[i].Remove(l);
                            count[l].Remove(i);
                            i2--;
                        }


                    }
                }
            }

            return count; //return null se nulla da fare...
        }

        private List<int>[] AdjustCandidateNodesForCollapse(List<int>[] count)
        {
            int i, j;

            //bisogna trovare i cicli nei cluster ed eliminarli...
            List<int>[] cluster = new List<int>[num_citta];
            //mark = new bool[n];
            int[] m = new int[num_citta];
            for (i = 0; i < num_citta; i++)
                m[i] = i;

            //creo i cluster...
            for (i = 0; i < num_citta; i++)
            {
                if (cluster[i] == null)
                    cluster[i] = new List<int>();
                //cerco il nodo i in che cluster è presente...
                for (j = i + 1; j < num_citta; j++)
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

            //poi elimino le ridondanze dei nodi presenti in + cluster
            for (i = 0; i < num_citta; i++)
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

            //unisco eventuali cluster non apparentemente collegati (esempio c1 = {0,3,4} c2={5,6,4} --> c1=c1+c2
            //se c'è un'intersezione nel cluster diventa un cluster unico.
            //i cluster devono essere insiemi disgiunti, se hanno un'intersezione si fondono insieme per un cluster + grande
            for (i = 0; i < num_citta; i++)
            {
                for (int il = 0; il < cluster[i].Count; il++)
                {
                    int l = cluster[i][il];
                    for (j = i + 1; j < num_citta; j++)
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

            //rimuovo i nodi duplicati nel cluster creati dalla fusione precedente.
            for (i = 0; i < num_citta; i++)
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

            return cluster;
        }

        public bool TSP_GraphReduction()
        {
            bool ret = false;

            do
            {
                ret = TSP_GraphReductionStep();
            } while (ret);

            if (ret == false)
                return true;
            else
                return false;
        }

        public bool TSP_GraphReductionStep()
        {
            //algoritmo di clustering dei nodi....

            /* idea di partenza:
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
             * crea i cluster per i 4 nodi degli angoli se si fossero trovati, o equivalentemnte associare il cluster 
             * contente quei nodi ai nodi stessi, ma meglio fare calcolare appositamente il cluster per quei nodi
             * 
             * RISULTATO aspettato: (obsoleto)
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

            int nodeBL, nodeBR, nodeTL, nodeTR;
            List<int>[] count = GetCandidateNodesForCollapse(out nodeBL, out nodeBR, out nodeTL, out nodeTR);
            if (count == null)
                return false;

            //ora costruire i cluster...
            List<int>[] cluster = AdjustCandidateNodesForCollapse(count);

        #region per sistemare i nodi in 0 cluster... (DA FARE ANCORA)
            //Adjusto il count, dopo aver creato il cluster, e rimuovo la ddove c'è solo 1 nodo contato per candidate cluster
            int i, j;

            for (i = 0; i < num_citta; i++)
            {
                if (count[i].Count == 1)
                {
                    //c1++;
                    if ((count[count[i][0]].Count) >= 1)
                        count[i].Clear();

                }
            }

            /*i cluster con 0 nodi, sono interessanti... 
             * perchè si possono aggregare ad espandere gli altri, 
             * se loro non risultassero in nessun cluster...
             */
            bool[] mark = new bool[num_citta];
            
            for (i = 0; i < num_citta; i++)
            {
                if (count[i].Count > 0)
                    mark[i] = true;
                foreach (int l in count[i])
                    mark[l] = true;
            }
            int c2 = 0;
            for (i = 0; i < num_citta; i++)
                if (!mark[i])
                {
                    //textBox1.AppendText("\r\nNodo " + i + " non appartenente a nessn cluster!");
                    for (j = 1; j < num_citta; j++)
                        if (count[sort_dist[i, j]].Count >= 1)
                        {
                            //textBox1.AppendText("\r\nil suo primo cluster è del nodo:" + tsp.sort_dist[i, j]);
                            break;
                        }
                }
                else
                    c2++;

            //textBox1.AppendText("\r\nTOT = " + c);
            //textBox1.AppendText("\r\nTot 1 = " + c1);
            //textBox1.AppendText("\r\nTot cluster = " + c2);
            //textBox1.AppendText("\r\nTot cluster[] = " + c3);
        #endregion


            


            //Devo Collassare il Grafo e memorizzare l'operazione per "l'undo"...
            if(stacknodes==null)
                stacknodes = new Stack<Node>();
            foreach (List<int> c in cluster)
                if(c.Count>0)
                    CollapseCluster(c,cluster,false);

            //risistemo
            tour_length = 0;
            tour_path = new int[num_citta];
            mst = new MST(num_citta);
            lb = new EDGE_LIST[num_citta];
            chull = null;
            alpha = null;
            sort_alpha = null;
            computed_lb = false;
            computed_tour = false;
            computed_chull = false;
            degree_node = new int[num_citta];
            delta = (int)(Math.Round(Math.Sqrt(num_citta) + Math.Log10(num_citta)));
            
            //collassati i cluster.. Calcolo e ordino le distanze...
            distance = new float[num_citta, num_citta];
            for(i=0;i<n1;i++)
                for(j=i+1;j<num_citta;j++)
                    distance[j,i]=ComputeDistance(i,j);

            //ora  ordino le distanze.
            sort_dist = new int[num_citta, num_citta];
            sort_distances();

            // ho collassto

            //devo controllare di non avere meno di 3 nodi a fine operazione...
            //if (num_citta < 3)
            //{
                //roll back... fino a 3 citta.
                while(num_citta<3)
                    TSP_Graph_Restore(true, true);
            //}


            return true;
        }
    
        //collassa il nodo c in c0 (facendo la media delle coord)
        private void Collapse1Node(Node n0, Node n, bool compute_dist)
        {
            //sistemo il nodo nuovo
            //num_citta -1
            num_citta--;
            n1--;
            
            int iold;
            int i;
            //vettore delle x e y da sistemare...
            float[] oldx = x;
            float[] oldy = y;
            x = new float[num_citta];
            y = new float[num_citta];

            for(i=0,iold=0; i<num_citta;i++)
            {
                if (i == n0.city)
                {
                    setNode(n0.city, n0.x, n0.y);
                    continue;
                }
                
                if (i == n.city)
                    iold++;
                
                setNode(i, oldx[i + iold], oldy[i + iold]);
                
            }
            //x=newx;
            //y=newy;

            /*
             * Deciso di ricalcolarle a fine collassamento nodi, così si calcolano una volta per tutte..
             * 
             */
            if (compute_dist)
            {
                //distanze da ricalcolare (solo per n0)
                float[,] olddist = distance;
                distance = new float[num_citta, num_citta];
                int j;
                iold = 0;
                int jold;

                for (i = 0; i < num_citta; i++)
                {
                    distance[i, i] = 0;

                    if ((i == n0.city))
                    {
                        iold = 1;
                        for (j = i + 1; j < num_citta; j++)
                            distance[j,i] = ComputeDistance(i, j);
                    }
                    else
                    {
                        for (j = i + 1, jold = 0; j < n1; j++)
                        {
                            if (j == n0.city)
                                continue;

                            if (j == n.city)
                                jold++;

                            distance[i, j] = distance[j, i] = olddist[i - iold, j - jold];
                        }
                    }
                }
            }
            //sort_dist...
            // ...si fa alla fine....
        
        }

        /// <summary>
        /// collassa il cluster
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="C"></param>
        private void CollapseCluster(List<int> cluster, List<int>[] C, bool media)
        {
            Node n = new Node();
            Node n0 = new Node();
            int c0 = cluster[0];

            n0.x = Get_x(c0);
            n0.y = Get_y(c0);
            n0.city = c0;


            for (int i = 1; i < cluster.Count;)
            {
                int c = cluster[i];
                n.city = c;
                n.x = Get_x(c);
                n.y = Get_y(c);

                //tengo quello di indice inferiore fra i 2... 
                if (n0.city > n.city)
                {
                    Node t = n;
                    n = n0;
                    n0 = t;
                }
                stacknodes.Push(n0); // ne memorizzo 2 perchè da 2 ne fondo in 1.. per ripristinare ho bisogno di entrambi...
                stacknodes.Push(n); //memorizzo per l'undo.

                //ora elimino il nodo e aggiusto il TSP
                //faccio la media della posizione per il nuovo nodo, o elimino semplicemente il vecchio?
                if (media)
                {
                    n0.x += n.x;
                    n0.y += n.y;
                    n0.x /= 2;
                    n0.y /= 2;
                }
                
                Collapse1Node(n0, n,false);
                cluster.Remove(c);
                
                if (cluster.Count == 1)
                    cluster.Clear();
                // dopo che ho collassato il nodo cluster.count si potrebbe decrementare, ma lasciam così x ora.

                //ora devo sistemare nella lista dei cluster il nodo collassato modificando il suo valore...
                //se è >n allora devo decrementare 1, giusto? ;)
                foreach(List<int> _c in C)
                {
                    //if(_c == cluster)
                    //    continue;

                    for (int _i = 0; _i < _c.Count; _i++)
                    {
                        if (_c[_i] >= n.city)
                            _c[_i]--;
                    }

                }
                
            }

            //sort_dist da ricalcolare.. fare a fine.... cosi si ricalcolano tutte 1 volta sola...
            // anche le distanze si potrebbero ricalcolare tutte alla fine procedimento...
            //qualcos'altro?
        }

        //ricorsivo fino alla fine....
        public bool TSP_Graph_Restore(bool compute_dist, bool step,bool insert=true)
        {
            if (stacknodes == null)
                return true;
            if (stacknodes.Count == 0)
            {
                stacknodes = null;
                return true;
            }

            //ora devo spoppare 2 nodi modificando dalla soluzione corrente il 2°
            Node n  = new Node(),
                 n0 = new Node();

            Last_Popped = n = stacknodes.Pop();
            n0 = stacknodes.Pop();

            Expand2Node(n0, n);

            //risistemo
            //tour_length = 0;
            //tour_path = new int[num_citta];
            mst = new MST(num_citta);
            lb = new EDGE_LIST[num_citta];
            chull = null;
            alpha = null;
            sort_alpha = null;
            computed_lb = false;
            //computed_tour = false;
            computed_chull = false;
            degree_node = new int[num_citta];
            delta = (int)(Math.Round(Math.Sqrt(num_citta) + Math.Log10(num_citta)));

            //ora sistemare le distanze e le sort distance...
            if (compute_dist)
            {
                //dovrei ricalcolare solo i valori relativi ai 2 nuovi nodi, 
                //le altre sono uguali..
                float[,] olddist = distance;
                distance = new float[num_citta, num_citta];
                int i, j;
                int iold = 0,
                    jold;

                int inn = n1;
                int jnn = num_citta;
                if (n.city == n1) //caso particolare
                {
                    inn++;
                    jnn--;
                }

                for (i = 0; i < inn; i++)
                {
                    distance[i, i] = 0;

                    if ((i == n0.city) || (i == n.city))
                    {
                        if (i == n.city)
                            iold = 1;
                        for (j = 0; j < num_citta; j++)
                            distance[j, i] = ComputeDistance(i, j);
                    }
                    else
                    {
                        if (i >= n.city)
                            jold = 1;
                        else
                            jold = 0;

                        for (j = i + 1; j < jnn; j++)
                        {
                            if (j == n0.city)
                            {
                                //distance[j, i] = ComputeDistance(i, j);
                                continue;
                            }

                            if (j == n.city)
                                jold++;

                            distance[i, j] = distance[j, i] = olddist[i - iold, j - jold];
                        }
                    }
                }

                //ora le distanze ordinate...
                sort_dist = new int[num_citta, num_citta];
                sort_distances();

                //ora il tour..
                if (computed_tour)
                {
                    //cerco fra n ed n0 quello + esterno....
                    RecalcultateCoordMinMaxXY();
                    Node center = new Node();
                    center.x = (max_x - min_x) / 2;
                    center.y = (max_y - min_y) / 2;
                    center.city = -1;
                    float dx, dy;
                    dx = center.x - n0.x;
                    dy = center.y - n0.y;
                    float n0dradius = (float)Math.Round(Math.Sqrt(dx*dx + dy*dy));
                    dx = center.x - n.x;
                    dy = center.y - n.y;
                    float ndradius = (float)Math.Round(Math.Sqrt(dx * dx + dy * dy));
                    //se n è esterno no problem perchè non inserito.
                    //altrimenti swappo n con n0
                    if ((n0dradius > ndradius)&&(insert))
                    {
                        center = n;
                        n = n0;
                        n0 = center;
                    }
                    //poi bisogna inserire il nodo escluso nell'arco più vicino..... 
                    //(???)
                    //da fare....

                   
                    //vecchia versione non funzionante bene... in certi problemi trova una soluzione euristica
                    //migliore... ma insomma inutile cosa così...
                    int[] tour_old = tour_path;
                    tour_path = new int[num_citta];
                    //copio...
                    for (i = 0; i < n1; i++)
                    {
                        if (n.city <= tour_old[i])
                            iold = 1;
                        else
                            iold = 0;

                        tour_path[i] = tour_old[i] + iold;
                    }
                    //ora dovrei sistemare i nodi n0 ed n fra i loro rispettivi 2 vicini.. (provare)
                    //
                    //caso particolare: se uno dei 2 nodi da inserire è uno dei 2 vicini. (forse è + facile)
                    //partire dal nodo n che è ancora da inserire... poi eventualmente verificare n0...
                    //se i 2 di n vicini sono consecutivi nel tour inserirlo in mezzo


                    if (insert)
                    {
                        if ((sort_dist[n.city, 1] == n0.city) || (sort_dist[n.city, 2] == n0.city))
                        {
                            //se un vicino è l'altro nodo n0.... controllo che lo sia anche per n0...
                            if ((sort_dist[n0.city, 1] == n.city) || (sort_dist[n0.city, 2] == n.city))
                            {
                                //allora inserisco n vicino ad n0...
                                //ma prima o dopo n0?
                                int in0prev = -2, in0next = -2, in0 = -2;
                                int n0p, n0n;
                                for (i = 0; i < num_citta; i++)
                                {
                                    if (tour_path[i] == n0.city)
                                    {
                                        in0prev = i - 1;
                                        in0next = i + 1;
                                        in0 = i;
                                        break;
                                    }
                                }
                                if (in0next == n1)
                                    in0next = 0;
                                if (in0prev == -1)
                                    in0prev = n1 - 1;

                                n0p = tour_path[in0prev];
                                n0n = tour_path[in0next];

                                if (distance[n.city, n0p] > distance[n.city, n0n])
                                {
                                    //altrimenti lo inserisco dopo n0
                                    in0++;
                                }
                                //caso particolare in0==n1 impossibile.
                                for (i = n1; i > in0; i--)
                                    tour_path[i] = tour_path[i - 1];
                                tour_path[in0] = n.city;

                            }
                            else
                            {
                                //altrimenti... (capire)
                                //se i 2 di n vicini sono consecutivi nel tour inserirlo in mezzo
                                int a = sort_dist[n.city, 1];
                                int b = sort_dist[n.city, 2];

                                int ia, ib;
                                for (ia = -1, ib = -1, i = 0; (i < num_citta) && ((ia == -1) || (ib == -1)); i++)
                                {
                                    if (tour_path[i] == a)
                                        ia = i;
                                    else if (tour_path[i] == b)
                                        ib = i;
                                }

                                if (ia > ib)
                                {
                                    int tmp = ib;
                                    ib = ia;
                                    ia = tmp;
                                }


                                if ((ia + 1 == ib))
                                {
                                    //sono vicini!!! inserisco n nel mezzo
                                    for (i = n1; i > ib; i--)
                                        tour_path[i] = tour_path[i - 1];
                                    tour_path[ib] = n.city;
                                }
                                else
                                {
                                    //se non sono vicini???
                                    tour_path[n1] = n.city;//provvisorio
                                }



                            }
                        }
                        else //altrimenti?
                        {
                            //altrimenti... (capire)
                            //se i 2 di n vicini sono consecutivi nel tour inserirlo in mezzo
                            //int a = sort_dist[n.city, 1];
                            //int b = sort_dist[n.city, 2];

                            //int ia, ib;
                            //for (ia = -1, ib = -1, i = 0; (i < num_citta) && ((ia == -1) || (ib == -1)); i++)
                            //{
                            //    if (tour_path[i] == a)
                            //        ia = i;
                            //    else if (tour_path[i] == b)
                            //        ib = i;
                            //}

                            //if (ia > ib)
                            //{
                            //    int tmp = ib;
                            //    ib = ia;
                            //    ia = tmp;
                            //}


                            //if ((ia + 1 == ib))
                            //{
                            //    //sono vicini!!! inserisco n nel mezzo
                            //    for (i = n1; i > ib; i--)
                            //        tour_path[i] = tour_path[i - 1];
                            //    tour_path[ib] = n.city;
                            //}
                            //else
                            //{
                            //    //se non sono vicini???
                            //    tour_path[n1] = n.city;//provvisorio
                            //}

                            tour_path[n1] = n.city;
                        }


                        tour_length = Calculate_tour_length();

                        //nel peggiore dei casi dovrebbe essere uno scambio 2-opt. invece
                        //che impazzire a capire dove metterlo faccio un 2-opt dal tour corrente.
                        //c2opt _2opt = new c2opt(this, 2, false);
                        //do
                        //{
                        //    _2opt.do2opt();
                        //}
                        //while (_2opt.GetResult());
                        c3opt _3opt = new c3opt(this, 1, false, 0, false);
                        do
                        {
                            _3opt.do3opt();
                        } while (_3opt.GetResult());
                    }
                }
                else
                {
                    tour_length = 0;
                    tour_path = new int[num_citta];
                    computed_tour = false;
                }
            }
            else
            {
                tour_length = 0;
                tour_path = new int[num_citta];
                computed_tour = false;
            }
            //ricorsivo fino ad aver ripristinato l'originale
            if (step)
                return true;
            else
                return TSP_Graph_Restore(compute_dist,step);
        }

        //aggiorna n0 ed aggiunge n al tsp
        private void Expand2Node(Node n0, Node n)
        {
            //sistemo il nodo nuovo
            //setNode(n0.city, n0.x, n0.y);

            //num_citta -1
            num_citta++;
            n1++;

            int iold;
            int i;
            //vettore delle x e y da sistemare...
            float[] oldx = x;
            float[] oldy = y;
            x = new float[num_citta];
            y = new float[num_citta];

            for (i = 0, iold = 0; i < num_citta; i++)
            {
                if (i == n0.city)
                    setNode(n0.city, n0.x, n0.y);
                else if (i == n.city)
                {
                    iold++;
                    setNode(n.city, n.x, n.y);
                }
                else
                {
                    x[i] = oldx[i - iold];
                    y[i] = oldy[i - iold];
                }
            }
        }

        public float TSP_Graph_Restore_1Tree()
        {
            TSP_Graph_Restore(true, true, false);
            
            if(stacknodes==null)
                return 0;

            return BuildLBFromTour(true);
            //return CalculateLBLength();
        }



        /* Per la riduzione del PR
         * usare le 2 regole base.
         * sono da applicare al tour, quindi prima trovare un tour iniziale
         * poi se (u,v)x = 1 --> ovvero se l'arco u,v è in soluzione, cioè nel tour.....
         * e
         * se uw o vw in soluzione allora shrink u,v
         */

        public int pr_Reduction_Rule1()
        {
            //cercare 3 nodi u,v,w tali che: se wuv=w,v,u allora shrink u,v
            int u,v,w;
            //int i,j,k;
            int count = 0;
            //u=v=w=-1;
            //la i la uso come w
            for(w=0;w<num_citta;w++)
            {
                //la j la uso come v
                for(v=0;v<num_citta;v++)
                {
                    if (v == w)
                        continue;
                    //la k la uso come u
                    for(u=v+1;u<num_citta;u++)
                    {
                        if((u==w)||(u==v))
                            continue;

                        //float wuv = distance[i,k] + distance[k,j];
                        //float wvu = distance[i,j] + distance[j,k];
                        float wuv = distance[w, u] ;
                        float wvu = distance[w, v] ;

                        if(wuv==wvu)
                        {
                            //shrink u v in un unico nodo u';
                            count++;
                        }
                    }
                }
            }

            return count;
        }

    }
}
