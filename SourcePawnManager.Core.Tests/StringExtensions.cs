using System;
using System.IO;

namespace SourcePawnManager.Core.Tests;

public static class StringExtensions
{
    public static string ToEnvironmentString(this string str)
    {
        if (Path.DirectorySeparatorChar != '\\')
        {
            str = str.Replace("\\\\", Path.DirectorySeparatorChar.ToString());
        }

        return str.Replace("\r\n", Environment.NewLine)
                  .Replace('\\', Path.DirectorySeparatorChar);
    }
}