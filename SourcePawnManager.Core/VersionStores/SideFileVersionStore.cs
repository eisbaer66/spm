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
using SourcePawnManager.Core.Apis.FileSystems;
using SourcePawnManager.Core.DependencyStrategy;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag;

namespace SourcePawnManager.Core.VersionStores;

public class SideFileVersionStore : IVersionStore
{
    private readonly IJsonFileSystem               _fileSystem;
    private readonly ILogger<SideFileVersionStore> _logger;

    public SideFileVersionStore(IJsonFileSystem fileSystem, ILogger<SideFileVersionStore> logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DependencyVersion?> GetExistingVersion(IDependency       dependency,
                                                             CancellationToken cancellationToken = default)
    {
        var sideFilePath      = GetSideFilePath(dependency);
        var dependencyVersion = await _fileSystem.ReadJson<DependencyVersion>(sideFilePath, cancellationToken);

        return dependencyVersion;
    }

    public void Clean(IEnumerable<string> previousDownloadDirectories, IEnumerable<IDependency> dependencies)
    {
        var lookup = dependencies.Select(d => d.DownloadPath + ".version").ToHashSet();
        bool NotUsed(string f) => !lookup.Contains(Path.GetRelativePath(Environment.CurrentDirectory, f));
        foreach (var previousDownloadDirectory in previousDownloadDirectories)
        {
            var danglingVersionFiles = _fileSystem.GetFiles(previousDownloadDirectory, "*.inc.version")
                                                  .Where(NotUsed);

            foreach (var danglingVersionFile in danglingVersionFiles)
            {
                _logger.LogInformation("deleting dangling .version-file {DanglingVersionFilePath}",
                                       danglingVersionFile);
                _fileSystem.Delete(danglingVersionFile);
            }
        }
    }

    public async Task Set(IDependency       dependency,
                          DependencyVersion version,
                          CancellationToken cancellationToken = default)
    {
        if (dependency == null)
        {
            throw new ArgumentNullException(nameof(dependency));
        }

        if (version == null)
        {
            throw new ArgumentNullException(nameof(version));
        }

        var sideFilePath = GetSideFilePath(dependency);

        await _fileSystem.WriteJson(version, sideFilePath, cancellationToken);
    }

    public void Delete(IDependency dependency)
    {
        if (dependency == null)
        {
            throw new ArgumentNullException(nameof(dependency));
        }

        var sideFilePath = GetSideFilePath(dependency);
        _fileSystem.Delete(sideFilePath);
    }

    private static string GetSideFilePath(IDependency dependency) => dependency.DownloadPath + ".version";
}