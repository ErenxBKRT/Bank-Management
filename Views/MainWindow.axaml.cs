using Avalonia.Controls;
using Avalonia.Interactivity;
using BankManaging.Kaeru;
using Npgsql;

namespace Bankmanaging.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Connecting (object sender, RoutedEventArgs e)
    {
        Status.Text = "Connecting...";
        try
        {
            IDatabaseConnection kaeru = new DatabaseConnection();
            using var conn = kaeru.Connected();
            await conn.OpenAsync();
            Status.Text = "Connected successfully";
        }
        catch (NpgsqlException ex)
        {
            Status.Text = $"{ex.Message} : Error during connection";
        }
    }

}