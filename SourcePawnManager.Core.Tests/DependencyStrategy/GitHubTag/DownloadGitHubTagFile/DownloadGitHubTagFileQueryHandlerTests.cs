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
using NSubstitute;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;

namespace SourcePawnManager.Core.Tests.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;

public class DownloadGitHubTagFileQueryHandlerTests
{
    private IGitHubApi _api = null!;
    private IFileSystem _fileSystem = null!;
    private IServiceProvider _serviceProvider = null!;

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
    public async Task DownloadStopsIfApiReturnsNull()
    {
        var downloadPath = "tempDownload";
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath);
        }

        var dependency = new GitHubTagFileDependency("owner", "repository", VersionRange.All, "test.inc", downloadPath);

        var version = new DependencyVersion(new(1, 0, 0), "v1.0");
        _api.Download(dependency.Owner, dependency.Repository, dependency.AssetName, version.Tag)
            .Returns((Stream?)null);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadGitHubTagFileQuery(dependency, version));

        await _fileSystem.DidNotReceive().Write(Arg.Any<Stream>(), dependency.DownloadPath);
    }

    [Test]
    public async Task DownloadWritesFileToFileSystem()
    {
        var dependency = new GitHubTagFileDependency("owner", "repository", VersionRange.All, "test.inc");

        var version = new DependencyVersion(new(1, 0, 0), "v1.0");
        var stream = Stream.Null;
        _api.Download(dependency.Owner, dependency.Repository, dependency.AssetName, version.Tag)
            .Returns(stream);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DownloadGitHubTagFileQuery(dependency, version));

        await _fileSystem.Received().Write(stream, dependency.DownloadPath);
    }
}