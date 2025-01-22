﻿namespace Lip;

public static class GoModule
{
    private static readonly string[] BadWindowsNames = new[]
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public static bool CheckPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (path[0] == '-')
            return false;

        if (path.Contains("//"))
            return false;

        if (path[^1] == '/')
            return false;

        string[] elements = path.Split('/');

        // First element special checks
        string first = elements[0];
        if (!first.Contains('.'))
            return false;

        // if (first[0] == '-')
        //     return false;

        foreach (char c in first)
        {
            if (!IsFirstPathOk(c))
                return false;
        }

        // Check all elements
        foreach (string elem in elements)
        {
            if (!CheckElem(elem))
                return false;
        }

        return true;
    }

    public static string EscapePath(string path)
    {
        if (!CheckPath(path))
        {
            throw new ArgumentException($"{path} is not a valid Go module path.");
        }

        return EscapeString(path);
    }

    private static bool CheckElem(string elem)
    {
        // if (string.IsNullOrEmpty(elem))
        //     return false;

        if (elem.All(c => c == '.'))
            return false;

        if (elem[0] == '.')
            return false;

        if (elem[^1] == '.')
            return false;

        foreach (char c in elem)
        {
            if (!IsModPathOk(c))
                return false;
        }

        // Windows name checks
        string shortName = elem;
        int dotIndex = elem.IndexOf('.');
        if (dotIndex >= 0)
            shortName = elem[..dotIndex];

        if (BadWindowsNames.Any(name => string.Equals(name, shortName, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Windows short-name check
        int tildeIndex = shortName.LastIndexOf('~');
        if (tildeIndex >= 0 && tildeIndex < shortName.Length - 1)
        {
            string suffix = shortName[(tildeIndex + 1)..];
            if (suffix.All(char.IsDigit))
            {
                return false;
            }
        }

        return true;
    }

    private static string EscapeString(string s)
    {
        bool haveUpper = false;
        foreach (char c in s)
        {
            if (c == '!' || c >= 0x80)
            {
                // This should be disallowed by CheckPath, but diagnose anyway.
                // The correctness of the escaping loop below depends on it.
                throw new InvalidOperationException("internal error: inconsistency in EscapePath");
            }
            if (c >= 'A' && c <= 'Z')
            {
                haveUpper = true;
            }
        }

        if (!haveUpper)
        {
            return s;
        }

        var buf = new List<char>();
        foreach (char c in s)
        {
            if (c >= 'A' && c <= 'Z')
            {
                buf.Add('!');
                buf.Add((char)(c + 'a' - 'A'));
            }
            else
            {
                buf.Add(c);
            }
        }
        return new string([.. buf]);
    }

    private static bool IsFirstPathOk(char c)
    {
        if (c == '-') return true;
        if (c == '.') return true;
        if (c >= '0' && c <= '9') return true;
        if (c >= 'a' && c <= 'z') return true;
        return false;
    }

    private static bool IsModPathOk(char c)
    {
        if (c == '-' || c == '.' || c == '_' || c == '~')
            return true;
        if (c >= '0' && c <= '9')
            return true;
        if (c >= 'A' && c <= 'Z')
            return true;
        if (c >= 'a' && c <= 'z')
            return true;
        return false;
    }
}
