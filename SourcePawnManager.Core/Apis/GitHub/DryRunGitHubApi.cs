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

using Microsoft.Extensions.Logging;

namespace SourcePawnManager.Core.Apis.GitHub;

public class DryRunGitHubApi : IGitHubApi
{
    private readonly IGitHubApi               _api;
    private readonly ILogger<DryRunGitHubApi> _logger;

    public DryRunGitHubApi(IGitHubApi api, ILogger<DryRunGitHubApi> logger)
    {
        _api    = api    ?? throw new ArgumentNullException(nameof(api));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GitHubTag[]?> GetVersions(string            owner,
                                                string            repository,
                                                CancellationToken cancellationToken = default) =>
        await _api.GetVersions(owner, repository, cancellationToken);

    public Task<Stream?> Download(string            owner,
                                  string            repository,
                                  string            assetName,
                                  string            tag,
                                  CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("would have downloaded {AssetName} from {Owner}:{Repository}@{Tag}",
                               assetName,
                               owner,
                               repository,
                               tag);

        return Task.FromResult<Stream?>(new MemoryStream());
    }
}