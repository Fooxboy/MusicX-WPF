using FluentAvalonia.UI.Controls;
using OneOf;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public abstract class MenuTabViewModel : ViewModelBase
{
    public abstract string Title { get; }
    public abstract OneOf<string, Symbol> Icon { get; }
}