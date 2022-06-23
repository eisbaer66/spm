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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.VersionStores;

namespace SourcePawnManager.Core.Tests.VersionStores;

public class SideFileVersionStoreTests
{
    private IJsonFileSystem  _fileSystem      = null!;
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        _fileSystem = Substitute.For<IJsonFileSystem>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void ResolveTest()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        Assert.IsNotNull(store);
    }

    [Test]
    public async Task GetExistingVersionDelegatesToJsonFileSystem()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var dependency   = Substitute.For<IDependency>();
        var downloadPath = Path.Combine("download", "path", "dependency.inc");
        var sideFilePath = Path.Combine("download", "path", "dependency.inc.version");
        dependency.DownloadPath.Returns(downloadPath);
        var version = DependencyVersion.Parse("1.0.0");
        _fileSystem.ReadJson<DependencyVersion>(sideFilePath, CancellationToken.None).Returns(version);

        var actual = await store.GetExistingVersion(dependency, CancellationToken.None);
        await _fileSystem.Received().ReadJson<DependencyVersion>(sideFilePath, CancellationToken.None);
        Assert.AreSame(version, actual);
    }

    [Test]
    public void CleanDoesNotDeleteIfDependenciesEmpty()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        store.Clean(Array.Empty<string>(), Array.Empty<IDependency>());

        _fileSystem.DidNotReceiveWithAnyArgs().Delete(Arg.Any<string>());
    }

    [Test]
    public void CleanDoesNotDeleteIfNoSideFileExists()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var dependency   = Substitute.For<IDependency>();
        var directory    = Path.Combine("download", "path");
        var downloadPath = Path.Combine("download", "path", "dependency.inc");
        dependency.DownloadPath.Returns(downloadPath);

        _fileSystem.GetFiles(directory, "*.inc.version").Returns(Array.Empty<string>());

        store.Clean(new[] { directory }, new[] { dependency });

        _fileSystem.Received().GetFiles(directory, "*.inc.version");
        _fileSystem.DidNotReceiveWithAnyArgs().Delete(Arg.Any<string>());
    }

    [Test]
    public void Clean()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();
        
        var directory    = Path.Combine("download",                   "path");
        var sideFilePath = Path.Combine(Environment.CurrentDirectory, "download", "path", "dependency.inc.version");

        _fileSystem.GetFiles(directory, "*.inc.version").Returns(new[] { sideFilePath });

        store.Clean(new[] { directory }, Array.Empty<IDependency>());

        _fileSystem.Received().GetFiles(directory, "*.inc.version");
        _fileSystem.Received().Delete(sideFilePath);
    }

    [Test]
    public void CleanNormalizesPaths()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var dependency   = Substitute.For<IDependency>();
        var directory    = Path.Combine("download",                   "path");
        var downloadPath = Path.Combine("download",                   "path",     "dependency.inc");
        var sideFilePath = Path.Combine(Environment.CurrentDirectory, "download", "path", "dependency.inc.version");
        dependency.DownloadPath.Returns(downloadPath);

        _fileSystem.GetFiles(directory, "*.inc.version").Returns(new[] { sideFilePath });

        store.Clean(new[] { directory }, new[] { dependency });

        _fileSystem.Received().GetFiles(directory, "*.inc.version");
        _fileSystem.DidNotReceiveWithAnyArgs().Delete(null!);
    }

    [Test]
    public void SetThrowsIfDependencyIsNull()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var version = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        Assert.ThrowsAsync<ArgumentNullException>(async () => await store.Set(null!, version, CancellationToken.None));
    }

    [Test]
    public void SetThrowsIfVersionIsNull()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var dependency   = Substitute.For<IDependency>();
        var downloadPath = Path.Combine("download", "path", "dependency.inc");
        dependency.DownloadPath.Returns(downloadPath);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await store.Set(dependency, null!, CancellationToken.None));
    }

    [Test]
    public async Task Set()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var dependency   = Substitute.For<IDependency>();
        var downloadPath = Path.Combine("download", "path", "dependency.inc");
        var sideFilePath = Path.Combine("download", "path", "dependency.inc.version");
        dependency.DownloadPath.Returns(downloadPath);

        var version = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        await store.Set(dependency, version, CancellationToken.None);

        await _fileSystem.Received().WriteJson(version, sideFilePath, CancellationToken.None);
    }

    [Test]
    public void DeleteThrowsIfDependencyIsNull()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        Assert.Throws<ArgumentNullException>(() => store.Delete(null!));
    }

    [Test]
    public void Delete()
    {
        var store = _serviceProvider.GetRequiredService<IVersionStore>();

        var dependency   = Substitute.For<IDependency>();
        var downloadPath = "download\\path\\dependency.inc";
        var sideFilePath = "download\\path\\dependency.inc.version";
        dependency.DownloadPath.Returns(downloadPath);

        store.Delete(dependency);

        _fileSystem.Received().Delete(sideFilePath);
    }
}