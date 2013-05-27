using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using NeoGraph.Collections;

namespace NeoGraph
{
    [DataContract(IsReference=true, Namespace="neograph.googlecode.com")]
    public class Graph
    {
        //const int INFINITY = Int32.MaxValue;

        [DataMember(Order=0)]
        public List<Vertex> VertexList { get; set; }
        [DataMember(Order=1)]
        public List<Edge> EdgeList { get; set; }
        
        public List<Edge> EdgeSolution { get; private set; }

        public Graph()
        {
            VertexList = new List<Vertex>();
            EdgeSolution = new List<Edge>();
            EdgeList = new List<Edge>();
            //VisitedVertex = 0;
        }

        public void Clear()
        {
            VertexList.Clear();
            EdgeSolution.Clear();
            EdgeList.Clear();
        }

        public void ClearSolution()
        {
            if (EdgeSolution == null)
                EdgeSolution = new List<Edge>();
            else
                EdgeSolution.Clear();
        }

        public void AddVertex(int x, int y)
        {
            AddVertex(new Vertex(x, y));
        }

        public void AddVertex(Vertex v)
        {
            VertexList.Add(v);
        }

        public void AddEdge(Vertex a, Vertex b)
        {
            Edge edga = new Edge(a,b);
            //Edge edgb = new Edge(b,a);

            //if (!EdgeList.Contains(edga) && !EdgeList.Contains(edgb))
            //{
                EdgeList.Add(edga);
            //}
        }

        #region Euclidean MST
        //Algo Prim
        // Struct Entry buat prim 
        struct Entry
        {
            public bool known;
            public double distance;
            public int predecessor;

            public Entry(bool known, double distance, int predecessor)
            {
                this.known = known;
                this.distance = distance;
                this.predecessor = predecessor;
            }
        }

        public void GenerateEuclideanPrimMst()
        {
            int s = 0;
            DateTime Start = DateTime.Now;
            int n = VertexList.Count;
            EdgeSolution.Clear();
            Entry[] table = new Entry[n];
            for (int v = 0; v < n; v++)
            {
                table[v] = new Entry(false, int.MaxValue, int.MaxValue);
            }
            table[s].distance = 0;
            PriorityQueue<int, double> heap2 = new PriorityQueue<int, double>();
            heap2.Enqueue(s, 0);

            //List<pqueue> heap1 = new List<pqueue>();
            //pqueue temp = new pqueue(s, 0);
            //heap1.Add(temp);

            //while (heap1.Count != 0)
            while (heap2.Count != 0)
            {
                //int v0 = heap1[0].vtx;
                //heap1.RemoveAt(0);
                PriorityQueueItem<int, double> pqitem = new PriorityQueueItem<int, double>();
                pqitem = heap2.Dequeue();
                int v0 = pqitem.Value;
                if (!table[v0].known)
                {
                    table[v0].known = true;
                    for (int i = 0; i < VertexList.Count; i++)
                    {
                        if (i != v0)
                        {
                            int v1 = i;
                            Edge e = new Edge(VertexList[v0], VertexList[v1]);
                            double d = e.Length;
                            if (!table[v1].known && table[v1].distance > d)
                            {
                                table[v1].distance = d;
                                table[v1].predecessor = v0;
                                heap2.Enqueue(v1, d);
                                //heap2.cekPQ();
                                /*pqueue temp1 = new pqueue(v1, d);
                                int j;
                                //Sortingnya O(n) males bikin AVL TREE =P
                                for (j = 0; j < heap1.Count && heap1[j].dist < temp1.dist; j++){}
                                if (j == heap1.Count) heap1.Add(temp1);
                                else heap1.Insert(j, temp1);*/
                                //heap1.Sort(new pqueueCompare());
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < VertexList.Count; i++)
            {
                if (table[i].predecessor != int.MaxValue)
                {
                    int next = table[i].predecessor;
                    Edge e = new Edge(VertexList[i], VertexList[next]);
                    EdgeSolution.Add(e);
                }
            }
            Console.WriteLine((DateTime.Now - Start).TotalMilliseconds + " ms");
            //this.VisitedVertex = VertexList.Count;
        }

        public void GenerateEuclideanKruskalMST()
        {
            if (VertexList.Count == 0)
                return;

            DateTime start = DateTime.Now;
            DisjointSet<Vertex> disjointSet = new DisjointSet<Vertex>(VertexList);
            EdgeSolution.Clear();
            
            C5.IPriorityQueue<Edge> heap = new C5.IntervalHeap<Edge>(new EdgeComparer());

            //Triangulate();
            //DelaunayTriangulation2d triangulator = new DelaunayTriangulation2d();
            //IList<Triangle> triangles = triangulator.Triangulate(new List<Vertex>(VertexList));

            //foreach (Triangle triangle in triangles)
            //{
            //    Edge e = new Edge(triangle.Vertex1, triangle.Vertex2);
            //    heap.Add(e);
            //    EdgeList.Add(e);
            //    e = new Edge(triangle.Vertex1, triangle.Vertex3);
            //    heap.Add(e);
            //    EdgeList.Add(e);
            //    e = new Edge(triangle.Vertex3, triangle.Vertex2);
            //    heap.Add(e);
            //    EdgeList.Add(e);

            //}
            //this.VisitedVertex = VertexList.Count;
            for (int i = 0; i < VertexList.Count - 1; i++)
                for (int j = i + 1; j < VertexList.Count; j++)
                {
                    Edge e = new Edge(VertexList[i], VertexList[j]);
                    heap.Add(e);
                }

            while (heap.Count > 0 && disjointSet.Count > 0)
            {
                Edge s = heap.DeleteMin();
                if (!disjointSet.IsSameSet(s.VertexFirst, s.VertexSecond))
                {

                    disjointSet.Union(s.VertexFirst, s.VertexSecond);
                    EdgeSolution.Add(s);
                }
            }
            
            Debug.WriteLine((DateTime.Now - start).TotalMilliseconds + " ms");
            //this.VisitedVertex = VertexList.Count;
        }

        public void GenerateEuclideanOurKruskalMST(int bucket)
        {
            if (VertexList.Count == 0)
                return;

            DateTime start = DateTime.Now;
            DisjointSet<Vertex> disjointSet = new DisjointSet<Vertex>(VertexList);
            EdgeSolution.Clear();
            C5.IPriorityQueue<Edge>[] heap2 = new C5.IntervalHeap<Edge>[bucket];
            C5.IPriorityQueue<Edge> heap = new C5.IntervalHeap<Edge>(new EdgeComparer());
            for(int i=0;i<heap2.Length;i++)
            {
                heap2[i] = new C5.IntervalHeap<Edge>(new EdgeComparer());
            }

            
            //this.VisitedVertex = VertexList.Count;
            for (int i = 0; i < VertexList.Count - 1; i++)
                for (int j = i + 1; j < VertexList.Count; j++)
                {
                    Edge e = new Edge(VertexList[i], VertexList[j]);
                    heap.Add(e);
                }

            double max = heap.FindMax().Length;
            double min = heap.FindMin().Length;

            while (heap.Count > 0)
            {
                Edge s = heap.DeleteMin();
                heap2[(int) Math.Floor((((s.Length - min) / (max - min)) * (bucket-1)))].Add(s);
            }

            for (int i = 0; i < bucket && disjointSet.Count > 0; i++)
            {
                while (heap2[i].Count > 0 && disjointSet.Count > 0)
                {
                    Edge s = heap2[i].DeleteMin();
                    if (!disjointSet.IsSameSet(s.VertexFirst, s.VertexSecond))
                    {

                        disjointSet.Union(s.VertexFirst, s.VertexSecond);
                        EdgeSolution.Add(s);
                    }
                }
            }

            Debug.WriteLine((DateTime.Now - start).TotalMilliseconds + " ms");
            //this.VisitedVertex = VertexList.Count;
        }

        public void GenerateEuclideanOurNeoKruskalMST(int bucket)
        {
            if (VertexList.Count == 0)
                return;

            DateTime start = DateTime.Now;
            NeoDisjointSet<Vertex> disjointSet = new NeoDisjointSet<Vertex>(VertexList);
            EdgeSolution.Clear();
            C5.IPriorityQueue<Edge>[] heap2 = new C5.IntervalHeap<Edge>[bucket];
            C5.IPriorityQueue<Edge> heap = new C5.IntervalHeap<Edge>(new EdgeComparer());
            for (int i = 0; i < heap2.Length; i++)
            {
                heap2[i] = new C5.IntervalHeap<Edge>(new EdgeComparer());
            }


            //this.VisitedVertex = VertexList.Count;
            for (int i = 0; i < VertexList.Count - 1; i++)
                for (int j = i + 1; j < VertexList.Count; j++)
                {
                    Edge e = new Edge(VertexList[i], VertexList[j]);
                    heap.Add(e);
                }

            double max = heap.FindMax().Length;
            double min = heap.FindMin().Length;

            while (heap.Count > 0)
            {
                Edge s = heap.DeleteMin();
                heap2[(int)Math.Floor((((s.Length - min) / (max - min)) * (bucket - 1)))].Add(s);
            }

            for (int i = 0; i < bucket && disjointSet.Count > 0; i++)
            {
                while (heap2[i].Count > 0 && disjointSet.Count > 0)
                {
                    Edge s = heap2[i].DeleteMin();
                    if (!disjointSet.IsSameSet(s.VertexFirst, s.VertexSecond))
                    {

                        disjointSet.Union(s.VertexFirst, s.VertexSecond);
                        EdgeSolution.Add(s);
                    }
                }
            }

            Debug.WriteLine((DateTime.Now - start).TotalMilliseconds + " ms");
            //this.VisitedVertex = VertexList.Count;
        }

        //private void Triangulate()
        //{
        //    List<TriangulationPoint> points = new List<TriangulationPoint>();
        //    foreach (Vertex data in VertexList)
        //        points.Add(new TriangulationPoint(data.X, data.Y));
        //    PointSet set = new PointSet(points);

        //    IList<PolygonPoint> ppoint = new List<PolygonPoint>();
        //    foreach (Vertex data in VertexList)
        //        ppoint.Add(new PolygonPoint(data.X, data.Y));
        //    Polygon pol = new Polygon(ppoint);
        //    try
        //    {
                
        //        P2T.Triangulate(set);
                
        //        IList<DelaunayTriangle> triangles = set.Triangles;
        //        foreach (DelaunayTriangle angle in triangles)
        //        {
        //            FixedArray3<TriangulationPoint> point = angle.Points;
        //            EdgeList.Add(new Edge(new Vertex((int) point[0].X, (int) point[0].Y), new Vertex((int) point[1].X, (int) point[1].Y)));
        //            EdgeList.Add(new Edge(new Vertex((int) point[2].X, (int) point[2].Y), new Vertex((int) point[1].X, (int) point[1].Y)));
        //            EdgeList.Add(new Edge(new Vertex((int) point[0].X, (int) point[0].Y), new Vertex((int) point[2].X, (int) point[2].Y)));
        //            Debug.WriteLine(String.Format("X:{0}, Y:{0}", point[0].X, point[0].Y));
        //            //if (VertexList.Contains(aaa))
        //            //    Debug.WriteLine("ada yang sama");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.StackTrace);
        //    }
            
        //}

        //public int Distance(Vertex a, Vertex b)
        //{
        //    return (int)(Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)));
        //}
        #endregion

        #region MST With Edge

        public void GenerateKruskalMST()
        {
            ClearSolution();
            if (VertexList.Count == 0)
                return;

            DisjointSet<Vertex> disjointSet = new DisjointSet<Vertex>(VertexList);
            C5.IPriorityQueue<Edge> heap = new C5.IntervalHeap<Edge>(new EdgeComparer());

            foreach(Edge e in EdgeList)
                    heap.Add(e);

            while (heap.Count > 0 && disjointSet.Count > 0)
            {
                Edge s = heap.DeleteMin();
                if (!disjointSet.IsSameSet(s.VertexFirst, s.VertexSecond))
                {
                    disjointSet.Union(s.VertexFirst, s.VertexSecond);
                    EdgeSolution.Add(s);
                }
            }
        }

        public void GenerateOurKruskalMST(int bucket, double max, double min)
        {
            if (VertexList.Count == 0)
                return;
            if (max == min)
            {
                GenerateKruskalMST();
                return;
            }

            DisjointSet<Vertex> disjointSet = new DisjointSet<Vertex>(VertexList);
            ClearSolution();
            C5.IPriorityQueue<Edge>[] bucketHeap = new C5.IntervalHeap<Edge>[bucket];
            
            for (int i = 0; i < bucketHeap.Length; i++)
            {
                bucketHeap[i] = new C5.IntervalHeap<Edge>(new EdgeComparer());
            }

            int factor = bucket - 1;
            double diff = max - min;

            foreach(Edge e in EdgeList)
            {
                bucketHeap[(int)Math.Floor((((e.Length - min) / (diff)) * (factor)))].Add(e);
            }

            for (int i = 0; i < bucket && disjointSet.Count > 0; i++)
            {
                while (bucketHeap[i].Count > 0 && disjointSet.Count > 0)
                {
                    Edge s = bucketHeap[i].DeleteMin();
                    if (!disjointSet.IsSameSet(s.VertexFirst, s.VertexSecond))
                    {
                        disjointSet.Union(s.VertexFirst, s.VertexSecond);
                        EdgeSolution.Add(s);
                    }
                }
            }
        }

        public void GenerateOurNeoKruskalMST(int bucket, double max, double min)
        {
            if (VertexList.Count == 0)
                return;
            if (max == min)
            {
                GenerateKruskalMST();
                return;
            }

            NeoDisjointSet<Vertex> disjointSet = new NeoDisjointSet<Vertex>(VertexList);
            ClearSolution();
            C5.IPriorityQueue<Edge>[] bucketHeap = new C5.IntervalHeap<Edge>[bucket];

            for (int i = 0; i < bucketHeap.Length; i++)
            {
                bucketHeap[i] = new C5.IntervalHeap<Edge>(new EdgeComparer());
            }

            int factor = bucket - 1;
            double diff = max - min;

            foreach (Edge e in EdgeList)
            {
                bucketHeap[(int)Math.Floor((((e.Length - min) / (diff)) * (factor)))].Add(e);
            }

            for (int i = 0; i < bucket && disjointSet.Count > 0; i++)
            {
                while (bucketHeap[i].Count > 0 && disjointSet.Count > 0)
                {
                    Edge s = bucketHeap[i].DeleteMin();
                    if (!disjointSet.IsSameSet(s.VertexFirst, s.VertexSecond))
                    {
                        disjointSet.Union(s.VertexFirst, s.VertexSecond);
                        EdgeSolution.Add(s);
                    }
                }
            }
        }

        public void GeneratePrimMst(double[,] matrix)
        {
            int s = 0;
            
            int n = VertexList.Count;
            ClearSolution();
            Entry[] table = new Entry[n];
            for (int v = 0; v < n; v++)
            {
                table[v] = new Entry(false, int.MaxValue, int.MaxValue);
            }
            table[s].distance = 0;
            PriorityQueue<int, double> heap2 = new PriorityQueue<int, double>();
            heap2.Enqueue(s, 0);

            
            while (heap2.Count != 0)
            {
            
                PriorityQueueItem<int, double> pqitem = new PriorityQueueItem<int, double>();
                pqitem = heap2.Dequeue();
                int v0 = pqitem.Value;
                if (!table[v0].known)
                {
                    table[v0].known = true;
                    for (int i = 0; i < VertexList.Count; i++)
                    {
                        if (i != v0 && matrix[v0,i] > 0.0)
                        {
                            int v1 = i;
                            //Edge e = new Edge(VertexList[v0], VertexList[v1]);
                            double d = matrix[v0, i]; //e.Length;
                            if (!table[v1].known && table[v1].distance > d)
                            {
                                table[v1].distance = d;
                                table[v1].predecessor = v0;
                                heap2.Enqueue(v1, d);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < VertexList.Count; i++)
            {
                if (table[i].predecessor != int.MaxValue)
                {
                    int next = table[i].predecessor;
                    Edge e = new Edge(VertexList[i], VertexList[next]);
                    e.Length = matrix[i, next];
                    EdgeSolution.Add(e);
                }
            }
        }

        #endregion
    }
}
