using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DashboardApp.Models;
using DashboardApp.Services;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace DashboardApp.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    private readonly ConfigService _configService;

    [ObservableProperty]
    private string _host = "localhost";

    [ObservableProperty]
    private int _port = 5432;

    [ObservableProperty]
    private string _database = "dashboard_db";

    [ObservableProperty]
    private string _username = "postgres";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isTesting = false;

    public bool DialogResult { get; private set; } = false;
    public Action? CloseAction { get; set; }

    public ConnectionViewModel(ConfigService configService)
    {
        _configService = configService;
        var settings = _configService.LoadSettings();

        Host = settings.Host;
        Port = settings.Port;
        Database = settings.Database;
        Username = settings.Username;
        Password = _configService.GetDecryptedPassword(settings);
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsTesting = true;
        StatusMessage = "Проверка соединения...";

        try
        {
            var tempSettings = new ConnectionSettings
            {
                Host = Host,
                Port = Port,
                Database = Database,
                Username = Username
            };

            string connStr = tempSettings.GetConnectionString(Password);

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            StatusMessage = "✓ Соединение успешно установлено!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new ConnectionSettings
        {
            Host = Host,
            Port = Port,
            Database = Database,
            Username = Username
        };

        _configService.SetEncryptedPassword(settings, Password);
        _configService.SaveSettings(settings);

        DialogResult = true;
        CloseAction?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        CloseAction?.Invoke();
    }
}