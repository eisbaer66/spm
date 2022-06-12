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

using System.IO.Compression;
using MediatR;
using Microsoft.Extensions.Logging;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.GitHub;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;

public class DownloadGitHubTagZipQueryHandler : IRequestHandler<DownloadGitHubTagZipQuery>
{
    private readonly IGitHubApi                                _api;
    private readonly IFileSystem                               _fileSystem;
    private readonly ILogger<DownloadGitHubTagZipQueryHandler> _logger;

    public DownloadGitHubTagZipQueryHandler(IGitHubApi                                api,
                                            IFileSystem                               fileSystem,
                                            ILogger<DownloadGitHubTagZipQueryHandler> logger)
    {
        _api        = api        ?? throw new ArgumentNullException(nameof(api));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<Unit> Handle(DownloadGitHubTagZipQuery request, CancellationToken cancellationToken)
    {
        await using var zipStream = await _api.Download(request.Dependency.Owner,
                                                        request.Dependency.Repository,
                                                        request.Dependency.AssetName,
                                                        request.Version.Tag,
                                                        cancellationToken);
        if (zipStream == null)
        {
            _logger.LogError("GitHubApi did return null for tag {GitHubApiTag} on {DependencyId}",
                             request.Version.Tag,
                             request.Dependency.Id);
            return Unit.Value;
        }

        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        var       entry      = zipArchive.Entries.FirstOrDefault(e => e.FullName == request.Dependency.FileInZip);
        if (entry == null)
        {
            _logger.LogError("ZipArchive does not contain a file for tag {GitHubApiTag} on {DependencyId}",
                             request.Version.Tag,
                             request.Dependency.Id);
            return Unit.Value;
        }

        await using var es = entry.Open();
        await _fileSystem.Write(es, request.Dependency.DownloadPath);

        return Unit.Value;
    }
}