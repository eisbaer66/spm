#region copyright

// Copyright (C) 2022 icebear <icebear@icebear.rocks>
// 
// This file is part of SourcePawnManager (spm).
// 
// SourcePawnManager (spm) is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// 
// SourcePawnManager (spm) is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with SourcePawnManager (spm). If not, see <https://www.gnu.org/licenses/>. 

#endregion

using System.Diagnostics.CodeAnalysis;

namespace SourcePawnManager.Core;

public class Throw
{
    public static void IfNullOrEmpty([NotNull] string? value, string name)
    {
        if (value == null)
        {
            throw new ArgumentNullException(name);
        }

        if (value == string.Empty)
        {
            throw new ArgumentException(name, name + " is empty");
        }
    }

    public static void IfNullOrWhitespace(string value, string name)
    {
        if (value == null)
        {
            throw new ArgumentNullException(name);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(name, name + " is empty");
        }
    }

    public static void IfNullOrEmpty<T>(T[] array, string name)
    {
        if (array == null)
        {
            throw new ArgumentNullException(name);
        }

        if (array.Length == 0)
        {
            throw new ArgumentException(name, name + " is empty");
        }
    }
}