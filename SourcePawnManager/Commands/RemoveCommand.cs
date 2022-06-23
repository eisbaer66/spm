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
using System.Globalization;
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core;
using SourcePawnManager.Core.DependencyStrategy;

namespace SourcePawnManager.Commands;

public class RemoveCommand : Command
{
    public RemoveCommand() : base("remove", "removes a already installed include")
    {
        AddAlias("rm");

        AddArgument(new Argument<string>("Id", "the id of the include or index form calling 'spm list'"));
    }

    // ReSharper disable once UnusedMember.Global
    public class CommandHandler : ICommandHandler
    {
        private readonly IIncludeManager _includeManager;
        private readonly ILogger         _logger;

        public CommandHandler(IIncludeManager includeManager, ILogger<IConsole> logger)
        {
            _includeManager = includeManager ?? throw new ArgumentNullException(nameof(includeManager));
            _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
        }

        // injected by System.CommandLine
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
        public IConsole Console { get; set; } = null!;

        public string Id { get; set; } = null!;

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var cancellationToken = context.GetCancellationToken();

            Func<IDependency, int, bool> predicate;
            if (int.TryParse(Id, NumberStyles.Any, CultureInfo.InvariantCulture, out var index))
            {
                predicate = (_, i) => i == index - 1;
            }
            else
            {
                predicate = (d, _) => d.Id == Id;
            }

            var result = await _includeManager.Remove(Environment.CurrentDirectory, predicate, cancellationToken);

            _logger.Log(result.LogLevel, result.Log.Message, result.Log.Args);
            return result.ExitCode;
        }
    }
}