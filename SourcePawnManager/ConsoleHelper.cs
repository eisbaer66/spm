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

using System.CommandLine;
using Spectre.Console;

namespace SourcePawnManager;

public class ConsoleHelper
{
    private readonly IConsole?   _console;
    private readonly TextWriter? _textWriter;

    public ConsoleHelper(IConsole console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public ConsoleHelper(TextWriter textWriter)
    {
        _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
    }

    public void WriteLine(string text)
    {
        _textWriter?.WriteLine(text);
        _console?.WriteLine(text);
    }

    public void WriteHeader()
    {
        AnsiConsole.Write(new FigletText("  spm"));
        WriteLine("          SourcePawnManager");
        WriteLine("(c) 2022 icebear <icebear@icebear.rocks>");
    }
}