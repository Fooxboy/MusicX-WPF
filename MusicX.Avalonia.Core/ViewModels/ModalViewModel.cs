using System.Reactive;
using DynamicData.Binding;
using ReactiveUI;

namespace MusicX.Avalonia.Core.ViewModels;

public abstract class ModalViewModel : ViewModelBase
{
    public abstract string Title { get; }
    public virtual bool AllowDismiss { get; set; } = true;
    public ReactiveCommand<Unit, Unit> DismissCommand { get; set; }

    protected ModalViewModel()
    {
        DismissCommand = ReactiveCommand.Create(Dismiss, this.WhenValueChanged(x => x.AllowDismiss));
    }

    protected virtual void Dismiss()
    {
    }
}