using System.Runtime.Serialization;
using System.Windows;

namespace NeoGraph
{
    [DataContract(IsReference=true,Namespace="neograph.googlecode.com")]
    public class Vertex
    {
        [DataMember]
        public int X { set; get; }
        [DataMember]
        public int Y { set; get; }
        
        public Vertex(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            return (a.Location == b.Location);
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            //return base.Equals(obj);
            Vertex a = (Vertex)obj;
            return a == this;
        }

        //[IgnoreDataMember]
        public Point Location
        {
            get
            {
                return new Point(X, Y);
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", X, Y);
        }
    }
}
