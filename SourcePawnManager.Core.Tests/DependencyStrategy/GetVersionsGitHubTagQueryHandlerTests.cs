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
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.GitHub;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.GetVersionsGitHubTag;

namespace SourcePawnManager.Core.Tests.DependencyStrategy;

public class GetVersionsGitHubTagQueryHandlerTests
{
    private IGitHubApi       _api             = null!;
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        _api = Substitute.For<IGitHubApi>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_api);
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task GetVersionsReturnEmptyListIfApiReturnsNull()
    {
        var dependency = new GitHubTagFileDependency("owner", "repository", VersionRange.All, "test.inc");

        _api.GetVersions(dependency.Owner, dependency.Repository).Returns((Core.Apis.GitHub.GitHubTag[]?)null);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var versions = await mediator.Send(new GetVersionsGitHubTagQuery(dependency));

        Assert.IsNotNull(versions);
        Assert.AreEqual(0, versions.Count);
    }

    [Test]
    public async Task GetVersionsReturnEmptyListIfVersionParsingFails()
    {
        var dependency = new GitHubTagFileDependency("owner", "repository", VersionRange.All, "test.inc");

        var gitHubTag = new Core.Apis.GitHub.GitHubTag("test");
        _api.GetVersions(dependency.Owner, dependency.Repository)
            .Returns(new[] { gitHubTag });

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var versions = await mediator.Send(new GetVersionsGitHubTagQuery(dependency));

        Assert.IsNotNull(versions);
        Assert.AreEqual(0, versions.Count);
    }

    [Test]
    public async Task GetVersionsReturnMatchingDependencyVersion()
    {
        var dependency = new GitHubTagFileDependency("owner", "repository", VersionRange.All, "test.inc");

        var gitHubTag    = new Core.Apis.GitHub.GitHubTag("1.0");
        var nuGetVersion = new NuGetVersion(1, 0, 0);
        _api.GetVersions(dependency.Owner, dependency.Repository)
            .Returns(new[] { gitHubTag });

        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var versions = await mediator.Send(new GetVersionsGitHubTagQuery(dependency));

        Assert.IsNotNull(versions);
        Assert.AreEqual(1,              versions.Count,      "not expected Count");
        Assert.AreEqual(gitHubTag.Name, versions[0].Tag,     "not expected Tag");
        Assert.AreEqual(nuGetVersion,   versions[0].Version, "not expected Version");
    }
}