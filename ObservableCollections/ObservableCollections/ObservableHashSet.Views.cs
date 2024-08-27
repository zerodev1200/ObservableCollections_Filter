﻿using ObservableCollections.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using System.Linq;

namespace ObservableCollections
{
    public sealed partial class ObservableHashSet<T> : IReadOnlyCollection<T>, IObservableCollection<T>
    {
        public ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform, bool _ = false)
        {
            return new View<TView>(this, transform);
        }

        sealed class View<TView> : ISynchronizedView<T, TView>
        {
            public ISynchronizedViewFilter<T, TView> CurrentFilter
            {
                get { lock (SyncRoot) return filter; }
            }

            readonly ObservableHashSet<T> source;
            readonly Func<T, TView> selector;
            readonly Dictionary<T, (T, TView)> dict;

            ISynchronizedViewFilter<T, TView> filter;

            public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;
            public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

            public object SyncRoot { get; }

            public View(ObservableHashSet<T> source, Func<T, TView> selector)
            {
                this.source = source;
                this.selector = selector;
                this.filter = SynchronizedViewFilter<T, TView>.Null;
                this.SyncRoot = new object();
                lock (source.SyncRoot)
                {
                    this.dict = source.set.ToDictionary(x => x, x => (x, selector(x)));
                    this.source.CollectionChanged += SourceCollectionChanged;
                }
            }

            public int Count
            {
                get
                {
                    lock (SyncRoot)
                    {
                        return dict.Count;
                    }
                }
            }

            public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
            {
                lock (SyncRoot)
                {
                    this.filter = filter;
                    foreach (var (_, (value, view)) in dict)
                    {
                        if (invokeAddEventForCurrentElements)
                        {
                            filter.InvokeOnAdd((value, view), -1);
                        }
                        else
                        {
                            filter.InvokeOnAttach(value, view);
                        }
                    }
                }
            }

            public void ResetFilter(Action<T, TView>? resetAction)
            {
                lock (SyncRoot)
                {
                    this.filter = SynchronizedViewFilter<T, TView>.Null;
                    if (resetAction != null)
                    {
                        foreach (var (_, (value, view)) in dict)
                        {
                            resetAction(value, view);
                        }
                    }
                }
            }

            public INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged()
            {
                lock (SyncRoot)
                {
                    return new NotifyCollectionChangedSynchronizedView<T, TView>(this, null);
                }
            }

            public INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged(ICollectionEventDispatcher? collectionEventDispatcher)
            {
                lock (SyncRoot)
                {
                    return new NotifyCollectionChangedSynchronizedView<T, TView>(this, collectionEventDispatcher);
                }
            }

            public IEnumerator<(T, TView)> GetEnumerator()
            {
                lock (SyncRoot)
                {
                    foreach (var item in dict)
                    {
                        if (filter.IsMatch(item.Value.Item1, item.Value.Item2))
                        {
                            yield return item.Value;
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Dispose()
            {
                this.source.CollectionChanged -= SourceCollectionChanged;
            }

            private void SourceCollectionChanged(in NotifyCollectionChangedEventArgs<T> e)
            {
                lock (SyncRoot)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.IsSingleItem)
                            {
                                var v = (e.NewItem, selector(e.NewItem));
                                dict.Add(e.NewItem, v);
                                filter.InvokeOnAdd(v, -1);
                            }
                            else
                            {
                                var i = e.NewStartingIndex;
                                foreach (var item in e.NewItems)
                                {
                                    var v = (item, selector(item));
                                    dict.Add(item, v);
                                    filter.InvokeOnAdd(v, i++);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (e.IsSingleItem)
                            {
                                if (dict.Remove(e.OldItem, out var value))
                                {
                                    filter.InvokeOnRemove(value, -1);
                                }
                            }
                            else
                            {
                                foreach (var item in e.OldItems)
                                {
                                    if (dict.Remove(item, out var value))
                                    {
                                        filter.InvokeOnRemove(value, -1);
                                    }
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            dict.Clear();
                            filter.InvokeOnReset();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                        case NotifyCollectionChangedAction.Move:
                        default:
                            break;
                    }

                    RoutingCollectionChanged?.Invoke(e);
                    CollectionStateChanged?.Invoke(e.Action);
                }
            }
        }
    }
}
