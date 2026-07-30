using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Bankmanaging.Views;

public partial class AccueilWindow : Window
{
    public AccueilWindow()
    {
        InitializeComponent();
    }

    private void OnMenuButtonClick(object? sender, RoutedEventArgs e)
    {
        if (this.FindControl<StackPanel>("Outlet") is { } outlet)
        {
            outlet.Children.Clear();
            outlet.Children.Add(new ActionView());
        }
    }
}
