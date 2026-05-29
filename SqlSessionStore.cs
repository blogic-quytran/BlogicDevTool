namespace BLogicDevTool;

/// <summary>
/// Process-wide holder for the active SQL Server connection profile.
/// Forms with DB-name selectors read this and subscribe to <see cref="ProfileChanged"/>
/// so a login performed in any form propagates to the others.
/// </summary>
public static class SqlSessionStore
{
    public static SqlConnectionProfile? Current { get; private set; }
    public static event Action? ProfileChanged;

    public static void SetProfile(SqlConnectionProfile? profile)
    {
        Current = profile;
        ProfileChanged?.Invoke();
    }
}
