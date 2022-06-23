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

using MediatR;
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.GitHub;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;

public class DownloadGitHubTagFileQueryHandler : IRequestHandler<DownloadGitHubTagFileQuery>
{
    private readonly IGitHubApi                                 _api;
    private readonly IFileSystem                                _fileSystem;
    private readonly ILogger<DownloadGitHubTagFileQueryHandler> _logger;

    public DownloadGitHubTagFileQueryHandler(IGitHubApi                                 api,
                                             IFileSystem                                fileSystem,
                                             ILogger<DownloadGitHubTagFileQueryHandler> logger)
    {
        _api        = api        ?? throw new ArgumentNullException(nameof(api));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<Unit> Handle(DownloadGitHubTagFileQuery request, CancellationToken cancellationToken)
    {
        await using var responseStream = await _api.Download(request.Dependency.Owner,
                                                             request.Dependency.Repository,
                                                             request.Dependency.AssetName,
                                                             request.Version.Tag,
                                                             cancellationToken);
        if (responseStream == null)
        {
            _logger.LogError("GitHubApi did return null for tag {GitHubApiTag} on {DependencyId}",
                             request.Version.Tag,
                             request.Dependency.Id);
            return Unit.Value;
        }

        await _fileSystem.Write(responseStream, request.Dependency.DownloadPath);
        return Unit.Value;
    }
}