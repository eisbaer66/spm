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
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.VersionStores;

public class DryRunVersionStore : IVersionStore
{
    private readonly ILogger<DryRunVersionStore> _logger;
    private readonly IVersionStore               _versionStore;

    public DryRunVersionStore(IVersionStore versionStore, ILogger<DryRunVersionStore> logger)
    {
        _versionStore = versionStore ?? throw new ArgumentNullException(nameof(versionStore));
        _logger       = logger       ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<DependencyVersion?> GetExistingVersion(IDependency       dependency,
                                                       CancellationToken cancellationToken = default) =>
        _versionStore.GetExistingVersion(dependency, cancellationToken);

    public void Clean(IEnumerable<string> previousDownloadDirectories, IEnumerable<IDependency> dependencies)
    {
        _logger.LogInformation("would have cleaned {PreviousDownloadDirectories}", previousDownloadDirectories);
    }

    public Task Set(IDependency dependency, DependencyVersion version, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("would have set version {DependencyVersion} for {Dependency}", version, dependency);
        return Task.CompletedTask;
    }

    public void Delete(IDependency dependency)
    {
        _logger.LogInformation("would have deleted {@Dependency}", dependency);
    }
}