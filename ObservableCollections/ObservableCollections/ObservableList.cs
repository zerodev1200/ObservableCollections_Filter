﻿using ObservableCollections.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ObservableCollections
{
    public sealed partial class ObservableList<T> : IList<T>, IReadOnlyObservableList<T>
    {
        readonly List<T> list;
        public object SyncRoot { get; } = new();

        public ObservableList()
        {
            list = new List<T>();
        }

        public ObservableList(int capacity)
        {
            list = new List<T>(capacity);
        }

        public ObservableList(IEnumerable<T> collection)
        {
            list = collection.ToList();
        }

        public T this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return list[index];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    var oldValue = list[index];
                    list[index] = value;
                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Replace(value, oldValue, index, index));
                }
            }
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return list.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

        public void Add(T item)
        {
            lock (SyncRoot)
            {
                var index = list.Count;
                list.Add(item);
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(item, index));
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (SyncRoot)
            {
                var index = list.Count;
                using (var xs = new CloneCollection<T>(items))
                {
                    // to avoid iterate twice, require copy before insert.
                    list.AddRange(xs.AsEnumerable());
                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(xs.Span, index));
                }
            }
        }

        public void AddRange(T[] items)
        {
            lock (SyncRoot)
            {
                var index = list.Count;
                list.AddRange(items);
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(items, index));
            }
        }

        public void AddRange(ReadOnlySpan<T> items)
        {
            lock (SyncRoot)
            {
                var index = list.Count;
                foreach (var item in items)
                {
                    list.Add(item);
                }

                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(items, index));
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                list.Clear();
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Reset());
            }
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                list.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                foreach (var item in list)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ForEach(Action<T> action)
        {
            lock (SyncRoot)
            {
                foreach (var item in list)
                {
                    action(item);
                }
            }
        }

        public int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                return list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                list.Insert(index, item);
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(item, index));
            }
        }

        public void InsertRange(int index, T[] items)
        {
            lock (SyncRoot)
            {
                list.InsertRange(index, items);
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(items, index));
            }
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            lock (SyncRoot)
            {
                using (var xs = new CloneCollection<T>(items))
                {
                    list.InsertRange(index, xs.AsEnumerable());
                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(xs.Span, index));
                }
            }
        }

        public void InsertRange(int index, ReadOnlySpan<T> items)
        {
            lock (SyncRoot)
            {
                using (var xs = new CloneCollection<T>(items))
                {
                    list.InsertRange(index, xs.AsEnumerable());
                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(xs.Span, index));
                }
            }
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                var index = list.IndexOf(item);

                if (index >= 0)
                {
                    list.RemoveAt(index);
                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Remove(item, index));
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                var item = list[index];
                list.RemoveAt(index);
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Remove(item, index));
            }
        }

        public void RemoveRange(int index, int count)
        {
            lock (SyncRoot)
            {
#if NET5_0_OR_GREATER
                var range = CollectionsMarshal.AsSpan(list).Slice(index, count);
#else
                var range = list.GetRange(index, count);
#endif

                // require copy before remove
                using (var xs = new CloneCollection<T>(range))
                {
                    list.RemoveRange(index, count);
                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Remove(xs.Span, index));
                }
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            lock (SyncRoot)
            {
                var removedItem = list[oldIndex];
                list.RemoveAt(oldIndex);
                list.Insert(newIndex, removedItem);
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Move(removedItem, newIndex, oldIndex));
            }
        }
    }
}
