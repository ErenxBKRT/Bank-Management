using Avalonia.Controls;
using Bankmanaging.Models;

namespace Bankmanaging.Views;

public partial class MainWindow : Window
{
    private readonly IDatabaseConnection _kaeru;
    public MainWindow()
    {
        InitializeComponent();
        _kaeru = DatabaseConnection.Instance;
    }
}