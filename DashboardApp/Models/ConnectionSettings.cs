namespace DashboardApp.Models;

public class ConnectionSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "dashboard_db";
    public string Username { get; set; } = "postgres";
    public string EncryptedPassword { get; set; } = string.Empty;

    // Для удобства — собираем строку подключения (пароль подставляется позже)
    public string GetConnectionString(string plainPassword)
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={plainPassword};SSL Mode=Prefer;";
    }
}