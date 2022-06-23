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
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace SourcePawnManager;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseNestedCommandHandlerFromAssembly<T>(this IHostBuilder hostBuilder) =>
        UseNestedCommandHandlerFromAssembly(hostBuilder, typeof(T).Assembly);

    public static IHostBuilder UseNestedCommandHandlerFromAssembly(this IHostBuilder hostBuilder, Assembly assembly)
    {
        var commands = assembly.DefinedTypes
                               .Where(t => t.IsAssignableTo(typeof(Command)))
                               .Select(t => (command: t,
                                             commandHandler:
                                             t.DeclaredNestedTypes.FirstOrDefault(nt =>
                                                                                      nt.IsAssignableTo(typeof(
                                                                                                            ICommandHandler)))))
                               .Where(x => x.commandHandler != null);
        foreach (var (command, commandHandler) in commands)
        {
            hostBuilder.UseCommandHandler(command, commandHandler!);
        }

        return hostBuilder;
    }
}