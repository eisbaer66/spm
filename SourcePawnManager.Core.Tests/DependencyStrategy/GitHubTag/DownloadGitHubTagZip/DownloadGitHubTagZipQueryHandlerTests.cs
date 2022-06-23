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
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;

public class DownloadGitHubTagZipQueryHandlerTests
{
    private IGitHubApi _api = null!;
    private IFileSystem _fileSystem = null!;
    private IServiceProvider _serviceProvider = null!;

    private static readonly object[] ConstructorCases =
    {
        new object[]
        {
            null!,
            Substitute.For<IFileSystem>(),
            Substitute.For<ILogger<DownloadGitHubTagZipQueryHandler>>(),
        },
        new object[]
        {
            Substitute.For<IGitHubApi>(),
            null!,
            Substitute.For<ILogger<DownloadGitHubTagZipQueryHandler>>(),
        },
        new object[]
        {
            Substitute.For<IGitHubApi>(),
            Substitute.For<IFileSystem>(),
            null!,
        },
    };

    [SetUp]
    public void Setup()
    {
        _api = Substitute.For<IGitHubApi>();
        _fileSystem = Substitute.For<IFileSystem>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_api);
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    [TestCaseSource(nameof(ConstructorCases))]
    public void ConstructorThrowsArgumentNullException(IGitHubApi api,
                                                       IFileSystem fileSystem,
                                                       ILogger<DownloadGitHubTagZipQueryHandler> logger)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new DownloadGitHubTagZipQueryHandler(api, fileSystem, logger));
    }

    [Test]
    public async Task DownloadStopsIfApiReturnsNull()
    {
        var downloadPath = "tempDownload";
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath);
        }

        var dependency =
            new GitHubTagZipDependency("owner", "repository", VersionRange.All, "test.inc", "fileInZip", downloadPath);

        var version = new DependencyVersion(new(1, 0, 0), "v1.0");
        _api.Download(dependency.Owner, dependency.Repository, dependency.AssetName, version.Tag)
            .Returns((Stream?)null);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadGitHubTagZipQuery(dependency, version));

        await _fileSystem.DidNotReceive().Write(Arg.Any<Stream>(), dependency.DownloadPath);
    }

    [Test]
    public async Task DownloadStopsIfZipIsNull()
    {
        var downloadPath = "tempDownload";
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath);
        }

        var dependency = new GitHubTagZipDependency("owner",
                                                    "repository",
                                                    VersionRange.All,
                                                    "test.inc",
                                                    "test.txt",
                                                    downloadPath);

        var version = new DependencyVersion(new(1, 0, 0), "v1.0");
        _api.Download(dependency.Owner, dependency.Repository, dependency.AssetName, version.Tag)
            .Returns((Stream?)null);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadGitHubTagZipQuery(dependency, version));

        await _fileSystem.DidNotReceive().Write(Arg.Any<Stream>(), dependency.DownloadPath);
    }

    [Test]
    public async Task DownloadStopsIfFileInZipCantBeFound()
    {
        var downloadPath = "tempDownload";
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath);
        }

        var dependency = new GitHubTagZipDependency("owner",
                                                    "repository",
                                                    VersionRange.All,
                                                    "test.inc",
                                                    "not existing.txt",
                                                    downloadPath);

        var version = new DependencyVersion(new(1, 0, 0), "v1.0");
        var zipBytes = await File.ReadAllBytesAsync("test.zip");
        Stream stream = new MemoryStream(zipBytes);
        _api.Download(dependency.Owner, dependency.Repository, dependency.AssetName, version.Tag)
            .Returns(stream);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadGitHubTagZipQuery(dependency, version));

        await _fileSystem.DidNotReceive().Write(Arg.Any<Stream>(), dependency.DownloadPath);
    }

    [Test]
    public async Task DownloadWritesFileToFileSystem()
    {
        var downloadPath = "tempDownload";
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath);
        }

        var dependency =
            new GitHubTagZipDependency("owner", "repository", VersionRange.All, "test.inc", "test.txt", downloadPath);

        var version = new DependencyVersion(new(1, 0, 0), "v1.0");
        var zipBytes = await File.ReadAllBytesAsync("test.zip");
        Stream stream = new MemoryStream(zipBytes);
        _api.Download(dependency.Owner, dependency.Repository, dependency.AssetName, version.Tag)
            .Returns(stream);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadGitHubTagZipQuery(dependency, version));

        await _fileSystem.Received().Write(Arg.Is((Stream s) => s != stream), dependency.DownloadPath); //s.Length == 13
    }
}