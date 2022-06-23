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
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.Results;

namespace SourcePawnManager.Commands;

public class InstallGitHubTagFileCommand : CommandBase
{
    public InstallGitHubTagFileCommand() : base("github-tag-file",
                                                "installs include-file from github identified by a git-tag")
    {
        AddAlias("ghtf");

        AddArgument(new Argument<string>("owner",        "owner of the github-repository"));
        AddArgument(new Argument<string>("repository",   "repository-name on github"));
        AddArgument(new Argument<string>("versionRange", "versionRange used when updating this include"));
        AddArgument(new Argument<string>("assetName",    "name of the include-asset in the GitHub-tag"));
        AddOption(new Option<string>(new[] { "--download-path", "-d" },
                                     "path where the include will be stored after download"));
        AddOption(new Option<string>(new[] { "--version-reg-ex", "-r" },
                                     () => GitHubTagDependencyBase.DefaultVersionRegEx,
                                     "Regular-Expression used to parse the version from the git-tag"));
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
        public string Owner        { get; set; } = string.Empty;
        public string Repository   { get; set; } = string.Empty;
        public string VersionRange { get; set; } = string.Empty;
        public string AssetName    { get; set; } = string.Empty;
        public string DownloadPath { get; set; } = string.Empty;

        public string VersionRegEx { get; set; } = string.Empty;

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

        protected override async Task<IResult> InvokeAsync(CancellationToken cancellationToken)
        {
            IDependency dependency = new GitHubTagFileDependency(Owner,
                                                                 Repository,
                                                                 NuGet.Versioning.VersionRange.Parse(VersionRange),
                                                                 AssetName,
                                                                 DownloadPath,
                                                                 VersionRegEx);
            return await _includeManager.Install(Environment.CurrentDirectory, dependency, cancellationToken);
        }
    }
}