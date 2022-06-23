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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NuGet.Versioning;
using NUnit.Framework;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DefinitionStores;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.GetVersionsGitHubTag;
using SourcePawnManager.Core.LocalStores;
using SourcePawnManager.Core.LockStores;
using SourcePawnManager.Core.Mediator.GetInstalledDependenciesQuery;
using SourcePawnManager.Core.Results;
using SourcePawnManager.Core.VersionStores;

namespace SourcePawnManager.Core.Tests;

public class IncludeManagerTests
{
    private static readonly object[] RemoveCases =
    {
        new object[] { new Func<IDependency, int, bool>((_, i) => i    == 0) },
        new object[] { new Func<IDependency, int, bool>((d, _) => d.Id == "GitHubTagFile:owner/repository:assetName") },
    };

    private IDefinitionStore _definitionStore = null!;
    private IFileSystem      _fileSystem      = null!;
    private ILocalStore      _localStore      = null!;
    private ILockStore       _lockStore       = null!;
    private IMediator        _mediator        = null!;
    private IServiceProvider _serviceProvider = null!;
    private IVersionStore    _versionStore    = null!;

    [SetUp]
    public void Setup()
    {
        _mediator        = Substitute.For<IMediator>();
        _versionStore    = Substitute.For<IVersionStore>();
        _localStore      = Substitute.For<ILocalStore>();
        _lockStore       = Substitute.For<ILockStore>();
        _definitionStore = Substitute.For<IDefinitionStore>();
        _fileSystem      = Substitute.For<IFileSystem>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSerilog();
        serviceCollection.AddSingleton(_mediator);
        serviceCollection.AddSingleton(_versionStore);
        serviceCollection.AddSingleton(_localStore);
        serviceCollection.AddSingleton(_lockStore);
        serviceCollection.AddSingleton(_definitionStore);
        serviceCollection.AddSingleton(_fileSystem);
        serviceCollection.AddSourcePawnManager();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void RestoreThrowsWhenBasePathIsNull()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await includeManager.Restore(null!));
    }

    [Test]
    public void RestoreThrowsWhenBasePathIsEmpty()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentException>(async () => await includeManager.Restore(string.Empty));
    }

    [Test]
    public async Task RestoreDoesNotCleanVersionStoreIfLocalDefinitionIsNull()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.DidNotReceiveWithAnyArgs().Clean(null!, null!);
    }

    [Test]
    public async Task RestoreDoesNotCleanVersionStoreIfPreviousDownloadPathsIsNull()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        var localDefinition = new IncludeManagerLocalDefinition
                              {
                                  PreviousDownloadPaths = null!,
                              };
        _localStore.Get(path, CancellationToken.None).Returns(localDefinition);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.DidNotReceiveWithAnyArgs().Clean(null!, null!);
    }

    [Test]
    public async Task RestoreDoesNotCleanVersionStoreIfPreviousDownloadPathsIsEmpty()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        var localDefinition = new IncludeManagerLocalDefinition
                              {
                                  PreviousDownloadPaths = Array.Empty<string>(),
                              };
        _localStore.Get(path, CancellationToken.None).Returns(localDefinition);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.DidNotReceiveWithAnyArgs().Clean(null!, null!);
    }

    [Test]
    public async Task RestoreCleansVersionStore()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        var localDefinition = new IncludeManagerLocalDefinition
                              {
                                  PreviousDownloadPaths = new[] { "firstPath" },
                              };
        _localStore.Get(path, CancellationToken.None).Returns(localDefinition);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.Received().Clean(localDefinition.PreviousDownloadPaths, dependencies);
    }

    [Test]
    public async Task RestoreDoesNotDownloadIfNoAvailableVersions()
    {
        var path            = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependency      = Substitute.For<IDependency>();
        var request         = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.Download(Arg.Any<DependencyVersion>()).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ErrorIs<NoBestMatchingVersionFoundResult>(result));

        await _mediator.DidNotReceive().Send(downloadRequest);
    }

    [Test]
    public async Task RestoreDoesNotDownloadIfAvailableVersionEqualsExistingVersion()
    {
        var path              = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var dependency        = Substitute.For<IDependency>();
        var request           = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest   = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.Download(dependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { dependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).Returns(dependencyVersion);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ErrorIs<BestMatchingVersionAlreadyInstalledResult>(result));

        await _mediator.DidNotReceive().Send(downloadRequest);
    }

    [Test]
    public async Task RestoreDoesNotDownloadIfLockedVersionIsNotAvailable()
    {
        var path                        = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var installedDependencyVersion  = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var lockedDependencyVersion     = new DependencyVersion(NuGetVersion.Parse("1.1.0"), "1.1.0");
        var availableDependencyVersion2 = new DependencyVersion(NuGetVersion.Parse("1.2.0"), "1.2.0");
        var dependencyId                = "dependencyId";
        var locks = new LockDefinition { IncludeLocks = new[] { new IncludeLockDefinition(dependencyId, lockedDependencyVersion) } };
        var dependency = Substitute.For<IDependency>();
        var request    = Substitute.For<IRequest<IList<DependencyVersion>>>();
        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { installedDependencyVersion, availableDependencyVersion2 };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ErrorIs<LockedVersionNotAvailableResult>(result));

        dependency.DidNotReceive().Download(Arg.Any<DependencyVersion>());
    }

    [Test]
    public async Task RestoreDownloadsWithoutExistingVersion()
    {
        var path              = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var dependencyId      = "dependencyId";
        var downloadPath      = "download/dependencyId.inc";
        var dependency        = Substitute.For<IDependency>();
        var request           = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest   = Substitute.For<IRequest>();

        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(dependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { dependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValidAndContains(result, dependencyId, dependencyVersion, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, dependencyVersion, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId && Equals(il.Version, dependencyVersion))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task RestoreDownloadsIfAvailableVersionHigherThenExistingVersion()
    {
        var path                       = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var existingDependencyVersion  = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var availableDependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.0.1"), "1.0.1");
        var dependencyId               = "dependencyId";
        var downloadPath               = "download/dependencyId.inc";
        var dependency                 = Substitute.For<IDependency>();
        var request                    = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest            = Substitute.For<IRequest>();

        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(availableDependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { availableDependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).Returns(existingDependencyVersion);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValidAndContains(result, dependencyId, availableDependencyVersion, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, availableDependencyVersion, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId && Equals(il.Version, availableDependencyVersion))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task RestoreDownloadsHighestAvailableVersion()
    {
        var path                        = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var existingDependencyVersion   = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var availableDependencyVersion1 = new DependencyVersion(NuGetVersion.Parse("1.0.1"), "1.0.1");
        var availableDependencyVersion2 = new DependencyVersion(NuGetVersion.Parse("1.0.2"), "1.0.2");
        var dependencyId                = "dependencyId";
        var downloadPath                = "download/dependencyId.inc";
        var dependency                  = Substitute.For<IDependency>();
        var request                     = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest             = Substitute.For<IRequest>();

        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(availableDependencyVersion2).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { availableDependencyVersion1, availableDependencyVersion2 };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).Returns(existingDependencyVersion);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValidAndContains(result, dependencyId, availableDependencyVersion2, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, availableDependencyVersion2, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId &&
                                                                                    Equals(il.Version, availableDependencyVersion2))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task RestoreDownloadsLockedVersion()
    {
        var path                        = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var installedDependencyVersion  = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var availableDependencyVersion1 = new DependencyVersion(NuGetVersion.Parse("1.1.0"), "1.1.0");
        var availableDependencyVersion2 = new DependencyVersion(NuGetVersion.Parse("1.2.0"), "1.2.0");
        var dependencyId                = "dependencyId";
        var downloadPath                = "download/dependencyId.inc";
        var locks = new LockDefinition { IncludeLocks = new[] { new IncludeLockDefinition(dependencyId, availableDependencyVersion1) } };
        var dependency      = Substitute.For<IDependency>();
        var request         = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(availableDependencyVersion1).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion>
                                                     {
                                                         installedDependencyVersion, availableDependencyVersion1,
                                                         availableDependencyVersion2,
                                                     };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValidAndContains(result, dependencyId, availableDependencyVersion1, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, availableDependencyVersion1, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId &&
                                                                                    Equals(il.Version, availableDependencyVersion1))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task RestorePreservesExistingPreviousDownloadPaths()
    {
        var path                       = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var availableDependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.1.0"), "1.1.0");
        var dependencyId               = "dependencyId";
        var downloadPath               = "download/dependencyId.inc";
        var dependency                 = Substitute.For<IDependency>();
        var request                    = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest            = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(availableDependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path, CancellationToken.None).Returns(new LockDefinition());
        var existingDirectory = "existingDirectory";
        var existingLocalDefinition = new IncludeManagerLocalDefinition
                                      {
                                          PreviousDownloadPaths = new[] { existingDirectory },
                                      };
        _localStore.Get(path, CancellationToken.None).Returns(existingLocalDefinition);

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { availableDependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Restore(path);

        Assert.That(ResultIsValidAndContains(result, dependencyId, availableDependencyVersion, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, availableDependencyVersion, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId &&
                                                                                    Equals(il.Version, availableDependencyVersion))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                existingDirectory,
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public void InstallThrowsIfBasePathIsNull()
    {
        string path       = null!;
        var    dependency = Substitute.For<IDependency>();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
                                                      await includeManager.Install(path,
                                                                                   dependency,
                                                                                   CancellationToken.None));
    }

    [Test]
    public void InstallThrowsIfBasePathIsEmpty()
    {
        var path       = string.Empty;
        var dependency = Substitute.For<IDependency>();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentException>(async () =>
                                                  await includeManager.Install(path,
                                                                               dependency,
                                                                               CancellationToken.None));
    }

    [Test]
    public void InstallThrowsIfDependencyIsNull()
    {
        var         path       = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        IDependency dependency = null!;

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
                                                      await includeManager.Install(path,
                                                                                   dependency,
                                                                                   CancellationToken.None));
    }

    [Test]
    public async Task InstallKeepsExistingLocks()
    {
        var path               = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyId       = "dependencyId";
        var dependency         = Substitute.For<IDependency>();
        var getVersionsRequest = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest    = Substitute.For<IRequest>();
        var version            = Substitute.For<DependencyVersion>(new NuGetVersion("1.0"), "1.0");
        var versions           = new List<DependencyVersion> { version };
        var locks = new LockDefinition
                    {
                        IncludeLocks = new[]
                                       {
                                           new IncludeLockDefinition("existingDependency1",
                                                                     new(new(1, 0, 0),
                                                                         "1.0")),
                                           new IncludeLockDefinition("existingDependency2",
                                                                     new(new(1, 0, 0),
                                                                         "1.0")),
                                       },
                    };

        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        var downloadPath = "download/dependency1.inc";
        dependency.DownloadPath.Returns(downloadPath);
        dependency.GetVersions().Returns(getVersionsRequest);
        dependency.Download(version).Returns(downloadRequest);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _mediator.Send(getVersionsRequest).Returns(versions);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Install(path, dependency, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, version, downloadPath));

        var expectedLock = new[]
                           {
                               new IncludeLockDefinition(dependencyId, version),
                           }
                           .Concat(locks.IncludeLocks)
                           .ToDictionary(x => x.Id, x => x.Version);
        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, version, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Count == expectedLock.Count &&
                                                          ld.IncludeLocks.All(il => expectedLock.ContainsKey(il.Id) &&
                                                                                    expectedLock[il.Id]
                                                                                        .Equals(il.Version))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task InstallOverridesExistingLocks()
    {
        var path               = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyId       = "dependencyId";
        var downloadPath       = "download/dependencyId.inc";
        var dependency         = Substitute.For<IDependency>();
        var downloadRequest    = Substitute.For<IRequest>();
        var getVersionsRequest = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var version            = Substitute.For<DependencyVersion>(new NuGetVersion("2.0"), "2.0");
        var versions           = new List<DependencyVersion> { version };
        var locks = new LockDefinition
                    {
                        IncludeLocks = new[]
                                       {
                                           new IncludeLockDefinition(dependencyId, new(new(1, 0, 0), "1.0")),
                                       },
                    };

        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.GetVersions().Returns(getVersionsRequest);
        dependency.Download(version).Returns(downloadRequest);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _mediator.Send(getVersionsRequest).Returns(versions);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Install(path, dependency, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, version, downloadPath));

        var expected = new[]
                       {
                           new IncludeLockDefinition(dependencyId, version),
                       }
            .ToDictionary(x => x.Id, x => x.Version);
        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, version, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Count == expected.Count &&
                                                          ld.IncludeLocks.All(il => expected.ContainsKey(il.Id) &&
                                                                                    expected[il.Id].Equals(il.Version))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task InstallsBestMatchingAvailableVersion()
    {
        var path               = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyId       = "dependencyId";
        var downloadPath       = "download/dependencyId.inc";
        var dependency         = Substitute.For<IDependency>();
        var downloadRequest    = Substitute.For<IRequest>();
        var getVersionsRequest = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var version            = Substitute.For<DependencyVersion>(new NuGetVersion("2.0"), "2.0");
        var versions = new List<DependencyVersion>
                       {
                           new(new("1.0"), "1.0"),
                           version,
                           new(new("3.0"), "3.0"),
                       };
        var locks = new LockDefinition
                    {
                        IncludeLocks = Array.Empty<IncludeLockDefinition>(),
                    };

        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[*,3.0)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.GetVersions().Returns(getVersionsRequest);
        dependency.Download(version).Returns(downloadRequest);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _mediator.Send(getVersionsRequest).Returns(versions);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Install(path, dependency, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, version, downloadPath));

        var expected = new[]
                       {
                           new IncludeLockDefinition(dependencyId, version),
                       }
            .ToDictionary(x => x.Id, x => x.Version);
        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, version, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Count == expected.Count &&
                                                          ld.IncludeLocks.All(il => expected.ContainsKey(il.Id) &&
                                                                                    expected[il.Id].Equals(il.Version))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task InstallStopsIfVersionAlreadyInstalled()
    {
        var path               = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyId       = "dependencyId";
        var downloadPath       = "download/dependencyId.inc";
        var dependency         = Substitute.For<IDependency>();
        var downloadRequest    = Substitute.For<IRequest>();
        var getVersionsRequest = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var version            = Substitute.For<DependencyVersion>(new NuGetVersion("2.0"), "2.0");
        var versions = new List<DependencyVersion>
                       {
                           version,
                       };
        var locks = new LockDefinition
                    {
                        IncludeLocks = Array.Empty<IncludeLockDefinition>(),
                    };

        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[*,3.0)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.GetVersions().Returns(getVersionsRequest);
        dependency.Download(version).Returns(downloadRequest);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _mediator.Send(getVersionsRequest).Returns(versions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).Returns(version);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Install(path, dependency, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.IsInstanceOf<BestMatchingVersionAlreadyInstalledResult>(result);

        await _definitionStore.DidNotReceive().Write(path, Arg.Any<IncludeManagerDefinition>(), CancellationToken.None);
        await _mediator.DidNotReceive().Send(downloadRequest);
        await _versionStore.DidNotReceive().Set(dependency, version, CancellationToken.None);
    }

    [Test]
    public async Task InstallWritesSpm()
    {
        var path               = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyId       = "dependencyId";
        var downloadPath       = "download/dependencyId.inc";
        var dependency         = Substitute.For<IDependency>();
        var downloadRequest    = Substitute.For<IRequest>();
        var getVersionsRequest = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var version            = Substitute.For<DependencyVersion>(new NuGetVersion("2.0"), "2.0");
        var versions = new List<DependencyVersion>
                       {
                           version,
                       };
        var locks = new LockDefinition
                    {
                        IncludeLocks = Array.Empty<IncludeLockDefinition>(),
                    };

        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[*,3.0)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.GetVersions().Returns(getVersionsRequest);
        dependency.Download(version).Returns(downloadRequest);
        _lockStore.Get(path, CancellationToken.None).Returns(locks);
        _mediator.Send(getVersionsRequest).Returns(versions);
        _localStore.Get(path, CancellationToken.None).Returns(new IncludeManagerLocalDefinition { PreviousDownloadPaths = new[] { "download" } });

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Install(path, dependency, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, version, downloadPath));

        await _definitionStore.Received().Write(path, Arg.Any<IncludeManagerDefinition>(), CancellationToken.None);
        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, version, CancellationToken.None);
    }

    [Test]
    public void UpdateThrowsWhenBasePathIsNull()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await includeManager.Update(null!, CancellationToken.None));
    }

    [Test]
    public void UpdateThrowsWhenBasePathIsEmpty()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentException>(async () => await includeManager.Update(string.Empty, CancellationToken.None));
    }

    [Test]
    public async Task UpdateDoesNotCleanVersionStoreIfLocalDefinitionIsNull()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.DidNotReceiveWithAnyArgs().Clean(null!, null!);
    }

    [Test]
    public async Task UpdateDoesNotCleanVersionStoreIfPreviousDownloadPathsIsNull()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        var localDefinition = new IncludeManagerLocalDefinition
                              {
                                  PreviousDownloadPaths = null!,
                              };
        _localStore.Get(path, CancellationToken.None).Returns(localDefinition);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.DidNotReceiveWithAnyArgs().Clean(null!, null!);
    }

    [Test]
    public async Task UpdateDoesNotCleanVersionStoreIfPreviousDownloadPathsIsEmpty()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        var localDefinition = new IncludeManagerLocalDefinition
                              {
                                  PreviousDownloadPaths = Array.Empty<string>(),
                              };
        _localStore.Get(path, CancellationToken.None).Returns(localDefinition);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.DidNotReceiveWithAnyArgs().Clean(null!, null!);
    }

    [Test]
    public async Task UpdateCleansVersionStore()
    {
        var path         = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencies = new List<IDependency>().AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        var localDefinition = new IncludeManagerLocalDefinition
                              {
                                  PreviousDownloadPaths = new[] { "firstPath" },
                              };
        _localStore.Get(path, CancellationToken.None).Returns(localDefinition);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValid(result));

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _mediator.DidNotReceive().Send(Arg.Any<GetVersionsGitHubTagQuery>());
        await _localStore.Received().Get(path, CancellationToken.None);
        _versionStore.Received().Clean(localDefinition.PreviousDownloadPaths, dependencies);
    }

    [Test]
    public async Task UpdateDoesNotDownloadIfNoAvailableVersions()
    {
        var path            = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependency      = Substitute.For<IDependency>();
        var request         = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.Download(Arg.Any<DependencyVersion>()).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ErrorIs<NoBestMatchingVersionFoundResult>(result));

        await _mediator.DidNotReceive().Send(downloadRequest);
    }

    [Test]
    public async Task UpdateDoesNotDownloadIfAvailableVersionEqualsExistingVersion()
    {
        var path              = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var dependency        = Substitute.For<IDependency>();
        var request           = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest   = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.Download(dependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { dependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).Returns(dependencyVersion);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ErrorIs<BestMatchingVersionAlreadyInstalledResult>(result));

        await _mediator.DidNotReceive().Send(downloadRequest);
    }

    [Test]
    public async Task UpdateDownloadsWithoutExistingVersion()
    {
        var path              = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var dependencyId      = "dependencyId";
        var downloadPath      = "download/dependencyId.inc";
        var dependency        = Substitute.For<IDependency>();
        var request           = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest   = Substitute.For<IRequest>();

        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(dependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { dependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, dependencyVersion, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, dependencyVersion, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId &&
                                                                                    Equals(il.Version, dependencyVersion))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task UpdateDownloadsIfAvailableVersionHigherThenExistingVersion()
    {
        var path                       = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var existingDependencyVersion  = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var availableDependencyVersion = new DependencyVersion(NuGetVersion.Parse("1.0.1"), "1.0.1");
        var dependencyId               = "dependencyId";
        var downloadPath               = "download/dependencyId.inc";
        var dependency                 = Substitute.For<IDependency>();
        var request                    = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest            = Substitute.For<IRequest>();

        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(availableDependencyVersion).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion> { availableDependencyVersion };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).Returns(existingDependencyVersion);

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, availableDependencyVersion, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, availableDependencyVersion, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId &&
                                                                                    Equals(il.Version, availableDependencyVersion))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public async Task UpdateDownloadsBestMatchingVersion()
    {
        var path                        = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var installedDependencyVersion  = new DependencyVersion(NuGetVersion.Parse("1.0.0"), "1.0.0");
        var availableDependencyVersion1 = new DependencyVersion(NuGetVersion.Parse("1.1.0"), "1.1.0");
        var availableDependencyVersion2 = new DependencyVersion(NuGetVersion.Parse("1.2.0"), "1.2.0");
        var dependencyId                = "dependencyId";
        var downloadPath                = "download/dependencyId.inc";
        var dependency                  = Substitute.For<IDependency>();
        var request                     = Substitute.For<IRequest<IList<DependencyVersion>>>();
        var downloadRequest             = Substitute.For<IRequest>();
        dependency.GetVersions().Returns(request);
        dependency.Id.Returns(dependencyId);
        dependency.VersionRange.Returns(VersionRange.Parse("[1.*,)"));
        dependency.DownloadPath.Returns(downloadPath);
        dependency.Download(availableDependencyVersion2).Returns(downloadRequest);
        var dependencies = new List<IDependency> { dependency };
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _localStore.Get(path, CancellationToken.None).ReturnsNull();

        IList<DependencyVersion> availableVersions = new List<DependencyVersion>
                                                     {
                                                         installedDependencyVersion, availableDependencyVersion1,
                                                         availableDependencyVersion2,
                                                     };
        _mediator.Send(request).Returns(availableVersions);
        _versionStore.GetExistingVersion(dependency, CancellationToken.None).ReturnsNull();

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Update(path, CancellationToken.None);

        Assert.That(ResultIsValidAndContains(result, dependencyId, availableDependencyVersion2, downloadPath));

        await _mediator.Received().Send(downloadRequest);
        await _versionStore.Received().Set(dependency, availableDependencyVersion2, CancellationToken.None);
        await _lockStore.Received()
                        .Set(Arg.Is<LockDefinition>(ld => ld.IncludeLocks.Any(il => il.Id == dependencyId &&
                                                                                    Equals(il.Version, availableDependencyVersion2))),
                             path,
                             CancellationToken.None);
        var expectedPreviousDownloadPaths = new[]
                                            {
                                                "download",
                                            }.ToHashSet();
        await _localStore.Received()
                         .Set(Arg.Is<IncludeManagerLocalDefinition>(ld => ld.PreviousDownloadPaths.Count == expectedPreviousDownloadPaths.Count &&
                                                                          ld.PreviousDownloadPaths.All(il => expectedPreviousDownloadPaths.Contains(il))),
                              path,
                              CancellationToken.None);
    }

    [Test]
    public void RemoveThrowsWhenBasePathIsNull()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await includeManager.Remove(null!, (_, _) => true));
    }

    [Test]
    public void RemoveThrowsWhenBasePathIsEmpty()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentException>(async () => await includeManager.Remove(string.Empty, (_, _) => true));
    }

    [Test]
    public void RemoveThrowsWhenPredicateIsNull()
    {
        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await includeManager.Remove("test", null!));
    }

    [Test]
    [TestCaseSource(nameof(RemoveCases))]
    public async Task Remove(Func<IDependency, int, bool> predicate)
    {
        var path        = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependency1 = new GitHubTagFileDependency("owner",  "repository",  VersionRange.All, "assetName");
        var dependency2 = new GitHubTagFileDependency("owner2", "repository2", VersionRange.All, "assetName2");
        var dependencies = new List<IDependency>
                           {
                               dependency1,
                               dependency2,
                           }.AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path)
                  .Returns(new LockDefinition
                           {
                               IncludeLocks = dependencies
                                              .Select(d => new IncludeLockDefinition(d.Id,
                                                                                     DependencyVersion.Parse("1.0")))
                                              .ToImmutableList(),
                           });

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Remove(path, predicate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsInstanceOf<DependencyRemovedResult>(result);
        var removedDependency = ((DependencyRemovedResult)result).Dependency;
        Assert.NotNull(removedDependency);
        Assert.AreSame(dependency1, removedDependency);

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
        await _lockStore.Received().Get(path);

        await _definitionStore.Received()
                              .Write(path,
                                     Arg.Is((IncludeManagerDefinition d) =>
                                                d.Dependencies.Count      == 1 &&
                                                d.Dependencies.First().Id == dependency2.Id),
                                     CancellationToken.None);
        _fileSystem.Received().Delete(dependency1.DownloadPath);
        _versionStore.Received().Delete(dependency1);
        await _lockStore.Received()
                        .Set(Arg.Is((LockDefinition d) =>
                                        d.IncludeLocks.Count == 1 && d.IncludeLocks.First().Id == dependency2.Id),
                             path,
                             CancellationToken.None);
    }

    [Test]
    public async Task RemoveReturnsErrorIfPredicateDoesNotFilAnyDependency()
    {
        var path        = Path.Combine(Environment.CurrentDirectory, "dependencies", "multiple");
        var dependency1 = new GitHubTagFileDependency("owner",  "repository",  VersionRange.All, "assetName");
        var dependency2 = new GitHubTagFileDependency("owner2", "repository2", VersionRange.All, "assetName2");
        var dependencies = new List<IDependency>
                           {
                               dependency1,
                               dependency2,
                           }.AsReadOnly();
        _mediator.Send(Arg.Any<GetInstalledDependenciesQuery>()).Returns(dependencies);
        _lockStore.Get(path)
                  .Returns(new LockDefinition
                           {
                               IncludeLocks = dependencies
                                              .Select(d => new IncludeLockDefinition(d.Id,
                                                                                     DependencyVersion.Parse("1.0")))
                                              .ToImmutableList(),
                           });

        var includeManager = _serviceProvider.GetRequiredService<IIncludeManager>();
        var result         = await includeManager.Remove(path, (_, _) => false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsInstanceOf<ErrorResult>(result);

        await _mediator.Received().Send(Arg.Any<GetInstalledDependenciesQuery>());
    }

    private static bool ResultIsValid(IResult result)
    {
        Assert.NotNull(result);
        Assert.IsInstanceOf<DependencyResult>(result);

        return true;
    }

    private static bool ResultIsValidAndContains(IResult           result,
                                                 string            dependencyId,
                                                 DependencyVersion version,
                                                 string            downloadPath)
    {
        Assert.NotNull(result);
        Assert.IsInstanceOf<DependencyResult>(result);
        var dependencyResult = (DependencyResult)result;
        Assert.AreEqual(1, dependencyResult.Dependencies.Count);
        var dependencyLock = dependencyResult.Dependencies.First();
        Assert.AreEqual(dependencyId, dependencyLock.Id);
        Assert.AreEqual(version,      dependencyLock.Version);
        Assert.AreEqual(downloadPath, dependencyLock.DownloadPath);

        return true;
    }

    private static bool ErrorIs<T>(IResult result)
    {
        Assert.NotNull(result);

        var dependencyResult = result as DependencyResult;
        Assert.IsNotNull(dependencyResult);
        Assert.IsNotNull(dependencyResult!.Errors);
        Assert.AreEqual(1, dependencyResult.Errors.Count);
        Assert.IsInstanceOf<T>(dependencyResult.Errors[0]);

        return true;
    }

    // ReSharper disable ObjectCreationAsStatement
    [Test]
    public void CtorThrowsWhenMediatorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IncludeManager(null!,
                                                                      _lockStore,
                                                                      _versionStore,
                                                                      _localStore,
                                                                      _definitionStore,
                                                                      _fileSystem));
    }

    [Test]
    public void CtorThrowsWhenLockStoreIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IncludeManager(_mediator,
                                                                      null!,
                                                                      _versionStore,
                                                                      _localStore,
                                                                      _definitionStore,
                                                                      _fileSystem));
    }

    [Test]
    public void CtorThrowsWhenVersionStoreIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IncludeManager(_mediator,
                                                                      _lockStore,
                                                                      null!,
                                                                      _localStore,
                                                                      _definitionStore,
                                                                      _fileSystem));
    }

    [Test]
    public void CtorThrowsWhenLocalStoreIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IncludeManager(_mediator,
                                                                      _lockStore,
                                                                      _versionStore,
                                                                      null!,
                                                                      _definitionStore,
                                                                      _fileSystem));
    }

    [Test]
    public void CtorThrowsWhenDefinitionStoreIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IncludeManager(_mediator,
                                                                      _lockStore,
                                                                      _versionStore,
                                                                      _localStore,
                                                                      null!,
                                                                      _fileSystem));
    }

    [Test]
    public void CtorThrowsWhenFileSystemIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IncludeManager(_mediator,
                                                                      _lockStore,
                                                                      _versionStore,
                                                                      _localStore,
                                                                      _definitionStore,
                                                                      null!));
    }

    // ReSharper restore ObjectCreationAsStatement
}