using ObservableCollections;
using System;

namespace WpfApp;
public class ItemsFilter : ISynchronizedViewFilter<Item, Item>
{
    public int? IdFilterText { get; set; }
    public string NameFilterText { get; set; } = string.Empty;
    public bool IsMatch(Item value, Item view)
    {
        if (!string.IsNullOrEmpty(NameFilterText) && !value.Name.Contains(NameFilterText, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }
        if (IdFilterText is not null && !value.Id.Equals(IdFilterText))
        {
            return false;
        }

        return true;
    }

    public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<Item, Item> eventArgs)
    {
    }

    public void WhenFalse(Item value, Item view)
    {
    }

    public void WhenTrue(Item value, Item view)
    {
    }
}
