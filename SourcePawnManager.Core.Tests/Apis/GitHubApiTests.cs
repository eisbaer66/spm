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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.Git;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.JsonSerialization;

namespace SourcePawnManager.Core.Tests.Apis;

public class GitHubApiTests
{
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSourcePawnManager();

        var configurationRoot = new ConfigurationBuilder()
                                .AddUserSecrets<GitHubApiTests>()
                                .AddEnvironmentVariables()
                                .Build();
        var token = configurationRoot["GitHub:Token"];
        if (!string.IsNullOrEmpty(token))
        {
            serviceCollection.Configure<GitHubToken>(GitHubToken.UserSpecified, t => t.Value = token);
            serviceCollection.Configure<GitHubToken>(t => t.Value                            = token);
        }

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    // ReSharper disable ObjectCreationAsStatement
    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfHttpClientFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GitHubApi(null!,
                                                                 Substitute.For<IJsonSerializationService>(),
                                                                 Substitute.For<IGitCredentials>(),
                                                                 Substitute.For<ILogger<GitHubApi>>()));
    }

    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfJsonSerializerOptionsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GitHubApi(Substitute.For<IHttpClientFactory>(),
                                                                 null!,
                                                                 Substitute.For<IGitCredentials>(),
                                                                 Substitute.For<ILogger<GitHubApi>>()));
    }

    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfGitCredentialsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GitHubApi(Substitute.For<IHttpClientFactory>(),
                                                                 Substitute.For<IJsonSerializationService>(),
                                                                 null!,
                                                                 Substitute.For<ILogger<GitHubApi>>()));
    }

    [Test]
    public void ConstructorThrowsArgumentNullExceptionIfLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GitHubApi(Substitute.For<IHttpClientFactory>(),
                                                                 Substitute.For<IJsonSerializationService>(),
                                                                 Substitute.For<IGitCredentials>(),
                                                                 null!));
    }

    // ReSharper restore ObjectCreationAsStatement

    [Test]
    [TestCase("nosoop",
              "tf2attributes",
              "1.3.3+1.0.14",
              "1.3.3+1.3.0",
              "1.3.3+1.4.0",
              "1.3.3+1.6.0",
              "1.7.0-pre.1",
              "1.7.1",
              "1.7.1.1",
              "v1.1.1",
              "v1.2.0",
              "v1.2.1")]
    [TestCase("nosoop", "SM-TFOnTakeDamage", "1.0.2-inlined-reads", "1.1.0", "1.1.1-pre", "1.1.2-pre", "1.2.0")]
    public async Task GetVersions(string owner, string repository, params string[] expectedVersions)
    {
        await GetVersions(expectedVersions, owner, repository);
    }

    [Test]
    [TestCase("nosoop",
              "SM-TFCustAttr",
              "1.0.0",
              "1.1.1",
              "r03",
              "r04",
              "r05",
              "r06",
              "r07-test-1",
              "r07-test-2",
              "workflow-build1",
              "workflow-build4",
              "workflow-build5",
              "workflow-build6",
              "workflow-build7",
              "workflow-build8")]
    [TestCase("peace-maker",
              "DHooks2",
              "v2.2.0-detours1",
              "v2.2.0-detours10",
              "v2.2.0-detours11",
              "v2.2.0-detours12",
              "v2.2.0-detours13",
              "v2.2.0-detours14",
              "v2.2.0-detours14a",
              "v2.2.0-detours15",
              "v2.2.0-detours16",
              "v2.2.0-detours17")]
    public async Task GetVersionsWithSpecialPattern(string owner, string repository, params string[] expectedVersions)
    {
        await GetVersions(expectedVersions, owner, repository);
    }

    private async Task GetVersions(string[] expectedVersions, string owner, string repository)
    {
        var api = _serviceProvider.GetRequiredService<IGitHubApi>();

        var versions = await api.GetVersions(owner, repository);

        Assert.IsNotNull(versions);
        Assert.GreaterOrEqual(versions!.Length, expectedVersions.Length, "not enough versions found");
        var versionsTail = versions.OrderBy(x => x.Name).Take(expectedVersions.Length).ToArray();
        for (var i = 0; i < expectedVersions.Length; i++)
        {
            var actual   = versionsTail[i].Name;
            var expected = expectedVersions[i];
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    [TestCase("nosoop", "tf2attributes", "tf2attributes.inc", "1.7.1.1", 15488)]
    public async Task Download(string owner, string repository, string assetName, string tag, int expectedLength)
    {
        var api = _serviceProvider.GetRequiredService<IGitHubApi>();

        var stream = await api.Download(owner, repository, assetName, tag);

        Assert.IsNotNull(stream);
        var memoryStream = new MemoryStream();
        await stream!.CopyToAsync(memoryStream);
        Assert.AreEqual(expectedLength, memoryStream.Length);
    }
}