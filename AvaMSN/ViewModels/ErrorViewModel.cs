using ReactiveUI;
using System;
using System.Reactive;

namespace AvaMSN.ViewModels;

public class ErrorViewModel : ViewModelBase
{
    public string Error { get; set; } = string.Empty;

    public event EventHandler? CloseTapped;

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public ErrorViewModel()
    {
        CloseCommand = ReactiveCommand.Create(Close);
    }

    public void Close()
    {
        CloseTapped?.Invoke(this, EventArgs.Empty);
    }
}
