using System.Collections.Generic;

namespace NeoGraph.Collections
{
    public class NeoDisjointSet<T> : IDisjointSet<T>
    {
        private Dictionary<T, T> disjointSet;
        private Dictionary<T, int> size;

        public int Count { get; private set; }

        public NeoDisjointSet(IList<T> items)
        {
            Count = items.Count;
            disjointSet = new Dictionary<T, T>();
            size = new Dictionary<T, int>();
            foreach (T item in items)
            {
                disjointSet.Add(item, item);
                size[item] = 1;
            }
        }

        public NeoDisjointSet()
        {
            Count = 0;
            disjointSet = new Dictionary<T, T>();
        }

        public T FindSet(T data)
        {
            T nxt, j;

            j = data;
            while (!disjointSet[data].Equals(data))
                data = disjointSet[data];

            while (!j.Equals(data))
            {
                nxt = disjointSet[j];
                disjointSet[j] = data;
                j = nxt;
            }

            return data;
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


            T x = FindSet(firstData);
            T y = FindSet(secondData);

            if (size[x] > size[y])
            {
                size[x] += size[y];
                disjointSet[y] = x;
            }
            else
            {
                size[y] += size[x];
                disjointSet[x] = y;
            }

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
