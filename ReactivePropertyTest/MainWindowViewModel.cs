using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;

namespace ReactivePropertyTest;
public class MainWindowViewModel : BindableBase, IDisposable
{
    private readonly IDisposable _disposable;
    public ReactivePropertySlim<string> AddText { get; } = new();
    public ReactiveCommand AddCommand { get; } = new();
    public ReactiveCommand DeleteCommand { get; } = new();


    public ReactivePropertySlim<int?> IdFilterText { get; } = new();
    public ReactivePropertySlim<string> NameFilterText { get; } = new();

    private ReactiveCollection<Item> sourceList { get; }
    public IFilteredReadOnlyObservableCollection<Item> FilteredView { get; }
    public MainWindowViewModel()
    {
        sourceList = [
                        new(){Id = 101,Name = "Apple"},
                        new(){Id = 102,Name = "Banana"},
                        //new(){Id = 103,Name = "Car"},
                        //new(){Id = 201,Name = "Dog"},
                        //new(){Id = 301,Name = "Eagle"},
                        new(){Id = 310,Name = "Fire"},
                     ];


        FilteredView = sourceList.ToFilteredReadOnlyObservableCollection(x => Filter(x));

        IdFilterText.Throttle(TimeSpan.FromMilliseconds(300))
                    .Skip(1)
                    .ObserveOnUIDispatcher()
                    .Subscribe(_ => FilteredView.Refresh(x => Filter(x)));

        NameFilterText.Throttle(TimeSpan.FromMilliseconds(300))
                      .Skip(1)
                      .ObserveOnUIDispatcher()
                      .Subscribe(_ => FilteredView.Refresh(x => Filter(x)));

        AddCommand.Subscribe(_ => AddToList());
        DeleteCommand.Subscribe(_ => DeleteFromSourceList());

    }

    private bool Filter(Item item)
    {
        if (!string.IsNullOrEmpty(NameFilterText.Value) && !item.Name.Contains(NameFilterText.Value, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }
        if (IdFilterText.Value is not null && !item.Id.Equals(IdFilterText.Value))
        {
            return false;
        }

        return true;
    }

    private void AddToList()
    {
        sourceList.Add(new Item() { Id = sourceList.Max(x => x.Id) + 1, Name = AddText.Value });
    }

    private void DeleteFromSourceList()
    {
        sourceList.Remove(sourceList.FirstOrDefault(x => x.Id == 101));
    }

    public void Dispose()
    {
        _disposable.Dispose();
        GC.SuppressFinalize(this);
    }
}
