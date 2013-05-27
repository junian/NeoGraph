using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NeoGraph
{
    [DataContract(IsReference=true, Namespace="neograph.googlecode.com")]
    public class Edge
    {
        [DataMember(Name="First", Order=0)]
        public Vertex VertexFirst { get; set; }
        [DataMember(Name="Second", Order=1)]
        public Vertex VertexSecond { get; set; }
        [DataMember(Order=2)]
        public double Length { get; set; }

        public Edge(Vertex first, Vertex second)
        {
            VertexFirst = first;
            VertexSecond = second;
            Length = Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2));
        }

        public override bool Equals(object obj)
        {
            //return base.Equals(obj);
            Edge a = (Edge)obj;
            return (a.VertexFirst == this.VertexFirst && a.VertexSecond == this.VertexSecond)
                || (a.VertexSecond == this.VertexFirst && a.VertexFirst == this.VertexSecond);
        }
    }

    public class EdgeComparer : IComparer<Edge>
    {
        public int Compare(Edge x, Edge y)
        {
            if (x.Length < y.Length)
                return -1;
            else if (x.Length == y.Length)
                return 0;
            return 1;
        }
    }
}
