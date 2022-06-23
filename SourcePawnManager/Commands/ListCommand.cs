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
using MediatR;
using SourcePawnManager.Core.Mediator.GetInstalledDependenciesQuery;

namespace SourcePawnManager.Commands;

public class ListCommand : Command
{
    public ListCommand() : base("list", "lists all already installed includes")
    {
        AddAlias("ls");
        AddAlias("l");
    }

    // ReSharper disable once UnusedMember.Global
    public class CommandHandler : ICommandHandler
    {
        private readonly IMediator _mediator;

        public CommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        // injected by System.CommandLine
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public IConsole Console { get; set; } = null!;

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var cancellationToken = context.GetCancellationToken();

            var dependencies = await _mediator.Send(new GetInstalledDependenciesQuery(Environment.CurrentDirectory),
                                                    cancellationToken);

            new ConsoleHelper(Console).WriteHeader();
            Console.WriteLine("");
            Console.WriteLine($"{dependencies.Count} dependencies are currently installed:");

            if (dependencies.Count == 0)
                return 0;

            var maxIdLength = dependencies.Select(d => d.Id.Length)
                                          .Max();
            var idPadding    = Math.Min(maxIdLength, 100);
            var indexPadding = dependencies.Count.ToString().Length;
            var index = 1;
            foreach (var dependency in dependencies)
            {
                Console.WriteLine($"[{index.ToString().PadLeft(indexPadding)}] {dependency.Id.PadRight(idPadding)} @ {dependency.VersionRange}");
                index++;
            }

            return 0;
        }
    }
}