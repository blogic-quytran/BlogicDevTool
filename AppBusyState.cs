namespace BLogicDevTool;

/// <summary>
/// Global flag set by feature controls while a long-running operation is in
/// progress. Form1 listens to <see cref="Changed"/> and disables navigation so
/// the user cannot switch to another feature tab mid-operation. Only one feature
/// control is hosted at a time, so a simple flag (not a counter) is sufficient.
/// </summary>
public static class AppBusyState
{
    private static bool _isBusy;

    /// <summary>Raised whenever the busy state flips. Argument is the new value.</summary>
    public static event Action<bool>? Changed;

    public static bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            Changed?.Invoke(value);
        }
    }
}
