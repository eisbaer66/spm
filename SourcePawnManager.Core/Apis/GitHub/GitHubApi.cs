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
using SourcePawnManager.Core.Apis.Git;
using SourcePawnManager.Core.JsonSerialization;

namespace SourcePawnManager.Core.Apis.GitHub;

public class GitHubApi : IGitHubApi
{
    public const     string                    ApiHttpClientName      = "GitHubApi";
    public const     string                    DownloadHttpClientName = "GitHubDownload";
    private readonly IGitCredentials           _gitCredentials;
    private readonly IHttpClientFactory        _httpClientFactory;
    private readonly IJsonSerializationService _jsonSerializationService;
    private readonly ILogger<GitHubApi>        _logger;

    public GitHubApi(IHttpClientFactory        httpClientFactory,
                     IJsonSerializationService jsonSerializationService,
                     IGitCredentials           gitCredentials,
                     ILogger<GitHubApi>        logger)
    {
        _httpClientFactory        = httpClientFactory        ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializationService = jsonSerializationService ?? throw new ArgumentNullException(nameof(jsonSerializationService));
        _gitCredentials           = gitCredentials           ?? throw new ArgumentNullException(nameof(gitCredentials));
        _logger                   = logger                   ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task<GitHubTag[]?> GetVersions(string            owner,
                                                string            repository,
                                                CancellationToken cancellationToken = default)
    {
        var url      = $"repos/{owner}/{repository}/tags";
        var client   = _httpClientFactory.CreateClient(ApiHttpClientName);
        var response = await client.GetAsync(url, cancellationToken);
        _gitCredentials.Update(response);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStreamAsync(cancellationToken);
        var tags = await _jsonSerializationService.DeserializeAsync<GitHubTag[]>(json, cancellationToken);

        if (tags == null)
        {
            _logger.LogError("could not read {GitHubTagsResponse} from {GitHubTagsUrl} as JSON",
                             await response.Content.ReadAsStringAsync(cancellationToken),
                             url);
            return null;
        }

        return tags;
    }

    public async Task<Stream?> Download(string            owner,
                                        string            repository,
                                        string            assetName,
                                        string            tag,
                                        CancellationToken cancellationToken = default)
    {
        var url       = $"repos/{owner}/{repository}/releases/tags/{tag}";
        var apiClient = _httpClientFactory.CreateClient(ApiHttpClientName);
        var response  = await apiClient.GetAsync(url, cancellationToken);
        _gitCredentials.Update(response);

        response.EnsureSuccessStatusCode();

        var json    = await response.Content.ReadAsStreamAsync(cancellationToken);
        var release = await _jsonSerializationService.DeserializeAsync<GitHubReleaseDetails>(json, cancellationToken);

        if (release == null)
        {
            _logger.LogError("could not read {GitHubTagsResponse} from {GitHubTagsUrl} as JSON",
                             await response.Content.ReadAsStringAsync(cancellationToken),
                             url);
            return null;
        }

        var asset = release.Assets?.FirstOrDefault(a => a.Name == assetName);
        if (asset == null)
        {
            _logger.LogError("could not find {AssetName} in Release {Tag}. Release: {@ReleaseDetails}",
                             assetName,
                             tag,
                             release);
            return null;
        }

        if (asset.Id == null)
        {
            _logger.LogError("asset {AssetName} has no Id in Release {Tag}. Release: {@ReleaseDetails}",
                             assetName,
                             tag,
                             release);
            return null;
        }

        var downloadClient = _httpClientFactory.CreateClient(DownloadHttpClientName);
        return await downloadClient.GetStreamAsync($"repos/{owner}/{repository}/releases/assets/{asset.Id}",
                                                   cancellationToken);
    }
}