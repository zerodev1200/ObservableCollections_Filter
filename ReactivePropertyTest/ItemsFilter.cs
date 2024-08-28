//using ObservableCollections;

//namespace ReactivePropertyTest;
//public class ItemsFilter : ISynchronizedViewFilter<Items, Items>
//{
//    public int? IdFilterText { get; set; }
//    public string NameFilterText { get; set; } = string.Empty;
//    public bool IsMatch(Items value, Items view)
//    {
//        if (!string.IsNullOrEmpty(NameFilterText) && !value.Name.Contains(NameFilterText, StringComparison.CurrentCultureIgnoreCase))
//        {
//            return false;
//        }
//        if (IdFilterText is not null && !value.Id.Equals(IdFilterText))
//        {
//            return false;
//        }

//        return true;
//    }

//    public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<Items, Items> eventArgs)
//    {
//    }

//    public void WhenFalse(Items value, Items view)
//    {
//    }

//    public void WhenTrue(Items value, Items view)
//    {
//    }
//}
