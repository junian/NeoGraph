using System.Collections.Generic;

namespace NeoGraph.Collections
{
    public class DisjointSet<T> : IDisjointSet<T>
    {
        private Dictionary<T, T> disjointSet;

        public int Count { get; private set; }

        public DisjointSet(IList<T> items)
        {
            Count = items.Count;
            disjointSet = new Dictionary<T, T>();
            foreach (T item in items)
                disjointSet.Add(item, item);
        }

        public DisjointSet()
        {
            Count = 0;
            disjointSet = new Dictionary<T, T>();
        }

        public T FindSet(T data)
        {
            if (!disjointSet.ContainsKey(data))
                return default(T);
            
            if (disjointSet[data].Equals(data))
                return data;
            return (disjointSet[data] = FindSet(disjointSet[data]));
        }

        public bool IsSameSet(T firstData, T secondData)
        {
            bool isSameSet = FindSet(firstData).Equals(FindSet(secondData));
            return isSameSet;
        }

        public bool Union(T firstData, T secondData)
        {
            if (IsSameSet(firstData, secondData))
                return false;
            disjointSet[FindSet(firstData)] = FindSet(secondData);
            Count--;
            return true;
        }

        public void MakeSet(T data)
        {
            if (!disjointSet.ContainsKey(data))
                disjointSet.Add(data, data);
        }
    }
}
