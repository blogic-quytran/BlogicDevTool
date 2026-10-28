using System.Runtime.InteropServices;

namespace BLogicDevTool;

/// <summary>Explorer's name ordering: digit runs compare as numbers, so PS-597 sorts
/// before PS-1429 instead of after it.</summary>
public sealed class NaturalComparer : IComparer<string>
{
    public static readonly NaturalComparer Instance = new();

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int StrCmpLogicalW(string x, string y);

    /// <summary>Set once shlwapi turns out to be unavailable, so we stop retrying per compare.</summary>
    private static bool _nativeBroken;

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        if (!_nativeBroken)
        {
            try { return StrCmpLogicalW(x, y); }
            catch (DllNotFoundException) { _nativeBroken = true; }
            catch (EntryPointNotFoundException) { _nativeBroken = true; }
        }
        return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }
}
