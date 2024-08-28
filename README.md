# ObservableCollections_Filter
filtering with Cysharp/ObservableCollections in WPF


# Filter Not Working in WPF Since v2.2.0

## Issue Description
Since `v2.2.0`, the filter has stopped working in WPF. We believe the cause is that `IList` in `ListNotifyCollectionChangedSynchronizedView<T, TView>` cannot properly retrieve elements after filtering.

## Partial Fix and New Problem
By modifying parts of the code in `v2.2.2`, filtering now functions. However, after filtering, adding items to the original source causes a `System.InvalidOperationException`.

## Reproduction
This issue can be reproduced in this repository.

## Code Changes
In `NotifyCollectionChangedSynchronizedView<T, TView>`:
```csharp
public virtual int Count => parent.Count; //added virtual keyword
```

In `ListNotifyCollectionChangedSynchronizedView<T, TView>`:
```csharp
public TView this[int index]
{
    get
    {
        lock (view.SyncRoot)
        {
            //return view.list[index].Item2;    // Old implementation
            return view.ElementAt(index).View;  // New implementation
        }
    }
    set => throw new NotSupportedException();
}

//added
public override int Count
{
    get
    {
        lock (view.SyncRoot)
        {
            //view.Count always returns the value before filtering 
            return view.Count();
        }
    }
}
```

## Observation
In versions prior to `v2.1.4`, where `IList` was not implemented, there was no access to the view during filtering or when adding elements to the original list (at least not during debugging).

However, in Views that implement `IList`, these operations cause access to occur, leading to display issues and exceptions.
