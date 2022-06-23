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
using SourcePawnManager.Core.DependencyStrategy.GitHubTag.DownloadGitHubTagZip;

namespace SourcePawnManager.Core.DependencyStrategy.GitHubTag;

public class GitHubTagZipDependency : GitHubTagDependencyBase, IDependency
{
    public GitHubTagZipDependency(string       owner,
                                  string       repository,
                                  VersionRange versionRange,
                                  string       assetName,
                                  string       fileInZip)
        : this(owner, repository, versionRange, assetName, fileInZip, null)
    {
    }

    public GitHubTagZipDependency(string       owner,
                                  string       repository,
                                  VersionRange versionRange,
                                  string       assetName,
                                  string       fileInZip,
                                  string?      downloadPath,
                                  string?      versionRegEx = null)
        : base(owner, repository, versionRange, DependencyType.GitHubTagZip, versionRegEx)
    {
        Throw.IfNullOrEmpty(assetName, nameof(assetName));
        Throw.IfNullOrEmpty(fileInZip, nameof(fileInZip));

        AssetName = assetName;
        FileInZip = fileInZip;
        DownloadPath = string.IsNullOrWhiteSpace(downloadPath)
                           ? Path.Combine("include", Path.GetFileName(FileInZip))
                           : downloadPath;
    }

    public string AssetName { get; }
    public string FileInZip { get; }

    [JsonIgnore]
    public string Id => $"GitHubTagZip:{Owner}/{Repository}:{AssetName}:{FileInZip}";

    [Required]
    public string DownloadPath { get; }

    public IRequest Download(DependencyVersion dependencyVersion) =>
        new DownloadGitHubTagZipQuery(this, dependencyVersion);

    protected bool Equals(GitHubTagZipDependency other) => base.Equals(other)              &&
                                                           AssetName    == other.AssetName &&
                                                           FileInZip    == other.FileInZip &&
                                                           DownloadPath == other.DownloadPath;

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

        return Equals((GitHubTagZipDependency)obj);
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), AssetName, FileInZip, DownloadPath);

    public static IDependency From(IDictionary<string, string> dict, VersionRange versionRange)
    {
        var owner        = dict.Read("owner");
        var repository   = dict.Read("repository");
        var assetName    = dict.Read("assetName");
        var fileInZip    = dict.Read("fileInZip");
        var downloadPath = dict.ContainsKey("downloadPath") ? dict["downloadPath"] : null;
        var versionRegEx = dict.ContainsKey("versionRegEx") ? dict["versionRegEx"] : null;

        return new GitHubTagZipDependency(owner,
                                          repository,
                                          versionRange,
                                          assetName,
                                          fileInZip,
                                          downloadPath,
                                          versionRegEx);
    }
}