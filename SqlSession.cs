namespace BLogicDevTool;

public sealed class SqlConnectionProfile
{
    public string Server { get; }
    public bool WindowsAuth { get; }
    public string Username { get; }
    public string Password { get; }

    public SqlConnectionProfile(string server, bool windowsAuth, string username = "", string password = "")
    {
        Server = server;
        WindowsAuth = windowsAuth;
        Username = username;
        Password = password;
    }

    public SqlServerHelper CreateHelper() =>
        new(Server, WindowsAuth, Username, Password);
}
