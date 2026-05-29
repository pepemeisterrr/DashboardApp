using DashboardApp.Models;
using DashboardApp.Helpers;
using System;
using System.IO;
using System.Text.Json;

namespace DashboardApp.Services;

public class ConfigService
{
    private readonly string _configPath;

    public ConfigService()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "DashboardApp");
        Directory.CreateDirectory(folder);
        _configPath = Path.Combine(folder, "config.json");
    }

    public ConnectionSettings LoadSettings()
    {
        if (!File.Exists(_configPath))
            return new ConnectionSettings();

        try
        {
            string json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<ConnectionSettings>(json) ?? new ConnectionSettings();
        }
        catch
        {
            return new ConnectionSettings();
        }
    }

    public void SaveSettings(ConnectionSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }

    public string GetDecryptedPassword(ConnectionSettings settings)
    {
        return CryptoHelper.Unprotect(settings.EncryptedPassword);
    }

    public void SetEncryptedPassword(ConnectionSettings settings, string plainPassword)
    {
        settings.EncryptedPassword = CryptoHelper.Protect(plainPassword);
    }

    public string GetConnectionString()
    {
        var settings = LoadSettings();
        string password = GetDecryptedPassword(settings);

        return new ConnectionSettings
        {
            Host = settings.Host,
            Port = settings.Port,
            Database = settings.Database,
            Username = settings.Username
        }.GetConnectionString(password);
    }
}