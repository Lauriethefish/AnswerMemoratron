using System.Diagnostics.CodeAnalysis;

namespace AnswerMematron;

public static class StringExtensions
{
    public static bool TryTrim(this string s, string beginningToTrim, [MaybeNullWhen(false)] out string trimmed)
    {
        if (s.StartsWith(beginningToTrim))
        {
            trimmed = s[beginningToTrim.Length..];
            return true;
        }

        trimmed = null;
        return false;
    }

    public static bool EqualsVague(this string s, string other)
    {
        return Normalise(s).Equals(Normalise(other), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalise(string s)
    {
        return new string(s.Trim().Where(c => c != ',' && c != '-' && c != '?').ToArray());
    }
}