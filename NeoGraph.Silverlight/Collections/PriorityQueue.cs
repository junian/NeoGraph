/*==
== Copyright : BlueCurve (c)
== Licence   : Gnu/GPL v2.x
== Author    : Teddy Albina
== Email     : bluecurveteam@gmail.com
== Web site  : http://www.codeplex.com/BlueCurve
*/
using System;
//using BlueCurve.Search.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NeoGraph.Collections
{
    /// <summary>
    /// Struct définissant un PriorityQueueItem
    /// </summary>
    /// <typeparam name="TValue">Clef</typeparam>
    /// <typeparam name="TPriority">Valeur</typeparam>
    public struct PriorityQueueItem<TValue, TPriority>
    {
        private TValue _value;
        public TValue Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private TPriority _priority;
        public TPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        internal PriorityQueueItem(TValue val, TPriority pri)
        {
            this._value = val;
            this._priority = pri;
        }
    }



    /// <summary>
    /// Fournit une implémentation de priorityQueue
    /// </summary>
    /// <typeparam name="TValue">Type de clef</typeparam>
    /// <typeparam name="TPriority">Type de valeur</typeparam>
    public class PriorityQueue<TValue, TPriority> : ICollection, IEnumerable<PriorityQueueItem<TValue, TPriority>>
    {
        /// <summary>
        /// This features enable the priority queue to increment automaticaly
        /// the priority of the item
        /// </summary>
        private PriorityQueueItem<TValue, TPriority>[] items;
        /// <summary>
        /// Default capacity of the queue
        /// </summary>
        private const Int32 DefaultCapacity = 1024;
        /// <summary>
        /// Capacity of the queue
        /// </summary>
        private Int32 capacity;
        /// <summary>
        /// Numbers of items in the queue
        /// </summary>
        private Int32 numItems;
        /// <summary>
        /// Comparison delegate
        /// </summary>
        private Comparison<TPriority> compareFunc;

        /// <summary>
        /// Initializes a new instance of the PriorityQueue class that is empty,
        /// has the default initial capacity, and uses the default IComparer.
        /// </summary>
        public PriorityQueue()
            : this(DefaultCapacity, Comparer<TPriority>.Default)
        {
        }

        public PriorityQueue(Int32 initialCapacity)
            : this(initialCapacity, Comparer<TPriority>.Default)
        {
        }

        public PriorityQueue(IComparer<TPriority> comparer)
            : this(DefaultCapacity, comparer)
        {
        }

        public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
        {
            Init(initialCapacity, new Comparison<TPriority>(comparer.Compare));
        }

        public PriorityQueue(Comparison<TPriority> comparison)
            : this(DefaultCapacity, comparison)
        {
        }

        public PriorityQueue(int initialCapacity, Comparison<TPriority> comparison)
        {
            Init(initialCapacity, comparison);
        }

        private void Init(int initialCapacity, Comparison<TPriority> comparison)
        {
            numItems = 0;
            compareFunc = comparison;
            SetCapacity(initialCapacity);
        }

        public int Count
        {
            get { return numItems; }
        }

        public int Capacity
        {
            get { return items.Length; }
            set { SetCapacity(value); }
        }

        private void SetCapacity(int newCapacity)
        {
            int newCap = newCapacity;
            if (newCap < DefaultCapacity)
                newCap = DefaultCapacity;

            // throw exception if newCapacity < NumItems
            if (newCap < numItems)
                throw new ArgumentOutOfRangeException("newCapacity", "New capacity is less than Count");

            this.capacity = newCap;
            if (items == null)
            {
                items = new PriorityQueueItem<TValue, TPriority>[newCap];
                return;
            }

            // Resize the array.
            Array.Resize<PriorityQueueItem<TValue, TPriority>>(ref items, newCap);
        }

        public void Enqueue(PriorityQueueItem<TValue, TPriority> newItem)
        {
            if (numItems == capacity)
            {
                // need to increase capacity
                // grow by 50 percent
                SetCapacity((3 * Capacity) / 2);
            }

            int i = numItems;
            ++numItems;
            while ((i > 0) && (compareFunc(items[(i - 1) / 2].Priority, newItem.Priority) > 0))
            {
                items[i] = items[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            items[i] = newItem;
        }

        /// <summary>
        /// Permet d'ajouter des elements dans la pile
        /// </summary>
        /// <param name="value">Clef</param>
        /// <param name="priority">Priorité</param>
        public void Enqueue(TValue value, TPriority priority)
        {
            Enqueue(new PriorityQueueItem<TValue, TPriority>(value, priority));
        }


        /// <summary>
        /// Permet de supprimer un intervale de la file d'attente
        /// </summary>
        /// <param name="index">début de l'intervalle</param>
        /// <returns>PriorityQueueItem</returns>
        private PriorityQueueItem<TValue, TPriority> RemoveAt(Int32 index)
        {
            PriorityQueueItem<TValue, TPriority> o = items[index];
            --numItems;
            // move the last item to fill the hole
            PriorityQueueItem<TValue, TPriority> tmp = items[numItems];
            // If you forget to clear this, you have a potential memory leak.
            items[numItems] = default(PriorityQueueItem<TValue, TPriority>);
            if (numItems > 0 && index != numItems)
            {
                // If the new item is greater than its parent, bubble up.
                int i = index;
                int parent = (i - 1) / 2;
                while (compareFunc(tmp.Priority, items[parent].Priority) < 0)
                {
                    items[i] = items[parent];
                    i = parent;
                    parent = (i - 1) / 2;
                }

                // if i == index, then we didn't move the item up
                if (i == index)
                {
                    // bubble down ...
                    while (i < (numItems) / 2)
                    {
                        int j = (2 * i) + 1;
                        if ((j < numItems - 1) && (compareFunc(items[j].Priority, items[j + 1].Priority) > 0))
                            ++j;
                        if (compareFunc(items[j].Priority, tmp.Priority) >= 0)
                            break;

                        items[i] = items[j];
                        i = j;
                    }
                }
                // Be sure to store the item in its place.
                items[i] = tmp;
            }

            return o;
        }


        /// <summary>
        /// Vérifie que les données de la pile sont cohérentes
        /// </summary>
        /// <returns>bool</returns>
        public bool VerifyQueue()
        {
            int i = 0;
            while (i < numItems / 2)
            {
                int leftChild = (2 * i) + 1;
                int rightChild = leftChild + 1;
                if (compareFunc(items[i].Priority, items[leftChild].Priority) > 0)
                    return false;
                if (rightChild < numItems && compareFunc(items[i].Priority, items[rightChild].Priority) > 0)
                    return false;
                ++i;
            }
            return true;
        }

        /// <summary>
        /// Permet d'obtenir un élement
        /// </summary>
        /// <returns>PriorityQueueItem</returns>
        public PriorityQueueItem<TValue, TPriority> Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("The queue is empty");
            return RemoveAt(0);
        }

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The passed equality comparison is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="comparer">An object that implements the IEqualityComparer interface
        /// for the type of item in the collection.</param>
        public void Remove(TValue item, IEqualityComparer comparer)
        {
            // need to find the PriorityQueueItem that has the Data value of o
            for (int index = 0; index < numItems; ++index)
            {
                if (comparer.Equals(item, items[index].Value))
                {
                    RemoveAt(index);
                    return;
                }
            }
            //throw new ApplicationException("The specified itemm is not in the queue.");
        }

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The default type comparison function is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        public void Remove(TValue item)
        {
            Remove(item, EqualityComparer<TValue>.Default);
        }

        /// <summary>
        /// Permet d'obtenir le premier élement de la pile
        /// </summary>
        /// <returns>PriorityQueueItem</returns>
        public PriorityQueueItem<TValue, TPriority> Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("The queue is empty");
            return items[0];
        }

        /// <summary>
        /// Permet de vider la pile
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < numItems; ++i)
            {
                items[i] = default(PriorityQueueItem<TValue, TPriority>);
            }
            numItems = 0;
            TrimExcess();
        }

        /// <summary>
        /// Set the capacity to the actual number of items, if the current
        /// number of items is less than 90 percent of the current capacity.
        /// </summary>
        public void TrimExcess()
        {
            if (numItems < (float)0.9 * capacity)
            {
                SetCapacity(numItems);
            }
        }

        /// <summary>
        /// Permet de savoir si un élement existe dans la pile
        /// </summary>
        /// <param name="o">element a tester</param>
        /// <returns>bool</returns>
        public bool Contains(TValue o)
        {
            foreach (PriorityQueueItem<TValue, TPriority> x in items)
            {
                if (x.Value.Equals(o))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Permet d'obtenir la priorité d'un element
        /// </summary>
        /// <param name="key">element dont on veut la priorité</param>
        /// <returns>TPriority</returns>
        public TPriority GetPriority(TValue key)
        {
            foreach (PriorityQueueItem<TValue, TPriority> x in items)
            {
                if (x.Value.Equals(key))
                    return x.Priority;
            }
            return default(TPriority);
        }

        public void cekPQ()
        {
            int tes;
            tes = 0;
            Console.WriteLine("Jumlah : " + numItems);
            foreach (PriorityQueueItem<TValue, TPriority> x in items)
            {
                if (tes < numItems)
                {
                    Console.WriteLine(x.Value + " " + x.Priority);
                    tes++;
                }
                else break;
            }
        }
        /// <summary>
        /// Permet de changer la priorité d'un élément
        /// </summary>
        /// <param name="key">élement dont on veut changer la priorité</param>
        /// <param name="priority">nouvelle prioritée</param>
        /// <returns>bool</returns>
        public bool SetPriority(TValue key, TPriority priority)
        {
            for (int i = 0; i < items.Length; i++)
            {
                PriorityQueueItem<TValue, TPriority> x = items[i];
                if (x.Value.Equals(key))
                {
                    Console.WriteLine(x.Value);
                    items[i].Priority = priority;
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Permet de copier les élements de la pile dans un PriorityQueueItem
        /// </summary>
        /// <param name="array">PriorityQueueItem</param>
        /// <param name="arrayIndex">index de l'array a copier</param>
        public void CopyTo(PriorityQueueItem<TValue, TPriority>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional.");
            if (numItems == 0)
                return;
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex is equal to or greater than the length of the array.");
            if (numItems > (array.Length - arrayIndex))
                throw new ArgumentException("The number of elements in the source ICollection is greater than the available space from arrayIndex to the end of the destination array.");

            for (int i = 0; i < numItems; i++)
            {
                array[arrayIndex + i] = items[i];
            }
        }

        #region ICollection Members

        /// <summary>
        /// Permet de copier a partir d'un Array
        /// </summary>
        /// <param name="array">pile source</param>
        /// <param name="index">index source</param>
        public void CopyTo(Array array, int index)
        {
            this.CopyTo((PriorityQueueItem<TValue, TPriority>[])array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return items.SyncRoot; }
        }

        #endregion

        #region IEnumerable<PriorityQueueItem<TValue,TPriority>> Members

        /// <summary>
        /// Enumérateur de la pile
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator<PriorityQueueItem<TValue, TPriority>> GetEnumerator()
        {
            for (int i = 0; i < numItems; i++)
            {
                yield return items[i];
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion




        #region 'Save'

        /// <summary>
        /// Permet de sauvegarder la pile
        /// </summary>
        /// <returns>bool</returns>
        public bool Save()
        {
            try
            {
                using (Stream stream = File.Open("QueuePriority.bin", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //BinaryFormatter formatter = new BinaryFormatter();
                    //formatter.Serialize(stream, items);
                    stream.Close();
                    stream.Dispose();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Permet de restaures la pile de priorité
        /// </summary>
        /// <returns></returns>
        public bool Reload()
        {
            PriorityQueueItem<TValue, TPriority>[] temp;
            try
            {
                using (Stream stream = File.Open("QueuePriority.bin", FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    //BinaryFormatter formatter = new BinaryFormatter();
                    //temp = (PriorityQueueItem<TValue, TPriority>[])formatter.Deserialize(stream);
                    stream.Close();
                    stream.Dispose();
                }

                //if (temp.Length > 0)
                    //items = (PriorityQueueItem<TValue, TPriority>[])temp.Clone();

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

    }
}