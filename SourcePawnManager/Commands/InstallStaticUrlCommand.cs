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
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.StaticUrl;
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Commands;

public class InstallStaticUrlCommand : CommandBase
{
    public InstallStaticUrlCommand() : base("static-url",
                                            "installs include-file from a static url")
    {
        AddAlias("su");
        AddAlias("url");

        AddArgument(new Argument<string>("url", "the url of the include-file"));
        AddOption(new Option<string>(new[] { "--download-path", "-d" },
                                     "path where the include will be stored after download"));
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

        // injected by System.CommandLine
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
        public string  Url          { get; set; } = string.Empty;
        public string? DownloadPath { get; set; } = null;

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

        protected override async Task<IResult> InvokeAsync(CancellationToken cancellationToken)
        {
            IDependency dependency = new StaticUrlDependency(Url, DownloadPath);
            return await _includeManager.Install(Environment.CurrentDirectory, dependency, cancellationToken);
        }
    }
}