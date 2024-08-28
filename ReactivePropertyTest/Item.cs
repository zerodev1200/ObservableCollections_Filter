using System.ComponentModel;

namespace ReactivePropertyTest;
public class Item : BindableBase
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}
