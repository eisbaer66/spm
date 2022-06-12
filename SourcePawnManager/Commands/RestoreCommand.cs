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
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core;
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Commands;

public class RestoreCommand : CommandBase
{
    public RestoreCommand() : base("restore", "restores installed includes according to spm.json and spm.lock.json")
    {
        AddAlias("r");
    }

    // ReSharper disable once UnusedMember.Global
    public class CommandHandler : CommandHandlerBase
    {
        private readonly IIncludeManager _includeManager;

        public CommandHandler(IIncludeManager includeManager, ILogger<IConsole> logger)
            : base(logger)
        {
            _includeManager = includeManager ?? throw new ArgumentNullException(nameof(includeManager));
        }

        protected override async Task<IResult> InvokeAsync(CancellationToken cancellationToken) =>
            await _includeManager.Restore(Environment.CurrentDirectory, cancellationToken);
    }
}