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

using System.Collections.Immutable;
using MediatR;
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DefinitionStores;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;
using SourcePawnManager.Core.LocalStores;
using SourcePawnManager.Core.LockStores;
using SourcePawnManager.Core.Mediator.GetInstalledDependenciesQuery;
using SourcePawnManager.Core.Results;
using SourcePawnManager.Core.Results.Internal;
using SourcePawnManager.Core.VersionStores;

namespace SourcePawnManager.Core;

public class IncludeManager : IIncludeManager
{
    private readonly IDefinitionStore _definitionStore;
    private readonly IFileSystem      _fileSystem;
    private readonly ILocalStore      _localStore;
    private readonly ILockStore       _lockStore;
    private readonly IMediator        _mediator;
    private readonly IVersionStore    _versionStore;

    public IncludeManager(IMediator        mediator,
                          ILockStore       lockStore,
                          IVersionStore    versionStore,
                          ILocalStore      localStore,
                          IDefinitionStore definitionStore,
                          IFileSystem      fileSystem)
    {
        _mediator        = mediator        ?? throw new ArgumentNullException(nameof(mediator));
        _lockStore       = lockStore       ?? throw new ArgumentNullException(nameof(lockStore));
        _versionStore    = versionStore    ?? throw new ArgumentNullException(nameof(versionStore));
        _localStore      = localStore      ?? throw new ArgumentNullException(nameof(localStore));
        _definitionStore = definitionStore ?? throw new ArgumentNullException(nameof(definitionStore));
        _fileSystem      = fileSystem      ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<IResult> Remove(string                       basePath,
                                      Func<IDependency, int, bool> predicate,
                                      CancellationToken            cancellationToken = default)
    {
        Throw.IfNullOrEmpty(basePath, nameof(basePath));
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var installedDependencies =
            await _mediator.Send(new GetInstalledDependenciesQuery(basePath), cancellationToken);
        var dependency = installedDependencies.Select((d, i) => new { d, i })
                                              .FirstOrDefault(x => predicate(x.d, x.i))
                                              ?.d;
        if (dependency == null)
        {
            return new ErrorResult("no matching dependency found", 1);
        }

        var dependencies   = installedDependencies.Where(d => d.Id != dependency.Id).ToImmutableList();
        var lockedVersions = await GetLockedVersions(basePath, cancellationToken);
        if (lockedVersions.ContainsKey(dependency.Id))
        {
            lockedVersions.Remove(dependency.Id);
        }

        await _definitionStore.Write(basePath,
                                     new() { Dependencies = dependencies },
                                     cancellationToken);
        _fileSystem.Delete(dependency.DownloadPath);
        _versionStore.Delete(dependency);
        await _lockStore.Set(new()
                             {
                                 IncludeLocks = lockedVersions
                                                .Select(kvp => new IncludeLockDefinition(kvp.Key, kvp.Value))
                                                .ToArray(),
                             },
                             basePath,
                             cancellationToken);

        return new DependencyRemovedResult(dependency);
    }

    public async Task<IResult> Install(string            basePath,
                                       IDependency       dependency,
                                       CancellationToken cancellationToken = default)
    {
        Throw.IfNullOrEmpty(basePath, nameof(basePath));
        if (dependency == null)
        {
            throw new ArgumentNullException(nameof(dependency));
        }

        var lockedVersions = await GetLockedVersions(basePath, cancellationToken);
        if (lockedVersions.ContainsKey(dependency.Id))
        {
            lockedVersions.Remove(dependency.Id);
        }

        var versions = await _mediator.Send(dependency.GetVersions(), cancellationToken);
        var result   = await GetDownloadVersion(lockedVersions, dependency, versions, cancellationToken);
        if (result is not DownloadVersionResult downloadVersionResult)
        {
            return result;
        }

        var installedDependencies = await _mediator.Send(new GetInstalledDependenciesQuery(basePath), cancellationToken);
        var dependencies = installedDependencies.Concat(new[] { dependency }).ToImmutableList();
        await _definitionStore.Write(basePath,
                                     new() { Dependencies = dependencies },
                                     cancellationToken);
        await Download(dependency, downloadVersionResult.DownloadVersion, false, lockedVersions, cancellationToken);
        await _lockStore.Set(new()
                             {
                                 IncludeLocks = lockedVersions
                                                .Select(kvp => new IncludeLockDefinition(kvp.Key, kvp.Value))
                                                .ToArray(),
                             },
                             basePath,
                             cancellationToken);
        await SaveLocalDefinition(dependencies, basePath, cancellationToken);

        return new DependencyResult(new List<IDependencyLock>
                                    {
                                        new DependencyLock(dependency.Id,
                                                           lockedVersions[dependency.Id],
                                                           dependency.DownloadPath),
                                    },
                                    "installed");
    }

    public async Task<IResult> Restore(string basePath, CancellationToken cancellationToken = default)
    {
        Throw.IfNullOrEmpty(basePath, nameof(basePath));

        var lockedVersions = await GetLockedVersions(basePath, cancellationToken);
        var result = await Download(basePath,
                                    lockedVersions,
                                    (dependency, versions) => GetDownloadVersion(lockedVersions, dependency, versions, cancellationToken),
                                    cancellationToken);
        if (result is not DownloadResult downloadResult)
        {
            return result;
        }

        return new DependencyResult(downloadResult.Dependencies, "restored");
    }

    public async Task<IResult> Update(string basePath, CancellationToken cancellationToken = default)
    {
        Throw.IfNullOrEmpty(basePath, nameof(basePath));

        var result = await Download(basePath,
                                    new Dictionary<string, DependencyVersion>(),
                                    (dependency, versions) => GetDownloadVersion(new Dictionary<string, DependencyVersion>(),
                                                                                 dependency,
                                                                                 versions,
                                                                                 cancellationToken),
                                    cancellationToken);
        if (result is not DownloadResult downloadResult)
        {
            return result;
        }

        return new DependencyResult(downloadResult.Dependencies, "updated");
    }

    private async Task<IResult> Download(string                                                     basePath,
                                         IDictionary<string, DependencyVersion>                     lockedVersions,
                                         Func<IDependency, IList<DependencyVersion>, Task<IResult>> getDownloadVersion,
                                         CancellationToken                                          cancellationToken)
    {
        Throw.IfNullOrEmpty(basePath, nameof(basePath));
        if (lockedVersions == null)
        {
            throw new ArgumentNullException(nameof(lockedVersions));
        }

        if (getDownloadVersion == null)
        {
            throw new ArgumentNullException(nameof(getDownloadVersion));
        }

        var dependencies = await _mediator.Send(new GetInstalledDependenciesQuery(basePath), cancellationToken);

        var dependencyLocks = new List<IDependencyLock>();
        foreach (var dependency in dependencies)
        {
            var versions = await _mediator.Send(dependency.GetVersions(), cancellationToken);
            var result   = await getDownloadVersion(dependency, versions);
            if (result is not DownloadVersionResult downloadVersionResult)
            {
                return result;
            }

            await Download(dependency,
                           downloadVersionResult.DownloadVersion,
                           downloadVersionResult.Locked,
                           lockedVersions,
                           cancellationToken);
            dependencyLocks.Add(new DependencyLock(dependency.Id,
                                                   downloadVersionResult.DownloadVersion,
                                                   dependency.DownloadPath));
        }

        await _lockStore.Set(new()
                             {
                                 IncludeLocks = lockedVersions
                                                .Select(kvp => new IncludeLockDefinition(kvp.Key, kvp.Value))
                                                .ToArray(),
                             },
                             basePath,
                             cancellationToken);
        var localDefinition = await _localStore.Get(basePath, cancellationToken);
        SaveLocalDefinition(localDefinition, dependencies, basePath, cancellationToken);
        Clean(localDefinition, dependencies);

        return new DownloadResult(dependencyLocks);
    }

    private void Clean(IncludeManagerLocalDefinition? localDefinition, IEnumerable<IDependency> dependencies)
    {
        if (localDefinition?.PreviousDownloadPaths == null)
        {
            return;
        }

        if (localDefinition.PreviousDownloadPaths.Count == 0)
        {
            return;
        }

        _versionStore.Clean(localDefinition.PreviousDownloadPaths, dependencies);
    }

    private async Task SaveLocalDefinition(IEnumerable<IDependency> dependencies,
                                           string                   path,
                                           CancellationToken        cancellationToken)
    {
        var localDefinition = await _localStore.Get(path, cancellationToken);
        SaveLocalDefinition(localDefinition, dependencies, path, cancellationToken);
    }

    private void SaveLocalDefinition(IncludeManagerLocalDefinition? localDefinition,
                                     IEnumerable<IDependency>       dependencies,
                                     string                         path,
                                     CancellationToken              cancellationToken)
    {
        if (localDefinition?.PreviousDownloadPaths is null)
        {
            localDefinition = new();
        }

        var hashSet = localDefinition.PreviousDownloadPaths.ToHashSet();
        var currentDirectories = dependencies.Select(d => Path.GetDirectoryName(d.DownloadPath))
                                             .Where(d => !string.IsNullOrEmpty(d))
                                             .GroupBy(d => d!)
                                             .Select(g => g.Key);
        foreach (var directory in currentDirectories)
        {
            if (hashSet.Contains(directory))
            {
                continue;
            }

            hashSet.Add(directory);
        }

        var newDefinition = new IncludeManagerLocalDefinition
                            {
                                PreviousDownloadPaths = hashSet,
                            };
        _localStore.Set(newDefinition, path, cancellationToken);
    }

    private async Task Download(IDependency                            dependency,
                                DependencyVersion                      downloadVersion,
                                bool                                   locked,
                                IDictionary<string, DependencyVersion> lockedVersions,
                                CancellationToken                      cancellationToken)
    {
        await _mediator.Send(dependency.Download(downloadVersion), cancellationToken);
        await _versionStore.Set(dependency, downloadVersion, cancellationToken);

        if (!locked)
        {
            lockedVersions.Add(dependency.Id, downloadVersion);
        }
    }

    private async Task<IResult> GetDownloadVersion(IDictionary<string, DependencyVersion> lockedVersions,
                                                   IDependency                            dependency,
                                                   IList<DependencyVersion>               versions,
                                                   CancellationToken                      cancellationToken)
    {
        if (lockedVersions.ContainsKey(dependency.Id))
        {
            var lockedVersion = lockedVersions[dependency.Id];
            if (versions.All(v => !Equals(v, lockedVersion)))
            {
                return new LockedVersionNotAvailableResult(lockedVersion, versions, dependency.Id);
            }

            return new DownloadVersionResult(true, lockedVersion);
        }

        var bestMatch = dependency.VersionRange.FindBestMatch(versions.OrderByDescending(v => v.Version).Select(v => v.Version));
        if (bestMatch == null)
        {
            return new NoBestMatchingVersionFoundResult(versions, dependency.Id);
        }

        var bestMatchingDependencyVersion = versions.First(v => v.Version == bestMatch);

        var existingVersion = await _versionStore.GetExistingVersion(dependency, cancellationToken);

        if (existingVersion != null && existingVersion.Version >= bestMatchingDependencyVersion.Version)
        {
            return new BestMatchingVersionAlreadyInstalledResult(bestMatchingDependencyVersion,
                                                                 existingVersion,
                                                                 dependency.Id);
        }

        return new DownloadVersionResult(false, bestMatchingDependencyVersion);
    }

    private async Task<IDictionary<string, DependencyVersion>> GetLockedVersions(string            basePath,
                                                                                 CancellationToken cancellationToken)
    {
        var lockDefinition = await _lockStore.Get(basePath, cancellationToken);
        return lockDefinition.IncludeLocks.ToDictionary(d => d.Id, d => d.Version);
    }
}