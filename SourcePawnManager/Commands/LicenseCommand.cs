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
using System.CommandLine.Invocation;

namespace SourcePawnManager.Commands;

public class LicenseCommand : Command
{
    public LicenseCommand() : base("show-license", "displays copyright/license information")
    {
        AddAlias("sl");
    }

    // ReSharper disable once UnusedMember.Global
    public class CommandHandler : ICommandHandler
    {
        // injected by System.CommandLine
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public IConsole Console { get; set; } = null!;

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

        public Task<int> InvokeAsync(InvocationContext context)
        {
            new ConsoleHelper(Console).WriteHeader();
            Console.WriteLine("");
            Console.WriteLine("SourcePawnManager (spm) is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.");
            Console.WriteLine("SourcePawnManager (spm) is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.");
            Console.WriteLine("You should have received a copy of the GNU Affero General Public License along with SourcePawnManager (spm). If not, see <https://www.gnu.org/licenses/>. ");
            Console.WriteLine("");
            Console.WriteLine("Source code is available on <https://github.com/eisbaer66/spm>");

            return Task.FromResult(0);
        }
    }
}