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

using System.Text.Json.Serialization;
using Json.Schema.Generation;
using MediatR;
using NuGet.Versioning;
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagFile;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag;

public class GitHubTagFileDependency : GitHubTagDependencyBase, IDependency
{
    public GitHubTagFileDependency(string owner, string repository, VersionRange versionRange, string assetName)
        : this(owner, repository, versionRange, assetName, null)
    {
    }

    public GitHubTagFileDependency(string       owner,
                                   string       repository,
                                   VersionRange versionRange,
                                   string       assetName,
                                   string?      downloadPath,
                                   string?      versionRegEx = null)
        : base(owner, repository, versionRange, DependencyType.GitHubTagFile, versionRegEx)
    {
        Throw.IfNullOrEmpty(assetName, nameof(assetName));

        AssetName    = assetName;
        DownloadPath = string.IsNullOrWhiteSpace(downloadPath) ? Path.Combine("include", assetName) : downloadPath;
    }

    public string AssetName { get; }

    [JsonIgnore]
    public string Id => $"GitHubTagFile:{Owner}/{Repository}:{AssetName}";

    [Required]
    public string DownloadPath { get; }

    public IRequest Download(DependencyVersion dependencyVersion) =>
        new DownloadGitHubTagFileQuery(this, dependencyVersion);

    protected bool Equals(GitHubTagFileDependency other) =>
        base.Equals(other) && AssetName == other.AssetName && DownloadPath == other.DownloadPath;

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((GitHubTagFileDependency)obj);
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), AssetName, DownloadPath);

    public static IDependency From(IDictionary<string, string> dict, VersionRange versionRange)
    {
        var owner        = dict.Read("owner");
        var repository   = dict.Read("repository");
        var assetName    = dict.Read("assetName");
        var downloadPath = dict.ContainsKey("downloadPath") ? dict["downloadPath"] : null;
        var versionRegEx = dict.ContainsKey("versionRegEx") ? dict["versionRegEx"] : null;

        return new GitHubTagFileDependency(owner, repository, versionRange, assetName, downloadPath, versionRegEx);
    }
}