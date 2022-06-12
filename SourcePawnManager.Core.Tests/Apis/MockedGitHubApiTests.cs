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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.Git;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.JsonSerialization;
using SourcePawnManager.Core.Tests.Mocks;

namespace SourcePawnManager.Core.Tests.Apis;

public class MockedGitHubApiTests
{
    private const string ValidGetReleaseDetailsResponse = @"{
  ""assets"": [
    {
      ""id"": 1234567,
      ""name"": ""assetName""
    }
  ]
}
";

    private IGitCredentials           _gitCredentials           = null!;
    private IJsonSerializationService _jsonSerializationService = null!;
    private IHttpClientFactory        _httpClientFactory        = null!;

    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientFactory        = Substitute.For<IHttpClientFactory>();
        _jsonSerializationService = Substitute.For<IJsonSerializationService>();
        _jsonSerializationService.DeserializeAsync<GitHubReleaseDetails>(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                                 .Returns(info => JsonSerializer.DeserializeAsync<GitHubReleaseDetails>(info.Arg<Stream>(),
                                                                                                        _serviceProvider.GetRequiredService<JsonSerializerOptions>(),
                                                                                                        info.Arg<CancellationToken>()));
        _jsonSerializationService.DeserializeAsync<GitHubTag[]>(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                                 .Returns(info => JsonSerializer.DeserializeAsync<GitHubTag[]>(info.Arg<Stream>(),
                                                                                               _serviceProvider.GetRequiredService<JsonSerializerOptions>(),
                                                                                               info.Arg<CancellationToken>()));
        _gitCredentials           = Substitute.For<IGitCredentials>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_httpClientFactory);
        serviceCollection.AddSingleton(_jsonSerializationService);
        serviceCollection.AddSingleton(_gitCredentials);
        serviceCollection.AddSerilog();
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task GetVersionsUpdatesGitCredentials()
    {
        var response = AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, "[]");

        const string owner      = nameof(owner);
        const string repository = nameof(repository);

        var api = _serviceProvider.GetRequiredService<IGitHubApi>();
        await api.GetVersions(owner, repository);

        _gitCredentials.Received().Update(response);
    }

    [Test]
    public async Task GetVersionsReturnsNullIfTagsNotPresent()
    {
        AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, string.Empty);

        _jsonSerializationService.DeserializeAsync<GitHubTag[]>(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                                 .Returns((GitHubTag[]?)null);

        const string owner      = nameof(owner);
        const string repository = nameof(repository);

        var api        = _serviceProvider.GetRequiredService<IGitHubApi>();
        var tags = await api.GetVersions(owner, repository);

        Assert.Null(tags);
        await _jsonSerializationService.Received().DeserializeAsync<GitHubTag[]>(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void DownloadThrowsExceptionIfApiReturns500InternalServerError()
    {
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName,
                                        HttpStatusCode.InternalServerError,
                                        ValidGetReleaseDetailsResponse);

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        var api = _serviceProvider.GetRequiredService<IGitHubApi>();
        Assert.ThrowsAsync<HttpRequestException>(() => api.Download(owner, repository, assetName, tag));

        _gitCredentials.Received().Update(apiResponse);
    }

    [Test]
    public async Task DownloadReturnsNullAndUpdatesGitCredentialsIfApiReturnsNoAssets()
    {
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, "{}");
        AddMockClient(GitHubApi.DownloadHttpClientName, HttpStatusCode.OK, "abc");

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        var api    = _serviceProvider.GetRequiredService<IGitHubApi>();
        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.Null(stream);
        _gitCredentials.Received().Update(apiResponse);
    }

    [Test]
    public async Task DownloadReturnsNullAndUpdatesGitCredentialsIfApiReturnsEmptyAssets()
    {
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, @"{ ""assets"": [] }");

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        var api    = _serviceProvider.GetRequiredService<IGitHubApi>();
        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.Null(stream);
        _gitCredentials.Received().Update(apiResponse);
    }

    [Test]
    public async Task DownloadReturnsNullAndUpdatesGitCredentialsIfApiReturnsAssetWithoutId()
    {
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName,
                                        HttpStatusCode.OK,
                                        @"{
  ""assets"": [
    {
      ""name"": ""tf2attributes.inc""
    }
  ]
}
");

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        var api    = _serviceProvider.GetRequiredService<IGitHubApi>();
        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.Null(stream);
        _gitCredentials.Received().Update(apiResponse);
    }

    [Test]
    public async Task DownloadUpdatesReturnNullAndUpdatesGitCredentialsIfReleaseNotPresent()
    {
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, ValidGetReleaseDetailsResponse);
        _jsonSerializationService.DeserializeAsync<GitHubReleaseDetails>(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                                 .Returns((GitHubReleaseDetails?)null);

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        var api    = _serviceProvider.GetRequiredService<IGitHubApi>();
        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.Null(stream);
        _gitCredentials.Received().Update(apiResponse);
        await _jsonSerializationService.ReceivedWithAnyArgs().DeserializeAsync<GitHubReleaseDetails>(null!, CancellationToken.None);
    }

    [Test]
    public async Task DownloadUpdatesReturnNullAndUpdatesGitCredentialsIfAssetIdNotPresent()
    {
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, ValidGetReleaseDetailsResponse);

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        _jsonSerializationService.DeserializeAsync<GitHubReleaseDetails>(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                                 .Returns(new GitHubReleaseDetails() { Assets = new List<GitHubAssetDetails> { new() { Id = null, Name = assetName} } });

        var api    = _serviceProvider.GetRequiredService<IGitHubApi>();
        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.Null(stream);
        _gitCredentials.Received().Update(apiResponse);
        await _jsonSerializationService.ReceivedWithAnyArgs().DeserializeAsync<GitHubReleaseDetails>(null!, CancellationToken.None);
    }

    [Test]
    public async Task DownloadUpdatesGitCredentials()
    {
        var expectedContent = "abc";
        var apiResponse = AddMockClient(GitHubApi.ApiHttpClientName, HttpStatusCode.OK, ValidGetReleaseDetailsResponse);
        AddMockClient(GitHubApi.DownloadHttpClientName, HttpStatusCode.OK, expectedContent);

        const string owner      = nameof(owner);
        const string repository = nameof(repository);
        const string assetName  = nameof(assetName);
        const string tag        = nameof(tag);

        var api    = _serviceProvider.GetRequiredService<IGitHubApi>();
        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.NotNull(stream);
        using (var reader = new StreamReader(stream!))
        {
            var content = await reader.ReadToEndAsync();
            Assert.AreEqual(expectedContent, content);
        }

        _gitCredentials.Received().Update(apiResponse);
    }

    private HttpResponseMessage AddMockClient(string name, HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage
                       {
                           StatusCode = statusCode,
                           Content    = new StringContent(content),
                       };
        var messageHandler = new MockedHttpMessageHandler(response);
        var client         = new HttpClient(messageHandler) { BaseAddress = new("https://api.github.com/") };
        _httpClientFactory.CreateClient(name).Returns(client);

        return response;
    }
}