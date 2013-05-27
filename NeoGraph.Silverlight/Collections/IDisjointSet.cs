using System.Collections.Generic;

namespace NeoGraph.Collections
{
    /// <summary>
    /// Disjoint Set data structure interace
    /// </summary>
    /// <typeparam name="T">Type of Disjoint Set</typeparam>
    interface IDisjointSet<T>
    {
        int Count { get; }

        void MakeSet(T data);
        T FindSet(T data);
        bool IsSameSet(T firstData, T secondData);
        bool Union(T firstData, T secondData);
    }
}
